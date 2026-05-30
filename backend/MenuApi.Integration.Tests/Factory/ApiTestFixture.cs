using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Xunit;


namespace MenuApi.Integration.Tests.Factory;

public class ApiTestFixture : IAsyncLifetime
{
    public DistributedApplication app { get; private set; } = null!;
    private IDistributedApplicationTestingBuilder appHost = null!;
    private AuthenticationHeaderValue cachedAuthHeader = null!;

    public async Task<HttpClient> GetHttpClient()
    {
        var httpClient = app.CreateHttpClient("apiservice");

        cachedAuthHeader ??= await new ApiAuthentication().GetAuthenticationHeaderValue().ConfigureAwait(false);

        httpClient.DefaultRequestHeaders.Authorization = cachedAuthHeader;
        return httpClient;
    }

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        // Set test mode to skip UI installation
        Environment.SetEnvironmentVariable("ASPIRE_TEST_MODE", "true");

        appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Menu_AppHost>();

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        appHost.Services.AddLogging(builder =>
        {
            builder.AddXUnit();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        appHost.WithContainersLifetime(ContainerLifetime.Session);

        app = await appHost.BuildAsync();

        // Retry app startup to handle Docker daemon health checks in CI environments
        const int maxRetries = 3;
        const int delayMs = 2000;
        Exception? lastException = null;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await app.StartAsync().ConfigureAwait(false);
                break;
            }
            catch (Exception ex) when (ex.Message.Contains("unhealthy"))
            {
                lastException = ex;
                if (attempt < maxRetries)
                {
                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
            }
        }

        if (lastException != null)
        {
            throw lastException;
        }

        var resourceNotificationService = app.Services
            .GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService.WaitForResourceAsync(
            "migrations",
            KnownResourceStates.Finished
            )
            .WaitAsync(TimeSpan.FromSeconds(120)).ConfigureAwait(false);

        await resourceNotificationService.WaitForResourceAsync(
            "apiservice",
            KnownResourceStates.Running
            )
            .WaitAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
    }
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await app.StopAsync().ConfigureAwait(false);
        await app.DisposeAsync().ConfigureAwait(false);
    }
}

[CollectionDefinition("API Host Collection")]
public class ApiHostCollection : ICollectionFixture<ApiTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

public static class TestExtensions
{
    public static TBuilder WithContainersLifetime<TBuilder>(this TBuilder builder, ContainerLifetime containerLifetime)
    where TBuilder : IDistributedApplicationTestingBuilder
    {
        var containerLifetimeAnnotations = builder.Resources.SelectMany(r => r.Annotations
            .OfType<ContainerLifetimeAnnotation>()
            .Where(c => c.Lifetime != containerLifetime))
            .ToList();

        foreach (var annotation in containerLifetimeAnnotations)
        {
            annotation.Lifetime = containerLifetime;
        }

        return builder;
    }

    /// <summary>
    /// Asserts that the response has the expected status code. On failure, includes
    /// the response body in the assertion message for easier debugging of 500 errors.
    /// </summary>
    public static async Task ShouldHaveStatusCode(this HttpResponseMessage response, System.Net.HttpStatusCode expected)
    {
        if (response.StatusCode != expected)
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new Xunit.Sdk.XunitException(
                $"Expected status code {(int)expected} {expected} but received {(int)response.StatusCode} {response.StatusCode}.\n\nResponse body:\n{body}");
        }
    }
}

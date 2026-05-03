using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Threading;
using Xunit;


namespace MenuApi.Integration.Tests.Factory;

public class ApiTestFixture : IAsyncLifetime
{
    private static readonly SemaphoreSlim SharedStateLock = new(1, 1);
    private static DistributedApplication sharedApp;
    private static AuthenticationHeaderValue sharedAuthHeader;
    private static int activeFixtureCount;
    private static bool applicationCreated;

    public DistributedApplication app { get; private set; }

    public async Task<HttpClient> GetHttpClient()
    {
        var httpClient = app.CreateHttpClient("apiservice");

        sharedAuthHeader ??= await new ApiAuthentication().GetAuthenticationHeaderValue();

        httpClient.DefaultRequestHeaders.Authorization = sharedAuthHeader;
        return httpClient;
    }

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        await SharedStateLock.WaitAsync();
        try
        {
            if (sharedApp is null)
            {
                if (applicationCreated)
                {
                    throw new InvalidOperationException(
                        "ApiTestFixture should only create the distributed application once per test run.");
                }

                sharedApp = await CreateSharedAppAsync();
                applicationCreated = true;
            }

            activeFixtureCount++;
            app = sharedApp;
        }
        finally
        {
            SharedStateLock.Release();
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        DistributedApplication appToDispose = null;

        await SharedStateLock.WaitAsync();
        try
        {
            if (activeFixtureCount > 0)
            {
                activeFixtureCount--;
            }

            if (activeFixtureCount == 0 && sharedApp is not null)
            {
                appToDispose = sharedApp;
                sharedApp = null;
                sharedAuthHeader = null;
            }
        }
        finally
        {
            SharedStateLock.Release();
        }

        if (appToDispose is not null)
        {
            await appToDispose.StopAsync();
            await appToDispose.DisposeAsync();
        }
    }

    private static async Task<DistributedApplication> CreateSharedAppAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
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

        var app = await appHost.BuildAsync();

        await app.StartAsync();

        var resourceNotificationService = app.Services
            .GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService.WaitForResourceAsync(
            "migrations",
            KnownResourceStates.Finished
            )
            .WaitAsync(TimeSpan.FromSeconds(120));

        await resourceNotificationService.WaitForResourceAsync(
            "apiservice",
            KnownResourceStates.Running
            )
            .WaitAsync(TimeSpan.FromSeconds(30));

        return app;
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
            var body = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"Expected status code {(int)expected} {expected} but received {(int)response.StatusCode} {response.StatusCode}.\n\nResponse body:\n{body}");
        }
    }
}

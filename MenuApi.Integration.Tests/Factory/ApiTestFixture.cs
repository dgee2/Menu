using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Xunit;
using Xunit.Abstractions;


namespace MenuApi.Integration.Tests.Factory;

public class ApiTestFixture : IAsyncLifetime
{
    public DistributedApplication app { get; private set; }
    private IDistributedApplicationTestingBuilder appHost;
    private readonly IMessageSink messageSink;
    private AuthenticationHeaderValue cachedAuthHeader;

    public ApiTestFixture(IMessageSink messageSink)
    {
        this.messageSink = messageSink;
    }

    public async Task<HttpClient> GetHttpClient()
    {
        var httpClient = app.CreateHttpClient("apiservice");

        cachedAuthHeader ??= await new ApiAuthentication().GetAuthenticationHeaderValue();

        httpClient.DefaultRequestHeaders.Authorization = cachedAuthHeader;
        return httpClient;
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Menu_AppHost>();

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        appHost.Services.AddLogging(builder =>
        {
            builder.AddXUnit(messageSink);
            builder.SetMinimumLevel(LogLevel.Information);
        });

        appHost.WithContainersLifetime(ContainerLifetime.Session);

        app = await appHost.BuildAsync();

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
    }
    async Task IAsyncLifetime.DisposeAsync()
    {
        await app.StopAsync();
        await app.DisposeAsync();
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
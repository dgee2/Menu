using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;


namespace MenuApi.Integration.Tests.Factory;

public class ApiTestFixture : IAsyncLifetime
{
    public DistributedApplication app { get; private set; }
    private IDistributedApplicationTestingBuilder appHost;

    public HttpClient GetHttpClient() => app.CreateHttpClient("apiservice");

    async Task IAsyncLifetime.InitializeAsync()
    {
        appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Menu_AppHost>();

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        appHost.WithContainersLifetime(ContainerLifetime.Session);

        app = await appHost.BuildAsync();

        await app.StartAsync();

        var resourceNotificationService = app.Services
            .GetRequiredService<ResourceNotificationService>();
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
}
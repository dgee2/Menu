var builder = DistributedApplication.CreateBuilder(args);

var auth0Domain = builder.AddParameter("Auth0Domain");
var auth0Audience = builder.AddParameter("Auth0Audience");

builder.AddRedis("cache");

var sql = builder.AddSqlServer("sql")
                 .WithDockerfile(".", "Dockerfile.sqlserver-fts")
                 .WithLifetime(ContainerLifetime.Persistent);

var menuDb = sql.AddDatabase("menu");

var migrations = builder.AddProject<Projects.Menu_MigrationService>("migrations")
    .WithReference(menuDb)
    .WaitFor(menuDb);

var isTestMode = Environment.GetEnvironmentVariable("ASPIRE_TEST_MODE") == "true";
var menuApi = builder.AddProject<Projects.MenuApi>("apiservice")
       .WithHttpEndpoint(name: "menuApiHttp", port: 5002)
       .WithReference(menuDb)
       .WaitForCompletion(migrations)
       .WithEnvironment("Auth0Domain", auth0Domain)
       .WithEnvironment("Auth0Audience", auth0Audience)
       .WithEnvironment("RecipeSearchProjection__PollingIntervalSeconds", isTestMode ? "1" : "5")
       .WithEnvironment(
           "E2eCleanupEnabled",
           Environment.GetEnvironmentVariable("MENU_E2E_TEST_MODE") == "true" ? "true" : "false");

if (!isTestMode)
{
    builder.AddViteApp("menu-ui", "../../ui/menu-website", "aspire")
        .WithPnpm()
        .WithReference(menuApi)
        .WithEnvironment("VITE_MENU_API_URL", menuApi.GetEndpoint("menuApiHttp"))
        .WithEnvironment("VITE_AUTH0_AUDIENCE", auth0Audience)
        .WaitFor(menuApi)
        // Keep the public callback URL stable and below Windows' dynamic port exclusion range.
        // AddViteApp passes Aspire's separate allocated target port to Vite.
        .WithEndpoint("http", endpoint => endpoint.Port = 5173)
        .WithHttpHealthCheck("/", endpointName: "http")
        .PublishAsDockerFile();
}

await builder.Build().RunAsync();

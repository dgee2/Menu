var builder = DistributedApplication.CreateBuilder(args);

var auth0Domain = builder.AddParameter("Auth0Domain");
var auth0Audience = builder.AddParameter("Auth0Audience");

builder.AddRedis("cache");

var sql = builder.AddSqlServer("sql")
                 .WithLifetime(ContainerLifetime.Persistent);

var menuDb = sql.AddDatabase("menu");

var migrations = builder.AddProject<Projects.Menu_MigrationService>("migrations")
    .WithReference(menuDb)
    .WaitFor(menuDb);

var menuApi = builder.AddProject<Projects.MenuApi>("apiservice")
       .WithHttpEndpoint(name: "menuApiHttp", port: 65273)
       .WithReference(menuDb)
       .WaitForCompletion(migrations)
       .WithEnvironment("Auth0Domain", auth0Domain)
       .WithEnvironment("Auth0Audience", auth0Audience);

builder.AddJavaScriptApp("menu-ui", "../ui/menu-website", "aspire")
    .WithPnpm()
    .WithReference(menuApi)
    .WithEnvironment("VITE_MENU_API_URL", menuApi.GetEndpoint("menuApiHttp"))
    .WaitFor(menuApi)
    .WithHttpEndpoint(name: "menuUI", env: "PORT", port: 65276, targetPort: 5173)
    .PublishAsDockerFile();

await builder.Build().RunAsync();

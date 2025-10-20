var builder = DistributedApplication.CreateBuilder(args);

var auth0Domain = builder.AddParameter("Auth0Domain");
var auth0Audience = builder.AddParameter("Auth0Audience");

builder.AddRedis("cache");

var sql = builder.AddSqlServer("sql")
                 .WithLifetime(ContainerLifetime.Persistent)
                 .AddDatabase("menu");

var menuDB = builder.AddSqlProject<Projects.MenuDB>("menuDB")
                    .WithReference(sql)
                    .WithParentRelationship(sql);

var menuApi = builder.AddProject<Projects.MenuApi>("apiservice")
       .WithHttpEndpoint(name: "menuApiHttp", port: 65273)
       .WithReference(sql)
       .WaitForCompletion(menuDB)
       .WithEnvironment("Auth0Domain", auth0Domain)
       .WithEnvironment("Auth0Audience", auth0Audience);

builder.AddPnpmApp("menu-ui", "../ui/menu-website", scriptName: "aspire")
    .WithReference(menuApi)
    .WithEnvironment("VITE_MENU_API_URL", menuApi.GetEndpoint("menuApiHttp"))
    .WaitFor(menuApi)
    .WithHttpEndpoint(name: "menuUI", env: "PORT", port: 65276, targetPort: 5173)
    .WithPnpmPackageInstallation()
    .PublishAsDockerFile();

await builder.Build().RunAsync();

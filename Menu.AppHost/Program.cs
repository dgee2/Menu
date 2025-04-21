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
       .WithHttpEndpoint()
       .WithReference(sql)
       .WaitForCompletion(menuDB)
       .WithEnvironment("Auth0Domain", auth0Domain)
       .WithEnvironment("Auth0Audience", auth0Audience);

builder.AddPnpmApp("menu-ui", "../ui/menu-website", scriptName: "dev")
    .WithReference(menuApi)
    .WaitFor(menuApi)
    .WithHttpEndpoint(env: "PORT", port: 65276)
    .WithPnpmPackageInstallation()
    .PublishAsDockerFile();

await builder.Build().RunAsync();

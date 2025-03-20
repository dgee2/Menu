var builder = DistributedApplication.CreateBuilder(args);

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
       .WaitForCompletion(menuDB);

builder.AddNpmApp("menu-ui", "../ui/menu-website", scriptName: "dev")
    .WithReference(menuApi)
    .WaitFor(menuApi)
    .WithHttpEndpoint(env: "PORT")
    .WithNpmPackageInstallation(useCI: true)
    .PublishAsDockerFile();

await builder.Build().RunAsync();

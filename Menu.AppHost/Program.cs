var builder = DistributedApplication.CreateBuilder(args);

builder.AddRedis("cache");

var sql = builder.AddSqlServer("sql")
                 .WithLifetime(ContainerLifetime.Persistent)
                 .AddDatabase("menu");

var menuDB = builder.AddSqlProject<Projects.MenuDB>("menuDB")
                    .WithReference(sql)
                    .WithParentRelationship(sql);

builder.AddProject<Projects.MenuApi>("apiservice")
       .WithHttpEndpoint()
       .WithHttpsEndpoint()
       .WithExternalHttpEndpoints()
       .WithReference(sql)
       .WaitForCompletion(menuDB);

await builder.Build().RunAsync();

var builder = DistributedApplication.CreateBuilder(args);

builder.AddRedis("cache");

var sql = builder.AddSqlServer("sql")
                 .WithLifetime(ContainerLifetime.Persistent)
                 .AddDatabase("menu");

var menuDB = builder.AddSqlProject<Projects.MenuDB>("menuDB")
                    .WithReference(sql);

builder.AddProject<Projects.MenuApi>("apiservice")
       .WithExternalHttpEndpoints()
       .WithHttpsEndpoint(port: 5001)
       .WithReference(sql)
       .WaitForCompletion(menuDB);

await builder.Build().RunAsync();

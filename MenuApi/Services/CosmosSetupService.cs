using MenuApi.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MenuApi.Services
{
    public class CosmosSetupService : IHostedService
    {
        private readonly ILogger<CosmosSetupService> logger;
        private readonly CosmosClient cosmosClient;
        private readonly CosmosConfig cosmosConfig;

        public CosmosSetupService(ILogger<CosmosSetupService> logger, CosmosClient cosmosClient, IOptions<CosmosConfig> cosmosConfig)
        {
            if (cosmosConfig is null)
            {
                throw new ArgumentNullException(nameof(cosmosConfig));
            }

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));

            this.cosmosConfig = cosmosConfig.Value;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Cosmos setup started");

            var dbResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosConfig.DatabaseId)
                                               .ConfigureAwait(false);

            logger.LogInformation("Cosmos DB {0} created with cost : {1}",cosmosConfig.DatabaseId, dbResponse.RequestCharge);
            
            var containerResponse = await dbResponse.Database
                                                    .CreateContainerIfNotExistsAsync(cosmosConfig.IngredientContainerId, @"/name")
                                                    .ConfigureAwait(false);
            
            logger.LogInformation("Cosmos container {0} created with cost : {1}",cosmosConfig.IngredientContainerId, containerResponse.RequestCharge);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

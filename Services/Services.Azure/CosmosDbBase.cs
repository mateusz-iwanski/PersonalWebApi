using Microsoft.Azure.Cosmos;
using PersonalWebApi.Models.Azure;

namespace PersonalWebApi.Services.Azure
{
    public class CosmosDbBase
    {
        protected readonly CosmosClient _cosmosClient;
        protected readonly Container _container;

        public CosmosDbBase(string connectionString, string databaseName, string containerName, string partitionKeyPath)
        {
            _cosmosClient = new CosmosClient(connectionString);
            _container = _cosmosClient.GetContainer(databaseName, containerName);
            CreateDatabaseAndContainerIfNotExistsAsync(databaseName, containerName, partitionKeyPath).Wait();
        }

        protected async Task CreateDatabaseAndContainerIfNotExistsAsync(string databaseName, string containerName, string partitionKeyPath)
        {
            // Create database if it doesn't exist
            await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
            // Create container if it doesn't exist with partition key
            var containerProperties = new ContainerProperties
            {
                Id = containerName,
                PartitionKeyPath = $"/{partitionKeyPath}"
            };
            await _cosmosClient.GetDatabase(databaseName).CreateContainerIfNotExistsAsync(containerProperties);
        }

        public async Task<List<SiteContentStoreCosmosDbDto>> GetItemsAsync(string query)
        {
            var queryDefinition = new QueryDefinition(query);
            var iterator = _container.GetItemQueryIterator<SiteContentStoreCosmosDbDto>(queryDefinition);
            var results = new List<SiteContentStoreCosmosDbDto>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

    }
}

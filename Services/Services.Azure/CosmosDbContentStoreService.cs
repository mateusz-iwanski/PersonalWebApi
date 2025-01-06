using Microsoft.Azure.Cosmos;
using MongoDB.Driver.Core.Configuration;
using Newtonsoft.Json;
using PersonalWebApi.Models;
using PersonalWebApi.Models.Azure;
using PersonalWebApi.Models.Models.Azure;
using PersonalWebApi.Services.Services.History;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PersonalWebApi.Services.Azure
{
    public class CosmosDbContentStoreService : CosmosDbBase, ICosmosDbContentStoreService
    {
        // TODO: add settings to appsettings.AzureService.json
        public CosmosDbContentStoreService(IConfiguration configuration)
            : base(
                  configuration.GetSection("Azure:CosmosDb:Connection").Value,
                  configuration.GetSection("Azure:CosmosDb:CosmosDbDatabaseName").Value
                  )
        {
        }

        public async Task<ItemResponse<T>> CreateItemAsync<T>(T cosmosDto) where T : CosmosDbDtoBase
        {
            await CreateDatabaseAndContainerIfNotExistsAsync(_databaseName, cosmosDto.ContainerName(), cosmosDto.PartitionKey());
            var container = _cosmosClient.GetContainer(_databaseName, cosmosDto.ContainerName());
            return await container.CreateItemAsync(cosmosDto, new PartitionKey(cosmosDto.PartitionKey()));
        }

        public async Task<ItemResponse<T>> GetItemAsync<T>(string id, string containerName, string uri) where T : CosmosDbDtoBase
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("Id, partition key or container name cannot be null or empty.");
            }
            var container = _cosmosClient.GetContainer(_databaseName, containerName);
            return await container.ReadItemAsync<T>(id, new PartitionKey(uri));
        }

        /// <summary>
        /// Get items by query from a container by query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        /// <remarks>
        /// 
        /// Example query:
        /// 
        ///     var query = $"SELECT * FROM c WHERE c.uuid = @uuid";
        ///     QueryDefinition queryDefinition = new QueryDefinition(query).WithParameter("@uuid", uuid);
        /// 
        /// </remarks>
        public async Task<T?> GetByQueryAsync<T>(QueryDefinition query, string containerName) where T : CosmosDbDtoBase
        {
            var container = _cosmosClient.GetContainer(_databaseName, containerName);
            var queryResultSetIterator = container.GetItemQueryIterator<T>(query);

            if (queryResultSetIterator.HasMoreResults)
            {
                var response = await queryResultSetIterator.ReadNextAsync();
                return response.FirstOrDefault(); // Return the first matching document or null if none found
            }

            return default(T); // No results found
        }

        /// <summary>
        /// Updates an item in the Cosmos DB container.
        /// </summary>
        /// <param name="id">The ID of the item to update.</param>
        /// <param name="item">The item to update.</param>
        /// <returns>The updated item response.</returns>
        /// <exception cref="ArgumentException">Thrown when the item or partition key (uri) is null or empty.</exception>
        public async Task<ItemResponse<T>> UpdateItemAsync<T>(T item) where T : CosmosDbDtoBase
        {
            var container = _cosmosClient.GetContainer(_databaseName,  item.ContainerName());
            return await container.ReplaceItemAsync(item, item.Id.ToString(), new PartitionKey(item.PartitionKey()));
        }

        /// <summary>
        /// Updates an item in the Cosmos DB container.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ItemResponse<T>> DeleteItemAsync<T>(T item) where T : CosmosDbDtoBase
        {
            var container = _cosmosClient.GetContainer(_databaseName, item.ContainerName());
            return await container.DeleteItemAsync<T>(item.Id.ToString(), new PartitionKey(item.PartitionKey()));
        }

    }
}

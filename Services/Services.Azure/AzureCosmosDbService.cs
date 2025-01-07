﻿using Microsoft.Azure.Cosmos;
using MongoDB.Driver.Core.Configuration;
using Newtonsoft.Json;
using PersonalWebApi.Models;
using PersonalWebApi.Models.Azure;
using PersonalWebApi.Models.Models.Azure;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PersonalWebApi.Services.Azure
{
    public class AzureCosmosDbService : CosmosDbBase, ICosmosDbService
    {
        public AzureCosmosDbService(IConfiguration configuration)
            : base(
                  configuration.GetSection("Azure:CosmosDb:Connection").Value,
                  configuration.GetSection("Azure:CosmosDb:CosmosDbDatabaseName").Value,
                  int.Parse(configuration.GetSection("Azure:CosmosDb:Throughput").Value)
                  )
        {
        }

        public async Task<ItemResponse<T>> CreateItemAsync<T>(T cosmosDto) where T : CosmosDbDtoBase
        {
            await CreateDatabaseAndContainerIfNotExistsAsync(_databaseName, cosmosDto.ContainerName(), cosmosDto.PartitionKeyName());
            var container = _cosmosClient.GetContainer(_databaseName, cosmosDto.ContainerName());
            var partitionKey = new PartitionKey(cosmosDto.PartitionKeyName());
            return await container.CreateItemAsync(cosmosDto, new PartitionKey(cosmosDto.PartitionKeyData()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">CosmosDbDtoBase</typeparam>
        /// <param name="id">The Cosmos item id</param>
        /// <param name="containerName">The name of the caontainer</param>
        /// <param name="partitionKeyData">The partition key for the item.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ItemResponse<T>> GetItemAsync<T>(string id, string containerName, string partitionKeyData) where T : CosmosDbDtoBase
        {
            var container = _cosmosClient.GetContainer(_databaseName, containerName);
            return await container.ReadItemAsync<T>(id, new PartitionKey(partitionKeyData));
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
            return await container.ReplaceItemAsync(item, item.Id.ToString());
        }

        /// <summary>
        /// Updates an item in the Cosmos DB container.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ItemResponse<T>> DeleteItemAsync<T>(string itemId, string containerName, string partitionKeyData) where T : CosmosDbDtoBase
        {
            var container = _cosmosClient.GetContainer(_databaseName, containerName);
            return await container.DeleteItemAsync<T>(itemId, new PartitionKey(partitionKeyData));
        }

    }
}

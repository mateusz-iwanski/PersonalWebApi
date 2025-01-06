﻿using Microsoft.Azure.Cosmos;
using PersonalWebApi.Models.Azure;
using PersonalWebApi.Models.Models.Azure;
using System.Net;

namespace PersonalWebApi.Services.Azure
{
    public class CosmosDbBase
    {
        protected readonly CosmosClient _cosmosClient;
        protected readonly string _databaseName;
        //protected readonly Container _container;

        public CosmosDbBase(string connectionString, string databaseName)
        {
            _cosmosClient = new CosmosClient(connectionString);
            _databaseName = databaseName;
            
        }

        public async Task CreateDatabaseAndContainerIfNotExistsAsync(string databaseName, string containerName, string partitionKeyPath)
        {
            // Create database if it doesn't exist
            var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);

            // Ensure at least 1 second delay if the database was created
            // Sometimes an error appears: the database is not ready and we want to insert a new container into it
            if (databaseResponse.StatusCode == HttpStatusCode.Created)
                await Task.Delay(1000); 

            // Create container if it doesn't exist with partition key
            var containerProperties = new ContainerProperties
            {
                Id = containerName,
                PartitionKeyPath = $"/{partitionKeyPath}"
            };
            await _cosmosClient.GetDatabase(databaseName).CreateContainerIfNotExistsAsync(containerProperties);
        }

        public async Task<List<T>> GetItemsAsync<T>(string containerName, string query) where T : CosmosDbDtoBase
        {
            var queryDefinition = new QueryDefinition(query);

            var container = _cosmosClient.GetContainer(_databaseName, containerName);
            var iterator = container.GetItemQueryIterator<T>(queryDefinition);

            var results = new List<T>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

        public async Task<List<string>> ListAllDatabasesAsync()
        {
            var databases = new List<string>();
            using (var iterator = _cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>())
            {
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    databases.AddRange(response.Select(db => db.Id));
                }
            }
            return databases;
        }

        public async Task<List<string>> ListAllContainersAsync(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentException("Database name cannot be null or empty.");
            }

            var containers = new List<string>();
            var database = _cosmosClient.GetDatabase(databaseName);
            using (var iterator = database.GetContainerQueryIterator<ContainerProperties>())
            {
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    containers.AddRange(response.Select(container => container.Id));
                }
            }
            return containers;
        }


    }
}

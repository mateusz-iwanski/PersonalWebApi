using Microsoft.Azure.Cosmos;
using PersonalWebApi.Models.Azure;
using PersonalWebApi.Models.Models.Azure;

namespace PersonalWebApi.Services.NoSQLDB
{
    public interface INoSqlDbService
    {
        Task<bool> DatabaseExistsAsync(string databaseName);
        Task<bool> ContainerExistsAsync(string databaseName, string containerName);
        Task<ItemResponse<T>> CreateItemAsync<T>(T cosmosDto) where T : CosmosDbDtoBase;
        Task<ItemResponse<T>> GetItemAsync<T>(string id, string containerName, string uri) where T : CosmosDbDtoBase;
        Task<List<T>> GetByQueryAsync<T>(QueryDefinition query, string containerName) where T : CosmosDbDtoBase;
        Task<ItemResponse<T>> UpdateItemAsync<T>(T item) where T : CosmosDbDtoBase;
        Task<ItemResponse<T>> DeleteItemAsync<T>(string itemId, string containerName, string partitionKeyData) where T : CosmosDbDtoBase;
        Task CreateDatabaseAndContainerIfNotExistsAsync(string databaseName, string containerName, string partitionKeyPath);
    }
}
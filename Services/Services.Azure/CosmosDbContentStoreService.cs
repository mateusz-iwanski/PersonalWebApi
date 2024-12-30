using Microsoft.Azure.Cosmos;
using PersonalWebApi.Models;
using PersonalWebApi.Models.Azure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalWebApi.Services.Azure
{
    public class CosmosDbContentStoreService : CosmosDbBase, ICosmosDbContentStoreService
    {
        // TODO: add settings to appsettings.AzureService.json
        public CosmosDbContentStoreService(IConfiguration configuration)
            : base(configuration.GetConnectionString("PersonalApiDbCosmos"), "PersonalApi", "Items", "uri")
        {
        }

        public async Task<ItemResponse<SiteContentStoreCosmosDbDto>> CreateItemAsync(SiteContentStoreCosmosDbDto item)
        {
            if (item == null || string.IsNullOrEmpty(item.Uri))
            {
                throw new ArgumentException("Item or partition key (Uri) cannot be null or empty.");
            }
            return await _container.CreateItemAsync(item, new PartitionKey(item.Uri));
        }

        public async Task<ItemResponse<SiteContentStoreCosmosDbDto>> GetItemAsync(string id, string uri)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("Id and partition key (Uri) cannot be null or empty.");
            }
            return await _container.ReadItemAsync<SiteContentStoreCosmosDbDto>(id, new PartitionKey(uri));
        }

        public async Task<SiteContentStoreCosmosDbDto> GetByUuidAsync(string uuid)
        {
            var query = $"SELECT * FROM c WHERE c.uuid = @uuid";
            var queryDefinition = new QueryDefinition(query).WithParameter("@uuid", uuid);

            var queryResultSetIterator = _container.GetItemQueryIterator<SiteContentStoreCosmosDbDto>(queryDefinition);

            if (queryResultSetIterator.HasMoreResults)
            {
                var response = await queryResultSetIterator.ReadNextAsync();
                return response.FirstOrDefault(); // Return the first matching document or null if none found
            }

            return null; // No results found
        }

        public async Task<ItemResponse<SiteContentStoreCosmosDbDto>> GetItemAsync(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                throw new ArgumentException("Id cannot be null or empty.");
            }
            return await _container.ReadItemAsync<SiteContentStoreCosmosDbDto>(uuid, new PartitionKey(uuid));
        }

        public async Task<ItemResponse<SiteContentStoreCosmosDbDto>> UpdateItemAsync(string id, SiteContentStoreCosmosDbDto item)
        {
            if (item == null || string.IsNullOrEmpty(item.Uri))
            {
                throw new ArgumentException("Item or partition key (Uri) cannot be null or empty.");
            }
            return await _container.ReplaceItemAsync(item, id, new PartitionKey(item.Uri));
        }

        public async Task<ItemResponse<SiteContentStoreCosmosDbDto>> DeleteItemAsync(string id, string uri)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("Id and partition key (Uri) cannot be null or empty.");
            }
            return await _container.DeleteItemAsync<SiteContentStoreCosmosDbDto>(id, new PartitionKey(uri));
        }
    }
}

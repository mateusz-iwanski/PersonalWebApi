using Microsoft.Azure.Cosmos;
using PersonalWebApi.Models.Azure;

namespace PersonalWebApi.Services.Azure
{
    public interface ICosmosDbContentStoreService
    {
        Task<ItemResponse<SiteContentStoreCosmosDbDto>> CreateItemAsync(SiteContentStoreCosmosDbDto item);
        Task<ItemResponse<object>> CreateItemAsync(object item);
        Task<ItemResponse<SiteContentStoreCosmosDbDto>> DeleteItemAsync(string id, string uri);
        Task<ItemResponse<SiteContentStoreCosmosDbDto>> GetItemAsync(string id, string uri);
        Task<ItemResponse<SiteContentStoreCosmosDbDto>> UpdateItemAsync(string id, SiteContentStoreCosmosDbDto item);
        Task<List<SiteContentStoreCosmosDbDto>> GetItemsAsync(string query);
        Task<SiteContentStoreCosmosDbDto> GetByUuidAsync(string uuid);
    }
}
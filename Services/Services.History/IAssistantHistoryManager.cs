using Microsoft.KernelMemory;
using PersonalWebApi.Models.Models.Azure;

namespace PersonalWebApi.Services.Services.History
{
    public interface IAssistantHistoryManager
    {
        Task<List<T>> LoadAsync<T>(Guid conversationUuid) where T : CosmosDbDtoBase;
        Task<T> SaveAsync<T>(T historyDto) where T : CosmosDbDtoBase;
    }
}
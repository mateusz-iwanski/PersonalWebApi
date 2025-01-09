using Microsoft.KernelMemory;
using PersonalWebApi.Models.Models.Agent;
using PersonalWebApi.Models.Models.Azure;

namespace PersonalWebApi.Agent
{
    public interface IAssistantHistoryManager
    {
        Task LoadAsync(Guid conversationUuid, IKernelMemory memory);
        Task<T> SaveAsync<T>(T historyDto) where T : CosmosDbDtoBase;
    }
}
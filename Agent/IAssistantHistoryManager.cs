using Microsoft.KernelMemory;
using PersonalWebApi.Models.Models.Azure;

namespace PersonalWebApi.Agent
{
    public interface IAssistantHistoryManager
    {
        Task LoadAsync(Guid conversationUuid, IKernelMemory memory);
        Task<ChatHistoryStoreDbDto> SaveAsync(Guid sessionUuid, Guid conversationUuid, string message);
    }
}
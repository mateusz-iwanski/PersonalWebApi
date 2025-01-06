using Microsoft.AspNetCore.Mvc;
using PersonalWebApi.Services.Services.History;

namespace PersonalWebApi.Controllers.Controllers.ChatRepository
{
    [ApiController]
    [Route("api/chat/repository")]
    public class ChatRepository
    {
        //private readonly IChatHistoryRepository _chatHistoryRepository;
        //public ChatRepository(IChatHistoryRepository chatHistoryRepository)
        //{
        //    _chatHistoryRepository = chatHistoryRepository;
        //}
        //[HttpPost("firstquetion/{conversationUuid:guid}")]
        //public async Task<string> ChatFirst(Guid conversationUuid)
        //{
        //    await _chatHistoryRepository.CollectChat(conversationUuid);
        //    return "Chat collected";
        //}

        //[HttpPost("secondquetion/{conversationUuid:guid}")]
        //public async Task<string> ChatSecond(Guid conversationUuid)
        //{
        //    await _chatHistoryRepository.GetChat(conversationUuid);
        //    return "Chat collected";    
        //}
    }
}

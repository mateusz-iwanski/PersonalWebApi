namespace PersonalWebApi.Models.Models.Memory
{
    public class ChatHistoryShortTermDeleteIndexDto : ChatHistoryShortTermMessageDto
    {
        public ChatHistoryShortTermDeleteIndexDto(Guid conversationUuid, Guid sessionUuid)
            : base(conversationUuid, sessionUuid)
        {
        }
    }
}

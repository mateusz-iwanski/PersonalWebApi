using Newtonsoft.Json;

namespace PersonalWebApi.Models.Models.Memory
{
    public class ChatHistoryShortTermDeleteDocumentDto : ChatHistoryShortTermMessageDto
    {
        [JsonProperty(PropertyName = "fileId")]
        public string FileId { get; set; }

        public ChatHistoryShortTermDeleteDocumentDto(Guid conversationUuid, Guid sessionUuid)
            : base(conversationUuid, sessionUuid)
        {
        }
    }
}

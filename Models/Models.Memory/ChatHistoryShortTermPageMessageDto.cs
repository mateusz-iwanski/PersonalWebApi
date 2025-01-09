using Newtonsoft.Json;

namespace PersonalWebApi.Models.Models.Memory
{
    public class ChatHistoryShortTermPageMessageDto : ChatHistoryShortTermMessageDto
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; } = string.Empty;

        public ChatHistoryShortTermPageMessageDto(Guid conversationUuid, Guid sessionUuid) 
            : base(conversationUuid, sessionUuid)
        {
        }

    }
}

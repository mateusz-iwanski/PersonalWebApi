using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace PersonalWebApi.Services.Services.History
{
    public class DocumentHistory
    {
        public Guid Id { get; set; }
        public Guid ConversationUuid { get; set; }
        public Guid SourceDocuemtUuid { get; set; }
        public string Text { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ConversationHistory
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class MessageHistory
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public string ConversationUuid { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ActionHistory
    {
        public Guid Id { get; set; }
        public Guid ActionUuid { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(JsonStringEnumConverter))]
        public ChatHistoryToolType ToolType { get; set; }
        public string Parameters { get; set; }
        public string Instruction { get; set; }
        public string Description { get; set; }
        public int Sequence { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ActionDocument
    {
        public Guid Id { get; set; }
        public Guid ConversationUuid { get; set; }
        public Guid ActionUuid { get; set; }
        public Guid DocumentHistoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class MessageDocument
    {
        public int Id { get; set; }
        public string MessageUuid { get; set; }
        public string DocumentUuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }




}

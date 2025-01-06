using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Models.Models.Azure
{
    public abstract class CosmosDbDtoBase
    {
        [Required]
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; } = Guid.NewGuid(); // Unique identifier. It can be GUID or uuid session from ai agent

        [Required]
        [JsonProperty(PropertyName = "conversationUuid")]
        public string ConversationUuid { get; set; } = string.Empty;

        abstract public string ContainerName();
        abstract public string PartitionKey(); 

    }
}

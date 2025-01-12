using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Models.Agent
{
    public class AgentBaseDto
    {
        [Required]
        [JsonProperty("sessionId")]
        public Guid SessionId { get; set; }

    }
}

using Newtonsoft.Json;
using PersonalWebApi.Models.Models.Azure;
using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Models.Azure
{
    /// <summary>
    /// Storing data as text from for example www sites
    /// </summary>
    public class SiteContentStoreCosmosDbDto : CosmosDbDtoBase
    {
        [Required]
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; } = Guid.NewGuid(); // Unique identifier. It can be GUID or uuid session from ai agent

        [Required]
        [JsonProperty(PropertyName = "uuid")]
        public string Uuid { get; set; } = string.Empty; // User Unique identifier from ai agent

        [Required]
        [JsonProperty(PropertyName = "domain")]
        public string Domain { get; set; } = string.Empty; // Partition key

        [Required]
        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; } = string.Empty; // URI of the content

        [Required]
        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; } = string.Empty; // Text data from the site

        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { get; set; } // List of tags

        [JsonProperty(PropertyName = "createdAt")]
        public DateTime CreatedAt{ get; set; } // Date and time of the creation

        public override string ContainerName() => "files";
        public override string PartitionKey() => "uri";
    }
}

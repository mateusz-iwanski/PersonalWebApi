using Newtonsoft.Json;
using PersonalWebApi.Models.Models.Azure;
using PersonalWebApi.Processes.Qdrant.Models;
using PersonalWebApi.Services.Agent;
using PersonalWebApi.Services.NoSQLDB;

namespace PersonalWebApi.Processes.Document.Models
{
    /// <summary>
    /// This class represents a document step data transfer object.
    /// It is used to transfer document step data between processes.
    /// </summary>
    public class DocumentStepDto : CosmosDbDtoBase
    {
        [JsonProperty("fileId")]
        public Guid FileId { get; set; }

        [JsonIgnore]
        public string Content { get; set; } = string.Empty;

        [JsonProperty("summary")]
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// This field is using on input
        /// </summary>
        [JsonIgnore]
        public IFormFile? iFormFile { get; set; } = default;

        /// <summary>
        /// This field is using on input
        /// TODO: Make - Create Policy about overwriting file with the same name
        /// </summary>
        [JsonProperty("overwrite")]
        public bool Overwrite { get; set; } = false;

        [JsonProperty("uri")]
        public Uri Uri { get; set; }

        [JsonIgnore]
        public List<DocumentChunkerDto> ChunkerCollection { get; set; } = new List<DocumentChunkerDto>();

        [JsonProperty("events")]
        public List<string> Events { get; set; } = new List<string>();  // for example uploaded on external server, etc.

        public DocumentStepDto(Guid fileId, IFormFile iFormFile, Guid conversationUuid, Guid sessionUuid)
            : base(conversationUuid, sessionUuid)
        {
            FileId = fileId;
            this.iFormFile = iFormFile;
        }

        /// <summary>
        /// Gets the static container name for the Cosmos DB.
        /// This method returns the name of the container where chat message content records are stored.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public static string ContainerNameStatic() => "document-action";

        /// <summary>
        /// Gets the static partition key name for the Cosmos DB.
        /// This method returns the name of the partition key used to partition chat message content records.
        /// </summary>
        /// <returns>The name of the partition key.</returns>
        public static string PartitionKeyNameStatic() => "/conversationUuid";

        /// <summary>
        /// Gets the container name for the Cosmos DB.
        /// This method overrides the base class method to return the specific container name for chat message content.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public override string ContainerName() => ContainerNameStatic();

        /// <summary>
        /// Gets the partition key name for the Cosmos DB.
        /// This method overrides the base class method to return the specific partition key name for chat message content.
        /// </summary>
        /// <returns>The name of the partition key.</returns>
        public override string PartitionKeyName() => PartitionKeyNameStatic();

        /// <summary>
        /// Gets the partition key data for the Cosmos DB.
        /// This method returns the value of the partition key, which is the conversation UUID.
        /// </summary>
        /// <returns>The partition key data.</returns>
        public override string PartitionKeyData() => ConversationUuid.ToString();
    }
}


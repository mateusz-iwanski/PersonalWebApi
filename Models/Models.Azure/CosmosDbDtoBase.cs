using Amazon.Auth.AccessControlPolicy;
using Elastic.Clients.Elasticsearch;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PersonalWebApi.Models.Models.Azure
{
    /// <summary>
    /// Base DTO record for saving logs to Cosmos DB.
    /// </summary>
    public abstract class CosmosDbDtoBase
    {
        /// <summary>
        /// The user who created the record.
        /// </summary>
        private string _createdBy { get; set; } = ClaimTypes.Anonymous;

        /// <summary>
        /// Unique identifier for the record.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Unique identifier for the conversation.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "conversationUuid")]
        public Guid ConversationUuid { get; set; }

        /// <summary>
        /// Unique identifier for the session.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "sessionUuid")]
        public Guid SessionUuid { get; set; }

        /// <summary>
        /// The user who created the record.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "createdBy")]
        public string CreatedBy { get => _createdBy; set => _createdBy = value; }

        /// <summary>
        /// List of tags.
        /// </summary>
        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Metadata.
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Date of creation.
        /// </summary>
        [JsonProperty(PropertyName = "createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets the name of the Cosmos DB container.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public abstract string ContainerName();

        /// <summary>
        /// Return the partition key data.
        /// It must be the value of class field that return PartitionKeyName().
        /// For example:
        ///     public string uri;
        ///     PartitionKeyName() => "uri"; --> !!!
        ///     PartitionKeyData() => uri;  
        /// </summary>
        /// <returns>Partition key data</returns>
        public abstract string PartitionKeyName();

        /// <summary>
        /// Return the partition key data.
        /// It must be the value of class field that return PartitionKeyName().
        /// For example:
        ///     public string uri;
        ///     PartitionKeyName() => "uri";
        ///     PartitionKeyData() => uri;  --> !!!!
        /// </summary>
        /// <returns>Partition key data</returns>
        public abstract string PartitionKeyData();

        public virtual void SetUser(ClaimsPrincipal userClaimPrincipal) => 
            _createdBy = userClaimPrincipal?.FindFirstValue(ClaimTypes.Name) ?? ClaimTypes.Anonymous;


        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbDtoBase"/> class.
        /// </summary>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <param name="sessionUuid">The unique identifier for the session.</param>
        public CosmosDbDtoBase(
            Guid conversationUuid,
            Guid sessionUuid
        )
        {
            ConversationUuid = conversationUuid;
            SessionUuid = sessionUuid;
        }
    }
}


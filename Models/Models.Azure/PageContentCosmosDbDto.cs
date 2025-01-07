using Newtonsoft.Json;
using PersonalWebApi.Models.Models.Azure;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PersonalWebApi.Models.Azure
{
    /// <summary>
    /// Represents a DTO for storing site content data in Cosmos DB.
    /// </summary>
    public class PageContentCosmosDbDto : CosmosDbDtoBase
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user from the AI agent.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "uuid")]
        public string Uuid { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the domain name, which is used as the partition key.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "domain")]
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URI of the content.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the content from the site.
        /// </summary>
        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file URI if the data is downloaded and stored in a file.
        /// </summary>
        [JsonProperty(PropertyName = "fileUri")]
        public string FileUri { get; set; } = string.Empty;

        /// <summary>
        /// Gets the static container name for the Cosmos DB.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public static string ContainerNameStatic() => "www-content";

        /// <summary>
        /// Gets the static partition key name for the Cosmos DB.
        /// </summary>
        /// <returns>The name of the partition key.</returns>
        public static string PartitionKeyNameStatic() => "uri";

        /// <summary>
        /// Gets the container name for the Cosmos DB.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public override string ContainerName() => ContainerNameStatic();

        /// <summary>
        /// Gets the partition key name for the Cosmos DB.
        /// </summary>
        /// <returns>The name of the partition key.</returns>
        public override string PartitionKeyName() => PartitionKeyNameStatic();

        /// <summary>
        /// Gets the partition key data for the Cosmos DB.
        /// </summary>
        /// <returns>The partition key data.</returns>
        public override string PartitionKeyData() => Uri;

        /// <summary>
        /// Sets the user information from the claims principal.
        /// </summary>
        /// <param name="userClaimPrincipal">The claims principal containing user information.</param>
        public override void SetUser(ClaimsPrincipal userClaimPrincipal) =>
            base.SetUser(userClaimPrincipal);

        /// <summary>
        /// Initializes a new instance of the <see cref="PageContentCosmosDbDto"/> class.
        /// </summary>
        /// <param name="uuid">The unique identifier for the user from the AI agent.</param>
        /// <param name="domain">The domain name.</param>
        /// <param name="uri">The URI of the content.</param>
        /// <param name="content">The content from the site.</param>
        /// <param name="tags">The list of tags.</param>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <param name="sessionUuid">The unique identifier for the session.</param>
        [JsonConstructor]
        public PageContentCosmosDbDto(
            string uuid,
            string domain,
            string uri,
            string content,
            List<string> tags,
            Guid conversationUuid,
            Guid sessionUuid
        ) : base(conversationUuid, sessionUuid)
        {
            Uuid = uuid;
            Domain = domain;
            Uri = uri;
            Content = content;
            Tags = tags;
        }
    }
}

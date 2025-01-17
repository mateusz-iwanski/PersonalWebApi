using Amazon.S3.Model;
using Newtonsoft.Json;
using PersonalWebApi.Models.Models.Azure;
using System.Security.Claims;

namespace PersonalWebApi.Models.Storage
{
    /// <summary>
    /// Log of storage events.
    /// </summary>
    public class StorageEventsDto : CosmosDbDtoBase
    {

        [JsonProperty(PropertyName = "eventName")]
        public string EventName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "serviceName")]
        public string ServiceName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "fileUri")]
        public string FileUri { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "fileId")]
        public string FileId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "isSuccess")]
        public bool IsSuccess { get; set; } = true;

        [JsonProperty(PropertyName = "actionType")]
        public string ActionType { get; set; } = string.Empty; // Upload, Download, or Read

        [JsonProperty(PropertyName = "fileSize")]
        public long FileSize { get; set; }

        [JsonProperty(PropertyName = "fileType")]
        public string FileType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        public StorageEventsDto(Guid conversationUuid, Guid sessionUuid, Guid fileId)
            : base(conversationUuid, sessionUuid)
        {
            FileId = fileId.ToString();
        }

        /// <summary>
        /// Gets the static container name for the Cosmos DB.
        /// This method returns the name of the container where chat message content records are stored.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public static string ContainerNameStatic() => "event-storage-action";

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

        /// <summary>
        /// Sets the user information from the claims principal.
        /// This method extracts user information from the provided claims principal and sets it in the base class.
        /// </summary>
        /// <param name="userClaimPrincipal">The claims principal containing user information.</param>
        public override void SetUser(ClaimsPrincipal userClaimPrincipal) =>
            base.SetUser(userClaimPrincipal);
    }
}

using Newtonsoft.Json;
using PersonalWebApi.Models.Models.Azure;
using System.Security.Claims;

namespace PersonalWebApi.Models.Models.SemanticKernel
{
    /// <summary>
    /// Represents a Data Transfer Object (DTO) for storing persistance chat message content in a Cosmos DB.
    /// This class extends the <see cref="CosmosDbDtoBase"/> to include specific properties
    /// related to chat messages, such as the message content, role of the sender, and any
    /// associated metadata. It provides methods to retrieve container and partition
    /// key names, ensuring the data is correctly partitioned and stored in the Cosmos DB.
    /// </summary>
    public class ChatMessagePersistenceDto : CosmosDbDtoBase
    {
        /// <summary>
        /// Gets or sets the chat message content.
        /// This property holds the actual text of the chat message.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role of the chat participant.
        /// This property indicates whether the message was sent by a user, bot, or system.
        /// </summary>
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the encoding of the message content.
        /// This property holds the encoding information of the message content.
        /// </summary>
        [JsonProperty(PropertyName = "encoding")]
        public string Encoding { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the metadata associated with the message.
        /// This property holds any additional metadata related to the message.
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, object?> Metadata { get; set; } = new Dictionary<string, object?>();

        public string AuthorName { get; set; } = string.Empty;


        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessagePersistenceDto"/> class.
        /// </summary>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <param name="sessionUuid">The unique identifier for the session.</param>
        public ChatMessagePersistenceDto(Guid conversationUuid, Guid sessionUuid)
            : base(conversationUuid, sessionUuid)
        {
        }

        /// <summary>
        /// Gets the static container name for the Cosmos DB.
        /// This method returns the name of the container where chat message content records are stored.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public static string ContainerNameStatic() => "history-chat-persisistence-messages";

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

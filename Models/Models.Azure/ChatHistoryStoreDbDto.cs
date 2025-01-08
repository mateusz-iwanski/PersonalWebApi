using LLama.Common;
using Microsoft.Extensions.AI;
using PersonalWebApi.Entities.System;
using System.Security.Claims;

namespace PersonalWebApi.Models.Models.Azure
{
    /// <summary>
    /// Represents a Data Transfer Object (DTO) for storing chat history in a Cosmos DB.
    /// This class extends the <see cref="CosmosDbDtoBase"/> to include specific properties
    /// related to chat messages, such as the message content, role of the sender, and any
    /// associated actions or metadata. It provides methods to retrieve container and partition
    /// key names, ensuring the data is correctly partitioned and stored in the Cosmos DB.
    /// </summary>
    public class ChatHistoryStoreDbDto : CosmosDbDtoBase
    {
        /// <summary>
        /// Gets or sets the chat message content.
        /// This property holds the actual text of the chat message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role of the chat participant.
        /// This property indicates whether the message was sent by a user, bot, or system.
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action associated with the message.
        /// This property can store the name of a plugin, download site, or any other action
        /// related to the message.
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action message.
        /// This property contains additional data related to the action, such as plugin data.
        /// </summary>
        public string ActionMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the message.
        /// This property indicates the type of the message, such as text, image, etc.
        /// </summary>
        public string MessageType { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHistoryStoreDbDto"/> class.
        /// </summary>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <param name="sessionUuid">The unique identifier for the session.</param>
        public ChatHistoryStoreDbDto(Guid conversationUuid, Guid sessionUuid)
            : base(conversationUuid, sessionUuid)
        {
        }

        /// <summary>
        /// Gets the static container name for the Cosmos DB.
        /// This method returns the name of the container where chat history records are stored.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public static string ContainerNameStatic() => "chat-history";

        /// <summary>
        /// Gets the static partition key name for the Cosmos DB.
        /// This method returns the name of the partition key used to partition chat history records.
        /// </summary>
        /// <returns>The name of the partition key.</returns>
        public static string PartitionKeyNameStatic() => "conversationUuid";

        /// <summary>
        /// Gets the container name for the Cosmos DB.
        /// This method overrides the base class method to return the specific container name for chat history.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public override string ContainerName() => ContainerNameStatic();

        /// <summary>
        /// Gets the partition key name for the Cosmos DB.
        /// This method overrides the base class method to return the specific partition key name for chat history.
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

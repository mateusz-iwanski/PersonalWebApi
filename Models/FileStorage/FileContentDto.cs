using Newtonsoft.Json;
using PersonalWebApi.Models.Models.Azure;
using System.Security.Claims;

namespace PersonalWebApi.Models.FileStorage
{
    /// <summary>
    /// Represents all crucial data for file storage operations.
    /// This DTO stores the main information about the content being uploaded to the system.
    /// </summary>
    /// <example>
    /// Example of a FileContentDto:
    /// {
    ///   "id": "b1f1e3b2-3d7f-488b-97f3-2a4c5f6d8a7c",
    ///   "fileName": "Company_Policy_2025.pdf",
    ///   "contentType": "application/pdf",
    ///   "fileSize": 204800,
    ///   "storagePath": "/data/files/Company_Policy_2025.pdf",
    ///   "checksum": "a6b4c3d2e1f0987654321abcd1234ef5",
    ///   "uploadedAt": "2025-01-15T12:00:00Z",
    ///   "uploadedBy": "admin",
    ///   "isProcessed": true,
    ///   "lastProcessedAt": "2025-01-16T08:30:00Z",
    ///   "description": "Company policy document for 2025.",
    ///   "PageCount": "50",
    ///   "Category": "HR"
    ///   "tags": ["Policy", "HR", "2025"]
    ///   "metadata" : [""...]
    ///   ...
    /// }
    /// </example>
    public class FileContentDto : CosmosDbDtoBase
    {
        [JsonProperty(PropertyName = "fileId")]
        public Guid FileId { get; set; }

        /// <summary>
        /// Original name of the file.
        /// </summary>
        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// MIME type of the file (e.g., text/plain, application/pdf).
        /// </summary>
        [JsonProperty(PropertyName = "contentType")]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        [JsonProperty(PropertyName = "fileSize")]
        public long FileSize { get; set; }

        /// <summary>
        /// Full path or URL where the file is stored.
        /// </summary>
        [JsonProperty(PropertyName = "storagePath")]
        public string StoragePath { get; set; } = string.Empty;

        /// <summary>
        /// File checksum for integrity verification (e.g., MD5, SHA256).
        /// </summary>
        [JsonProperty(PropertyName = "checksum")]
        public string Checksum { get; set; } = string.Empty;

        /// <summary>
        /// User or system that uploaded the file.
        /// </summary>
        [JsonProperty(PropertyName = "uploadedBy")]
        public string UploadedBy { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "pageCount")]
        public int PageCount { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of the last processing.
        /// </summary>
        [JsonProperty(PropertyName = "lastProcessedAt")]
        public DateTime? LastProcessedAt { get; set; }

        /// <summary>
        /// Optional description of the file’s purpose.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "userDescription")]
        public string UserDescription { get; set; } = string.Empty;

        public FileContentDto(Guid conversationUuid, Guid sessionUuid, Guid fileId)
            : base(conversationUuid, sessionUuid)
        {
            FileId = fileId;
        }

        /// <summary>
        /// Gets the static container name for the Cosmos DB.
        /// This method returns the name of the container where chat message content records are stored.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public static string ContainerNameStatic() => "file-content-metadata";

        /// <summary>
        /// Gets the static partition key name for the Cosmos DB.
        /// This method returns the name of the partition key used to partition chat message content records.
        /// </summary>
        /// <returns>The name of the partition key.</returns>
        public static string PartitionKeyNameStatic() => "/fileId";
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
        public override string PartitionKeyData() => FileId.ToString();

        /// <summary>
        /// Sets the user information from the claims principal.
        /// This method extracts user information from the provided claims principal and sets it in the base class.
        /// </summary>
        /// <param name="userClaimPrincipal">The claims principal containing user information.</param>
        public override void SetUser(ClaimsPrincipal userClaimPrincipal) =>
            base.SetUser(userClaimPrincipal);
    }
}


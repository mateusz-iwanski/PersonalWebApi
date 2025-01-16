namespace PersonalWebApi.Models.FileStorage
{
    /// <summary>
    /// Represents all crucial data for file storage operations.
    /// This DTO stores the main information about the content being uploaded to the system.
    /// </summary>
    /// <example>
    /// Example of a ContentMetadataDto:
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
    ///   "metadata": {
    ///     "PageCount": "50",
    ///     "Category": "HR"
    ///   },
    ///   "tags": ["Policy", "HR", "2025"]
    /// }
    /// </example>
    public class ContentMetadataDto
    {
        /// <summary>
        /// Unique identifier for the file record.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Original name of the file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// MIME type of the file (e.g., text/plain, application/pdf).
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Full path or URL where the file is stored.
        /// </summary>
        public string StoragePath { get; set; }

        /// <summary>
        /// File checksum for integrity verification (e.g., MD5, SHA256).
        /// </summary>
        public string Checksum { get; set; }

        /// <summary>
        /// Timestamp when the file was uploaded.
        /// </summary>
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// User or system that uploaded the file.
        /// </summary>
        public string UploadedBy { get; set; }

        /// <summary>
        /// Indicates whether the file has been processed (e.g., embedded, chunked).
        /// </summary>
        public bool IsProcessed { get; set; }

        /// <summary>
        /// Timestamp of the last processing.
        /// </summary>
        public DateTime? LastProcessedAt { get; set; }

        /// <summary>
        /// Optional description of the file’s purpose.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Flexible additional metadata for the file.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// Tags for categorization of the file.
        /// </summary>
        public List<string> Tags { get; set; }
    }
}


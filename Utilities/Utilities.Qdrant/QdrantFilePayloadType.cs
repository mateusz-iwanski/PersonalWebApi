namespace PersonalWebApi.Utilities.Utilities.Qdrant
{
    public class QdrantFilePayloadType
    {
        public string BlobUri { get; set; }
        public string Text { get; set; }
        public string ConversationId { get; set; }
        public string EndPosition { get; set; }
        public string UploadedBy { get; set; }
        public string EmbeddingModel { get; set; }
        public string StartPosition { get; set; }
        public string Author { get; set; }
        public string FileName { get; set; }
        public string CreatedAt { get; set; }
        public string FileId { get; set; }
        public string Summary { get; set; }
        public string Tags { get; set; }
        public string Title { get; set; }
        public string MimeType { get; set; }
        public string Type { get => "File"; }
    }
}

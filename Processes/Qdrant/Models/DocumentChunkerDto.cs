namespace PersonalWebApi.Processes.Qdrant.Models
{
    /// <summary>
    /// Represents a chunk of text with metadata.
    /// </summary>
    /// <param name="conversationId">The ID of the conversation.</param>
    /// <param name="startPosition">The start position of the chunk in the original text.</param>
    /// <param name="endPosition">The end position of the chunk in the original text.</param>
    /// <param name="line">The chunked line of text.</param>
    public class DocumentChunkerDto
    {
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}

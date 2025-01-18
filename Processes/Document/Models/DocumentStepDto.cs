using PersonalWebApi.Processes.Qdrant.Models;
using PersonalWebApi.Services.Agent;

namespace PersonalWebApi.Processes.Document.Models
{
    /// <summary>
    /// This class represents a document step data transfer object.
    /// It is used to transfer document step data between processes.
    /// </summary>
    public class DocumentStepDto
    {
        public Guid ConversationUuid { get; set; }
        public Guid SessionUuid { get; set; }

        public Guid FileId { get; set; }
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// For collecting information about file, content, etc.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// This field is using on input
        /// </summary>
        public IFormFile? iFormFile { get; set; } = default;

        /// <summary>
        /// This field is using on input
        /// TODO: Make - Create Policy about overwriting file with the same name
        /// </summary>
        public bool Overwrite { get; set; } = false;

        public Uri Uri { get; set; } = new Uri("http://example.com");

        public List<DocumentChunkerDto> ChunkerCollection = new List<DocumentChunkerDto>();

        public List<string> Events = new List<string>();  // for example uploaded on external server, etc.

        public DocumentStepDto(Guid fileId, IFormFile iFormFile, Guid conversationUuid, Guid sessionUuid)
        {
            FileId = fileId;
            this.iFormFile = iFormFile;
            ConversationUuid = conversationUuid;
            SessionUuid = sessionUuid;
        }
    }
}

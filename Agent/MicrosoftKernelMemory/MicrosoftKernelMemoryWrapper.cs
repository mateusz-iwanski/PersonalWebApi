using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Context;

namespace PersonalWebApi.Agent.MicrosoftKernelMemory
{
    public class MicrosoftKernelMemoryWrapper : IKernelMemory
    {
        private readonly IKernelMemory _innerKernelMemory;
        private readonly IAssistantHistoryManager _assistantHistoryManager;

        public event EventHandler<DocumentImportedEventArgs>? DocumentImported;

        public MicrosoftKernelMemoryWrapper(IKernelMemory innerKernelMemory, IAssistantHistoryManager assistantHistoryManager)
        {
            _innerKernelMemory = innerKernelMemory;
            _assistantHistoryManager = assistantHistoryManager;
        }

        public async Task ImportDocumentAsync(Document document, string? index = null, IEnumerable<string>? steps = null, IContext? context = null, CancellationToken cancellationToken = default)
        {
            
            await _innerKernelMemory.ImportDocumentAsync(document, index, steps, context, cancellationToken);
        }



        protected virtual void OnDocumentImported(DocumentImportedEventArgs e)
        {
            DocumentImported?.Invoke(this, e);
        }

        // Implement other members of IKernelMemory by delegating to _innerKernelMemory
        // For example:

        public async Task<string> ImportDocumentAsync(string filePath, string? documentId = null, TagCollection? tags = null, string? index = null, IEnumerable<string>? steps = null, IContext? context = null, CancellationToken cancellationToken = default)
        {
            var converationUuid = Guid.Parse(tags["conversationUuid"].FirstOrDefault());
            var sessionUuid = Guid.Parse(tags["sessionUuid"].FirstOrDefault());

            var message = $"""
                <conversationUuid>{converationUuid.ToString()}</conversationUuid>
                <sessionUuid>{sessionUuid.ToString()}</sessionUuid>
                Document imported to memory <sourceFilePath>{filePath}</sourceFilePath>
                """;

            await _assistantHistoryManager.SaveAsync(sessionUuid, converationUuid, message);

            var result = await _innerKernelMemory.ImportDocumentAsync(filePath, documentId, tags, index, steps, context, cancellationToken);
            return result;
        }

        public async Task<string> ImportDocumentAsync(DocumentUploadRequest uploadRequest, IContext? context = null, CancellationToken cancellationToken = default)
        {
            var result = await _innerKernelMemory.ImportDocumentAsync(uploadRequest, context, cancellationToken);
            return result;
        }

        public async Task<string> ImportDocumentAsync(Stream content, string? fileName = null, string? documentId = null, TagCollection? tags = null, string? index = null, IEnumerable<string>? steps = null, IContext? context = null, CancellationToken cancellationToken = default)
        {
            var result = await _innerKernelMemory.ImportDocumentAsync(content, fileName, documentId, tags, index, steps, context, cancellationToken);
            return result;
        }

        public Task<string> ImportTextAsync(string text, string? documentId = null, TagCollection? tags = null, string? index = null, IEnumerable<string>? steps = null, IContext? context = null, CancellationToken cancellationToken = default)
        {
            return _innerKernelMemory.ImportTextAsync(text, documentId, tags, index, steps, context, cancellationToken);
        }

        public Task<string> ImportWebPageAsync(string url, string? documentId = null, TagCollection? tags = null, string? index = null, IEnumerable<string>? steps = null, IContext? context = null, CancellationToken cancellationToken = default)
        {
            return _innerKernelMemory.ImportWebPageAsync(url, documentId, tags, index, steps, context, cancellationToken);
        }

        public Task<IEnumerable<IndexDetails>> ListIndexesAsync(CancellationToken cancellationToken = default)
        {
            return _innerKernelMemory.ListIndexesAsync(cancellationToken);
        }

        public Task DeleteIndexAsync(string? index = null, CancellationToken cancellationToken = default)
        {
            return _innerKernelMemory.DeleteIndexAsync(index, cancellationToken);
        }

        public Task DeleteDocumentAsync(string documentId, string? index = null, CancellationToken cancellationToken = default)
        {
            return _innerKernelMemory.DeleteDocumentAsync(documentId, index, cancellationToken);
        }

        public Task<bool> IsDocumentReadyAsync(string documentId, string? index = null, CancellationToken cancellationToken = default)
        {
            return _innerKernelMemory.IsDocumentReadyAsync(documentId, index, cancellationToken);
        }

        public Task<DataPipelineStatus?> GetDocumentStatusAsync(string documentId, string? index = null, CancellationToken cancellationToken = default)
        {
            return _innerKernelMemory.GetDocumentStatusAsync(documentId, index, cancellationToken);
        }

        public Task<StreamableFileContent> ExportFileAsync(string documentId, string fileName, string? index = null, CancellationToken cancellationToken = default)
        {
            return _innerKernelMemory.ExportFileAsync(documentId, fileName, index, cancellationToken);
        }

        public Task<SearchResult> SearchAsync(string query, string? index = null, MemoryFilter? filter = null, ICollection<MemoryFilter>? filters = null, double minRelevance = 0, int limit = -1, IContext? context = null, CancellationToken cancellationToken = default)
        {
            return _innerKernelMemory.SearchAsync(query, index, filter, filters, minRelevance, limit, context, cancellationToken);
        }

        public IAsyncEnumerable<MemoryAnswer> AskStreamingAsync(string question, string? index = null, MemoryFilter? filter = null, ICollection<MemoryFilter>? filters = null, double minRelevance = 0, SearchOptions? options = null, IContext? context = null, CancellationToken cancellationToken = default)
        {
            return _innerKernelMemory.AskStreamingAsync(question, index, filter, filters, minRelevance, options, context, cancellationToken);
        }

        Task<string> IKernelMemory.ImportDocumentAsync(Document document, string? index, IEnumerable<string>? steps, IContext? context, CancellationToken cancellationToken)
        {
            return _innerKernelMemory.ImportDocumentAsync(document, index, steps, context, cancellationToken);
        }
    }

    public class DocumentImportedEventArgs : EventArgs
    {
        public string FilePath { get; }
        public Dictionary<string, string> Tags { get; }

        public DocumentImportedEventArgs(string filePath, Dictionary<string, string> tags)
        {
            FilePath = filePath;
            Tags = tags;
        }
    }
}

using DocumentFormat.OpenXml.Office2010.Word;
using LLama.Common;
using Microsoft.IdentityModel.Tokens;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Context;
using PersonalWebApi.Entities.System;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Models.Models.Agent;
using System;
using System.Security.Claims;

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

        /// <summary>
        /// Check if in tag collection exists conversationUuid and sessionUuid
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        private bool _checkExistsingMainUuidInTag(TagCollection tags) 
        {
            if (!tags.ContainsKey("conversationUuid") || !tags.ContainsKey("sessionUuid"))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Imports a document into the kernel memory asynchronously.
        /// This method is currently not implemented and will always throw a ChatHistoryMessageException.
        /// 
        /// The exception message provides detailed information about the required parameters and their expected formats:
        /// - The TagCollection must contain 'conversationUuid' and 'sessionUuid' keys.
        /// - The Document must include a valid 'documentId' in UUID format.
        /// 
        /// The exception message also includes guidance on how to ensure these requirements are met.
        /// </summary>
        /// <param name="document">The document to be imported.</param>
        /// <param name="steps">Optional steps to be taken during the import process.</param>
        /// <param name="context">Optional context for the import operation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ChatHistoryMessageException">Thrown to indicate that the method is not implemented and to provide detailed validation error information.</exception>
        public async Task ImportDocumentAsync(Document document, string? index = null, IEnumerable<string>? steps = null, IContext? context = null, CancellationToken cancellationToken = default)
        {

            throw new ChatHistoryMessageException(
                $@"""
                <ChatHistoryMessageException>
                    <message>
                        Data validation error during document import:
                        - Required TagCollection keys: 'conversationUuid', 'sessionUuid'
                        - Required Document field: 'documentId' (UUID)
                    </message>
                    <details>
                        Ensure that the TagCollection contains both 'conversationUuid' and 'sessionUuid' keys.
                        The Document must include a valid 'documentId' in UUID format.
                    </details>
                </ChatHistoryMessageException>
                """
            );
        }

        /// Imports a document into the kernel memory asynchronously.
        /// This method performs several validation checks and saves the document import history.
        /// 
        /// Argument conversationUuid is set as index in memory, 
        /// so only data with this index will be used for the user with this conversationUuid.
        /// 
        /// The method ensures the following:
        /// - The TagCollection contains 'conversationUuid' and 'sessionUuid' keys.
        /// - The Document includes a valid 'documentId' in UUID format.
        /// 
        /// If any of these conditions are not met, a ChatHistoryMessageException is thrown with detailed information.
        /// 
        /// The method also saves the import history using the IAssistantHistoryManager.
        /// 
        /// Example usage:
        /// var text1 = "Mateusz ma zielone oczy";
        /// var text2 = "Mateusz ma długi nos";
        /// var text3 = "Piotr ma czerowne oczy";
        /// 
        /// await _memory.ImportTextAsync(text1, index: "1"); index is conversation uuid
        /// await _memory.ImportTextAsync(text2, index: "1");
        /// await _memory.ImportTextAsync(text3, index: "2");
        /// 
        /// await _memory.AskAsync("Kto ma zielone oczy?", index:"1");  // reposnse: Mateusz
        /// await _memory.AskAsync("Kto ma zielone oczy?", index: "2");  // empty
        /// await _memory.AskAsync("Kto ma czerowne oczy?", index: "2");  // reponse: Piotr
        /// await _memory.AskAsync("Kto ma długi nos?", index: "1");  // response: Mateusz
        /// await _memory.AskAsync("Kto ma długi nos?", index: "2");  // empty
        /// 
        /// </summary>
        /// <param name="filePath">The file path of the document to be imported.</param>
        /// <param name="documentId">The unique identifier of the document in UUID format.</param>
        /// <param name="tags">A collection of tags associated with the document.</param>
        /// <param name="index">The user ID or name associated with the document.</param>
        /// <param name="steps">Optional steps to be taken during the import process.</param>
        /// <param name="context">Optional context for the import operation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with a string result indicating the import status.</returns>
        /// <exception cref="ChatHistoryMessageException">Thrown to indicate validation errors or missing required parameters.</exception>
        /// <remarks>
        /// The 'index' parameter is used to identify the user owner of the document. 
        /// The memory will later use only data with this index for the user with this ID.
        /// </remarks>
        public async Task<string> ImportDocumentAsync(string filePath, string? documentId = null, TagCollection? tags = null, string? index = null, IEnumerable<string>? steps = null, IContext? context = null, CancellationToken cancellationToken = default)
        {
            var conversationUuid = Guid.Parse(tags["conversationUuid"].FirstOrDefault());
            var sessionUuid = Guid.Parse(tags["sessionUuid"].FirstOrDefault());
            Guid fileId = new Guid();

            if (!Guid.TryParse(documentId, out fileId) || !_checkExistsingMainUuidInTag(tags))
            {
                throw new ChatHistoryMessageException(
                    $@"""
                    <ChatHistoryMessageException>
                        <documentUuid>{documentId}</documentUuid>
                        <message>
                            Document ID has not proper Guid format in TagCollection.Document must have it.
                            Please ensure the following:
                            -The 'conversationUuid' and 'sessionUuid' keys are present in the TagCollection.
                            -The 'documentId' is a valid UUID format.
                        </message>
                        <argumentSent>
                            <conversationUuid>{conversationUuid}</conversationUuid>
                            <sessionUuid>{sessionUuid}</sessionUuid>
                            <documentId>{documentId}</documentId>
                        </argumentSent>
                        <details>
                            <requiredKeys>
                                <key>conversationUuid</key>
                                <key>sessionUuid</key>
                            </requiredKeys>
                            <requiredDocumentField>
                                <field>documentId</field>
                                <format>UUID</format>
                            </requiredDocumentField>
                        </details>
                    </ChatHistoryMessageException>
                    """
                );
            }

            // Store the document in the memory
            var result = await _innerKernelMemory.ImportDocumentAsync(filePath, documentId, tags, conversationUuid.ToString(), steps, context, cancellationToken);


            // save action history
            var chatMessage = new ChatHistoryShortTermFileMessageDto(conversationUuid, sessionUuid, fileId)
            {
                Action = "ImportDocument",
                ActionMessage = "Document imported to short term memory",
                Role = AuthorRole.User.ToString(),
                MessageType = "Action",
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Tags = tags.ToKeyValueList().Select(kv => kv.Value).ToList(),
                Metadata = new Dictionary<string, string>
                {
                    { "memoryIndex", conversationUuid.ToString() },
                    { "steps", string.Join(", ", steps ?? Enumerable.Empty<string>()) },
                    { "importStatus", "Ended" }
                },
            };

            await _assistantHistoryManager.SaveAsync(chatMessage);

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

        public Task<string> ImportDocumentAsync(DocumentUploadRequest uploadRequest, IContext? context = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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

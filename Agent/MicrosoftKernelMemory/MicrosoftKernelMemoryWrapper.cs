using DocumentFormat.OpenXml.Office2010.Word;
using LLama.Common;
using Microsoft.IdentityModel.Tokens;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Context;
using MongoDB.Driver.Core.WireProtocol.Messages;
using PersonalWebApi.Entities.System;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Models.Models.Memory;
using SharpCompress.Common;
using System;
using System.Security.Claims;

namespace PersonalWebApi.Agent.MicrosoftKernelMemory
{
    /// <summary>
    /// The <see cref="MicrosoftKernelMemoryWrapper"/> class is a wrapper implementation of the <see cref="IKernelMemory"/> interface.
    /// It provides additional functionality for managing kernel memory operations, including document import, text import, and web page import.
    /// This class also integrates with the <see cref="IAssistantHistoryManager"/> to save action histories for various memory operations.
    /// 
    /// The class ensures that all required validation checks are performed before executing memory operations. 
    /// It throws <see cref="ChatHistoryMessageException"/> for any validation errors, providing detailed information about the required parameters and their expected formats.
    /// 
    /// The wrapper requires additional data not provided in the original implementation, such as the conversation UUID, session UUID, document ID, etc.
    /// It throws an exception every time if the required data is not provided.
    /// 
    /// Additionally, the class supports the following operations:
    /// - Importing documents, text, and web pages into the kernel memory.
    /// - Deleting documents and indexes from the kernel memory.
    /// - Listing indexes, checking document readiness, getting document status, exporting files, and searching memory.
    /// 
    /// The class also saves the history of these operations using the <see cref="IAssistantHistoryManager"/> to maintain a record of actions performed.
    /// 
    /// <example>
    /// <code>
    /// // We have two conversations that are independently retrieved by conversation uuid
    /// // It's just one memory, but we choose which part we want to use via a uuid conversation.
    /// 
    /// // DATA CONVERSATION 1
    /// 
    ///     var conversation_1Id = Guid.NewGuid().ToString();
    ///     var session_1Id = Guid.NewGuid().ToString();
    /// 
    ///     var memory_1 = "Mateusz has green eyes";  // memory for conversation 1
    ///     var memory_2 = "Mateusz has a long nose";   // memory for conversation 1
    /// 
    ///     TagCollection conversation_1_id_tags = new TagCollection();  
    ///     conversation_1_id_tags.Add("sessionUuid", session_1Id);  // add session UUID to tags (required)
    /// 
    /// // DATA CONVERSATION 2
    /// 
    ///     var conversation_2Id = Guid.NewGuid().ToString();
    ///     var session_2Id = Guid.NewGuid().ToString();
    /// 
    ///     var memory_3 = "Piotr has red eyes";
    /// 
    ///     TagCollection conversation_id2_tags = new TagCollection(); 
    ///     conversation_id2_tags.Add("sessionUuid", session_2Id);  // add session UUID to tags (required)
    /// 
    /// // IMPORT TO MEMORY CONVERSATION 1
    /// 
    ///     await _memory.ImportTextAsync(memory_1, index: conversation_1Id, tags: conversation_1_id_tags);
    ///     await _memory.ImportTextAsync(memory_2, index: conversation_1Id, tags: conversation_1_id_tags);
    /// 
    /// // IMPORT TO MEMORY CONVERSATION 2
    /// 
    ///     await _memory.ImportTextAsync(memory_3, index: conversation_2Id, tags: conversation_id2_tags);
    /// 
    /// // CHAT :
    ///     var r1 = await _memory.AskAsync("Who has green eyes?", index: conversation_1Id);  // response: Mateusz
    ///     var r2 = await _memory.AskAsync("Who has green eyes?", index: conversation_2Id);  // none - not in memory by index
    ///     var r3 = await _memory.AskAsync("Who has red eyes?", index: conversation_2Id);   // response: Piotr
    ///     var r4 = await _memory.AskAsync("Who has a long nose?", index: conversation_1Id);  // response: Mateusz
    ///     var r5 = await _memory.AskAsync("Who has a long nose?", index: conversation_2Id);  // none - not in memory by index
    /// 
    /// // the same is with rest of the data you use
    ///     await _memory.ImportDocumentAsync(filePath, tags: conversation_1_id_tags, index: conversation_1Id, documentId:Guid.NewGuid().ToString());
    ///     
    /// </code>
    /// </example>
    /// </summary>

    public class MicrosoftKernelMemoryWrapper : IKernelMemory
    {
        private readonly IKernelMemory _innerKernelMemory;
        private readonly IAssistantHistoryManager _assistantHistoryManager;

        public MicrosoftKernelMemoryWrapper(IKernelMemory innerKernelMemory, IAssistantHistoryManager assistantHistoryManager)
        {
            _innerKernelMemory = innerKernelMemory;
            _assistantHistoryManager = assistantHistoryManager;
        }

        /// <summary>
        /// Returns a string that represents the type of message for memory actions.
        /// This method is used to mark all actions with the same name in history.
        /// </summary>
        /// <returns>A string "MemoryAction" indicating the type of memory action message.</returns>
        public static string GetMessageType() => "MemoryAction";


        #region interface_with_additional_implementation

        /// <summary>
        /// Check if in tag collection exists sessionUuid
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        private bool _checkExistsingMainUuidInTag(TagCollection tags) 
        {
            if (!tags.ContainsKey("sessionUuid"))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Imports a document into the kernel memory asynchronously.
        /// This method is currently not implemented and will always throw a ChatHistoryMessageException,
        /// there is not enough data to use it.
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
        Task<string> IKernelMemory.ImportDocumentAsync(Document document, string? index, IEnumerable<string>? steps, IContext? context, CancellationToken cancellationToken)
        {
            throw new ChatHistoryMessageException(
                $@"""
                <ChatHistoryMessageException>
                    <message>
                        Required data validation error during document import:
                        -The 'index' (conversation ID) - require UUID format.
                        -The 'sessionUuid' is not present in TagCollection.
                        -The 'documentId' - require UUID format.
                    </message>
                    <details>
                        Required arguments not exists.
                    </details>
                </ChatHistoryMessageException>
                """
            );
        }

        /// <summary>
        /// See above
        /// </summary>
        public async Task ImportDocumentAsync(Document document, string? index = null, IEnumerable<string>? steps = null, IContext? context = null, CancellationToken cancellationToken = default)
        {
            throw new ChatHistoryMessageException(
                $@"""
                <ChatHistoryMessageException>
                    <message>
                        Required data validation error during document import:
                        -The 'index' (conversation ID) - require UUID format.
                        -The 'sessionUuid' is not present in TagCollection.
                        -The 'documentId' - require UUID format.
                    </message>
                    <details>
                        Required arguments not exists.
                    </details>
                </ChatHistoryMessageException>
                """
            );
        }

        /// <summary>
        /// See above
        /// </summary>
        /// <return><exception cref="ChatHistoryMessageException"></exception></return>
        public Task<string> ImportDocumentAsync(DocumentUploadRequest uploadRequest, IContext? context = null, CancellationToken cancellationToken = default)
        {
            throw new ChatHistoryMessageException(
                $@"""
                <ChatHistoryMessageException>
                    <message>
                        Required data validation error during document import:
                        -The 'index' (conversation ID) - require UUID format.
                        -The 'sessionUuid' is not present in TagCollection.
                        -The 'documentId' - require UUID format.
                    </message>
                    <details>
                        Required arguments not exists.
                    </details>
                </ChatHistoryMessageException>
                """
            );
        }

        /// <summary>
        /// 
        /// Imports a document into the kernel memory asynchronously.
        /// This method performs several validation checks and saves the document import history.
        /// 
        /// Argument conversationUuid is set as index in memory, 
        /// so only data with this index will be used for the user with this conversationUuid.
        /// 
        /// The method ensures the following:
        /// - The TagCollection contains 'conversationUuid' and 'sessionUuid' keys.
        /// - The 'documentId' must be in UUID format, it must exists.
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
        /// <param name="index">The conversation UUID.</param>
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
            var conversationUuid = Guid.Parse(index);
            var sessionUuid = Guid.Parse(tags["sessionUuid"].FirstOrDefault());
            Guid fileId = new Guid();

            if (!Guid.TryParse(documentId, out fileId) || !_checkExistsingMainUuidInTag(tags))
            {
                throw new ChatHistoryMessageException(
                    $@"""
                    <ChatHistoryMessageException>
                        <action>ImportDocumentAsFilePathToMemory</action>
                        <documentUuid>{documentId}</documentUuid>
                        <message>
                            Required - documentID, tags (TagCollection) withsessionUuid , index as conversation UUID.
                            Please ensure the following:
                            -The 'index' - exists and is a valid conversation UUID format
                            -The 'sessionUuid' - keys exists in the TagCollection and is a valid session UUID format.
                            -The 'documentId' - exists with valid UUID format.
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
                Action = "ImportDocumentAsFilePathToMemory",
                ActionMessage = "Document imported to short term memory",
                Role = AuthorRole.User.ToString(),
                FileId = fileId,
                MessageType = GetMessageType(),
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Tags = tags.ToKeyValueList().Select(kv => kv.Value).ToList(),
                Metadata = new Dictionary<string, string>
                {
                    { "memoryIndex", conversationUuid.ToString() },
                    { "steps", string.Join(", ", steps ?? Enumerable.Empty<string>()) },
                    { "status", "Ended" }
                },
            };

            await _assistantHistoryManager.SaveAsync(chatMessage);

            return result;
        }
        
        /// <summary>
        /// See above
        /// </summary>
        public async Task<string> ImportDocumentAsync(Stream content, string? fileName = null, string? documentId = null, TagCollection? tags = null, string? index = null, IEnumerable<string>? steps = null, IContext? context = null, CancellationToken cancellationToken = default)
        {
            var conversationUuid = Guid.Parse(index);
            var sessionUuid = Guid.Parse(tags["sessionUuid"].FirstOrDefault());
            Guid fileId = new Guid();

            if (!Guid.TryParse(documentId, out fileId) || !_checkExistsingMainUuidInTag(tags))
            {
                throw new ChatHistoryMessageException(
                    $@"""
                    <ChatHistoryMessageException>
                        <action>ImportDocumentAsStreamContent</action>
                        <documentUuid>{documentId}</documentUuid>
                        <message>
                            Required - documentID, tags (TagCollection) withsessionUuid , index as conversation UUID.
                            Please ensure the following:
                            -The 'index' - exists and is a valid conversation UUID format
                            -The 'sessionUuid' - keys exists in the TagCollection and is a valid session UUID format.
                            -The 'documentId' - exists with valid UUID format.
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

            var result = await _innerKernelMemory.ImportDocumentAsync(content, fileName, documentId, tags, index, steps, context, cancellationToken);

            // Save action history
            var chatMessage = new ChatHistoryShortTermFileMessageDto(conversationUuid, sessionUuid, fileId)
            {
                Action = "ImportDocumentAsStreamContent",
                ActionMessage = "Document imported to short term memory",
                Role = AuthorRole.User.ToString(),
                MessageType = GetMessageType(),
                FilePath = fileName,
                FileName = fileName,
                Tags = tags.ToKeyValueList().Select(kv => kv.Value).ToList(),
                Metadata = new Dictionary<string, string>
                {
                    { "memoryIndex", conversationUuid.ToString() },
                    { "steps", string.Join(", ", steps ?? Enumerable.Empty<string>()) },
                    { "status", "Ended" }
                },
            };

            return result;
        }

        /// <summary>
        /// Imports text into the kernel memory asynchronously.
        /// This method performs several validation checks and saves the text import history.
        /// 
        /// The method ensures the following:
        /// - The TagCollection contains 'conversationUuid' and 'sessionUuid' keys.
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
        /// await _memory.ImportTextAsync(text1, index: "1"); // index is conversation uuid
        /// await _memory.ImportTextAsync(text2, index: "1");
        /// await _memory.ImportTextAsync(text3, index: "2");
        /// 
        /// await _memory.AskAsync("Kto ma zielone oczy?", index:"1");  // response: Mateusz
        /// await _memory.AskAsync("Kto ma zielone oczy?", index: "2");  // empty
        /// await _memory.AskAsync("Kto ma czerowne oczy?", index: "2");  // response: Piotr
        /// await _memory.AskAsync("Kto ma długi nos?", index: "1");  // response: Mateusz
        /// await _memory.AskAsync("Kto ma długi nos?", index: "2");  // empty
        /// </summary>
        /// <param name="text">The text content to be imported.</param>
        /// <param name="documentId">The unique identifier of the document in UUID format.</param>
        /// <param name="tags">A collection of tags associated with the text.</param>
        /// <param name="index">The user ID or name associated with the text.</param>
        /// <param name="steps">Optional steps to be taken during the import process.</param>
        /// <param name="context">Optional context for the import operation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with a string result indicating the import status.</returns>
        /// <exception cref="ChatHistoryMessageException">Thrown to indicate validation errors or missing required parameters.</exception>
        /// <remarks>
        /// The 'index' parameter is used to identify the user owner of the text. 
        /// The memory will later use only data with this index for the user with this ID.
        /// </remarks>
        public async Task<string> ImportTextAsync(string text, string? documentId = null, TagCollection? tags = null, string? index = null, IEnumerable<string>? steps = null, IContext? context = null, CancellationToken cancellationToken = default)
        {
            var conversationUuid = Guid.Parse(index);
            var sessionUuid = Guid.Parse(tags["sessionUuid"].FirstOrDefault());

            if (!_checkExistsingMainUuidInTag(tags))
            {
                throw new ChatHistoryMessageException(
                    $@"""
                    <ChatHistoryMessageException>
                        <action>ImportTextToMemory</action>
                        <documentUuid>{documentId}</documentUuid>
                        <message>
                            Required - tags (TagCollection) withsessionUuid , index as conversation UUID.
                            Please ensure the following:
                            -The 'index' - exists and is a valid conversation UUID format
                            -The 'sessionUuid' - keys exists in the TagCollection and is a valid session UUID format.
                            -The 'documentId' - exists with valid UUID format.
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

            // save to memory
            var result = await _innerKernelMemory.ImportTextAsync(text, documentId, tags, index, steps, context, cancellationToken);

            var chatMessage = new ChatHistoryShortTermMessageDto(conversationUuid, sessionUuid)
            {
                Action = "ImportTextToMemory",
                ActionMessage = "Text imported to short term memory",
                Role = AuthorRole.User.ToString(),
                MessageType = GetMessageType(),
                Tags = tags.ToKeyValueList().Select(kv => kv.Value).ToList(),
                Metadata = new Dictionary<string, string>
                {
                    { "memoryIndex", conversationUuid.ToString() },
                    { "steps", string.Join(", ", steps ?? Enumerable.Empty<string>()) },
                    { "status", "Ended" }
                },
            };

            // save history
            await _assistantHistoryManager.SaveAsync(chatMessage);

            return result;
        }

        /// <summary>
        /// Imports a web page into the kernel memory asynchronously.
        /// This method performs several validation checks and saves the web page import history.
        /// 
        /// The method ensures the following:
        /// - The TagCollection contains 'conversationUuid' and 'sessionUuid' keys.
        /// 
        /// If any of these conditions are not met, a ChatHistoryMessageException is thrown with detailed information.
        /// 
        /// The method also saves the import history using the IAssistantHistoryManager.
        /// </summary>
        /// <param name="url">The URL of the web page to be imported.</param>
        /// <param name="documentId">The unique identifier of the document in UUID format.</param>
        /// <param name="tags">A collection of tags associated with the web page.</param>
        /// <param name="index">The user ID or name associated with the web page.</param>
        /// <param name="steps">Optional steps to be taken during the import process.</param>
        /// <param name="context">Optional context for the import operation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with a string result indicating the import status.</returns>
        /// <exception cref="ChatHistoryMessageException">Thrown to indicate validation errors or missing required parameters.</exception>
        /// <remarks>
        /// The 'index' parameter is used to identify the user owner of the web page. 
        /// The memory will later use only data with this index for the user with this ID.
        /// </remarks>
        public async Task<string> ImportWebPageAsync(string url, string? documentId = null, TagCollection? tags = null, string? index = null, IEnumerable<string>? steps = null, IContext? context = null, CancellationToken cancellationToken = default)
        {
            var conversationUuid = Guid.Parse(index);
            var sessionUuid = Guid.Parse(tags["sessionUuid"].FirstOrDefault());

            if (!_checkExistsingMainUuidInTag(tags))
            {
                throw new ChatHistoryMessageException(
                    $@"""
                    <ChatHistoryMessageException>
                        <action>ImportWebByUrlToMemory</action>
                        <message>
                            Required - tags (TagCollection) withsessionUuid , index as conversation UUID.
                            Please ensure the following:
                            -The 'index' - exists and is a valid conversation UUID format
                            -The 'sessionUuid' - keys exists in the TagCollection and is a valid session UUID format.
                            -The 'documentId' - exists with valid UUID format.
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

            var result = await _innerKernelMemory.ImportWebPageAsync(url, documentId, tags, index, steps, context, cancellationToken);

            var chatMessage = new ChatHistoryShortTermPageMessageDto(conversationUuid, sessionUuid)
            {
                Action = "ImportWebByUrlToMemory",
                ActionMessage = "Page imported to short term memory",
                Role = AuthorRole.User.ToString(),
                MessageType = GetMessageType(),
                Tags = tags.ToKeyValueList().Select(kv => kv.Value).ToList(),
                Metadata = new Dictionary<string, string>
                {
                    { "memoryIndex", conversationUuid.ToString() },
                    { "steps", string.Join(", ", steps ?? Enumerable.Empty<string>()) },
                    { "status", "Ended" }
                },
            };

            await _assistantHistoryManager.SaveAsync(chatMessage);

            return result;
        }

        /// <summary>
        /// Delete data from memory by index. Index is conversation UUID so will delete all memory from conversation.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DeleteIndexAsync(string? index = null, CancellationToken cancellationToken = default)
        {
            var conversationUuid = Guid.Parse(index);

            await _innerKernelMemory.DeleteIndexAsync(index, cancellationToken);

            var chatMessage = new ChatHistoryShortTermDeleteIndexDto(conversationUuid, Guid.NewGuid())
            {
                Action = "DeleteIndexFromMemoryByIndex",
                ActionMessage = "Delete data from memory by index (conversation UUID)",
                Role = AuthorRole.System.ToString(),  // only system should delete data from memory
                MessageType = GetMessageType(),
                Metadata = new Dictionary<string, string>
                {
                    { "status", "Ended" }
                },
            };

            await _assistantHistoryManager.SaveAsync(chatMessage);
        }

        /// <summary>
        /// Deletes a specified document from the kernel memory asynchronously.
        /// This method performs validation checks and saves the document deletion history.
        /// 
        /// The method ensures the following:
        /// - The 'documentId' is in a valid UUID format.
        /// 
        /// If this condition is not met, a ChatHistoryMessageException is thrown with detailed information.
        /// 
        /// The method also saves the deletion history using the IAssistantHistoryManager.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document in UUID format.</param>
        /// <param name="index">The user ID or name associated with the document.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ChatHistoryMessageException">Thrown to indicate validation errors or missing required parameters.</exception>
        public async Task DeleteDocumentAsync(string documentId, string? index = null, CancellationToken cancellationToken = default)
        {
            Guid fileId = new Guid();

            if (!Guid.TryParse(documentId, out fileId))
            {
                throw new ChatHistoryMessageException(
                    $@"""
                    <ChatHistoryMessageException>
                        <action>DeleteDocumentFromMemory</action>
                        <documentUuid>{documentId}</documentUuid>
                        <message>
                            Please ensure the following:
                            -The 'documentId' is a valid UUID format.
                        </message>
                        <argumentSent>
                            <documentId>{documentId}</documentId>
                        </argumentSent>
                        <details>
                            <requiredKeys>
                                <key>documentId</key>
                            </requiredKeys>
                        </details>
                    </ChatHistoryMessageException>
                    """
                );
            }

            await _innerKernelMemory.DeleteDocumentAsync(documentId, index, cancellationToken);

            var chatMessage = new ChatHistoryShortTermDeleteDocumentDto(Guid.NewGuid(), Guid.NewGuid())
            {
                Action = "DeleteIndexFromMemoryByIndex",
                ActionMessage = "Delete document from memory by document id",
                Role = AuthorRole.System.ToString(),  // only system should delete data from memory
                FileId = documentId,
                MessageType = GetMessageType(),
                Metadata = new Dictionary<string, string>
                {
                    { "status", "Ended" }
                },
            };

            await _assistantHistoryManager.SaveAsync(chatMessage);

        }

        #endregion

        #region interface_without_additional_implementation

        public Task<IEnumerable<IndexDetails>> ListIndexesAsync(CancellationToken cancellationToken = default)
        {
            return _innerKernelMemory.ListIndexesAsync(cancellationToken);
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

        #endregion

    }
}

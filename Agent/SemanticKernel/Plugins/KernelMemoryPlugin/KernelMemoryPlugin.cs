using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PersonalWebApi.Agent.Memory.Observability;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Models.Models.Memory;
using PersonalWebApi.Services.Services.System;
using System.ComponentModel;

namespace PersonalWebApi.Agent.SemanticKernel.Plugins.KernelMemoryPlugin
{
    public class KernelMemoryPlugin
    {
        private readonly KernelMemoryWrapper _kernelMemory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public KernelMemoryPlugin(
            KernelMemoryWrapper kernelMemory,
            IHttpContextAccessor httpContextAccessor)
        {
            _kernelMemory = kernelMemory;
            _httpContextAccessor = httpContextAccessor;
        }

        [KernelFunction("load_file_from_url_to_kernel_memory")]
        [Description("Load a file from url to the kernel memory")]
        [return: Description("True if memory import data")]
        public async Task<bool> AddToMemoryAsync(string fileUrl)
        {
            (Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(_httpContextAccessor);

            var documentId = Guid.NewGuid();

            await _kernelMemory.ImportDocumentAsync(fileUrl, index: conversationUuid.ToString(), documentId: documentId.ToString());

            return true;
        }

        [KernelFunction("import_web_page_to_kernel_memory")]
        [Description("Imports a web page into the kernel memory asynchronously.")]
        [return: Description("A string result indicating the import status.")]
        public async Task<string> ImportWebPageToKernelMemoryAsync(string url, string? documentId = null, TagCollection? tags = null, string? index = null, IEnumerable<string>? steps = null)
        {
            (Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(_httpContextAccessor);

            var result = await _kernelMemory.ImportWebPageAsync(url, index: conversationUuid.ToString());

            return result;
        }


        [KernelFunction("ask_question_to_kernel_memory")]
        [Description("Ask a kernel memory")]
        [return: Description("Response from memory")]
        public async Task<string> AskMemoryAsync(string questionToMemory)
        {
            (Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(_httpContextAccessor);

            var result = await _kernelMemory.AskAsync(questionToMemory, index: conversationUuid.ToString());

            return result.Result;
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
        /// </summary>
        /// <param name="text">The text content to be imported.</param>
        /// <param name="documentId">The unique identifier of the document in UUID format.</param>
        /// <param name="tags">A collection of tags associated with the text.</param>
        /// <param name="index">The user ID or name associated with the text.</param>
        /// <param name="steps">Optional steps to be taken during the import process.</param>
        /// <param name="context">Optional context for the import operation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>Document ID.</returns>
        /// <exception cref="ChatHistoryMessageException">Thrown to indicate validation errors or missing required parameters.</exception>
        /// <remarks>
        /// The 'index' parameter is used to identify the user owner of the text. 
        /// The memory will later use only data with this index for the user with this ID.
        /// </remarks>
        [KernelFunction("import_text_to_kernel_memory")]
        [Description("Imports text into the kernel memory asynchronously.")]
        [return: Description("A string result is document id.")]
        public async Task<string> ImportTextToKernelMemoryAsync(string text, string? documentId = null, TagCollection? tags = null, string? index = null, IEnumerable<string>? steps = null)
        {
            (Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(_httpContextAccessor);

            var result = await _kernelMemory.ImportTextAsync(text, documentId, tags, index, steps, context: null, cancellationToken: default);

            return result;
        }

    }
}

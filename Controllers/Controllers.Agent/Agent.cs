using DocumentFormat.OpenXml.Office2010.Word;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Prompts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MongoDB.Bson;
using PersonalWebApi.Agent;
using PersonalWebApi.Agent.MicrosoftKernelMemory;
using PersonalWebApi.Controllers.Controllers.Qdrant;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Extensions;
using PersonalWebApi.Models.Agent;
using PersonalWebApi.Models.Azure;
using PersonalWebApi.Services.Azure;
using PersonalWebApi.Services.Services.Qdrant;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using Qdrant.Client.Grpc;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace PersonalWebApi.Controllers.Agent
{
    [ApiController]
    [Route("api/agent")]
    public class Agent : ControllerBase
    {
        private readonly Kernel _kernel;
        private readonly MicrosoftKernelMemoryWrapper _memory;  // IkernelMemory
        private readonly IBlobStorageService _blobStorage;
        private readonly IDocumentReaderDocx _documentReaderDocx;
        private readonly IQdrantFileService _qdrant;
        private readonly IConfiguration _configuration;

        private readonly IAssistantHistoryManager _assistantHistoryManager;

        public Agent(
            Kernel kernel,
            MicrosoftKernelMemoryWrapper memory,
            IBlobStorageService blobStorageService,
            IDocumentReaderDocx documentReaderDocx,
            IQdrantFileService qdrant,
            IConfiguration configuration,
            
            IAssistantHistoryManager assistantHistoryManager

            )
        {
            _kernel = kernel;
            _memory = memory;
            _blobStorage = blobStorageService;
            _documentReaderDocx = documentReaderDocx;
            _qdrant = qdrant;
            _configuration = configuration;

            _assistantHistoryManager = assistantHistoryManager;
        }



        [HttpPost("chat/{conversationUuid}/{id:int}")]
        [Experimental("SKEXP0050")]
        public async Task<string> Chat(string conversationUuid = "30f4373b-5b18-41fd-8b40-5953825b3c0d", int id = 1)
        {
            var sessionId = Guid.NewGuid().ToString();


            //await _chatRepo.LoadChatHistoryToMemoryAsync(User, Guid.Parse(conversationUuid), _memory);

            // Name of the plugin. This is the name you'll use in skPrompt, e.g. {{memory.ask ...}}
            var pluginName = "memory";

            string filePath = Path.Combine(AppContext.BaseDirectory, "bajka.docx");

            TagCollection tags = new TagCollection();
            tags.Add("sessionUuid", sessionId.ToString());
            tags.Add("conversationUuid", conversationUuid);



            var aa = "Mateusz Iwański ma zielone oczy";

            var b = "Piotr ma czerowne oczy";

            //_memory.ImportDocumentAsync(filePath, index:)

            await _memory.ImportTextAsync(aa, index: "1");
            await _memory.ImportTextAsync(b, index: "2");

            var k = await _memory.AskAsync("Kto ma zielone oczy?", index:"1");

            var c = await _memory.AskAsync("Kto ma zielone oczy?", index: "2");

            await _memory.ImportDocumentAsync(filePath, tags: tags);

            // Import the plugin into the kernel.
            var memoryPlugin = _kernel.ImportPluginFromObject(
                new MemoryPlugin(_memory, waitForIngestionToComplete: true),
                pluginName);

            var skPrompt = """
                        Question to Memory: {{$input}}

                        Answer from Memory: {{memory.ask $input}}

                        If the answer is empty look forward. If you find answer say 'I haven't in memory but ai found the answer - <answer>' otherwise reply with a preview of the answer,
                        truncated to 15 words. Prefix with one emoji relevant to the content.
                        """;

            await _assistantHistoryManager.LoadAsync(Guid.Parse(conversationUuid), _memory);


            ChatMessage a = new ChatMessage();

            var f = new PromptExecutionSettings()
            {
                ExtensionData = new Dictionary<string, object>()
                {
                    { "conversationUuid", conversationUuid },
                    { "sessionUuid", sessionId }
                }
            };


            var myFunction = _kernel.CreateFunctionFromPrompt(skPrompt, f);

            //myFunction.Metadata.AdditionalProperties.TryAdd("sessionUuid", sessionId.ToString());
            //myFunction.Metadata.AdditionalProperties.TryAdd("conversationUuid", conversationUuid.ToString());

            var answer = await myFunction.InvokeAsync(_kernel, "Kto skręcił sobie nogę?");

            return answer.ToString();
        }
    }
}

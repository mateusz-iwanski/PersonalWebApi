using DocumentFormat.OpenXml.Office2010.Word;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PersonalWebApi.Controllers.Controllers.Qdrant;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Models.Agent;
using PersonalWebApi.Models.Azure;
using PersonalWebApi.Services.Azure;
using PersonalWebApi.Services.Services.History;
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
        private readonly IKernelMemory _memory;
        private readonly IBlobStorageService _blobStorage;
        private readonly IDocumentReaderDocx _documentReaderDocx;
        private readonly IQdrantFileService _qdrant;
        private readonly IConfiguration _configuration;
        private readonly ChatHistoryRepository _chatHistoryRepository;

        public Agent(
            Kernel kernel, 
            IKernelMemory memory, 
            IBlobStorageService blobStorageService, 
            IDocumentReaderDocx documentReaderDocx,
            IQdrantFileService qdrant,
            IConfiguration configuration,
            ChatHistoryRepository chatHistoryRepository
            )
        {
            _kernel = kernel;
            _memory = memory;
            _blobStorage = blobStorageService;
            _documentReaderDocx = documentReaderDocx;
            _qdrant = qdrant;
            _configuration = configuration;
        }

        [HttpPost("chat/{conversationUuid:guid}/{id:int}")]
        [Experimental("SKEXP0050")]
        public async Task<string> Chat(Guid conversationUuid, int id)
        {

            // Name of the plugin. This is the name you'll use in skPrompt, e.g. {{memory.ask ...}}
            var pluginName = "memory";

            await _memory.ImportTextAsync("Mateusz Iwański - rozwalił głowę");

            string filePath = Path.Combine(AppContext.BaseDirectory, "bajka.docx");
            await _memory.ImportDocumentAsync(filePath, tags: new() { { "bajka", "dziewczynka z mama" } });

            // example of upload to qdrant

            //SemanticKernelTextChunker s = new SemanticKernelTextChunker("text-embedding-3-small");
            //var chunked = s.ChunkText("newId", 100, );

            //return JsonSerializer.Serialize(chunked);

            // Import the plugin into the kernel.
            // 'waitForIngestionToComplete' set to true forces memory write operations to wait for completion.
            var memoryPlugin = _kernel.ImportPluginFromObject(
                new MemoryPlugin(_memory, waitForIngestionToComplete: true),
                pluginName);


            //var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            //var test = await chatCompletionService.GetChatMessageContentAsync("powiedz jak wygląda wigilia w usa");
            //if (id == 1)
            //{
            //    string filePath = Path.Combine(AppContext.BaseDirectory, "bajka.docx");
            //    await _memory.ImportDocumentAsync(filePath, tags: new() { { "bajka", "dziewczynka z mama" } });

            //}



            //var context = new KernelArguments
            //{
            //    [MemoryPlugin.FilePathParam] = Path.Combine(AppContext.BaseDirectory, "bajka.docx"),
            //    [MemoryPlugin.DocumentIdParam] = "B1"
            //};


            //await memoryPlugin["SaveFile"].InvokeAsync(_kernel, context);

            var skPrompt = """
                Question to Memory: {{$input}}

                Answer from Memory: {{memory.ask $input}}

                If the answer is empty look forward. If you find answer say 'I haven't in memory but ai found the answer - <answer>' otherwise reply with a preview of the answer,
                truncated to 15 words. Prefix with one emoji relevant to the content.
                """;


            var myFunction = _kernel.CreateFunctionFromPrompt(skPrompt);

            var answer = await myFunction.InvokeAsync(_kernel,
                "Kto skręcił sobie nogę?");





            return answer.ToString();

            //await memoryPlugin["SaveFile"].InvokeAsync(_kernel, context);


            //var answer1 = await _memory.AskAsync("kto skręcił sobie nogę?");

            //return answer1.ToJson(true);
        }
    }
}

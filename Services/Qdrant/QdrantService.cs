using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using PersonalWebApi.Services.Services.Agent;
using System.Security.Claims;
using Qdrant.Client.Grpc;
using Elastic.Clients.Elasticsearch.IndexManagement;
using PersonalWebApi.Utilities.Utilities.Qdrant;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using PersonalWebApi.Services.FileStorage;
using PersonalWebApi.Exceptions;
using System.Collections;
using PersonalWebApi.Agent;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin;
using Microsoft.SemanticKernel.Process.Runtime;
using DocumentFormat.OpenXml.Wordprocessing;
using PersonalWebApi.Services.Agent;
using System;
using Microsoft.EntityFrameworkCore.Storage;
using PersonalWebApi.Processes.FileStorage.Steps;
using PersonalWebApi.Processes.Document.Steps;
using PersonalWebApi.Processes.Qdrant.Events;
using PersonalWebApi.Processes.Qdrant.Pipelines;
using iText.Commons.Utils;
using static Microsoft.KernelMemory.Constants.CustomContext;
using YamlDotNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Linq;

namespace PersonalWebApi.Services.Services.Qdrant
{
    public class QdrantService : IQdrantService
    {
        private readonly string _collectionName;

        private readonly IEmbedding _embeddingOpenAi;
        private readonly IConfiguration _configuration;

        private QdrantApi _qdrantApi { get; set; }
        private ClaimsPrincipal _userClaimsPrincipal { get; set; }
        private string _modelEmbedding { get; set; }
        private string _qdrantCollectionName { get; set; }
        private ulong _qdrantCollectionSize { get; set; }
        private bool _overwrite { get; set; }


        public QdrantService(
            IEmbedding embeddingOpenAi,
            IConfiguration configuration
            )
        {
            _configuration = configuration;

            _embeddingOpenAi = embeddingOpenAi;

            //_userClaimsPrincipal = httpContextAccessor.HttpContext?.User ?? throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext.User));

            _collectionName = _configuration.GetSection("Qdrant:Container:Name").Value ?? throw new SettingsException("Qdrant:Container:Name not exists");

            var collectionDistance = _configuration.GetSection("Qdrant:Container:Distance").Value ??
                throw new SettingsException("Qdrant:Container:Distance not exists in appsettings");

            var collectionSize = ulong.Parse(_configuration.GetSection("Qdrant:Container:Size").Value ??
                throw new SettingsException("Qdrant:Container:Size not exists in appsettings"));

            var modelEmbedding = _configuration.GetSection("Qdrant:Container:OpenAiModelEmbedding").Value ??
                throw new SettingsException("Qdrant:Container:OpenAiModelEmbedding not exists in appsettings");

            var modelApiKey = _configuration.GetSection("OpenAI:Access:ApiKey").Value ??
                throw new SettingsException("OpenAI:Access:ApiKey not exists in appsettings");

            var qdrantApiKey = _configuration.GetSection("Qdrant:Access:Key").Value ??
                throw new SettingsException("Qdrant:Access:Key not exists in appsettings");

            var qdrantUrl = _configuration.GetSection("Qdrant:Access:Uri").Value ??
                throw new SettingsException("Qdrant:Access:Uri not exists in appsettings");

            var maxTokenFileChunked = _configuration.GetSection("Qdrant:MaxTokenFileChunked").Value ??
                throw new SettingsException("Qdrant:MaxTokenFileChunked not exists in appsettings");

            var maxSummaryFileCharacters = int.Parse(_configuration.GetSection("Qdrant:MaxSummaryFileCharacters").Value ??
                throw new SettingsException("Qdrant:MaxSummaryFileCharacters not exists in appsettings"));

            Setup(
                modelEmbedding: modelEmbedding,
                modelEmbeddingApiKey: modelApiKey,
                qdrantUri: qdrantUrl,
                qdrantApiKey: qdrantApiKey,
                qdrantCollectionName: _collectionName,
                qdrantCollectionDistance: Distance.Cosine,
                qdrantCollectionSize: collectionSize,
                overwrite: true,
                user: _userClaimsPrincipal
                );
        }

        /// <summary>
        /// Sets up the QdrantService with the necessary parameters.
        /// </summary>
        /// <param name="modelEmbedding">The model embedding to use.</param>
        /// <param name="modelEmbeddingApiKey">The API key for the model embedding.</param>
        /// <param name="qdrantUri">The URI of the Qdrant server.</param>
        /// <param name="qdrantApiKey">The API key for the Qdrant server.</param>
        /// <param name="qdrantCollectionName">The name of the Qdrant collection.</param>
        /// <param name="qdrantCollectionDistance">The distance metric for the Qdrant collection.</param>
        /// <param name="qdrantCollectionSize">The size of the Qdrant collection.</param>
        /// <param name="overwrite">Whether to overwrite existing files.</param>
        /// <param name="user">The user making the request.</param>
        protected void Setup(
            string modelEmbedding,
            string modelEmbeddingApiKey,
            string qdrantUri,
            string qdrantApiKey,
            string qdrantCollectionName,
            Distance qdrantCollectionDistance,
            ulong qdrantCollectionSize,
            bool overwrite,
            ClaimsPrincipal user
            )
        {
            _modelEmbedding = modelEmbedding;
            _qdrantCollectionName = qdrantCollectionName;
            _qdrantCollectionSize = qdrantCollectionSize;
            _overwrite = overwrite;

            _userClaimsPrincipal = user;

            _embeddingOpenAi.Setup(modelEmbedding, modelEmbeddingApiKey);

            _qdrantApi = new QdrantApi(_embeddingOpenAi, qdrantUri, qdrantApiKey, _qdrantCollectionSize, qdrantCollectionDistance);
        }

        [Experimental("SKEXP0080")]
        public async Task test()
        {
            //Kernel kernel = new AgentRouter(_configuration).AddOpenAIChatCompletion();
            ////IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            //// To enable manual function invocation, set the `autoInvoke` parameter to `false`.
            //KernelPlugin getTag = kernel.ImportPluginFromType<TagCollectorPlugin>();

            //var k = new PromptExecutionSettings() { ModelId = "defasssult" };


            //FunctionResult summary = await kernel.InvokeAsync(
            //        getTag["generate_tag_for_text_content"], new KernelArguments(k) { ["textContent"] = "samochód to fajny pojazd" });

            //// Use the ToString() method to get the result as a string
            //string resultString = summary.ToString();

            //foreach (var item in summary.GetValue<List<string>>())
            //{
            //    Console.WriteLine(item);
            //}


            //ProcessBuilder process = new("AdvancedChatBot");

            //var introStep = process.AddStepFromType<IntroStep>();
            //var userInputStep = process.AddStepFromType<UserInputStep>();
            //var responseStep = process.AddStepFromType<ChatBotResponseStep>();
            //var exitStep = process.AddStepFromType<ExitStep>();

            //// Start with the intro step
            //process.OnInputEvent(ChatBotEvents.StartProcess)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(introStep));  // 3 - IntroStep.PrintIntroMessage

            //// After intro, proceed to user input
            //introStep.OnFunctionResult(nameof(IntroStep.PrintIntroMessage))
            //    .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));  // 1 - inicjalizacja UserInputState  UserInputStep : KernelProcessStep<UserInputState>

            //// When user input is received, process it
            //userInputStep.OnEvent(ChatBotEvents.UserInputReceived)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(responseStep, parameterName: "userMessage"));  // 2 - inicjalizacja ChatBotState ChatBotResponseStep : KernelProcessStep<ChatBotState>

            //// After bot response, loop back to user input
            //responseStep.OnEvent(ChatBotEvents.ResponseGenerated)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));  // ublic override ValueTask ActivateAsync(KernelProcessStepState<UserInputState> state)

            //// Handle exit event
            //userInputStep.OnEvent(ChatBotEvents.Exit)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(exitStep));

            //// Stop process after exit step
            //exitStep.OnFunctionResult(nameof(ExitStep.HandleExit))
            //    .StopProcess();

            //KernelProcess kernelProcess = process.Build();  // ## 0

            //await kernelProcess.StartAsync(kernel, new KernelProcessEvent { Id = ChatBotEvents.StartProcess });

            ///////

            //ProcessBuilder process = new("ChatBot");
            //var startStep = process.AddStepFromType<Steps>();
            ////var doSomeWorkStep = process.AddStepFromType<NextSteps>();
            //// Define the process flow
            //process
            //    .OnInputEvent(QdrantEvents.StartProcess)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(startStep, functionName: Steps.Functions.MyExecute, "i"));

            //KernelProcess kernelProcess = process.Build();

            //using var runningProcess = await kernelProcess.StartAsync(
            //    kernel,
            //   new KernelProcessEvent()
            //   {
            //       Id = QdrantEvents.StartProcess,
            //       Data = null
            //   });
            //#################
            //ProcessBuilder process = new("QdrantAction");
            //var uploadSourceFileToStorageStep = process.AddStepFromType<FileStorageStep>();
            //var reader = process.AddStepFromType<DocumentReaderStep>();
            //var chunker = process.AddStepFromType<TextChunkerStep>();
            ////var tagify = process.AddStepFromType<TagifyStep>();
            //var qdrant = process.AddStepFromType<QdrantStep>();

            //process
            //    .OnInputEvent(QdrantEvents.StartProcess)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(uploadSourceFileToStorageStep, nameof(FileStorageFunctions.UploadIFormFile)));

            //uploadSourceFileToStorageStep.OnEvent(FileEvents.Uploaded)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(reader, parameterName: "uri"));

            //reader.OnEvent(DocumentReaderStepOutputEvents.Readed)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(chunker, parameterName: "content"));

            //chunker.OnEvent(TextChunkerStepOutputEvents.Chunked)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(qdrant, parameterName: "chunks"));

            //KernelProcess kernelProcess = process.Build();

            //using var runningProcess = await kernelProcess.StartAsync(
            //    _kernel,
            //   new KernelProcessEvent()
            //   {
            //       Id = QdrantEvents.StartProcess,
            //       Data = new FileStorageIFormFileStepItem()
            //   });

            //ProcessBuilder process = new("ChatBot");
            //var introStep = process.AddStepFromType<TestIntroStep>();
            //var startStep = process.AddStepFromType<TestStepsOne>();
            //var stopStep = process.AddStepFromType<TestSteps2>();
            //// Define the process flow
            //Kernel kernel = new AgentRouter(_configuration).AddOpenAIChatCompletion();

            //const string SetMessage = nameof(SetMessage);

            ////process
            ////   .OnInputEvent(ChatBotEvents.StartProcess)
            ////   .SendEventTo(new ProcessFunctionTargetBuilder(introStep));

            //process
            //    .OnInputEvent(ChatBotEvents.StartProcess)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(introStep, nameof(TestIntroStep.PrintIntroMessage)));

            //introStep
            //    .OnFunctionResult(nameof(TestIntroStep.PrintIntroMessage))
            //    .SendEventTo(new ProcessFunctionTargetBuilder(introStep, nameof(SetMessage)));

             

            //introStep
            //    .OnEvent(ChatBotEvents2.UserInputReceived)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(startStep, parameterName: "userMessage"));

            ////process
            ////    .OnFunctionResult(nameof(TestIntroStep.PrintIntroMessage))
            ////    .SendEventTo(new ProcessFunctionTargetBuilder(introStep));


            ////process
            ////    .OnFunctionResult(nameof(TestIntroStep.PrintIntroMessage))
            ////    .SendEventTo(new ProcessFunctionTargetBuilder(stopStep, nameof(TestSteps2.MyExecute), "message"));

            ////process.OnFunctionResult(nameof(ChatBotEvents2.Exit)).SendEventTo(new ProcessFunctionTargetBuilder(exitStep));
            ////   .StopProcess(); 

            //KernelProcess kernelProcess = process.Build();

            //using var runningProcess = await kernelProcess.StartAsync(
            //    kernel,
            //   new KernelProcessEvent()
            //   {
            //       Id = QdrantEvents.StartProcess,
            //       Data = "Kij w dupie"
            //   });



        }

        /// <summary>
        /// The state object for the <see cref="ScriptedUserInputStep"/>
        /// </summary>
        public record UserInputState
        {
            public List<string> UserInputs { get; init; } = [];

            public int CurrentInputIndex { get; set; } = 0;
        }


        /// <summary>
        /// Asynchronously adds a file to the Qdrant database.
        /// First, the file is uploaded to blob storage with metadata including FileId.
        /// Then, the file is added to Qdrant with the same FileId in the payload data.
        /// Embeddings and metadata with generated tags, summaries, and other useful information are included.
        /// </summary>
        /// <param name="document">The document to be added.</param>
        /// <param name="conversationUuid">The UUID of the conversation associated with the document.</param>
        /// <param name="maxTokensPerLine">Maximum number of tokens per line.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the UUID of the added file.</returns>
        /// <remarks>
        /// This method uses the Semantic Kernel to chunk the text and generate tags and summaries for each chunk.
        /// The generated tags and summaries are then added to the Qdrant database along with the embeddings.
        /// </remarks>
        /// <example>
        /// <code>
        /// public async Task ExampleUsage(IFormFile document, Guid conversationUuid)
        /// {
        ///     var qdrantFileService = new QdrantService(kernel, blobStorageService, documentReaderDocx, embeddingOpenAi);
        ///     qdrantFileService.Setup("modelEmbedding", "modelEmbeddingApiKey", "qdrantUri", "qdrantApiKey", "qdrantCollectionName", Distance.Cosine, 1000, true, user);
        ///     var fileUuid = await qdrantFileService.AddAsync(document, conversationUuid);
        ///     Console.WriteLine($"File added with UUID: {fileUuid}");
        /// }
        /// </code>
        /// </example>



        public async Task AddAsync(
            string chunk,
            Dictionary<string, string> metadata, 
            Guid conversationUuid, 
            Guid fileId
            )
        {
            await _qdrantApi.CheckCollectionExists("personalagent");
            await _qdrantApi.AddEmbeddingToQdrantAsync(_qdrantCollectionName, chunk, metadata);
        }
        
        [Experimental("SKEXP0080")]  // for semantic kernel
        public async Task<Guid> AddAsync(IFormFile document, Guid conversationUuid, int maxTokensPerLine=200, int maxSummaryCharacters = 100)
        {
            //var uri = await _blobStorage.UploadToContainerAsync(Guid.NewGuid(), document, _overwrite);

            //var reader = await _documentReaderDocx.ReadAsync(uri);

            //var chunker = new SemanticKernelTextChunker();
            //chunker.Setup("text-embedding-3-small");
            //var chunks = chunker.ChunkText(maxTokensPerLine, reader);


            //var chat = _kernel.GetRequiredService<IChatCompletionService>();

            //string authorName = DocumentReaderBase.GetAuthorNameFromDocument(document);

            //await _qdrantApi.CheckCollectionExists(_qdrantCollectionName);

            //var tasks = chunks.Select(async chunk =>
            //{

            //    var tagAsString = await chat.GetChatMessageContentAsync(
            //        @$"""Generate tags for the following text in a comma-separated list format.

            //        <text>
            //        {chunk.line}
            //        </text>

            //        Output must be a comma-separated list of tags. If there are no tags, return an empty list.

            //        <output>
            //        tag1, tag2
            //        </output>

            //        """);

            //    var summary = await chat.GetChatMessageContentAsync(
            //        $@"""

            //        Summarize the following text in no more than {maxSummaryCharacters} characters for use in a vector database. 
            //        The summary must remain in the same language as the original text, focusing on semantic clarity and key ideas that 
            //        can aid similarity search. 

            //        <text> 
            //        {chunk.line}    
            //        </text>

            //        """);

            //    var tagList = tagAsString.Content.Split(", ").ToList();

            //    var uploadedBy = _userClaimsPrincipal?.FindFirstValue(ClaimTypes.Name) ?? ClaimTypes.Anonymous;

            //    await _qdrantApi.AddEmbeddingToQdrantAsync(Guid.NewGuid(), _qdrantCollectionName, chunk.line, new Dictionary<string, object>
            //    {
            //        { "Title", Path.GetFileNameWithoutExtension(document.FileName) },
            //        { "Author", authorName },
            //        { "Text", chunk.line },
            //        { "CreatedAt", DateTime.Now.ToString("o") },
            //        { "UploadedBy", uploadedBy },
            //        { "SourceFileName", document.FileName },
            //        { "ConversationId", conversationUuid },
            //        { "BlobUri", uri.ToString() },
            //        { "FileId", "" },
            //        { "MimeType", document.ContentType },
            //        { "Tags", string.Join(", ", tagList) },
            //        { "Summary", summary.Content },
            //        { "EmbeddingModel", _modelEmbedding },
            //        { "StartPosition", chunk.startPosition },
            //        { "EndPosition", chunk.endPosition },
            //        { "DataType", QdrantDataType.Document }
            //    });

            //});

            //await Task.WhenAll(tasks);

            var fileUuid = Guid.NewGuid();

            //QdrantPipelines qdrantPipelines = new QdrantPipelines();
            //await qdrantPipelines.Add(
            //    _kernel,
            //    new FileStorageIFormFileStepItem(document, true, fileUuid)
            //    );

            return fileUuid;
        }


        /// <summary>
        /// Asynchronously searches for similar vectors in a Qdrant collection.
        /// </summary>
        /// <param name="collectionName">The name of the collection to search.</param>
        /// <param name="queries">The list of query strings to embed and search for.</param>
        /// <param name="filter">Optional filter to apply to the search.</param>
        /// <param name="limit">The maximum number of results to return.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of search results.</returns>
        /// <example>
        /// <code>
        /// public async Task ExampleSearchUsage()
        /// {
        ///     var qdrantFileService = new QdrantService(kernel, blobStorageService, documentReaderDocx, embeddingOpenAi);
        ///     qdrantFileService.Setup("modelEmbedding", "modelEmbeddingApiKey", "qdrantUri", "qdrantApiKey", "qdrantCollectionName", Distance.Cosine, 1000, true, user);
        ///     var results = await qdrantFileService.SearchAsync(new List<string> { "kto złamał nogę", "query2" }, null, 5);
        ///     foreach (var result in results)
        ///     {
        ///         Console.WriteLine($"Found result with ID: {result.Id}");
        ///     }
        /// }
        /// </code>
        /// </example>
        public async Task<List<Dictionary<string, object>>> SearchAsync(List<string> queries, Dictionary<string, string> filter = null, int limit = 5)
        {
            var searchResults = await Task.WhenAll(queries.Select(query =>
                _qdrantApi.SearchAsync(_collectionName, query, filter, limit)
            ));

            var results = new List<Dictionary<string, object>>();

            foreach (var result in searchResults.SelectMany(r => r))
            {
                var payload = result.Payload.ToDictionary(
                kvp => kvp.Key,
                    kvp => kvp.Value ?? ""
                );

                var searchResult = new Dictionary<string, object>
                {
                    { "Id", result.Id.Uuid },
                    { "Payload", payload },
                    { "Score", result.Score },
                    { "Version", result.Version.ToString() }
                };

                results.Add(searchResult);
            }

            return results;
        }
    }
}

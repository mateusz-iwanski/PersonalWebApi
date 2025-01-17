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
using PersonalWebApi.Utilities.Utilities.Models;
using PersonalWebApi.Services.FileStorage;
using PersonalWebApi.Exceptions;
using System.Collections;
using PersonalWebApi.Agent;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin;
using Microsoft.SemanticKernel.Process.Runtime;
using PersonalWebApi.Services.Qdrant.Processes;
using DocumentFormat.OpenXml.Wordprocessing;
using PersonalWebApi.Services.Qdrant.Processes.Steps;

namespace PersonalWebApi.Services.Services.Qdrant
{
    public class QdrantService : IQdrantService
    {
        private readonly string _collectionName;

        private readonly IFileStorageService _blobStorage;
        private readonly IDocumentReaderDocx _documentReaderDocx;
        private readonly Kernel _kernel;
        private readonly IEmbedding _embeddingOpenAi;
        private readonly IConfiguration _configuration;

        private QdrantApi _qdrantApi { get; set; }
        private ClaimsPrincipal _userClaimsPrincipal { get; set; }
        private string _modelEmbedding { get; set; }
        private string _qdrantCollectionName { get; set; }
        private ulong _qdrantCollectionSize { get; set; }
        private bool _overwrite { get; set; }


        public QdrantService(
            Kernel kernel,
            IFileStorageService blobStorageService,
            IDocumentReaderDocx documentReaderDocx,
            IEmbedding embeddingOpenAi,
            IConfiguration configuration
            //IHttpContextAccessor httpContextAccessor
            )
        {
            _configuration = configuration;

            _blobStorage = blobStorageService;
            _documentReaderDocx = documentReaderDocx;
            _kernel = kernel;
            _embeddingOpenAi = embeddingOpenAi;

            //_userClaimsPrincipal = httpContextAccessor.HttpContext?.User ?? throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext.User));

            _collectionName = _configuration.GetSection("Qdrant:Container:Name").Value ?? throw new SettingsException("Qdrant:Container:defaultName not exists");

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
            //Kernel kernel = new AgentCollection(_configuration).AddOpenAIChatCompletion();
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
            //    .OnInputEvent(ProcessEvents.StartProcess)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(startStep, functionName: Steps.Functions.MyExecute, "i"));

            //KernelProcess kernelProcess = process.Build();

            //using var runningProcess = await kernelProcess.StartAsync(
            //    kernel,
            //   new KernelProcessEvent()
            //   {
            //       Id = ProcessEvents.StartProcess,
            //       Data = null
            //   });


            ProcessBuilder process = new("ChatBot");
            var introStep = process.AddStepFromType<TestIntroStep>();
            var startStep = process.AddStepFromType<TestStepsOne>();
            var stopStep = process.AddStepFromType<TestSteps2>();
            // Define the process flow
            Kernel kernel = new AgentCollection(_configuration).AddOpenAIChatCompletion();

            const string SetMessage = nameof(SetMessage);

            //process
            //   .OnInputEvent(ChatBotEvents.StartProcess)
            //   .SendEventTo(new ProcessFunctionTargetBuilder(introStep));

            process
                .OnInputEvent(ChatBotEvents.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(introStep, nameof(TestIntroStep.PrintIntroMessage)));

            introStep
                .OnFunctionResult(nameof(TestIntroStep.PrintIntroMessage))
                .SendEventTo(new ProcessFunctionTargetBuilder(introStep, nameof(SetMessage)));

             

            introStep
                .OnEvent(ChatBotEvents2.UserInputReceived)
                .SendEventTo(new ProcessFunctionTargetBuilder(startStep, parameterName: "userMessage"));

            //process
            //    .OnFunctionResult(nameof(TestIntroStep.PrintIntroMessage))
            //    .SendEventTo(new ProcessFunctionTargetBuilder(introStep));


            //process
            //    .OnFunctionResult(nameof(TestIntroStep.PrintIntroMessage))
            //    .SendEventTo(new ProcessFunctionTargetBuilder(stopStep, nameof(TestSteps2.MyExecute), "message"));

            //process.OnFunctionResult(nameof(ChatBotEvents2.Exit)).SendEventTo(new ProcessFunctionTargetBuilder(exitStep));
            //   .StopProcess(); 

            KernelProcess kernelProcess = process.Build();

            using var runningProcess = await kernelProcess.StartAsync(
                kernel,
               new KernelProcessEvent()
               {
                   Id = ProcessEvents.StartProcess,
                   Data = "Kij w dupie"
               });



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
        [Experimental("SKEXP0050")]  // for SemanticKernelTextChunker
        public async Task<Guid> AddAsync(IFormFile document, Guid conversationUuid, int maxTokensPerLine=200, int maxSummaryCharacters = 100)
        {
            await test();

            var fileUuid = Guid.NewGuid();

            //_blobStorage.SetContainer("qdrant");

            Kernel kernel = new AgentCollection(_configuration).AddOpenAIChatCompletion();
            ProcessBuilder process = new("ChatBot");
            var uploadSourceFileToStorageStep = process.AddStepFromType<FileStorageStep>();
            //var doSomeWorkStep = process.AddStepFromType<NextSteps>();
            // Define the process flow
            process 
                .OnInputEvent(ProcessEvents.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(uploadSourceFileToStorageStep));

            KernelProcess kernelProcess = process.Build();
            using var runningProcess = await kernelProcess.StartAsync(
                kernel,
               new KernelProcessEvent()
               {
                   Id = ProcessEvents.StartProcess,
                   Data = null
               });




            var uri = await _blobStorage.UploadToContainerAsync(document, _overwrite, fileId: fileUuid.ToString());
            
            var reader = await _documentReaderDocx.ReadAsync(uri);

            var chunker = new SemanticKernelTextChunker(_modelEmbedding);
            var chunks = chunker.ChunkText(conversationUuid.ToString(), maxTokensPerLine, reader);


            var chat = _kernel.GetRequiredService<IChatCompletionService>();

            string authorName = DocumentReaderBase.GetAuthorNameFromDocument(document);

            await _qdrantApi.CheckCollectionExists(_qdrantCollectionName);

            var tasks = chunks.Select(async chunk =>
            {

                var tagAsString = await chat.GetChatMessageContentAsync(
                    @$"""Generate tags for the following text in a comma-separated list format.

                    <text>
                    {chunk.line}
                    </text>

                    Output must be a comma-separated list of tags. If there are no tags, return an empty list.

                    <output>
                    tag1, tag2
                    </output>

                    """);

                var summary = await chat.GetChatMessageContentAsync(
                    $@"""

                    Summarize the following text in no more than {maxSummaryCharacters} characters for use in a vector database. 
                    The summary must remain in the same language as the original text, focusing on semantic clarity and key ideas that 
                    can aid similarity search. 
                        
                    <text> 
                    {chunk.line}    
                    </text>

                    """);

                var tagList = tagAsString.Content.Split(", ").ToList();

                var uploadedBy = _userClaimsPrincipal?.FindFirstValue(ClaimTypes.Name) ?? ClaimTypes.Anonymous;

                await _qdrantApi.AddEmbeddingToQdrantAsync(Guid.NewGuid(), _qdrantCollectionName, chunk.line, new Dictionary<string, object>
                {
                    { "Title", Path.GetFileNameWithoutExtension(document.FileName) },
                    { "Author", authorName },
                    { "Text", chunk.line },
                    { "CreatedAt", DateTime.Now.ToString("o") },
                    { "UploadedBy", uploadedBy },
                    { "SourceFileName", document.FileName },
                    { "ConversationId", conversationUuid },
                    { "BlobUri", uri.ToString() },
                    { "FileId", fileUuid.ToString() },
                    { "MimeType", document.ContentType },
                    { "Tags", string.Join(", ", tagList) },
                    { "Summary", summary.Content },
                    { "EmbeddingModel", _modelEmbedding },
                    { "StartPosition", chunk.startPosition },
                    { "EndPosition", chunk.endPosition },
                    { "DataType", QdrantDataType.Document }
                });

            });

            await Task.WhenAll(tasks);

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
        public async Task<List<QdrantFileSearchResultType>> SearchAsync(List<string> queries, Dictionary<string, string> filter = null, int limit = 5)
        {
            var searchResults = await Task.WhenAll(queries.Select(query =>
                _qdrantApi.SearchAsync(_collectionName, query, filter, limit)
            ));

            var results = new List<QdrantFileSearchResultType>();

            foreach (var result in searchResults.SelectMany(r => r))
            {
                var searchResult = new QdrantFileSearchResultType
                {
                    Id = result.Id.Uuid,
                    Payload = new QdrantFilePayloadType
                    {
                        BlobUri = result.Payload["BlobUri"].StringValue,
                        Text = result.Payload["Text"].StringValue,
                        ConversationId = result.Payload["ConversationId"].StringValue,
                        EndPosition = result.Payload["EndPosition"].StringValue,
                        UploadedBy = result.Payload["UploadedBy"].StringValue,
                        EmbeddingModel = result.Payload["EmbeddingModel"].StringValue,
                        StartPosition = result.Payload["StartPosition"].StringValue,
                        Author = result.Payload["Author"].StringValue,
                        FileName = result.Payload["SourceFileName"].StringValue,
                        CreatedAt = result.Payload["CreatedAt"].StringValue,
                        FileId = result.Payload["FileId"].StringValue,
                        Summary = result.Payload["Summary"].StringValue,
                        Tags = result.Payload["Tags"].StringValue,
                        Title = result.Payload["Title"].StringValue,
                        MimeType = result.Payload["MimeType"].StringValue,
                        DataType = Enum.Parse<QdrantDataType>(result.Payload["DataType"].StringValue)
                    },
                    Score = result.Score,
                    Version = result.Version.ToString()
                };

                results.Add(searchResult);
            }

            return results;
        }
    }
}

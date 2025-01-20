using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using PersonalWebApi.Agent.Memory.Observability;
using PersonalWebApi.Agent.SemanticKernel.Observability;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Processes.Document.Events;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.Document.Steps;
using PersonalWebApi.Processes.FileStorage.Events;
using PersonalWebApi.Processes.FileStorage.Processes;
using PersonalWebApi.Processes.FileStorage.Steps;
using PersonalWebApi.Processes.Metadata.Events;
using PersonalWebApi.Processes.Metadata.Steps;
using PersonalWebApi.Processes.NoSQLDB.Steps;
using PersonalWebApi.Processes.Qdrant.Events;
using PersonalWebApi.Processes.Qdrant.Processes;
using PersonalWebApi.Processes.Qdrant.Steps;
using PersonalWebApi.Services.Agent;
using PersonalWebApi.Services.FileStorage;
using PersonalWebApi.Services.NoSQLDB;
using PersonalWebApi.Services.Qdrant.Processes.Steps;
using PersonalWebApi.Services.Services.Agent;
using PersonalWebApi.Services.Services.History;
using PersonalWebApi.Services.Services.Qdrant;
using PersonalWebApi.Services.WebScrapper;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using PersonalWebApi.Utilities.WebScrapper;
using PersonalWebApi.Utilities.WebScrappers;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.Qdrant.Pipelines
{
    public class QdrantPipelines
    {
        [Experimental("SKEXP0080")]
        public async Task Add(Kernel kernel, DocumentStepDto documentStepDto)
        {
            // add from kernelrouter

            ProcessBuilder process = new("QdrantPipielineAdd");

            // file storage steps
            var fileStorage = process.AddStepFromType<FileStorageStep>();
            var logFileAction = process.AddStepFromType<LogFileActionStep>();

            // qdrant steps
            var reader = process.AddStepFromType<DocumentReaderStep>();
            var chunker = process.AddStepFromType<TextChunkerStep>();
            var fileMetadataComposer = process.AddStepFromType<FileMetadataComposer>();
            var qdrantAdd = process.AddStepFromType<QdrantStep>();
            var noSqlDb = process.AddStepFromType<DocumentDbCreateStep>();
            var tagify = process.AddStepFromType<TagifyStep>();
            var tagifyChunk = process.AddStepFromType<TagifyChunksStep>();
            var summarizeDocument = process.AddStepFromType<SummarizeStep>();
            var setDocType = process.AddStepFromType<SpecifyDocumentTypeStep>();
            var setDocLanguage = process.AddStepFromType<SpecifyDocumentLanguageStep>();

            process.OnInputEvent(QdrantEvents.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(fileStorage, functionName: FileStorageFunctions.UploadIFormFile));
            
            fileStorage.OnEvent(FileEvents.Uploaded)                
                .SendEventTo(new ProcessFunctionTargetBuilder(logFileAction, functionName: LogFileActionStepFunctions.SaveActionLog, parameterName: "documentStepDto"))
                .SendEventTo(new ProcessFunctionTargetBuilder(reader, functionName: DocumentReaderStepFunctions.ReadUri, parameterName: "documentStepDto"))
                .SendEventTo(new ProcessFunctionTargetBuilder(fileMetadataComposer, functionName: FileMetadataComposerFunction.Collect, parameterName: "documentStepDto"));

            reader.OnEvent(DocumentEvents.Readed)
                .SendEventTo(new ProcessFunctionTargetBuilder(chunker, functionName: TextChunkerStepFunctions.ChunkText, parameterName: "documentStepDto"))
                .SendEventTo(new ProcessFunctionTargetBuilder(tagify, functionName: TagifyStepFunctions.GenerateTags, parameterName: "documentStepDto"))
                .SendEventTo(new ProcessFunctionTargetBuilder(setDocType, functionName: DocumentInfoStepFunctions.SpecifyDocumentType, parameterName: "documentStepDto"))
                .SendEventTo(new ProcessFunctionTargetBuilder(setDocLanguage, functionName: DocumentLanguageStepFunctions.SpecifyDocumentLanguage, parameterName: "documentStepDto"));
            //
            chunker.OnEvent(DocumentEvents.Chunked)
                .SendEventTo(new ProcessFunctionTargetBuilder(tagifyChunk, functionName: TagifyStepFunctions.GenerateChunksTags, parameterName: "documentStepDto"))
                .SendEventTo(new ProcessFunctionTargetBuilder(summarizeDocument, functionName: SummarizeStepFunctions.SummarizeText, parameterName: "documentStepDto"));
            
            tagifyChunk.OnEvent(QdrantEvents.ChunksTagified)
                .SendEventTo(new ProcessFunctionTargetBuilder(qdrantAdd, functionName: QdrantStepFunctions.AddEmbedding, parameterName: "documentStepDto"));

            qdrantAdd.OnEvent(QdrantEvents.EmbeddingAdded)
                .SendEventTo(new ProcessFunctionTargetBuilder(noSqlDb, functionName: DocumentDbFunctions.Save, parameterName: "documentStepDto"));
                


            // TODO: cos wymyslec, mozliwe ze sam framework nie dziala prawidlowo
            // nie dziala poniżej, nie wiem czemu nie moge odczytac OnEvent w tym miejscu z UploadFileProcess.CreateProcess
            // w UploadFileProcess.CreateProcess dziala i eventy sie komunikuja
            // w tym miejscu nie reaguje na zaden onevent wykonany w UploadFileProcess.CreateProcess
            // ponizej wszystkie mozliwe wywolania onevent, nic nie dziala
            //var documentreadersterptest = process.AddStepFromType<DocumentReaderStep>();

            //process.OnInputEvent(QdrantEvents.StartProcess).SendEventTo(fileStorage.WhereInputEventIs(FileEvents.StartProcess));

            //fileStorage.OnEvent(FileEvents.Uploaded).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));
            //fileStorage.OnEvent(FileEvents.ActionLogSaved).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));
            //fileStorage.OnEvent(FileEvents.Uploaded).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));

            //process.OnEvent(FileEvents.Uploaded).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));
            //process.OnEvent(FileEvents.ActionLogSaved).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));
            //process.OnEvent(FileEvents.Uploaded).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));

            //process.OnInputEvent(QdrantEvents.StartProcess).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));

            //process.OnEvent(FileEvents.Uploaded).SendEventTo(qdrantAdd.WhereInputEventIs(QdrantEvents.StartProcess));

            var kernelProcess = process.Build();

            using var runningProcess = await kernelProcess.StartAsync(
                kernel,
                new KernelProcessEvent()
                {
                    Id = QdrantEvents.StartProcess,
                    Data = documentStepDto
                });

        }

        [Experimental("SKEXP0080")]
        public static Kernel PrepareKelnerForPipeline(IConfiguration configuration)
        {
            var kernelBuilder = Kernel.CreateBuilder();

            var apiKey = configuration.GetSection("OpenAI:Access:ApiKey").Value ??
                throw new SettingsException("OpenAi ApiKey not exists in appsettings");

            var defaultModelId = configuration.GetSection("OpenAI:DefaultModelId").Value ??
                throw new SettingsException("OpenAi DefaultModelId not exists in appsettings");

            kernelBuilder.AddOpenAIChatCompletion(
                defaultModelId,
                apiKey
            );

            kernelBuilder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
            });

            kernelBuilder.Services.AddSingleton<IConfiguration>(configuration);
            kernelBuilder.Services.AddHttpContextAccessor();
            kernelBuilder.Services.AddScoped<IEmbedding, EmbeddingOpenAi>();
            kernelBuilder.Services.AddScoped<IQdrantService, QdrantService>();
            kernelBuilder.Services.AddScoped<IPersistentChatHistoryService, PersistentChatHistoryService>();
            kernelBuilder.Services.AddScoped<INoSqlDbService, AzureCosmosDbService>();
            kernelBuilder.Services.AddScoped<IAssistantHistoryManager, AssistantHistoryManager>();
            kernelBuilder.Services.AddScoped<IPromptRenderFilter, RenderedPromptFilterHandler>();
            kernelBuilder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
            kernelBuilder.Services.AddScoped<IDocumentReaderDocx, DocumentReaderDocx>();
            kernelBuilder.Services.AddScoped<IWebScrapperClient, Firecrawl>();
            kernelBuilder.Services.AddScoped<IWebScrapperService, WebScrapperService>();
            kernelBuilder.Services.AddScoped<ITextChunker, SemanticKernelTextChunker>();

            IKernelMemory memory = new KernelMemoryBuilder()
                .WithOpenAIDefaults(apiKey)
                .Build<MemoryServerless>();

            kernelBuilder.Services.AddScoped<IKernelMemory>(_ => memory);
            kernelBuilder.Services.AddScoped<KernelMemoryWrapper>(provider =>
            {
                var innerKernelMemory = provider.GetRequiredService<IKernelMemory>();
                var assistantHistoryManager = provider.GetRequiredService<IAssistantHistoryManager>();
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                var blobStorageConnector = provider.GetRequiredService<IFileStorageService>();

                return new KernelMemoryWrapper(innerKernelMemory, assistantHistoryManager, httpContextAccessor, blobStorageConnector);
            });

            return kernelBuilder.Build();
        }
    }
}

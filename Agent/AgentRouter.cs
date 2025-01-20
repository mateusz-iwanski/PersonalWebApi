using Amazon.Runtime.Internal.Transform;
using Azure.Identity;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.SemanticKernel;
using PersonalWebApi.Agent.SemanticKernel.Observability;
using PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Processes;
using PersonalWebApi.Processes.Document.Steps;
using PersonalWebApi.Services.FileStorage;
using PersonalWebApi.Services.NoSQLDB;
using PersonalWebApi.Services.Services.Agent;
using PersonalWebApi.Services.Services.History;
using PersonalWebApi.Services.Services.Qdrant;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;

namespace PersonalWebApi.Agent
{
    public class AgentRouter
    {
        private IKernelBuilder _kernelBuilder { get; set; }
        private readonly IConfiguration _configuration;
        
        private record AssistantConfiguration(string key, string defaultModelId, string? endpoint, string? deploymentName);
        private record StepAgentMappingConfiguration(string Type, string ModelId);

        public AgentRouter(IConfiguration configuration)
        {
            _kernelBuilder = Kernel.CreateBuilder();
            
            _kernelBuilder.Services.AddSingleton<IConfiguration>(configuration);
            _kernelBuilder.Services.AddHttpContextAccessor();
            // register services

            _kernelBuilder.Services.AddScoped<IEmbedding, EmbeddingOpenAi>();
            _kernelBuilder.Services.AddScoped<IQdrantService, QdrantService>();
            _kernelBuilder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
            _kernelBuilder.Services.AddScoped<IDocumentReaderDocx, DocumentReaderDocx>();
            _kernelBuilder.Services.AddScoped<INoSqlDbService, AzureCosmosDbService>();
            _kernelBuilder.Services.AddScoped<IAssistantHistoryManager, AssistantHistoryManager>();

            // Add the RenderedPromptFilterHandler as a service
            _kernelBuilder.Services.AddScoped<IPromptRenderFilter, RenderedPromptFilterHandler>();

            _configuration = configuration;
        }

        public Kernel AddOpenAIChatCompletion(string? chatModelId = null) => _kernelBuilder.AddOpenAIChatCompletion(chatModelId ?? OpenAiSettings.defaultModelId, OpenAiSettings.key).Build();

        #region assistant settings

        #region assistant assigned to step

        //var agentRouter = new AgentRouter(context.Configuration);
        //var stepKernel = agentRouter.GetStepKernel<SummarizeStepFunctions>(SummarizeStepFunctions.SummarizeText);
        public Kernel GetStepKernel<T>(string stepFunctionName) where T : IProcessStepFuntion, new()
        {
            var stepFunction = new T();
            if (!stepFunction.GetFunctionNames().Contains(stepFunctionName))
            {
                throw new ArgumentException($"Unsupported step function: {stepFunctionName}", nameof(stepFunctionName));
            }

            return stepFunctionName switch
            {
                SummarizeStepFunctions.SummarizeText => ListKernel(StepAgentMappingSettings(SummarizeStepFunctions.SummarizeText).ModelId),
                SummarizeStepFunctions.AnotherFunction => AddOpenAIChatCompletion(), // Add appropriate handling for other functions
                _ => throw new ArgumentException($"Unsupported step function: {stepFunctionName}", nameof(stepFunctionName)),
            };
        }

        public Kernel ListKernel(string model)
        {
            return model.ToLower() switch
            {
                "gpt-4o" => _kernelBuilder.AddOpenAIChatCompletion("gpt-4o", OpenAiSettings.defaultModelId, OpenAiSettings.key).Build(),
                "gpt-4o-mini" => _kernelBuilder.AddOpenAIChatCompletion("gpt-4o-mini", OpenAiSettings.defaultModelId, OpenAiSettings.key).Build()
                "gpt-35-turbo" => _kernelBuilder.AddOpenAIChatCompletion("gpt-35-turbo", OpenAiSettings.defaultModelId, OpenAiSettings.key).Build()
            };
        }


        private AssistantConfiguration OpenAiSettings 
        { 
            get => new AssistantConfiguration(
                _configuration.GetSection("OpenAI:Access:ApiKey").Value ?? throw new SettingsException("OpenAI:Access:ApiKey not exists"),
                _configuration.GetSection("OpenAI:DefaultModelId").Value ?? throw new SettingsException("OpenAI:DefaultModelId not exists"),
                null,
                null
            );
        }

        private StepAgentMappingConfiguration StepAgentMappingSettings(string stepFunctionName){
            return stepFunctionName switch
            {
                "SummarizeText" =>
                    new StepAgentMappingConfiguration(
                       _configuration.GetSection("StepAgentMapping:SummarizeText:Type").Value ?? throw new SettingsException("StepAgentMapping:Type not exists"),
                       _configuration.GetSection("StepAgentMapping:SummarizeText:ModelId").Value ?? throw new SettingsException("StepAgentMapping:ModelId not exists")
                    )
            };
        }

        #endregion
    }
}

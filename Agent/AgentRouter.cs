using Azure.Identity;
using Microsoft.SemanticKernel;
using PersonalWebApi.Agent.SemanticKernel.Observability;
using PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin;
using PersonalWebApi.Exceptions;
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

        private AssistantConfiguration OpenAiSettings { get => new AssistantConfiguration(
            _configuration.GetSection("OpenAI:Access:ApiKey").Value ?? throw new SettingsException("OpenAI:Access:ApiKey not exists"),
            _configuration.GetSection("OpenAI:DefaultModelId").Value ?? throw new SettingsException("OpenAI:DefaultModelId not exists"),
            null,
            null
            );
        }

        #endregion
    }
}

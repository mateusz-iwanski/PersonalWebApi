using Azure.Identity;
using Microsoft.SemanticKernel;
using PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin;
using PersonalWebApi.Exceptions;

namespace PersonalWebApi.Agent
{
    public class AgentCollection
    {
        private IKernelBuilder _kernelBuilder { get; set; }
        private readonly IConfiguration _configuration;
        
        private record AssistantConfiguration(string key, string defaultModelId, string? endpoint, string? deploymentName); 

        public AgentCollection(IConfiguration configuration)
        {
            _kernelBuilder = Kernel.CreateBuilder();
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

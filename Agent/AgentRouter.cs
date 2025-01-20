using Microsoft.SemanticKernel;
using PersonalWebApi.Processes;
using PersonalWebApi.Processes.Document.Steps;
using Microsoft.Extensions.Configuration;
using PersonalWebApi.Exceptions;
using System.Diagnostics.CodeAnalysis;
using PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin;

namespace PersonalWebApi.Agent
{
    public class AgentRouter
    {
        private readonly IConfiguration _configuration;

        public record StepAgentMappingConfiguration(string Type, string ModelId);

        public AgentRouter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the kernel for the specified step function name.
        /// </summary>
        /// <param name="stepFunctionName">The name of the step function.</param>
        /// <returns>The kernel for the specified step function.</returns>
        /// <exception cref="ArgumentException">Thrown when the step function name is not supported.</exception>
        public Kernel GetStepKernel(string stepFunctionName)
        {
            var config = GetConfiguration(stepFunctionName);

            switch (config.Type)
            {
                case "OpenAi":
                    return AddOpenAIChatCompletion(config.ModelId);
                // case "AnotherAgentType":
                //     return AddAnotherAgent(config.ModelId);
                default:
                    throw new ArgumentException($"Unsupported agent type: {config.Type}", nameof(config.Type));
            }
        }

        /// <summary>
        /// Gets the configuration for the specified step function name.
        /// </summary>
        /// <param name="stepFunctionName">The name of the step function.</param>
        /// <returns>The configuration for the specified step function.</returns>
        /// <exception cref="SettingsException">Thrown when the configuration is not found in the settings.</exception>
        public StepAgentMappingConfiguration GetConfiguration(string stepFunctionName)
        {
            StepAgentMappingConfiguration config = new(
                _configuration.GetSection($"StepAgentMappings:{stepFunctionName}:Type").Value ??
                    throw new SettingsException($"StepAgentMappings:{stepFunctionName}:Type not exists in appsetings"),
                _configuration.GetSection($"StepAgentMappings:{stepFunctionName}:ModelId").Value ??
                    throw new SettingsException($"StepAgentMappings:{stepFunctionName}:Type not exists in appsetings")
                );
            return config;
        }

        /// <summary>
        /// Every kernel should have some basic plugins.
        /// </summary>
        /// <param name="kernelBuilder"></param>
        public void AddBasicPlugins(Kernel kernel)
        {
            //kernel.Plugins.AddFromType<TextUtilsPlugin>();
        }

        private Kernel AddOpenAIChatCompletion(string modelId)
        {
            var apiKey = _configuration["OpenAI:Access:ApiKey"] ?? throw new SettingsException("OpenAI:Access:ApiKey not exists");
            var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, apiKey).Build();
            AddBasicPlugins(kernelBuilder);
            
            return kernelBuilder;
        }

        private Kernel AddAnotherAgent(string modelId)
        {
            // Implement the logic to add another type of agent
            throw new NotImplementedException("AddAnotherAgent is not implemented yet.");
        }
    }
}

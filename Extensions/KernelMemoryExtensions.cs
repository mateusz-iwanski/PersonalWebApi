using Elastic.Transport;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DataFormats.AzureAIDocIntel;
using Microsoft.KernelMemory.MemoryStorage;
using PersonalWebApi.Agent;
using PersonalWebApi.Agent.MicrosoftKernelMemory;
using PersonalWebApi.Exceptions;
using System.Reflection.PortableExecutable;

namespace PersonalWebApi.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IKernelMemory"/> and service registration.
    /// </summary>
    internal static class KernelMemoryExtensions
    {
        /// <summary>
        /// Inject <see cref="IKernelMemory"/>.
        /// </summary>
        public static WebApplicationBuilder AddKernelMemoryServices(this WebApplicationBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();

            var apiKey = builder.Configuration.GetSection("OpenAI:Access:ApiKey").Value ??
                throw new SettingsException("OpenAi ApiKey not exists in appsettings");

            // Register IHttpContextAccessor early
            builder.Services.AddHttpContextAccessor();

            IKernelMemory memory = new KernelMemoryBuilder()
                .WithOpenAIDefaults(apiKey)
                .Build<MemoryServerless>();

            builder.Services.AddScoped<IAssistantHistoryManager, AssistantHistoryManager>();

            builder.Services.AddScoped<IKernelMemory>(_ => memory);

            builder.Services.AddScoped<MicrosoftKernelMemoryWrapper>(provider =>
            {
                var innerKernelMemory = provider.GetRequiredService<IKernelMemory>();
                var assistantHistoryManager = provider.GetRequiredService<IAssistantHistoryManager>();

                return new MicrosoftKernelMemoryWrapper(innerKernelMemory, assistantHistoryManager);
            });

            return builder;
        }

    }
}

using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DataFormats.AzureAIDocIntel;
using PersonalWebApi.Extensions.ExtensionsSettings;
using System.Reflection.PortableExecutable;

namespace PersonalWebApi.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IKernelMemory"/> and service registration.
    /// </summary>
    internal static class KernelMemoryClientExtensions
    {
        /// <summary>
        /// Inject <see cref="IKernelMemory"/>.
        /// </summary>
        public static WebApplicationBuilder AddKernelMemoryServices(this WebApplicationBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var semanticKernelOptions = serviceProvider.GetRequiredService<IOptions<SemanticKernelOptions>>().Value;

            IKernelMemory memory = new KernelMemoryBuilder()
                .WithOpenAIDefaults(semanticKernelOptions.Access.OpenAi.ApiKey)
                .Build<MemoryServerless>();

            builder.Services.AddSingleton(memory);

            return builder;
        }
    }
}

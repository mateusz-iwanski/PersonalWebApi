﻿using Microsoft.SemanticKernel;
using PersonalWebApi.Exceptions;

namespace PersonalWebApi.Extensions
{
    internal static class SemanticKernelExtensions
    {
        /// <summary>
        /// Delegate to register functions with a Semantic Kernel
        /// </summary>
        public delegate Task RegisterFunctionsWithKernel(IServiceProvider sp, Kernel kernel);

        /// <summary>
        /// Delegate for any complimentary setup of the kernel, i.e., registering custom plugins, etc.
        /// See webapi/README.md#AddAsync-Custom-Setup-to-Chat-Copilot's-Kernel for more details.
        /// </summary>
        public delegate Task KernelSetupHook(IServiceProvider sp, Kernel kernel);

        /// <summary>
        /// AddAsync Semantic Kernel services
        /// </summary>
        public static WebApplicationBuilder AddSemanticKernelServices(this WebApplicationBuilder builder)
        {

            var apiKey = builder.Configuration.GetSection("OpenAI:Access:ApiKey").Value ??
                throw new SettingsException("OpenAi ApiKey not exists in appsettings");

            var defaultModelId = builder.Configuration.GetSection("OpenAI:DefaultModelId").Value ??
                throw new SettingsException("OpenAi DefaultModelId not exists in appsettings");


            builder.Services.AddScoped<Kernel>(sp =>
            {
                var kernelBuilder = Kernel.CreateBuilder();

                kernelBuilder.AddOpenAIChatCompletion(
                    defaultModelId,
                    apiKey
                );

                return kernelBuilder.Build();
            });

            // Semantic Kernel
            //builder.Services.AddScoped<Kernel>(
            //    sp =>
            //    {
            //        var provider = sp.GetRequiredService<SemanticKernelProvider>();
            //        var kernel = provider.GetCompletionKernel();

            //        sp.GetRequiredService<RegisterFunctionsWithKernel>()(sp, kernel);

            //        // If KernelSetupHook is not null, invoke custom kernel setup.
            //        sp.GetService<KernelSetupHook>()?.Invoke(sp, kernel);
            //        return kernel;
            //    });

            // Azure Content Safety
            //builder.Services.AddContentSafety();

            // Register plugins
            //builder.Services.AddScoped<RegisterFunctionsWithKernel>(sp => RegisterChatCopilotFunctionsAsync);

            // AddAsync any additional setup needed for the kernel.
            // Uncomment the following line and pass in a custom hook for any complimentary setup of the kernel.
            // builder.Services.AddKernelSetupHook(customHook);

            return builder;
        }

        /// <summary>
        /// AddAsync embedding model
        /// </summary>
        //public static WebApplicationBuilder AddBotConfig(this WebApplicationBuilder builder)
        //{
        //    builder.Services.AddScoped(sp => sp.WithBotConfig(builder.Configuration));

        //    return builder;
        //}

        /// <summary>
        /// Register custom hook for any complimentary setup of the kernel.
        /// </summary>
        /// <param name="hook">The delegate to perform any additional setup of the kernel.</param>
        public static IServiceCollection AddKernelSetupHook(this IServiceCollection services, KernelSetupHook hook)
        {
            // AddAsync the hook to the service collection
            services.AddScoped<KernelSetupHook>(sp => hook);
            return services;
        }

        /// <summary>
        /// Register the chat plugin with the kernel.
        /// </summary>
        //public static Kernel RegisterChatPlugin(this Kernel kernel, IServiceProvider sp)
        //{
        //    // Chat plugin
        //    kernel.ImportPluginFromObject(
        //        new ChatPlugin(
        //            kernel,
        //            memoryClient: sp.GetRequiredService<IKernelMemory>(),
        //            chatMessageRepository: sp.GetRequiredService<ChatMessageRepository>(),
        //            chatSessionRepository: sp.GetRequiredService<ChatSessionRepository>(),
        //            messageRelayHubContext: sp.GetRequiredService<IHubContext<MessageRelayHub>>(),
        //            promptOptions: sp.GetRequiredService<IOptions<PromptsOptions>>(),
        //            documentImportOptions: sp.GetRequiredService<IOptions<DocumentMemoryOptions>>(),
        //            contentSafety: sp.GetService<AzureContentSafety>(),
        //            logger: sp.GetRequiredService<ILogger<ChatPlugin>>()),
        //        nameof(ChatPlugin));

        //    return kernel;
        //}

        //private static void InitializeKernelProvider(this WebApplicationBuilder builder)
        //{
        //    builder.Services.AddSingleton(sp => new SemanticKernelProvider(sp, builder.Configuration, sp.GetRequiredService<IHttpClientFactory>()));
        //}

        /// <summary>
        /// Register functions with the main kernel responsible for handling Chat Copilot requests.
        /// </summary>
        //private static TaskHistory RegisterChatCopilotFunctionsAsync(IServiceProvider sp, Kernel kernel)
        //{
        //    // Chat Copilot functions
        //    kernel.RegisterChatPlugin(sp);

        //    // Time plugin
        //    kernel.ImportPluginFromObject(new TimePlugin(), nameof(TimePlugin));

        //    return TaskHistory.CompletedTask;
        //}

        /// <summary>
        /// Register plugins with a given kernel.
        /// </summary>
        //private static TaskHistory RegisterPluginsAsync(IServiceProvider sp, Kernel kernel)
        //{
        //    var logger = kernel.LoggerFactory.CreateLogger(nameof(Kernel));

        //    // Semantic plugins
        //    ServiceOptions options = sp.GetRequiredService<IOptions<ServiceOptions>>().Value;
        //    if (!string.IsNullOrWhiteSpace(options.SemanticPluginsDirectory))
        //    {
        //        foreach (string subDir in Directory.GetDirectories(options.SemanticPluginsDirectory))
        //        {
        //            try
        //            {
        //                kernel.ImportPluginFromPromptDirectory(options.SemanticPluginsDirectory, Path.GetFileName(subDir)!);
        //            }
        //            catch (KernelException ex)
        //            {
        //                logger.LogError("Could not load plugin from {Directory}: {MessageHistory}", subDir, ex.MessageHistory);
        //            }
        //        }
        //    }

        //    // Native plugins
        //    if (!string.IsNullOrWhiteSpace(options.NativePluginsDirectory))
        //    {
        //        // Loop through all the files in the directory that have the .cs extension
        //        var pluginFiles = Directory.GetFiles(options.NativePluginsDirectory, "*.cs");
        //        foreach (var file in pluginFiles)
        //        {
        //            // Parse the name of the class from the file name (assuming it matches)
        //            var className = Path.GetFileNameWithoutExtension(file);

        //            // Get the type of the class from the current assembly
        //            var assembly = Assembly.GetExecutingAssembly();
        //            var classType = assembly.GetTypes().FirstOrDefault(t => t.Name.Contains(className, StringComparison.CurrentCultureIgnoreCase));

        //            // If the type is found, create an instance of the class using the default constructor
        //            if (classType != null)
        //            {
        //                try
        //                {
        //                    var plugin = Activator.CreateInstance(classType);
        //                    kernel.ImportPluginFromObject(plugin!, classType.Name!);
        //                }
        //                catch (KernelException ex)
        //                {
        //                    logger.LogError("Could not load plugin from file {File}: {Details}", file, ex.MessageHistory);
        //                }
        //            }
        //            else
        //            {
        //                logger.LogError("Class type not found. Make sure the class type matches exactly with the file name {FileName}", className);
        //            }
        //        }
        //    }

        //    return TaskHistory.CompletedTask;
        //}

        /// <summary>
        /// Adds Azure Content Safety
        /// </summary>
        //internal static void AddContentSafety(this IServiceCollection services)
        //{
        //    IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        //    var options = configuration.GetSection(ContentSafetyOptions.PropertyName).Get<ContentSafetyOptions>() ?? new ContentSafetyOptions { Enabled = false };
        //    services.AddSingleton<IContentSafetyService>(sp => new AzureContentSafety(options.Endpoint, options.Key));
        //}

        /// <summary>
        /// Get the embedding model from the configuration.
        /// </summary>
        //private static ChatArchiveEmbeddingConfig WithBotConfig(this IServiceProvider provider, IConfiguration configuration)
        //{
        //    var memoryOptions = provider.GetRequiredService<IOptions<KernelMemoryConfig>>().Value;

        //    switch (memoryOptions.Retrieval.EmbeddingGeneratorType)
        //    {
        //        case string x when x.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase):
        //        case string y when y.Equals("AzureOpenAIEmbedding", StringComparison.OrdinalIgnoreCase):
        //            var azureAIOptions = memoryOptions.GetServiceConfig<AzureOpenAIConfig>(configuration, "AzureOpenAIEmbedding");
        //            return
        //                new ChatArchiveEmbeddingConfig
        //                {
        //                    AIService = ChatArchiveEmbeddingConfig.AIServiceType.AzureOpenAIEmbedding,
        //                    DeploymentOrModelId = azureAIOptions.Deployment,
        //                };

        //        case string x when x.Equals("OpenAI", StringComparison.OrdinalIgnoreCase):
        //            var openAIOptions = memoryOptions.GetServiceConfig<OpenAIConfig>(configuration, "OpenAI");
        //            return
        //                new ChatArchiveEmbeddingConfig
        //                {
        //                    AIService = ChatArchiveEmbeddingConfig.AIServiceType.OpenAI,
        //                    DeploymentOrModelId = openAIOptions.EmbeddingModel,
        //                };

        //        default:
        //            throw new ArgumentException($"Invalid {nameof(memoryOptions.Retrieval.EmbeddingGeneratorType)} value in 'KernelMemory' settings.");
        //    }
        //}
    }
}

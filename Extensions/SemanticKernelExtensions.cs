using Microsoft.SemanticKernel;
using OpenTelemetry.Resources;
using OpenTelemetry;
using PersonalWebApi.Exceptions;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using OpenTelemetry.Trace;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using PersonalWebApi.Agent.SemanticKernel.Observability;
using PersonalWebApi.Services.Services.History;
using PersonalWebApi.Services.NoSQLDB;

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

                // Use the correct method to add logging
                kernelBuilder.Services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                });

                kernelBuilder.Services.AddSingleton<IConfiguration>(builder.Configuration);

                // Register IHttpContextAccessor early
                kernelBuilder.Services.AddHttpContextAccessor();



                //kernelBuilder.Services.AddSingleton<IConfiguration>(builder.Configuration);

                kernelBuilder.Services.AddScoped<INoSqlDbService, AzureCosmosDbService>();

                kernelBuilder.Services.AddScoped<IAssistantHistoryManager, AssistantHistoryManager>();

                // Add the RenderedPromptFilterHandler as a service
                kernelBuilder.Services.AddScoped<IPromptRenderFilter, RenderedPromptFilterHandler>();

                return kernelBuilder.Build();
            });

            return builder;
        }


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
    }
}

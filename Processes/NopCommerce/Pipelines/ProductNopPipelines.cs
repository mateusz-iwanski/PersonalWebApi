using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using nopCommerceApiHub.WebApi;
using nopCommerceApiHub.WebApi.Exceptions;
using PersonalWebApi.Agent.Memory.Observability;
using PersonalWebApi.Agent.SemanticKernel.Observability;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.FileStorage.Events;
using PersonalWebApi.Processes.FileStorage.Steps;
using PersonalWebApi.Processes.NopCommerce.Events;
using PersonalWebApi.Processes.NopCommerce.Models;
using PersonalWebApi.Processes.NopCommerce.Steps;
using PersonalWebApi.Processes.NoSQLDB.Steps;
using PersonalWebApi.Processes.Qdrant.Events;
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

namespace PersonalWebApi.Processes.FileStorage.Processes
{
    /// <summary>
    /// Gt product with all data table connections
    /// </summary>
    public class ProductNopPipelines
    {
        [Experimental("SKEXP0080")]
        public async Task CollectProductPipeline(Kernel kernel, ProductCollectNopStepDto productNopStepDto)
        {
            var process = new ProcessBuilder("CollectProductPipeline");

            var nopProductSteps = process.AddStepFromType<ProductNopStep>();

            process.OnInputEvent(ProductNopEvents.StartProcess).SendEventTo(
                new ProcessFunctionTargetBuilder(nopProductSteps, functionName: ProductNopFunctions.GetTitle, parameterName: "productNopStep"));

            var kernelProcess = process.Build();

            using var runningProcess = await kernelProcess.StartAsync(
                kernel,
                    new KernelProcessEvent()
                    {
                        Id = ProductNopEvents.StartProcess,
                        Data = productNopStepDto
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

            kernelBuilder.Services.Configure<StolargoPLApiSettings>(configuration.GetSection("NopCommerceStolargoPLApiSettings"));
            kernelBuilder.Services.Configure<StolargoPLTokentSettings>(configuration.GetSection("NopCommerceStolargoPLTokenSettings"));
            kernelBuilder.Services.AddScoped<PersonalWebApi.Services.NopCommerce.NopCommerce>();

            return kernelBuilder.Build();
        }
    }
}

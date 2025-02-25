using Microsoft.SemanticKernel;
using nopCommerceApiHub.WebApi;
using nopCommerceApiHub.WebApi.Exceptions;
using PersonalWebApi.Processes.FileStorage.Events;
using PersonalWebApi.Processes.FileStorage.Steps;
using PersonalWebApi.Processes.NopCommerce.Events;
using PersonalWebApi.Processes.NopCommerce.Steps;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.NopCommerce.Processes
{
    public class CollectDataProcess
    {

        [Experimental("SKEXP0080")]
        public static ProcessBuilder CreateProcess(string processName = "CollectData")
        {
            var process = new ProcessBuilder(processName);

            var nopProductSteps = process.AddStepFromType<ProductNopStep>();

            process.OnInputEvent(ProductNopEvents.StartProcess).SendEventTo(
                new ProcessFunctionTargetBuilder(nopProductSteps, functionName: ProductNopFunctions.GetTitle, parameterName: "sku"));

            return process;
        }

        [Experimental("SKEXP0080")]
        public static Kernel PrepareKelner(IConfiguration configuration)
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

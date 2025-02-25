using Microsoft.SemanticKernel;
using System.Diagnostics.CodeAnalysis;
using PersonalWebApi.Services.NopCommerce;
using PersonalWebApi.Agent;
using Microsoft.ML.OnnxRuntimeGenAI;
using PersonalWebApi.Processes.Document.Steps;
using Microsoft.Extensions.AI;
using PersonalWebApi.Utilities.Document;
using PersonalWebApi.Processes.NopCommerce.Models;
using Newtonsoft.Json;
using PersonalWebApi.Processes.NopCommerce.Events;

namespace PersonalWebApi.Processes.NopCommerce.Steps
{
    public static class ProductNopFunctions
    {
        public const string GetTitle = nameof(GetTitle);
        public const string ParaphraseTitle = nameof(ParaphraseTitle);
    }

    /// <summary>
    /// Connect and get title from nopCommerce API
    /// </summary>
    /// <return>Title of nopCOmmerce product</return>
    [Experimental("SKEXP0080")]
    public sealed class ProductNopStep : KernelProcessStep
    {
        public string Title { get; set; }

        [KernelFunction(ProductNopFunctions.GetTitle)]
        public async ValueTask GetProductAsync(KernelProcessStepContext context, Kernel kernel, ProductCollectNopStepDto productNopStep)
        {
            // get kernel by appsettings.StepAgentMappings
            var NopCommerce = kernel.GetRequiredService<PersonalWebApi.Services.NopCommerce.NopCommerce>();
            var product = await NopCommerce.Product.GetBySkuAsync(productNopStep.Sku);
            productNopStep.Product = product;

            await context.EmitEventAsync(new() { Id = ProductNopEvents.Readed, Data = productNopStep });
        }

        [KernelFunction(ProductNopFunctions.ParaphraseTitle)]
        public async ValueTask ParaphraseTitleAsync(KernelProcessStepContext context, Kernel kernel, ProductCollectNopStepDto productDetail)
        {
            var config = kernel.GetRequiredService<IConfiguration>();
            var agent = new AgentRouter(config).GetStepKernel(ProductNopFunctions.ParaphraseTitle);

            var prompts = kernel.CreatePluginFromPromptDirectory(Path.Combine(AppContext.BaseDirectory, "Agent/SemanticKernel/Prcocesses/Prompt"));
            string completeMessage = string.Empty;

            await foreach (var message in kernel.InvokeStreamingAsync<StreamingChatMessageContent>
                (
                    prompts["NopCommercParaphraseTitle"], new() 
                        { 
                            { "title", productDetail.Product.Name },
                            { "description", productDetail.Product.FullDescription }
                        })
                )
            {
                completeMessage += message;
            }


            var newTtitle = TextFormatter.CleanResponse(completeMessage);

            Title = newTtitle;

            await context.EmitEventAsync(new() { Id = ProductNopEvents.Paraphrased, Data = completeMessage });
        }
    }
}

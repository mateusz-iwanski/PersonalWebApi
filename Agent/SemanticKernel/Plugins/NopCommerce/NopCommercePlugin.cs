using Microsoft.SemanticKernel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using DocumentFormat.OpenXml.Wordprocessing;
using PersonalWebApi.Utilities.Document;
using PersonalWebApi.Processes.NopCommerce.Events;
using PersonalWebApi.Processes.NopCommerce.Models;
using PersonalWebApi.Processes.NopCommerce.Steps;
using PersonalWebApi.Processes.FileStorage.Processes;
using Microsoft.Extensions.AI;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Agent.SemanticKernel.Plugins.NopCommerce
{
    public class NopCommercePlugin
    {
        [Experimental("SKEXP0080")]
        [KernelFunction("paraphrise_product_title")]
        [Description("Paraphrase the product title for e-commerce using the nopCommerce SKU.")]
        public async Task<string> ParaphraseTitleAsync(string productSku, Kernel kernel)
        {
            var config = kernel.GetRequiredService<IConfiguration>();

            // run steps in pipelnie to collect data from nopCommerce
            var productNopStepDto = new ProductCollectNopStepDto()
            {
                Sku = productSku
            };

            ProductNopPipelines pipeline = new ProductNopPipelines();
            await pipeline.CollectProductPipeline(kernel, productNopStepDto);

            // chatbot
            var agent = new AgentRouter(config).GetStepKernel("NopCommerceParaphraseTitle");

            var prompts = kernel.CreatePluginFromPromptDirectory(Path.Combine(AppContext.BaseDirectory, "Agent/SemanticKernel/Plugins/Prompt/NopCommerce"));
            string completeMessage = string.Empty;

            var categoryNames = string.Join(", ", productNopStepDto.Category.Select(c => c.Name));

            await foreach (var message in kernel.InvokeStreamingAsync<StreamingChatMessageContent>
                (
                    prompts["NopCommercParaphraseTitlePlugin"], new()
                        {
                            { "title", productNopStepDto.Product.Name },
                            { "description", productNopStepDto.Product.FullDescription },
                            { "categories", categoryNames }
                        })
                )
            {
                completeMessage += message;
            }


            var newTtitle = TextFormatter.CleanResponse(completeMessage);

            return newTtitle;
        }
    }
}

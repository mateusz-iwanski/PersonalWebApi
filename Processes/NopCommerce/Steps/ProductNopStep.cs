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
using nopCommerceApiHub.WebApi.DTOs;

namespace PersonalWebApi.Processes.NopCommerce.Steps
{
    public static class ProductNopFunctions
    {
        public const string GetProduct = nameof(GetProduct);
        public const string GetCategory = nameof(GetCategory);
    }

    /// <summary>
    /// Connect and get title from nopCommerce API
    /// </summary>
    /// <return>Title of nopCOmmerce product</return>
    [Experimental("SKEXP0080")]
    public sealed class ProductNopStep : KernelProcessStep
    {
        public string Title { get; set; }

        [KernelFunction(ProductNopFunctions.GetProduct)]
        public async ValueTask GetProductAsync(KernelProcessStepContext context, Kernel kernel, ProductCollectNopStepDto productNopStep)
        {
            // get kernel by appsettings.StepAgentMappings
            var NopCommerce = kernel.GetRequiredService<PersonalWebApi.Services.NopCommerce.NopCommerce>();
            var product = await NopCommerce.Product.GetBySkuAsync(productNopStep.Sku);
            productNopStep.Product = product;

            await context.EmitEventAsync(new() { Id = ProductNopEvents.ReadedProduct, Data = productNopStep });
        }

        /// <summary>
        /// Get product category from nopCommerce.
        /// Category -> mapping cattegory - > product
        /// </summary>
        /// <param name="context"></param>
        /// <param name="kernel"></param>
        /// <param name="productNopStep"></param>
        /// <returns></returns>
        [KernelFunction(ProductNopFunctions.GetCategory)]
        public async ValueTask GetCategoryAsync(KernelProcessStepContext context, Kernel kernel, ProductCollectNopStepDto productNopStep)
        {
            var productCategories = new List<CategoryDto>();

            var NopCommerce = kernel.GetRequiredService<PersonalWebApi.Services.NopCommerce.NopCommerce>();
            var categoryMapping = await NopCommerce.ProductCategoryMapping.GetByProductIdAsync(productNopStep.Product.Id);

            foreach (var categoryMap in categoryMapping)
            {
                var category = await NopCommerce.Category.GetByIdAsync(categoryMap.CategoryId);
                productCategories.Add(category);
            }
            
            productNopStep.Category = productCategories;

            await context.EmitEventAsync(new() { Id = ProductNopEvents.ReadedCategory, Data = productNopStep });
        }


    }
}

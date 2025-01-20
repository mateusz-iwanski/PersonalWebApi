using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using OllamaSharp;
using PersonalWebApi.Services.Services.System;
using PersonalWebApi.Utilities.Document;
using System.ComponentModel;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin
{
    public class TagCollectorPlugin
    {
        [KernelFunction("generate_tag_for_text_content")]
        [Description("Generate/collect tag for text content")]
        [return: Description("List<string> with tags")]
        public async Task<List<string>> GenerateTagsPluginAsync(string textContent, Kernel kernel)
        {
            var prompts = kernel.CreatePluginFromPromptDirectory(Path.Combine(AppContext.BaseDirectory, "Agent/SemanticKernel/Plugins/Prompt"));

            string completeMessage = string.Empty;

            await foreach (var message in kernel.InvokeStreamingAsync<StreamingChatMessageContent>(prompts["TagCollectPlugin"], new() { { "textContent", textContent }, }))
            {
                completeMessage += message;
            }

            var cleanedMessage = TextFormatter.CleanResponse(completeMessage);

            return JsonConvert.DeserializeObject<List<string>>(cleanedMessage);
        }
    }
}

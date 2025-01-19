using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin
{
    public class DocumentInfoPlugin
    {
        [KernelFunction("specify_document_type")]
        [Description("Specify the type of content (e.g., article, essay, report)")]
        [return: Description("List<string> with tags")]
        public async Task<List<string>> SpecifyDocumentType(string textContent, Kernel kernel)
        {
            var prompts = kernel.CreatePluginFromPromptDirectory(Path.Combine(AppContext.BaseDirectory, "Agent/SemanticKernel/Plugins/Prompt"));

            string completeMessage = string.Empty;
            var result = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(prompts["TagCollectPlugin"], new() { { "textContent", textContent }, });

            await foreach (var message in kernel.InvokeStreamingAsync<StreamingChatMessageContent>(prompts["TagCollectPlugin"], new() { { "textContent", textContent }, }))
            {
                completeMessage += message;
            }

            return JsonConvert.DeserializeObject<List<string>>(completeMessage);
        }
    }
}

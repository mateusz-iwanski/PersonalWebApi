using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using PersonalWebApi.Utilities.Document;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin
{
    public class DocumentInfoPlugin
    {
        [KernelFunction("specify_document_type")]
        [Description("Specify the type of content (e.g., article, essay, report)")]
        [return: Description("List<string> with types")]
        public async Task<List<string>> SpecifyDocumentType(string textContent, Kernel kernel)
        {
            var prompts = kernel.CreatePluginFromPromptDirectory(Path.Combine(AppContext.BaseDirectory, "Agent/SemanticKernel/Plugins/Prompt"));

            string completeMessage = string.Empty;
            
            await foreach (var message in kernel.InvokeStreamingAsync<StreamingChatMessageContent>(prompts["DocumentTypePlugin"], new() { { "textContent", textContent }, }))
            {
                completeMessage += message;
            }

            string cleanedMessage = TextFormatter.CleanResponse(completeMessage);

            return JsonConvert.DeserializeObject<List<string>>(cleanedMessage);
        }

       


        [KernelFunction("specify_document_language")]
        [Description("Specify the language of content (e.g., Polish, English, ...)")]
        [return: Description("string")]
        public async Task<string> SpecifyDocumentLanguage(string textContent, Kernel kernel)
        {
            var prompts = kernel.CreatePluginFromPromptDirectory(Path.Combine(AppContext.BaseDirectory, "Agent/SemanticKernel/Plugins/Prompt"));

            string completeMessage = string.Empty;

            await foreach (var message in kernel.InvokeStreamingAsync<StreamingChatMessageContent>(prompts["DocumentLanguagePlugin"], new() { { "textContent", textContent }, }))
            {
                completeMessage += message;
            }

            return completeMessage;
        }
    }
}

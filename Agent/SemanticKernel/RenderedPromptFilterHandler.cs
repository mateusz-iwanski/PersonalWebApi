using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Math;
using Microsoft.SemanticKernel;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Models.Models.SemanticKernel;
using System.Text.Json;

namespace PersonalWebApi.Agent.SemanticKernel
{
    public class RenderedPromptFilterHandler : IPromptRenderFilter
    {
        private readonly IAssistantHistoryManager _assistantHistoryManager;

        public RenderedPromptFilterHandler(IAssistantHistoryManager assistantHistoryManager)
        {
            _assistantHistoryManager = assistantHistoryManager;
        }

        // raise when FunctionResult - _kernel.CreateFunctionFromPrompt(skPrompt).InvokeAsync
        // https://learn.microsoft.com/en-us/semantic-kernel/concepts/enterprise-readiness/filters?pivots=programming-language-csharp
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            // Call the next filter in the pipeline
            await next(context);

            // Save the rendered prompt to history
            context.Function.ExecutionSettings["default"].ExtensionData.TryGetValue("conversationUuid", out object conversationUuidObj);
            context.Function.ExecutionSettings["default"].ExtensionData.TryGetValue("sessionUuid", out object sessionUuidObj);

            Guid.TryParse(conversationUuidObj?.ToString(), out Guid conversationUuidGuid);
            Guid.TryParse(sessionUuidObj?.ToString(), out Guid sessionUuidGuid);

            string status = "Done";
            string inputArguments = context.Arguments["input"].ToString();
            string functionName = context.Function.Name;
            string functionDescription = context.Function.Description;
            string renderedPrompt = context.RenderedPrompt;
            string usedPluginNames = string.Join("][", context.Kernel.Plugins
                .Select(x => x.Name)
                .Where(name => !string.IsNullOrEmpty(name)));

            if (!string.IsNullOrEmpty(usedPluginNames))
            {
                usedPluginNames = "[" + usedPluginNames + "]";
            }

            var functionExecutingHistory = new FunctionExecutingHistory(
                conversationUuidGuid,
                sessionUuidGuid,
                inputArguments,
                functionName,
                functionDescription,
                renderedPrompt,
                usedPluginNames,
                status);

            await _assistantHistoryManager.SaveAsync(functionExecutingHistory);
            
        }

        public void OnRender(string prompt)
        {
            // Save or log the rendered prompt
            SaveRenderedPrompt(prompt);
        }

        private void SaveRenderedPrompt(string prompt)
        {
            // Here you can save the prompt to a file, database, or any other storage
            System.IO.File.AppendAllText("rendered_prompts.log", prompt + Environment.NewLine);
        }
    }




}

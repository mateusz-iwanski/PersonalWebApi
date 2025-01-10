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

        private (Guid, Guid) _getUuid(KernelArguments arguments)
        {
            if (!arguments.ContainsName("conversationUuid") || !arguments.ContainsName("sessionUuid"))
            {
                throw new InvalidUuidException("The required keys 'conversationUuid' and 'sessionUuid' are not present in the kernel arguments.");
            }

            if (string.IsNullOrEmpty(arguments["conversationUuid"].ToString()) || string.IsNullOrEmpty(arguments["sessionUuid"].ToString()))
            {
                throw new InvalidUuidException("Invalid conversation UUID or session UUID. They must be valid GUIDs.");
            }

            if (!Guid.TryParse(arguments["conversationUuid"].ToString(), out Guid conversationUuid) || !Guid.TryParse(arguments["sessionUuid"].ToString(), out Guid sessionUuid))
            {
                throw new InvalidUuidException("Invalid conversation UUID or session UUID. They must be valid GUIDs.");
            }

            return (conversationUuid, sessionUuid);
        }

        // raise when FunctionResult when kernel invoke
        // https://learn.microsoft.com/en-us/semantic-kernel/concepts/enterprise-readiness/filters?pivots=programming-language-csharp
        /// <summary>
        /// This method is called when the Kernel invoke.
        /// </summary>
        /// <param name="context">The context must have KernelArguments with conversationUuid and sessionUuid</param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            // Call the next filter in the pipeline
            await next(context);

            (Guid conversationUuid, Guid sessionUuid) = _getUuid(context.Arguments);

            await _assistantHistoryManager.SaveAsync(new FunctionExecutingHistory(
                conversationUuid: conversationUuid,
                sessionUuid: sessionUuid,
                inputArguments: context.Arguments.ToDictionary(arg => arg.Key, arg => arg.Value.ToString()),
                functionName: context.Function.Name,
                pluginName: context.Function.PluginName,
                functionDescription: context.Function.Description,
                renderedPrompt: context.RenderedPrompt,
                status: "Done",
                executionSettings: context.Function.ExecutionSettings?
                    .Select(arg => arg.Value.ExtensionData.ToDictionary(
                        ed => string.IsNullOrEmpty(arg.Value.ModelId) ? $"{arg.Key}-{ed.Key}" : $"{arg.Key}-{arg.Value.ModelId}_{ed.Key}",
                        ed => ed.Value.ToString()
                    )).ToList() ?? new List<Dictionary<string, string?>>()
            ));
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

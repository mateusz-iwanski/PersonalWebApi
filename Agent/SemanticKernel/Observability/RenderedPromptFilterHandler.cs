using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Math;
using Microsoft.SemanticKernel;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Models.Models.SemanticKernel;
using PersonalWebApi.Services.Services.History;
using PersonalWebApi.Services.Services.System;
using System.Text.Json;

namespace PersonalWebApi.Agent.SemanticKernel.Observability
{
    /// <summary>
    /// Handles the rendering of prompts and logs the execution history of functions invoked by the Semantic Kernel.
    /// </summary>
    public class RenderedPromptFilterHandler : IPromptRenderFilter
    {
        private readonly IAssistantHistoryManager _assistantHistoryManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderedPromptFilterHandler"/> class.
        /// </summary>
        /// <param name="assistantHistoryManager">The assistant history manager responsible for saving function execution history.</param>
        public RenderedPromptFilterHandler(IAssistantHistoryManager assistantHistoryManager, IHttpContextAccessor httpContextAccessor)
        {
            _assistantHistoryManager = assistantHistoryManager;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Called when the Kernel invokes a kernel function. Logs the execution history of the function.
        /// </summary>
        /// <param name="context">The context containing the kernel arguments and function details.</param>
        /// <param name="next">The next filter in the pipeline.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            (Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(_httpContextAccessor);

            // Call the next filter in the pipeline
            await next(context);

            await _assistantHistoryManager.SaveAsync(new FunctionExecutingHistoryDto(
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

        /// <summary>
        /// Called to render a prompt. Saves or logs the rendered prompt.
        /// </summary>
        /// <param name="prompt">The rendered prompt.</param>
        public void OnRender(string prompt)
        {
            // Save or log the rendered prompt
            SaveRenderedPrompt(prompt);
        }

        /// <summary>
        /// Saves the rendered prompt to a file.
        /// </summary>
        /// <param name="prompt">The rendered prompt.</param>
        private void SaveRenderedPrompt(string prompt)
        {
            // Here you can save the prompt to a file, database, or any other storage
            File.AppendAllText("rendered_prompts.log", prompt + Environment.NewLine);
        }
    }
}

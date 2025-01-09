using Microsoft.SemanticKernel;

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
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            // Call the next filter in the pipeline
            await next(context);

            // Get the rendered prompt
            var renderedPrompt = context.RenderedPrompt;

            // Save or log the rendered prompt
            //SaveRenderedPrompt(renderedPrompt);

            object conversationUuidObj;
            object sessionUuidObj;
            Guid conversationUuidGuidObj;
            Guid sessionUuidGuidObj;

            context.Function.ExecutionSettings["default"].ExtensionData.TryGetValue("conversationUuid", out conversationUuidObj);
            context.Function.ExecutionSettings["default"].ExtensionData.TryGetValue("sessionUuid", out sessionUuidObj);

            Guid.TryParse(conversationUuidObj.ToString(), out conversationUuidGuidObj);
            Guid.TryParse(sessionUuidObj.ToString(), out sessionUuidGuidObj);

            //await _assistantHistoryManager.SaveAsync(conversationUuidGuidObj, sessionUuidGuidObj, renderedPrompt);
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

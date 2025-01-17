using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Services.Qdrant.Processes.Steps
{
    public static class SummarizeStepFunctions
    {
        public const string SummarizeText = nameof(SummarizeText);
    }

    public static class SummarizeStepOutputEvents
    {
        public const string SummaryGenerated = nameof(SummaryGenerated);
    }

    [Experimental("SKEXP0080")]
    public sealed class SummarizeStep : KernelProcessStep
    {
        [KernelFunction(SummarizeStepFunctions.SummarizeText)]
        public async ValueTask SummarizeTextAsync(KernelProcessStepContext context, Kernel kernel, string content, int maxSummaryCharacters)
        {
            var chat = kernel.GetRequiredService<IChatCompletionService>();

            var summary = await chat.GetChatMessageContentAsync(
                $@"""

                Summarize the following text in no more than {maxSummaryCharacters} characters for use in a vector database. 
                The summary must remain in the same language as the original text, focusing on semantic clarity and key ideas that 
                can aid similarity search. 
                    
                <text> 
                {content}    
                </text>

                """);

            await context.EmitEventAsync(new() { Id = SummarizeStepOutputEvents.SummaryGenerated, Data = summary.Content });
        }
    }
}



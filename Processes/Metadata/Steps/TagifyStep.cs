using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.Metadata.Steps
{
    public static class TagifyStepFunctions
    {
        public const string GenerateTags = nameof(GenerateTags);
    }

    public static class TagifyStepOutputEvents
    {
        public const string TagsGenerated = nameof(TagsGenerated);
    }

    [Experimental("SKEXP0080")]
    public sealed class TagifyStep : KernelProcessStep
    {
        [KernelFunction(TagifyStepFunctions.GenerateTags)]
        public async ValueTask GenerateTagsAsync(KernelProcessStepContext context, Kernel kernel, string content)
        {
            var chat = kernel.GetRequiredService<IChatCompletionService>();

            var tagAsString = await chat.GetChatMessageContentAsync(
                @$"""Generate tags for the following text in a comma-separated list format.

                <text>
                {content}
                </text>

                Output must be a comma-separated list of tags. If there are no tags, return an empty list.

                <output>
                tag1, tag2
                </output>

                """);

            var tagList = tagAsString.Content.Split(", ").ToList();

            await context.EmitEventAsync(new() { Id = TagifyStepOutputEvents.TagsGenerated, Data = tagList });
        }
    }
}


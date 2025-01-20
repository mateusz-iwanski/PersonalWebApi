using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.Metadata.Events;
using System.Diagnostics.CodeAnalysis;
using PersonalWebApi.Processes.Qdrant.Events;

namespace PersonalWebApi.Processes.Qdrant.Steps
{
    /// <summary>
    /// It is used to tagify chunks in DocumentStepDto.
    /// Not use this anywhere else.
    /// </summary>
    /// <remarks>Use it after chunked</remarks>
    [Experimental("SKEXP0080")]
    public sealed class TagifyChunksStep : KernelProcessStep
    {

        public static class TagifyStepFunctions
        {
            public const string GenerateChunksTags = nameof(GenerateChunksTags);
        }
        [KernelFunction(TagifyStepFunctions.GenerateChunksTags)]
        public async ValueTask TagifyChunksAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            var tagifyPlugin = new TagCollectorPlugin();

            foreach(var chunk in documentStepDto.ChunkerCollection)
            {
                var tagsCollectionForChunk = await tagifyPlugin.GenerateTags(chunk.Content, kernel);
                foreach (var tag in tagsCollectionForChunk)
                    chunk.Tags.Add(tag);
            }

            documentStepDto.Events.Add("chunks tagified");

            await context.EmitEventAsync(new() { Id = QdrantEvents.ChunksTagified, Data = documentStepDto });
        }
    }
}

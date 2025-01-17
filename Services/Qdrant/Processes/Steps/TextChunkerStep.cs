using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using Microsoft.SemanticKernel;
using PersonalWebApi.Services.Agent;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Services.Qdrant.Processes.Steps
{
    public record TextChunkerStepItem(string Content, int MaxTokensPerLine, string ModelEmbedding);

    public static class TextChunkerStepFunctions
    {
        public const string ChunkText = nameof(ChunkText);
    }

    public static class TextChunkerStepOutputEvents
    {
        public const string Chunked = nameof(Chunked);
    }

    [Experimental("SKEXP0080")] 
    public sealed class TextChunkerStep : KernelProcessStep
    {

        [KernelFunction(TextChunkerStepFunctions.ChunkText)]
        public async ValueTask ChunkTextAsync(KernelProcessStepContext context, Kernel kernel, string content)
        {
            var chunker = kernel.GetRequiredService<ITextChunker>();
            chunker.Setup("text-embedding-3-small");

            // todo tu inaczejto zrobić
            List<StringChunkerFormat> chunks = chunker.ChunkText(100, content);

            await context.EmitEventAsync(new() { Id = TextChunkerStepOutputEvents.Chunked, Data = chunks });
        }
    }
}

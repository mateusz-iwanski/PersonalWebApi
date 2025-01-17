using Microsoft.SemanticKernel;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using PersonalWebApi.Utilities.Utilities.Models;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Services.Qdrant.Processes.Steps
{
    public record TextChunkerStepItem(string ConversationId, string Text, int MaxTokensPerLine, string ModelEmbedding);

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
        private readonly SemanticKernelTextChunker _textChunker;

        public TextChunkerStep(SemanticKernelTextChunker textChunker)
        {
            _textChunker = textChunker;
        }

        [KernelFunction(TextChunkerStepFunctions.ChunkText)]
        public async ValueTask ChunkTextAsync(KernelProcessStepContext context, Kernel kernel, TextChunkerStepItem textChunkerStepItem)
        {
            var chunker = new SemanticKernelTextChunker(textChunkerStepItem.ModelEmbedding);
            var chunks = _textChunker.ChunkText(textChunkerStepItem.ConversationId, textChunkerStepItem.MaxTokensPerLine, textChunkerStepItem.Text);

            await context.EmitEventAsync(new() { Id = TextChunkerStepOutputEvents.Chunked, Data = chunks });
        }
    }
}

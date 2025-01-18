using Azure.AI.FormRecognizer.DocumentAnalysis;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using Microsoft.SemanticKernel;
using PersonalWebApi.Processes.Document.Events;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.Qdrant.Models;
using PersonalWebApi.Services.Agent;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Services.Qdrant.Processes.Steps
{
    public static class TextChunkerStepFunctions
    {
        public const string ChunkText = nameof(ChunkText);
    }

    [Experimental("SKEXP0080")] 
    public sealed class TextChunkerStep : KernelProcessStep
    {

        [KernelFunction(TextChunkerStepFunctions.ChunkText)]
        public async ValueTask ChunkTextAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            var chunker = kernel.GetRequiredService<ITextChunker>();
            chunker.Setup("text-embedding-3-small");

            documentStepDto.ChunkerCollection = chunker.ChunkText(100, documentStepDto.Content);
            
            await context.EmitEventAsync(
                new KernelProcessEvent() 
                { 
                    Id = DocumentEvents.Chunked, 
                    Data = documentStepDto
                });
        }
    }
}

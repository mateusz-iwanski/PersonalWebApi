using Azure.AI.FormRecognizer.DocumentAnalysis;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using Microsoft.SemanticKernel;
using PersonalWebApi.Exceptions;
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

        /// <summary>
        /// Embedding and chunk text
        /// 
        /// Normally in steps model is set from the appsettings.StepAgentMappings.json but here settings are readed
        /// from appsettings.Qdrant.json file.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="kernel"></param>
        /// <param name="documentStepDto"></param>
        /// <returns></returns>
        [KernelFunction(TextChunkerStepFunctions.ChunkText)]
        public async ValueTask ChunkTextAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            var chunker = kernel.GetRequiredService<ITextChunker>();
            var configuration = kernel.GetRequiredService<IConfiguration>();

            var embeddingModel = configuration.GetSection("Qdrant:Container:OpenAiModelEmbedding").Value ?? 
                throw new SettingsException("Qdrant:Container:OpenAiModelEmbedding can't read from appsettings");
            var maxTokenFileChunked = configuration.GetSection("Qdrant:MaxTokenFileChunked").Value ??
                throw new SettingsException("Qdrant:MaxTokenFileChunked not exists in appsettings");

            chunker.Setup(embeddingModel);

            documentStepDto.ChunkerCollection = chunker.ChunkText(int.Parse(maxTokenFileChunked), documentStepDto.Content);

            documentStepDto.Events.Add("content chunked");

            await context.EmitEventAsync(
                new KernelProcessEvent() 
                { 
                    Id = DocumentEvents.Chunked, 
                    Data = documentStepDto
                });
        }
    }
}

using Microsoft.SemanticKernel;
using System.Diagnostics.CodeAnalysis;
using PersonalWebApi.Utilities.Utilities.Qdrant;
using System.Security.Claims;
using PersonalWebApi.Models.Storage;
using PersonalWebApi.Services.Services.History;
using PersonalWebApi.Models.FileStorage;
using DocumentFormat.OpenXml.Drawing.Charts;
using PersonalWebApi.Services.Agent;
using LLama.Common;
using PersonalWebApi.Processes.Qdrant.Events;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Services.Services.Qdrant;
using PersonalWebApi.Services.Services.System;
using PersonalWebApi.Agent;

namespace PersonalWebApi.Services.Qdrant.Processes.Steps
{
    public record QdrantStepInput(string FileId, List<string> Content, string QdrantCollectionName);

    public static class QdrantStepFunctions
    {
        public const string AddEmbedding = nameof(AddEmbedding);
    }

    // filemetadatacollector - bedzie zwracac wszystkie mozliwe informacje po logach wczesniej wygernerowanych
    // np skopiuje plik na blob, to z logow wyciagnac informacje, w innym kroku tez pewnie zapisane informacje to je tez tam wyciagac
    // etc.

    [Experimental("SKEXP0080")]
    public sealed class QdrantStep : KernelProcessStep
    {
        [KernelFunction(QdrantStepFunctions.AddEmbedding)]
        public async ValueTask AddEmbeddingAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            var configuration = kernel.GetRequiredService<IConfiguration>();
            var customKernel = new AgentRouter(configuration);
            kernel = customKernel.AddOpenAIChatCompletion();

            var httpContextAccessor = kernel.GetRequiredService<IHttpContextAccessor>();
            (Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(httpContextAccessor);

            var qdrantService = kernel.GetRequiredService<IQdrantService>();
            var userClaimsPrincipal = httpContextAccessor.HttpContext?.User ?? throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext.User));

            foreach (var chunk in documentStepDto.ChunkerCollection)
            {
                var metadata = new Dictionary<string, string>();

                // Collect metadata from DocumentStepDto
                foreach (var kvp in documentStepDto.Metadata)
                {
                    metadata[$"SourceDocument{kvp.Key}"] = kvp.Value;
                }

                // Collect metadata from each chunk
                foreach (var kvp in chunk.Metadata)
                {
                    metadata[$"Chunk{kvp.Key}"] = kvp.Value;
                }

                foreach (var evnt in documentStepDto.Events)
                {
                    metadata[$"SourceDocumentEvent"] = string.Join(";", evnt);
                }

                // Add content to metadata
                metadata["ChunkContent"] = chunk.Content;
                metadata["Uri"] = documentStepDto.Uri.ToString();

                // Call QdrantService.AddAsync for each chunk
                await qdrantService.AddAsync(chunk.Content, metadata, conversationUuid, documentStepDto.FileId);
            }

            await context.EmitEventAsync(new() { Id = QdrantEvents.EmbeddingAdded, Data = documentStepDto });
        }

    }
}




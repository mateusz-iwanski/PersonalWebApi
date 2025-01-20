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

            documentStepDto.Events.Add("added to qdrant");

            foreach (var chunk in documentStepDto.ChunkerCollection)
            {
                var metadata = new Dictionary<string, string>();

                metadata["chunk_start_position"] = chunk.StartPosition.ToString();
                metadata["chunk_end_position"] = chunk.EndPosition.ToString();
                metadata["last_chunk_end_position"] = documentStepDto.ChunkerCollection.LastOrDefault().EndPosition.ToString();

                // Collect metadata from DocumentStepDto
                foreach (var kvp in documentStepDto.Metadata)
                {
                    if (!string.IsNullOrEmpty(kvp.Value))
                        metadata[$"file_{kvp.Key.ToLower()}"] = kvp.Value.ToLower();
                }

                // Collect metadata from each chunk
                foreach (var kvp in chunk.Metadata)
                {
                    if (!string.IsNullOrEmpty(kvp.Value))
                        metadata[$"chunk_{kvp.Key.ToLower()}"] = kvp.Value;
                }

                metadata["file_event"] = string.Join(";", documentStepDto.Events).ToLower();
                metadata["chunk_tags"] = string.Join(", ", chunk.Tags); ;
                metadata["file_tags"] = string.Join(", ", documentStepDto.Tags);
                metadata["chunk_content"] = chunk.Content.ToLower();
                metadata["file_uri"] = documentStepDto.Uri?.ToString() ?? "";
                metadata["file_summary"] = documentStepDto.Summary?.ToLower() ?? "not available";
                metadata["language"] = documentStepDto.Language;
                metadata["file_type"] = string.Join(", ", documentStepDto.DocumentType);

                metadata["total_chunks"] = documentStepDto.ChunkerCollection.Count.ToString();
                metadata["chunk_number"] = documentStepDto.ChunkerCollection.IndexOf(chunk).ToString();

                // Call QdrantService.AddAsync for each chunk
                await qdrantService.AddAsync(chunk.Content, metadata, conversationUuid, documentStepDto.FileId);
            }

            await context.EmitEventAsync(new() { Id = QdrantEvents.EmbeddingAdded, Data = documentStepDto });
        }

    }
}




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

namespace PersonalWebApi.Services.Qdrant.Processes.Steps
{
    public static class a
    {
        public static void gethistory(this Kernel a) 
        {
            ChatHistory a
        }
    }

    public record QdrantStepInput(string FileId, List<string> Content, string QdrantCollectionName);

    public static class QdrantStepFunctions
    {
        public const string AddEmbedding = nameof(AddEmbedding);
    }

    public static class QdrantStepOutputEvents
    {
        public const string EmbeddingAdded = nameof(EmbeddingAdded);
    }

    // filemetadatacollector - bedzie zwracac wszystkie mozliwe informacje po logach wczesniej wygernerowanych
    // np skopiuje plik na blob, to z logow wyciagnac informacje, w innym kroku tez pewnie zapisane informacje to je tez tam wyciagac
    // etc.

    [Experimental("SKEXP0080")]
    public sealed class QdrantStep : KernelProcessStep
    {
        [KernelFunction(QdrantStepFunctions.AddEmbedding)]
        public async ValueTask AddEmbeddingAsync(KernelProcessStepContext context, Kernel kernel, List<StringChunkerFormat> chunks)
        {
            var qdrantApi = kernel.GetRequiredService<QdrantApi>();
            var userClaimsPrincipal = kernel.GetRequiredService<ClaimsPrincipal>();
            var historyManger = kernel.GetRequiredService<IAssistantHistoryManager>();

            var uploadedBy = userClaimsPrincipal?.FindFirstValue(ClaimTypes.Name) ?? ClaimTypes.Anonymous;

            var generatedMetadata = await historyManger.LoadItemsAsync<FileContentDto>(new Dictionary<string, object> { { "FileId", "1" } });

            if (generatedMetadata == null || !generatedMetadata.Any())
            {
                throw new InvalidOperationException("No metadata found for the specified file ID.");
            }

            var metadata = new Dictionary<string, object>
            {
                { "FileId", "1" },
                { "FileName", generatedMetadata.First().FileName },
                { "ContentType", generatedMetadata.First().ContentType },
                { "FileSize", generatedMetadata.First().FileSize },
                { "StoragePath", generatedMetadata.First().StoragePath },
                { "Checksum", generatedMetadata.First().Checksum },
                { "UploadedBy", generatedMetadata.First().UploadedBy },
                { "PageCount", generatedMetadata.First().PageCount },
                { "Category", generatedMetadata.First().Category },
                { "LastProcessedAt", generatedMetadata.First().LastProcessedAt?.ToString("o") },
                { "Description", generatedMetadata.First().Description },
                { "UserDescription", generatedMetadata.First().UserDescription },
                { "ConversationUuid", generatedMetadata.First().ConversationUuid.ToString() },
                { "SessionUuid", generatedMetadata.First().SessionUuid.ToString() },
                { "Tags", string.Join(",", generatedMetadata.First().Tags) },
                { "Metadata", string.Join(",", generatedMetadata.First().Metadata.Select(kv => $"{kv.Key}:{kv.Value}")) }
            };

            await qdrantApi.CheckCollectionExists("personalagent");

            foreach (var chunk in chunks)
            {
                await qdrantApi.AddEmbeddingToQdrantAsync(Guid.NewGuid(), "personalagent", chunk.line, metadata);
            }

            await context.EmitEventAsync(new() { Id = QdrantStepOutputEvents.EmbeddingAdded, Data = metadata });
        }

    }
}




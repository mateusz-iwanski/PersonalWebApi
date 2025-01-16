using PersonalWebApi.Utilities.Utilities.Qdrant;
using Qdrant.Client.Grpc;
using System.Security.Claims;
using static PersonalWebApi.Services.Services.Qdrant.QdrantService;

namespace PersonalWebApi.Services.Services.Qdrant
{
    public interface IQdrantService
    {
        //void Setup(
        //    string modelEmbedding,
        //    string modelEmbeddingApiKey,
        //    string qdrantUri,
        //    string qdrantApiKey,
        //    string qdrantCollectionName,
        //    Distance qdrantCollectionDistance,
        //    ulong qdrantCollectionSize,
        //    bool overwrite,
        //    ClaimsPrincipal user
        //    );

        Task<Guid> AddAsync(IFormFile document, Guid conversationUuid, int maxTokensPerLine, int maxSummaryCharacters);
        Task<List<QdrantFileSearchResultType>> SearchAsync(List<string> queries, Dictionary<string, string> filter = null, int limit = 5);
    }
}
using PersonalWebApi.Services.Agent;
using PersonalWebApi.Utilities.Utilities.Qdrant;
using Qdrant.Client.Grpc;
using System.Security.Claims;
using static PersonalWebApi.Services.Services.Qdrant.QdrantService;

namespace PersonalWebApi.Services.Services.Qdrant
{
    public interface IQdrantService
    {
        Task AddAsync(string chunk, Dictionary<string, string> metadata, Guid conversationUuid, Guid fileId);
        Task<Guid> AddAsync(IFormFile document, Guid conversationUuid, int maxTokensPerLine, int maxSummaryCharacters);
        Task<List<Dictionary<string, object>>> SearchAsync(List<string> queries, Dictionary<string, string> filter = null, int limit = 5);
    }
}

using static PersonalWebApi.Services.Services.Qdrant.QdrantCustom;

namespace PersonalWebApi.Services.Services.Qdrant
{
    public interface IQdrant
    {
        Task<string> AddToQdrant(IFormFile document,
            string userName,
            string collectionName,
            string collectionDistance,
            int collectionSize,
            string modelEmbedding,
            bool overwrite);
    }
}
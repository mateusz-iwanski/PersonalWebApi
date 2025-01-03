using Microsoft.AspNetCore.Mvc;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Services.Services.Qdrant;
using static PersonalWebApi.Services.Services.Qdrant.QdrantCustom;

namespace PersonalWebApi.Controllers.Controllers.Qdrant
{
    [Route("api/qdrant")]
    [ApiController]
    public class QdrantController : ControllerBase
    {
        private readonly IQdrant _qdrant;
        private readonly IConfiguration _configuration;

        public QdrantController(IQdrant qdrant, IConfiguration configuration)
        {
            _qdrant = qdrant;
            _configuration = configuration;
        }

        [HttpPost("file-add")]
        public async Task<string> AddToQdrant(IFormFile document)
        {
            var collectionName = _configuration.GetSection("QdrantCloud:FileCollection:Name").Value ??
                throw new SettingsException("QdrantCloud FileCollection->Name not exists in appsettings");

            var collectionDistance = _configuration.GetSection("QdrantCloud:FileCollection:Distance").Value ??
                throw new SettingsException("QdrantCloud FileCollection->Distance not exists in appsettings");

            var collectionSize = int.Parse(_configuration.GetSection("QdrantCloud:FileCollection:Size").Value ??
                throw new SettingsException("QdrantCloud FileCollection->Size not exists in appsettings"));

            var modelEmbedding = _configuration.GetSection("QdrantCloud:FileCollection:OpenAiModelEmbedding").Value ??
                throw new SettingsException("QdrantCloud FileCollection->OpenAiModelEmbedding not exists in appsettings");

            var userName = User.Identity.Name;

            return await _qdrant.AddToQdrant(
                document: document, 
                userName: userName, 
                collectionName: collectionName,
                collectionDistance: collectionDistance,
                collectionSize: collectionSize,
                modelEmbedding: modelEmbedding,
                overwrite: true
                );
        }
    }
}

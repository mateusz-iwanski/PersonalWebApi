using DocumentFormat.OpenXml.Wordprocessing;
using Elastic.Transport;
using Microsoft.AspNetCore.Mvc;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Services.Services.Qdrant;
using Qdrant.Client.Grpc;
using System.Security.Claims;
using static PersonalWebApi.Services.Services.Qdrant.QdrantService;

namespace PersonalWebApi.Controllers.Controllers.Qdrant
{
    [Route("api/qdrant")]
    [ApiController]
    public class QdrantController : ControllerBase
    {
        private readonly IQdrantService _qdrant;
        private readonly IConfiguration _configuration;

        public QdrantController(IQdrantService qdrant, IConfiguration configuration)
        {
            _qdrant = qdrant;
            _configuration = configuration;
        }

        /// <summary>
        /// Adds a document to the Qdrant collection.
        /// </summary>
        /// <param name="document">The document to be added.</param>
        /// <param name="converationId">The ID of the conversation.</param>
        /// <returns>File UUID as string.</returns>
        /// <response code="200">Returns the result of the operation</response>
        /// <response code="400">If the input is invalid</response>
        /// <response code="500">If there is an internal server error</response>
        /// <remarks>
        /// First uploadAsync to Azure Blob Storage, then add to Qdrant.
        /// Including embeddings and metadata with generated tags, summaries, and other useful information.
        /// The collection details are retrieved from the configuration settings.
        /// </remarks>
        [HttpPost("{converationId:guid}/file/add")]
        public async Task<string> InsertDocumentToQdrant(IFormFile document, Guid converationId)
        {
            var maxTokenFileChunked = _configuration.GetSection("Qdrant:MaxTokenFileChunked").Value ??
                throw new SettingsException("Qdrant:MaxTokenFileChunked not exists in appsettings");

            var maxSummaryFileCharacters = int.Parse(_configuration.GetSection("Qdrant:MaxSummaryFileCharacters").Value ??
                throw new SettingsException("Qdrant:MaxSummaryFileCharacters not exists in appsettings"));

            var fileUUID = await _qdrant.AddAsync(document: document, conversationUuid: converationId);

            return fileUUID.ToString();
        }
    }
}

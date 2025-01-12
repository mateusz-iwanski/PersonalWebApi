﻿using DocumentFormat.OpenXml.Wordprocessing;
using Elastic.Transport;
using Microsoft.AspNetCore.Mvc;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Services.Services.Qdrant;
using Qdrant.Client.Grpc;
using System.Security.Claims;
using static PersonalWebApi.Services.Services.Qdrant.QdrantFileService;

namespace PersonalWebApi.Controllers.Controllers.Qdrant
{
    [Route("api/qdrant")]
    [ApiController]
    public class QdrantController : ControllerBase
    {
        private readonly IQdrantFileService _qdrant;
        private readonly IConfiguration _configuration;

        public QdrantController(IQdrantFileService qdrant, IConfiguration configuration)
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
        /// First upload to Azure Blob Storage, then add to Qdrant.
        /// Including embeddings and metadata with generated tags, summaries, and other useful information.
        /// The collection details are retrieved from the configuration settings.
        /// </remarks>
        [HttpPost("{converationId:guid}/file/add")]
        public async Task<string> InsertDocumentToQdrant(IFormFile document, Guid converationId)
        {

            var collectionName = _configuration.GetSection("Qdrant:FileCollection:Name").Value ??
               throw new SettingsException("Qdrant:FileCollection:Name not exists in appsettings");

            var collectionDistance = _configuration.GetSection("Qdrant:FileCollection:Distance").Value ??
                throw new SettingsException("Qdrant:FileCollection:Distance not exists in appsettings");

            var collectionSize = ulong.Parse(_configuration.GetSection("Qdrant:FileCollection:Size").Value ??
                throw new SettingsException("Qdrant:FileCollection:Size not exists in appsettings"));

            var modelEmbedding = _configuration.GetSection("Qdrant:FileCollection:OpenAiModelEmbedding").Value ??
                throw new SettingsException("Qdrant:FileCollection:OpenAiModelEmbedding not exists in appsettings");

            var modelApiKey = _configuration.GetSection("OpenAI:Access:ApiKey").Value ??
                throw new SettingsException("OpenAI:Access:ApiKey not exists in appsettings");

            var qdrantApiKey = _configuration.GetSection("Qdrant:Access:Key").Value ??
                throw new SettingsException("Qdrant:Access:Key not exists in appsettings");

            var qdrantUrl = _configuration.GetSection("Qdrant:Access:Uri").Value ??
                throw new SettingsException("Qdrant:Access:Uri not exists in appsettings");

            var maxTokenFileChunked = _configuration.GetSection("Qdrant:MaxTokenFileChunked").Value ??
                throw new SettingsException("Qdrant:MaxTokenFileChunked not exists in appsettings");

            var maxSummaryFileCharacters = int.Parse(_configuration.GetSection("Qdrant:MaxSummaryFileCharacters").Value ??
                throw new SettingsException("Qdrant:MaxSummaryFileCharacters not exists in appsettings"));

            _qdrant.Setup(
                modelEmbedding: modelEmbedding,
                modelEmbeddingApiKey: modelApiKey,
                qdrantUri: qdrantUrl,
                qdrantApiKey: qdrantApiKey,
                qdrantCollectionName: collectionName,
                qdrantCollectionDistance: Distance.Cosine,
                qdrantCollectionSize: collectionSize,
                overwrite: true,
                user: User
                );

            var fileUUID = await _qdrant.AddAsync(
                document: document,
                conversationUuid: converationId,
                maxTokensPerLine: int.Parse(maxTokenFileChunked),
                maxSummaryCharacters: maxSummaryFileCharacters
                );

            return fileUUID.ToString();
        }
    }
}
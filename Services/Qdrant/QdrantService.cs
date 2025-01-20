using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using PersonalWebApi.Services.Services.Agent;
using System.Security.Claims;
using Qdrant.Client.Grpc;
using Elastic.Clients.Elasticsearch.IndexManagement;
using PersonalWebApi.Utilities.Utilities.Qdrant;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using PersonalWebApi.Services.FileStorage;
using PersonalWebApi.Exceptions;
using System.Collections;
using PersonalWebApi.Agent;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin;
using Microsoft.SemanticKernel.Process.Runtime;
using DocumentFormat.OpenXml.Wordprocessing;
using PersonalWebApi.Services.Agent;
using System;
using Microsoft.EntityFrameworkCore.Storage;
using PersonalWebApi.Processes.FileStorage.Steps;
using PersonalWebApi.Processes.Document.Steps;
using PersonalWebApi.Processes.Qdrant.Events;
using PersonalWebApi.Processes.Qdrant.Pipelines;
using iText.Commons.Utils;
using static Microsoft.KernelMemory.Constants.CustomContext;
using YamlDotNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Linq;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Services.Services.System;
using Microsoft.KernelMemory;
using PersonalWebApi.Agent.Memory.Observability;
using PersonalWebApi.Agent.SemanticKernel.Observability;
using PersonalWebApi.Services.NoSQLDB;
using PersonalWebApi.Services.Services.History;
using PersonalWebApi.Services.WebScrapper;
using PersonalWebApi.Utilities.WebScrapper;
using PersonalWebApi.Utilities.WebScrappers;

namespace PersonalWebApi.Services.Services.Qdrant
{
    public class QdrantService : IQdrantService
    {
        private readonly string _collectionName;

        private QdrantApi _qdrantApi { get; set; }
        private readonly IConfiguration _configuration;

        public QdrantService(
            IEmbedding embeddingOpenAi,
            IConfiguration configuration
            )
        {
            _configuration = configuration;
            _collectionName = configuration.GetSection("Qdrant:Container:Name").Value ?? throw new SettingsException("Qdrant:Container:Name not exists");

            var qdrantCollectionSize = ulong.Parse(configuration.GetSection("Qdrant:Container:Size").Value ??
                throw new SettingsException("Qdrant:Container:Size not exists in appsettings"));

            var qdrantApiKey = configuration.GetSection("Qdrant:Access:Key").Value ??
                throw new SettingsException("Qdrant:Access:Key not exists in appsettings");

            var qdrantUrl = configuration.GetSection("Qdrant:Access:Uri").Value ??
                throw new SettingsException("Qdrant:Access:Uri not exists in appsettings");

            _qdrantApi = new QdrantApi(embeddingOpenAi, qdrantUrl, qdrantApiKey, qdrantCollectionSize, Distance.Cosine);
        }

        public async Task AddAsync(
            string chunk,
            Dictionary<string, string> metadata, 
            Guid conversationUuid, 
            Guid fileId
            )
        {
            await _qdrantApi.CheckCollectionExists(_collectionName);
            await _qdrantApi.AddEmbeddingToQdrantAsync(_collectionName, chunk, metadata);
        }

        [Experimental("SKEXP0080")]
        public async Task<Guid> AddAsync(IFormFile document, Guid conversationUuid)
        {

            var fileUuid = Guid.NewGuid();

            QdrantPipelines qdrantPipelines = new QdrantPipelines();
            await qdrantPipelines.Add(
                QdrantPipelines.PrepareKelnerForPipeline(_configuration),
                new DocumentStepDto(fileUuid, document, conversationUuid, Guid.NewGuid()) { Overwrite = true }
                );

            return fileUuid;
        }


        /// <summary>
        /// Asynchronously searches for similar vectors in a Qdrant collection.
        /// </summary>
        /// <param name="collectionName">The name of the collection to search.</param>
        /// <param name="queries">The list of query strings to embed and search for.</param>
        /// <param name="filter">Optional filter to apply to the search.</param>
        /// <param name="limit">The maximum number of results to return.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of search results.</returns>
        /// <example>
        /// <code>
        /// public async Task ExampleSearchUsage()
        /// {
        ///     var qdrantFileService = new QdrantService(kernel, blobStorageService, documentReaderDocx, embeddingOpenAi);
        ///     qdrantFileService.Setup("modelEmbedding", "modelEmbeddingApiKey", "qdrantUri", "qdrantApiKey", "qdrantCollectionName", Distance.Cosine, 1000, true, user);
        ///     var results = await qdrantFileService.SearchAsync(new List<string> { "kto złamał nogę", "query2" }, null, 5);
        ///     foreach (var result in results)
        ///     {
        ///         Console.WriteLine($"Found result with ID: {result.Id}");
        ///     }
        /// }
        /// </code>
        /// </example>
        public async Task<List<Dictionary<string, object>>> SearchAsync(List<string> queries, Dictionary<string, string> filter = null, int limit = 5)
        {
            var searchResults = await Task.WhenAll(queries.Select(query =>
                _qdrantApi.SearchAsync(_collectionName, query, filter, limit)
            ));

            var results = new List<Dictionary<string, object>>();

            foreach (var result in searchResults.SelectMany(r => r))
            {
                var payload = result.Payload.ToDictionary(
                kvp => kvp.Key,
                    kvp => kvp.Value ?? ""
                );

                var searchResult = new Dictionary<string, object>
                {
                    { "Id", result.Id.Uuid },
                    { "Payload", payload },
                    { "Score", result.Score },
                    { "Version", result.Version.ToString() }
                };

                results.Add(searchResult);
            }

            return results;
        }
    }
}

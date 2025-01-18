using Qdrant.Client.Grpc;
using Qdrant.Client;
using System.Diagnostics.CodeAnalysis;
using PersonalWebApi.Services.Services.Agent;
using Microsoft.SemanticKernel;

namespace PersonalWebApi.Utilities.Utilities.Qdrant
{
    /// <summary>
    /// Provides methods to interact with the Qdrant vector database.
    /// </summary>
    public class QdrantApi
    {
        private readonly QdrantClient _qdrantClient;
        private readonly ulong _dimensions;
        private readonly Distance _distance;
        private readonly IEmbedding _embeddingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="QdrantApi"/> class.
        /// </summary>
        /// <param name="embedding">The embedding service to use.</param>
        /// <param name="_uri">The URI of the Qdrant server.</param>
        /// <param name="_key">The API key for the Qdrant server.</param>
        /// <param name="collectionSize">The size of the collection.</param>
        /// <param name="distance">The distance metric to use.</param>
        /// <exception cref="System.Exception">Thrown when the embedding service is not set up.</exception>
        public QdrantApi(IEmbedding embedding, string _uri, string _key, ulong collectionSize, Distance distance)
        {
            _embeddingService = embedding;
            _qdrantClient = new QdrantClient(
              host: _uri,
              https: true,
              apiKey: _key
            );

            _dimensions = collectionSize;
            _distance = distance;
        }

        /// <summary>
        /// Checks if a collection exists, and creates it if it does not.
        /// </summary>
        /// <param name="collectionName">The name of the collection to check.</param>
        public async Task CheckCollectionExists(string collectionName)
        {
            var collections = _qdrantClient.ListCollectionsAsync().Result;
            if (!collections.Contains(collectionName))
            {
                await _qdrantClient.CreateCollectionAsync(collectionName, new VectorParams { Size = _dimensions, Distance = _distance });
            }
        }

        /// <summary>
        /// Adds an embedding to a Qdrant collection.
        /// </summary>
        /// <param name="pointId">The ID of the point to add.</param>
        /// <param name="collectionName">The name of the collection to add the point to.</param>
        /// <param name="input">The input string to embed.</param>
        /// <param name="metadata">The metadata to associate with the point.</param>
        /// <returns>The result of the update operation.</returns>
        public async Task<UpdateResult> AddEmbeddingToQdrantAsync(string collectionName, string input, Dictionary<string, string> metadata)
        {
            await CheckCollectionExists(collectionName);

            var embeddedData = await _embeddingService.EmbeddingAsync(input, _dimensions);

            var point = new PointStruct
            {
                
                Id = new PointId { Uuid = Guid.NewGuid().ToString() },
                Vectors = new Vectors { Vector = new Vector { Data = { embeddedData.ToArray() } } }
            };

            foreach (var kvp in metadata)
            {
                point.Payload.Add(kvp.Key, new Value { StringValue = kvp.Value ?? "" });
            }
           
            var points = new List<PointStruct> { point };

            var response = await _qdrantClient.UpsertAsync(collectionName, points);

            return response;
        }


        /// <summary>
        /// Deletes a collection from Qdrant.
        /// </summary>
        /// <param name="collectionName">The name of the collection to delete.</param>
        public async Task DeleteCollectionAsync(string collectionName)
        {
            await _qdrantClient.DeleteCollectionAsync(collectionName);
        }

        /// <summary>
        /// Lists all collections in Qdrant.
        /// </summary>
        /// <returns>A list of collection names.</returns>
        public async Task<IReadOnlyList<string>> ListCollectionsAsync()
        {
            return await _qdrantClient.ListCollectionsAsync();
        }

        /// <summary>
        /// Searches for similar vectors in a Qdrant collection.
        /// </summary>
        /// <param name="collectionName">The name of the collection to search.</param>
        /// <param name="query">The query string to embed and search for.</param>
        /// <param name="filter">Optional filter to apply to the search.</param>
        /// <param name="limit">The maximum number of results to return.</param>
        /// <returns>A list of scored points that match the query.</returns>
        public async Task<IReadOnlyList<ScoredPoint>> SearchAsync(string collectionName, string query, Dictionary<string, string> filter = null, int limit = 5)
        {
            var vectors = await _embeddingService.EmbeddingAsync(query, _dimensions);

            Filter? qdrantFilter = null;

            if (filter != null)
            {
                qdrantFilter = new Filter();
                qdrantFilter.Must.AddRange(filter.Select(kvp => new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = kvp.Key,
                        Match = new Match
                        {
                            Keyword = kvp.Value
                        }
                    }
                }));
            }

            return await _qdrantClient.SearchAsync(
                collectionName,
                vectors,
                qdrantFilter,
                null,
                (ulong)limit);
        }

        /// <summary>
        /// Retrieves all points from a Qdrant collection.
        /// </summary>
        /// <param name="collectionName">The name of the collection to retrieve points from.</param>
        /// <param name="pointIds">The IDs of the points to retrieve.</param>
        /// <returns>A list of retrieved points.</returns>
        public async Task<IReadOnlyList<RetrievedPoint>> RetrievePointsAsync(string collectionName, Guid pointIds)
        {
            var retrivedPoint = await _qdrantClient.RetrieveAsync
            (
                collectionName: collectionName,
                id: pointIds,
                withVectors: true
            );

            return retrivedPoint;
        }
    }
}

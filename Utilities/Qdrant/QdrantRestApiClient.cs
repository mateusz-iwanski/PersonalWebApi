﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Utilities.Utilities.HttUtils;
using Qdrant.Client.Grpc;

namespace PersonalWebApi.Utilities.Utilities.Qdrant
{
    public class QdrantRestApiClient
    {
        private readonly string _key;
        private readonly string _uri;
        private readonly IApiClient _apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="QdrantRestApiClient"/> class.
        /// </summary>
        /// <param name="apiClient">The API client to use for HTTP requests.</param>
        /// <param name="configuration">The configuration to retrieve QdrantRestApiClient settings.</param>
        /// <exception cref="SettingsException">Thrown when the QdrantRestApiClient key or URI is not found in the configuration.</exception>
        public QdrantRestApiClient(IApiClient apiClient, IConfiguration configuration)
        {
            _apiClient = apiClient;

            _key = configuration.GetSection("Qdrant:Access:Key").Value ??
                throw new SettingsException("Qdrant:Access:Key not exists in appsettings");
            _uri = configuration.GetSection("Qdrant:Access:Uri").Value ??
                throw new SettingsException("Qdrant:Access:Uri not exists in appsettings");
        }

        /// <summary>
        /// Checks if a collection exists.
        /// </summary>
        /// <param name="collectionName">Name of the collection to check.</param>
        /// <returns>HTTP response message.</returns>
        /// <remarks>
        /// This method sends a GET request to check if a collection exists.
        /// </remarks>
        public async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var url = $"{_uri}/collections/{collectionName}/exists";


            try
            {
                var response = await _apiClient.GetWithApiKeyAsync(url, _key);
            }
            catch (HttpRequestException exception)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a collection with a default dense vector.
        /// </summary>
        /// <param name="collectionName">Name of the collection to create.</param>
        /// <param name="vectorSize">Size of the vector.</param>
        /// <param name="distance">Distance metric for the vector.</param>
        /// <returns>HTTP response message.</returns>
        /// <remarks>
        /// This method sends a PUT request to create a collection with a default dense vector.
        /// </remarks>
        public async Task<HttpResponseMessage> CreateCollectionAsync(
            string collectionName,
            ulong vectorSize,
            string distance)
        {
            var url = $"{_uri}/collections/{collectionName}";

            var requestBody = new
            {
                vectors = new
                {
                    size = vectorSize,
                    distance
                }
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            return await _apiClient.PutWithApiKeyAsync(url, _key, jsonRequestBody);
        }

        /// <summary>
        /// Retrieves all details from multiple points.
        /// </summary>
        /// <param name="collectionName">Name of the collection to retrieve from.</param>
        /// <param name="ids">List of point IDs to look for.</param>
        /// <param name="consistency">Define read consistency guarantees for the operation.</param>
        /// <param name="timeout">Overrides global timeout for this request. Unit is seconds.</param>
        /// <param name="shardKey">Specify in which shards to look for the points, if not specified - look in all shards.</param>
        /// <param name="withPayload">Select which payload to return with the response. Default is true.</param>
        /// <param name="withVector">Options for specifying which vector to include.</param>
        /// <returns>HTTP response message.</returns>
        /// <remarks>
        /// This method sends a POST request to retrieve details from multiple points in the collection.
        /// </remarks>
        public async Task<HttpResponseMessage> RetrievePointsAsync(
            string collectionName,
            List<object> ids,
            int? consistency = null,
            int? timeout = null,
            object shardKey = null,
            object withPayload = null,
            object withVector = null)
        {
            var url = $"{_uri}/collections/{collectionName}/points/retrieve";

            var requestBody = new
            {
                ids,
                consistency,
                timeout,
                shard_key = shardKey,
                with_payload = withPayload,
                with_vector = withVector
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            return await _apiClient.PostWithApiKeyAsync(url, _key, jsonRequestBody);
        }

        /// <summary>
        /// Performs the insert + update action on specified points. Any point with an existing {id} will be overwritten.
        /// </summary>
        /// <param name="jsonPointRepresentation">Qdrant point as json format</param>
        /// <remarks>
        /// This method sends a PUT request to upsert points in the collection.
        /// </remarks>
        public async Task<HttpResponseMessage> UpsertPointsAsync(string collectionName, string jsonPointRepresentation)
        {
            var url = $"{_uri}/collections/{collectionName}/points?wait=true";

            return await _apiClient.PutWithApiKeyAsync(url, _key, jsonPointRepresentation);
        }

        /// <summary>
        /// Performs the insert + update action on specified points. Any point with an existing {id} will be overwritten.
        /// </summary>
        /// <param name="collectionName">Name of the collection to update from.</param>
        /// <param name="points">List of points to upsert.</param>
        /// <param name="wait">If true, wait for changes to actually happen.</param>
        /// <param name="ordering">Define ordering guarantees for the operation.</param>
        /// <returns>HTTP response message.</returns>
        /// <remarks>
        /// This method sends a PUT request to upsert points in the collection.
        /// </remarks>
        public async Task<HttpResponseMessage> UpsertPointsAsync(
            string collectionName,
            List<object> points,
            bool? wait = null,
            string ordering = null)
        {
            var url = $"{_uri}/collections/{collectionName}/points";

            var requestBody = new
            {
                points,
                wait,
                ordering
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            return await _apiClient.PutWithApiKeyAsync(url, _key, jsonRequestBody);
        }

        /// <summary>
        /// Retrieves all details from a single point.
        /// </summary>
        /// <param name="collectionName">Name of the collection to retrieve from.</param>
        /// <param name="id">ID of the point to retrieve.</param>
        /// <param name="consistency">Define read consistency guarantees for the operation.</param>
        /// <returns>HTTP response message.</returns>
        /// <remarks>
        /// This method sends a GET request to retrieve details from a single point in the collection.
        /// </remarks>
        public async Task<HttpResponseMessage> RetrievePointAsync(
            string collectionName,
            object id,
            int? consistency = null)
        {
            var url = $"{_uri}/collections/{collectionName}/points/{id}";

            if (consistency.HasValue)
            {
                url += $"?consistency={consistency.Value}";
            }

            return await _apiClient.GetWithApiKeyAsync(url, _key);
        }

        /// <summary>
        /// Deletes specified points from the collection by filter.
        /// </summary>
        /// <param name="collectionName">Name of the collection to delete from.</param>
        /// <param name="filter">Filter to specify which points to delete.</param>
        /// <param name="wait">If true, wait for changes to actually happen.</param>
        /// <param name="ordering">Define ordering guarantees for the operation.</param>
        /// <param name="shardKey">Specify in which shards to delete the points, if not specified - delete in all shards.</param>
        /// <returns>HTTP response message.</returns>
        /// <remarks>
        /// This method sends a POST request to delete specified points from the collection based on the provided filter.
        /// </remarks>
        public async Task<HttpResponseMessage> DeletePointsByFilterAsync(
            string collectionName,
            object filter,
            bool? wait = null,
            string ordering = null,
            object shardKey = null)
        {
            var url = $"{_uri}/collections/{collectionName}/points/delete";

            var requestBody = new
            {
                filter,
                wait,
                ordering,
                shard_key = shardKey
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            return await _apiClient.PostWithApiKeyAsync(url, _key, jsonRequestBody);
        }

        /// <summary>
        /// Updates specified vectors on points. All other unspecified vectors will stay intact.
        /// </summary>
        /// <param name="collectionName">Name of the collection to update from.</param>
        /// <param name="points">List of points with named vectors to update.</param>
        /// <param name="wait">If true, wait for changes to actually happen.</param>
        /// <param name="ordering">Define ordering guarantees for the operation.</param>
        /// <returns>HTTP response message.</returns>
        /// <remarks>
        /// This method sends a PUT request to update specified vectors on points in the collection.
        /// </remarks>
        public async Task<HttpResponseMessage> UpdateVectorsAsync(
            string collectionName,
            List<object> points,
            bool? wait = null,
            string ordering = null)
        {
            var url = $"{_uri}/collections/{collectionName}/points/vectors";

            if (wait.HasValue)
            {
                url += $"?wait={wait.Value}";
            }

            if (!string.IsNullOrEmpty(ordering))
            {
                url += string.IsNullOrEmpty(url) ? "?" : "&";
                url += $"ordering={ordering}";
            }

            var requestBody = new
            {
                points
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            return await _apiClient.PutWithApiKeyAsync(url, _key, jsonRequestBody);
        }

        /// <summary>
        /// Deletes specified vectors from points based on the provided filter. All other unspecified vectors will stay intact.
        /// </summary>
        /// <param name="collectionName">Name of the collection to delete from.</param>
        /// <param name="filter">Filter to specify which points to delete vectors from.</param>
        /// <param name="vectors">List of vector names to delete.</param>
        /// <param name="wait">If true, wait for changes to actually happen.</param>
        /// <param name="ordering">Define ordering guarantees for the operation.</param>
        /// <param name="shardKey">Specify in which shards to delete the vectors, if not specified - delete in all shards.</param>
        /// <returns>HTTP response message.</returns>
        /// <remarks>
        /// This method sends a POST request to delete specified vectors from points in the collection based on the provided filter.
        /// </remarks>
        public async Task<HttpResponseMessage> DeleteVectorsByFilterAsync(
            string collectionName,
            object filter,
            List<string> vectors,
            bool? wait = null,
            string ordering = null,
            object shardKey = null)
        {
            var url = $"{_uri}/collections/{collectionName}/points/vectors/delete";

            var requestBody = new
            {
                filter,
                vectors,
                wait,
                ordering,
                shard_key = shardKey
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            return await _apiClient.PostWithApiKeyAsync(url, _key, jsonRequestBody);
        }

        /// <summary>
        /// Searches for points based on vector similarity and given filtering conditions.
        /// </summary>
        /// <param name="collectionName">Name of the collection to search in.</param>
        /// <param name="vector">Vector data for similarity search.</param>
        /// <param name="limit">Max number of results to return.</param>
        /// <param name="filter">Filtering conditions for the search.</param>
        /// <param name="consistency">Read consistency guarantees for the operation.</param>
        /// <param name="timeout">Overrides global timeout for this request. Unit is seconds.</param>
        /// <returns>HTTP response message.</returns>
        /// <remarks>
        /// This method sends a POST request to search for points based on vector similarity and given filtering conditions.
        /// </remarks>
        public async Task<HttpResponseMessage> SearchAsync(
            string collectionName,
            List<double> vector,
            int limit,
            object filter = null,
            int? consistency = null,
            int? timeout = null)
        {
            var url = $"{_uri}/collections/{collectionName}/points/search";

            var requestBody = new
            {
                vector,
                limit
                //filter,
                //consistency,
                //timeout
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            var response = await _apiClient.PostWithApiKeyAsync(url, _key, jsonRequestBody);

            return response;
        }
    }
}


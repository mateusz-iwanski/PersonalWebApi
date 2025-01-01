using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Services.HttpUtils;

namespace PersonalWebApi.Services.Services.Qdrant
{
    public class QdrantCloud
    {
        private readonly string _key;
        private readonly string _uri;
        private readonly IApiClient _apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="QdrantCloud"/> class.
        /// </summary>
        /// <param name="apiClient">The API client to use for HTTP requests.</param>
        /// <param name="configuration">The configuration to retrieve QdrantCloud settings.</param>
        /// <exception cref="SettingsException">Thrown when the QdrantCloud key or URI is not found in the configuration.</exception>
        public QdrantCloud(IApiClient apiClient, IConfiguration configuration)
        {
            _apiClient = apiClient;

            _key = configuration.GetSection("QdrantCloud:Key").Value ??
                throw new SettingsException("QdrantCloud Key not exists in appsettings");
            _uri = configuration.GetSection("QdrantCloud:Uri").Value ??
                throw new SettingsException("QdrantCloud Uri not exists in appsettings");
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
    }
}


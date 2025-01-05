using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace PersonalWebApi.Utilities.Utilities.HttUtils
{
    // EXAMPLES:
    // 1. Authorization by Bearer Token:
    // var apiClient = new ApiClient();
    // apiClient.SetAuthorizationHeader("Bearer", "your_token_here");
    // var response = await apiClient.GetAsync("https://api.example.com/data");
    //
    // 2. Authorization by QdrantController API Key:
    // var apiClient = new ApiClient();
    // var response = await apiClient.GetWithApiKeyAsync("http://localhost:6333/collections/collection_name/points/42", "your_api_key_here");

    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public ApiClient()
        {
            return;
        }

        /// <summary>
        /// Sends a POST request with JSON data. Optionally checks the status code of the response.
        /// </summary>
        public async Task<HttpResponseMessage> PostJsonAsync(string url, object data, string mediaType = "application/json", bool checkStatusCode = true)
        {
            var jsonContent = JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, mediaType);

            var response = await _httpClient.PostAsync(url, content);
            if (checkStatusCode) response.EnsureSuccessStatusCode();

            return response;
        }

        /// <summary>
        /// Sends a GET request to the specified URL and ensures the response status code is successful.
        /// </summary>
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return response;
        }

        /// <summary>
        /// Sends a POST request with a JSON string. Validates the JSON string before sending.
        /// </summary>
        public async Task<HttpResponseMessage> PostJsonAsync(string url, string dataRequest, string mediaType = "application/json")
        {
            try
            {
                JsonConvert.DeserializeObject<object>(dataRequest);
            }
            catch (JsonException)
            {
                throw new ArgumentException("The provided dataRequest is not a valid JSON string.");
            }

            var content = new StringContent(dataRequest, Encoding.UTF8, mediaType);

            var response = await _httpClient.PostAsync(url, content);

            response.EnsureSuccessStatusCode();

            return response;
        }

        /// <summary>
        /// Sends a POST request to upload a file along with additional model data.
        /// </summary>
        public async Task<HttpResponseMessage> PostFileAsync(string url, Stream fileStream, string fileName, string model, string mediaType = "multipart/form-data")
        {
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            content.Add(fileContent, "file", fileName);
            content.Add(new StringContent(model), "model");

            var response = await _httpClient.PostAsync(url, content);

            return response;
        }

        /// <summary>
        /// Reads the content of an HTTP response and deserializes it into the specified type.
        /// </summary>
        public async Task<T> ReadContentAsAsync<T>(HttpResponseMessage response)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// Sets the authorization header for the HTTP client using the specified scheme and token.
        /// </summary>
        public void SetAuthorizationHeader(string scheme, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
        }

        /// <summary>
        /// Sends a POST request with an API key included in the headers.
        /// </summary>
        public async Task<HttpResponseMessage> PostWithApiKeyAsync(string url, string apiKey, string rawData)
        {
            _httpClient.DefaultRequestHeaders.Clear(); // Clear existing headers
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
            var content = new StringContent(rawData, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            return response;
        }

        /// <summary>
        /// Sends a GET request with an API key included in the headers.
        /// </summary>
        public async Task<HttpResponseMessage> GetWithApiKeyAsync(string url, string apiKey)
        {
            _httpClient.DefaultRequestHeaders.Clear(); // Clear existing headers
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return response;
        }

        /// <summary>
        /// Sends a PUT request with an API key included in the headers.
        /// </summary>
        public async Task<HttpResponseMessage> PutWithApiKeyAsync(string url, string apiKey, string rawData)
        {
            _httpClient.DefaultRequestHeaders.Clear(); // Clear existing headers
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
            var content = new StringContent(rawData, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content);
            return response;
        }
    }
}



namespace PersonalWebApi.Services.HttpUtils
{
    public interface IApiClient
    {
        Task<HttpResponseMessage> GetAsync(string url);
        Task<HttpResponseMessage> GetWithApiKeyAsync(string url, string apiKey);
        Task<HttpResponseMessage> PostFileAsync(string url, Stream fileStream, string fileName, string model, string mediaType = "multipart/form-data");
        Task<HttpResponseMessage> PostJsonAsync(string url, object data, string mediaType = "application/json", bool checkStatusCode = true);
        Task<HttpResponseMessage> PostJsonAsync(string url, string dataRequest, string mediaType = "application/json");
        Task<HttpResponseMessage> PostWithApiKeyAsync(string url, string apiKey, string rawData);
        Task<HttpResponseMessage> PutWithApiKeyAsync(string url, string apiKey, string rawData);
        Task<T> ReadContentAsAsync<T>(HttpResponseMessage response);
        void SetAuthorizationHeader(string scheme, string token);
    }
}
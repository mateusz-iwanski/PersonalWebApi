using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Rest;
using PersonalWebApi.Exceptions;

namespace PersonalWebApi.Utilities.Kql
{
    public class KqlApplicationInsightsApi
    {
        private readonly ApplicationInsightsDataClient _client;
        private readonly string _applicationId;

        public KqlApplicationInsightsApi(IConfiguration configuration)
        {
            _applicationId = configuration.GetSection("Telemetry:ApplicationInsights:ApplicationId").Value ??
                throw new SettingsException("Telemetry:ApplicationInsights:ApplicationId not exists in settings");

            var apiKey = configuration.GetSection("Telemetry:ApplicationInsights:KqlClientApiKey").Value ??
                throw new SettingsException("Telemetry:ApplicationInsights:KqlClientApiKey not exists in settings");

            var credentials = new ApiKeyClientCredentials(apiKey);
            
            _client = new ApplicationInsightsDataClient(credentials);
        }

        public async Task<HttpOperationResponse<QueryResults>> ExecuteQueryAsync(string query)
        {
            return await _client.Query.ExecuteWithHttpMessagesAsync(_applicationId, query);
        }
    }
}

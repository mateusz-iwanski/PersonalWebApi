using DocumentFormat.OpenXml.Math;
using Google.Protobuf.WellKnownTypes;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Models.Models.Azure;

namespace PersonalWebApi.Models.Models.SemanticKernel
{
    // kiedy wywolywana jest OnPromptRenderAsync w RenderedPromptFilterHandler (IPromptRenderFilter) 
    // RenderedPromptFilterHandler wywoluje sie w momencie wywolania zapytania do agenta
    // Nastepnie wywoluje sie funkcja ktora uzywa zalaczonych do kernela np pluginow pobierając, szukajć dane etc.
    // wynik z takiej funkcji jest wysyłany do contextu agenta

    // klasa ta ma na celu zapisania informacji o samej funkcji ktora zostala wykonana
    public class FunctionExecutingHistoryDto : CosmosDbDtoBase
    {
        [JsonProperty(PropertyName = "inputArguments")]
        public Dictionary<string, string> InputArguments { get; set; }

        [JsonProperty(PropertyName = "functionName")]
        public string FunctionName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "pluginName")]
        public string PluginName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "functionDescription")]
        public string FunctionDescription { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "renderedPrompt")]
        public string RenderedPrompt { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "executionSettings ")]
        public List<Dictionary<string, string?>> ExecutionSettings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionExecutingHistoryDto"/> class.
        /// </summary>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <param name="sessionUuid">The unique identifier for the session.</param>
        /// <param name="inputArguments">The input arguments for the function.</param>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="functionDescription">The description of the function.</param>
        /// <param name="renderedPrompt">The rendered prompt.</param>
        /// <param name="usedPluginNames">The names of the used plugins.</param>
        /// <param name="status">The status of the function execution.</param>
        public FunctionExecutingHistoryDto(
            Guid conversationUuid, 
            Guid sessionUuid,
            Dictionary<string, string> inputArguments,
            string functionName,
            string pluginName,
            string functionDescription,
            string renderedPrompt,
            string status,
            List<Dictionary<string, string?>> executionSettings
            ) : base(conversationUuid, sessionUuid)
        {
            InputArguments = inputArguments;
            FunctionName = functionName;
            FunctionDescription = functionDescription;
            RenderedPrompt = renderedPrompt;
            Status = status;
            ExecutionSettings = executionSettings;
            PluginName = pluginName;
        }

        /// <summary>
        /// Gets the static container name for the Cosmos DB.
        /// This method returns the name of the container where chat history records are stored.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public static string ContainerNameStatic() => "history-function-executing";

        /// <summary>
        /// Gets the static partition key name for the Cosmos DB.
        /// This method returns the name of the partition key used to partition chat history records.
        /// </summary>
        /// <returns>The name of the partition key.</returns>
        public static string PartitionKeyNameStatic() => "/conversationUuid";

        /// <summary>
        /// Gets the container name for the Cosmos DB.
        /// This method overrides the base class method to return the specific container name for chat history.
        /// </summary>
        /// <returns>The name of the container.</returns>
        public override string ContainerName() => ContainerNameStatic();

        /// <summary>
        /// Gets the partition key name for the Cosmos DB.
        /// This method overrides the base class method to return the specific partition key name for chat history.
        /// </summary>
        /// <returns>The name of the partition key.</returns>
        public override string PartitionKeyName() => PartitionKeyNameStatic();

        /// <summary>
        /// Gets the partition key data for the Cosmos DB.
        /// This method returns the value of the partition key, which is the conversation UUID.
        /// </summary>
        /// <returns>The partition key data.</returns>
        public override string PartitionKeyData() => ConversationUuid.ToString();
    }
}

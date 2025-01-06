using LLama.Common;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using UglyToad.PdfPig.Logging;
using ChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;
using AuthorRole = Microsoft.SemanticKernel.ChatCompletion.AuthorRole;
using LLama.Batched;
using System.Net.Mime;
using Microsoft.KernelMemory.Pipeline;
using Amazon.SecurityToken.Model;
using PersonalWebApi.Services.Azure;
using Microsoft.Azure.Cosmos;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel.Services;
using Microsoft.Identity.Client;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Text.Json;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Text;
using Elastic.Clients.Elasticsearch;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using PersonalWebApi.Models.Azure;

namespace PersonalWebApi.Services.Services.History
{
    public class ChatCosmoDto
    {
        [Required]
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; } = Guid.NewGuid(); // Unique identifier. It can be GUID or uuid session from ai agent

        [Required]
        [JsonProperty(PropertyName = "uuid")]
        public string Uuid { get; set; } = string.Empty; // User Unique identifier from ai agent

        [Required]
        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; } = string.Empty; // URI of the content

        [Required]
        [JsonProperty(PropertyName = "conversationId")]
        public Guid ConversationId { get; set; }

        [Required]
        [JsonProperty(PropertyName = "requestId")]
        public Guid RequestId { get; set; }

        [Required]
        [JsonProperty(PropertyName = "modelId")]
        public string ModelId { get; set; } = string.Empty;

        [Required]
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; } = string.Empty;

        [Required]
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [JsonProperty(PropertyName = "mimeType")]
        public string MimeType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "source")]
        public string? Source { get; set; }

        [JsonProperty(PropertyName = "messageAdditionalMetadata")]
        public Dictionary<string, object?>? MessageAdditionalMetadata { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string? Name { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        [Required]
        [JsonProperty(PropertyName = "author")]
        public string Author { get; set; } = "Anonymous";

        [Required]
        [JsonProperty(PropertyName = "createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class ConcreteKernelContent : ChatMessageContent
    {
        // Implement the abstract members or interface methods
    }

    public interface IChatHistoryRepository
    {
        //Task Add(Guid conversationId, Guid requestId, string modelId, AuthorRole role, string message, MimeTypeCustom mimeType, string? source = null, Dictionary<string, object?>? messageAdditionalMetadata = null, string? name = null, string author = "Anonymous");
        //Task CollectChat(Guid conversationUuid);
        //Task GetChat(Guid conversationUuid);
        //Task<ItemResponse<object>> UploadDocumentAsync(DocumentHistory document, ActionDocument actionDocument, List<ActionHistory> actions);
    }

    public class ChatHistoryRepository : IChatHistoryRepository
    {
        private ChatHistory _chatHistory { get; set; }
        IKernelMemory _memory;  // test
        private Kernel _kernel;
        private readonly IConfiguration _configuration;

        private readonly string _cosmosConnectionString;

        public ChatHistoryRepository(IKernelMemory kernelMemory, Kernel kernel, IConfiguration configuration)
        {
            _memory = kernelMemory;
            _kernel = kernel;
            _configuration = configuration;

            _cosmosConnectionString = _configuration.GetConnectionString("PersonalApiDbCosmos");

            _chatHistory = new ChatHistory();
        }

        //public async Task CollectChat(Guid conversationUuid)
        //{
        //    SiteContentStoreCosmosDbDto siteContentStoreCosmosDbDto = new SiteContentStoreCosmosDbDto
        //    {
        //        //Id = Guid.NewGuid(),
        //        Uuid = Guid.NewGuid().ToString(),
        //        Domain = "Personal",
        //        Uri = "uri",
        //        Data = "data",
        //        Tags = new List<string> { "tag1", "tag2" },
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    CosmosDbContentStoreService cosmosDbContentStoreService = new CosmosDbContentStoreService(
        //        _cosmosConnectionString, "Personal", "containerTest", siteContentStoreCosmosDbDto.PartitionKey()) ;

        //    await cosmosDbContentStoreService.CreateItemAsync(siteContentStoreCosmosDbDto);


        //    //var pluginName = "memory";

        //    //var userMessage = "Kto złamał nogę";

        //    //await _memory.ImportTextAsync(userMessage);

        //    //string filePath = Path.Combine(AppContext.BaseDirectory, "bajka.docx");
        //    //await _memory.ImportDocumentAsync(filePath, tags: new() { { "bajka", "dziewczynka z mama" } });

        //    //var memoryPlugin = _kernel.ImportPluginFromObject(
        //    //    new MemoryPlugin(_memory, waitForIngestionToComplete: true),
        //    //    pluginName);

        //    //var skPrompt = """
        //    //    Question to Memory: {{$input}}

        //    //    Answer from Memory: {{memory.ask $input}}

        //    //    If the answer is empty look forward. If you find answer say 'I haven't in memory but ai found the answer - <answer>' otherwise reply with a preview of the answer,
        //    //    truncated to 15 words. Prefix with one emoji relevant to the content.
        //    //    """;


        //    //var myFunction = _kernel.CreateFunctionFromPrompt(skPrompt);

        //    //var answer = await myFunction.InvokeAsync(_kernel, userMessage);





        //    //var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        //    //var model = chatCompletionService.GetModelId() ?? "Unknown";

        //    //var sessionId = Guid.NewGuid();

        //    //await Add(conversationUuid, sessionId, model, AuthorRole.User, skPrompt, MimeTypeCustom.PlainText);
        //    //await Add(conversationUuid, sessionId, model, AuthorRole.Assistant, answer.ToString(), MimeTypeCustom.PlainText);


        //}

        //public async Task GetChat(Guid conversationUuid)
        //{
        //    var history = await _comsmosDBservice.GetByUuidAsync<ChatHistory>(conversationUuid.ToString());

        //    var pluginName = "memory";

        //    var userMessage = "Jak się nazywała babcia";

        //    await _memory.ImportTextAsync("chat_history", JsonConvert.SerializeObject(history));

        //    await _memory.ImportTextAsync(userMessage);


        //    var memoryPlugin = _kernel.ImportPluginFromObject(
        //       new MemoryPlugin(_memory, waitForIngestionToComplete: true),
        //       pluginName);

        //    var skPrompt = """
        //        Question to Memory: {{$input}}

        //        Answer from Memory: {{memory.ask $input}}

        //        If the answer is empty look forward. If you find answer say 'I haven't in memory but ai found the answer - <answer>' otherwise reply with a preview of the answer,
        //        truncated to 15 words. Prefix with one emoji relevant to the content.
        //        """;


        //    var myFunction = _kernel.CreateFunctionFromPrompt(skPrompt);

        //    var answer = await myFunction.InvokeAsync(_kernel, userMessage);

        //    var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        //    var model = chatCompletionService.GetModelId() ?? "Unknown";


        //    var sessionId = Guid.NewGuid();

        //    var requestId = Guid.NewGuid();
        //    await Add(conversationUuid, sessionId, model, AuthorRole.User, skPrompt, MimeTypeCustom.PlainText);
        //    await Add(conversationUuid, sessionId, model, AuthorRole.Assistant, answer.ToString(), MimeTypeCustom.PlainText);



        //}

        //public ChatHistory Get(Guid conversationId) 
        //{
        //    cosmosDbContentStoreService
        //}

        //public async Task<ItemResponse<object>> UploadDocumentAsync(DocumentHistory document, ActionDocument actionDocument, List<ActionHistory> actions)
        //{
        //    var history = new
        //    {
        //        id = Guid.NewGuid(),
        //        HistoryData = new
        //        {
        //            Document = document,
        //            ActionDocument = actionDocument,
        //            Actions = actions.Select(x => new
        //            {
        //                x.Id,
        //                x.ActionUuid,
        //                x.ToolType,
        //                x.Parameters,
        //                x.Sequence,
        //                x.Status,
        //                x.CreatedAt,
        //                x.UpdatedAt
        //            }).ToList()
        //        }
        //    };


        //    return await _comsmosDBservice.CreateItemAsync(history, new PartitionKey(Guid.NewGuid().ToString()));

        //}

        //public async Task Add(
        //    Guid conversationId,
        //    Guid requestId,
        //    string modelId,
        //    AuthorRole role,
        //    string message,
        //    MimeTypeCustom mimeType,
        //    string? source = null,
        //    Dictionary<string, object?>? messageAdditionalMetadata = null,
        //    string? name = null,
        //    string author = "Anonymous"
        //    )
        //{
        //    var mess = new ChatMessageContent()
        //    {
        //        Role = role,
        //        MimeType = mimeType.GetMimeType(),
        //        Metadata = metaDataBuild(
        //            conversationId: conversationId,
        //            sessionId: requestId,
        //            modelId: modelId,
        //            author: author,
        //            additionalMetaData: messageAdditionalMetadata,
        //            source: source,
        //            name: name
        //            ),
        //        Items = new ChatMessageContentItemCollection
        //        {
        //            new TextContent {
        //                Text = message,
        //            },
        //        }
        //    };
        //    // how to get 
        //    _chatHistory.Add(mess);

        //    var response = new ChatCosmoDto
        //    {
        //        Uri = "ss",
        //        Uuid = conversationId.ToString(),
        //        Role = role.ToString(),
        //        MimeType = mimeType.GetMimeType(),
        //        Metadata = (Dictionary<string, object>)metaDataBuild(
        //            conversationId: conversationId,
        //            sessionId: requestId,
        //            modelId: modelId,
        //            author: author,
        //            additionalMetaData: messageAdditionalMetadata,
        //            source: source,
        //            name: name
        //            ),
        //        Items = new ChatMessageContentItemCollection
        //        {
        //            new TextContent {
        //                Text = message,
        //            },
        //        }

        //    };


        //    await _comsmosDBservice.CreateItemAsync(response);
        //}

        //private IReadOnlyDictionary<string, object?> metaDataBuild(
        //    Guid conversationId,  // whole coversation id
        //    Guid sessionId,  // one response request id
        //    string modelId,
        //    string author,
        //    Dictionary<string, object?>? additionalMetaData = null,
        //    string? source = null,
        //    string? name = null)
        //{
        //    var metadata = new Dictionary<string, object?>
        //            {
        //                { "uuid", Guid.NewGuid().ToString() },
        //                { "conversation_uuid", conversationId.ToString() },
        //                { "sessionId", sessionId },
        //                { "modelId", modelId },
        //                { "createdAt", DateTime.Now.ToString() },
        //                { "source", source },
        //                { "name", name },
        //                { "author", author },
        //            };

        //    if (additionalMetaData != null)
        //    {
        //        foreach (var item in additionalMetaData)
        //        {
        //            metadata[item.Key] = item.Value;
        //        }
        //    }

        //    return metadata;
        //}
    }
}

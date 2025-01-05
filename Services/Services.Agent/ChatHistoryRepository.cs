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

namespace PersonalWebApi.Services.Services.Agent
{
    public class ChatHistoryRepository
    {
        private ChatHistory _chatHistory { get; set; }
        private readonly ICosmosDbContentStoreService _comsmosDBservice;

        public ChatHistoryRepository(ICosmosDbContentStoreService comsmosDBservice)
        {
            _comsmosDBservice = comsmosDBservice;
        }

        //public ChatHistory Get(Guid conversationId) 
        //{
        //    cosmosDbContentStoreService
        //}

        public async Task<ItemResponse<object>> UploadDocumentAsync(DocumentHistory document, ActionDocument actionDocument, List<ActionHistory> actions)
        {
            var history = new
            {
                id = Guid.NewGuid(),
                HistoryData = new
                {
                    Document = document,
                    ActionDocument = actionDocument,
                    Actions = actions.Select(x => new
                    {
                        x.Id,
                        x.ActionUuid,
                        x.ToolType,
                        x.Parameters,
                        x.Sequence,
                        x.Status,
                        x.CreatedAt,
                        x.UpdatedAt
                    }).ToList()
                }
            };

            return await _comsmosDBservice.CreateItemAsync(history);

        }

        public void Add(
            Guid conversationId,
            string modelId,
            AuthorRole role,
            string message,
            MimeTypeCustom mimeType,
            string? source = null,
            Dictionary<string, object?>? messageAdditionalMetadata = null,
            string? name = null,
            string author = "Anonymous"
            )
        {
            // how to get 
            _chatHistory.Add(new ChatMessageContent()
            {
                Role = role,
                MimeType = MimeTypeCustomExtensions.GetMimeType(mimeType),
                Metadata = metaDataBuild(
                    conversationId: conversationId,
                    modelId: modelId,
                    author: author,
                    additionalMetaData: messageAdditionalMetadata,
                    source: source,
                    name: name
                    ),
                Items = new ChatMessageContentItemCollection
                {
                    new TextContent {
                        Text = message,
                    },
                }
            });

            _comsmosDBservice.CreateItemAsync(_chatHistory);
        }

        private IReadOnlyDictionary<string, object?> metaDataBuild(
            Guid conversationId,
            string modelId,
            string author,
            Dictionary<string, object?>? additionalMetaData = null,
            string? source = null,
            string? name = null)
        {
            var metadata = new Dictionary<string, object?>
                    {
                        { "uuid", Guid.NewGuid().ToString() },
                        { "conversation_uuid", conversationId.ToString() },
                        { "modelId", modelId },
                        { "createdAt", DateTime.Now.ToString() },
                        { "source", source },
                        { "name", name },
                        { "author", author }
                    };

            if (additionalMetaData != null)
            {
                foreach (var item in additionalMetaData)
                {
                    metadata[item.Key] = item.Value;
                }
            }

            return metadata;
        }
    }
}

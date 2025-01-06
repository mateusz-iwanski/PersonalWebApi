using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using PersonalWebApi.Services.Azure;
using System.Diagnostics.CodeAnalysis;
using PersonalWebApi.Services.Services.Agent;
using System.Security.Claims;
using Qdrant.Client.Grpc;
using Elastic.Clients.Elasticsearch.IndexManagement;
using PersonalWebApi.Utilities.Utilities.Qdrant;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using PersonalWebApi.Utilities.Utilities.Models;

namespace PersonalWebApi.Services.Services.Qdrant
{
    public class QdrantFileService : IQdrantFileService
    {
        private readonly IBlobStorageService _blobStorage;
        private readonly IDocumentReaderDocx _documentReaderDocx;
        private readonly Kernel _kernel;
        private readonly IEmbedding _embeddingOpenAi;

        private QdrantApi _qdrantApi { get; set; }
        private ClaimsPrincipal _user { get; set; }
        private string _modelEmbedding { get; set; }
        private string _qdrantCollectionName { get; set; }
        private ulong _qdrantCollectionSize { get; set; }
        private bool _overwrite { get; set; }

        public QdrantFileService(
            Kernel kernel,
            IBlobStorageService blobStorageService,
            IDocumentReaderDocx documentReaderDocx,
            IEmbedding embeddingOpenAi
            )
        {
            _blobStorage = blobStorageService;
            _documentReaderDocx = documentReaderDocx;
            _kernel = kernel;
            _embeddingOpenAi = embeddingOpenAi;
        }

        /// <summary>
        /// Sets up the QdrantFileService with the necessary parameters.
        /// </summary>
        /// <param name="modelEmbedding">The model embedding to use.</param>
        /// <param name="modelEmbeddingApiKey">The API key for the model embedding.</param>
        /// <param name="qdrantUri">The URI of the Qdrant server.</param>
        /// <param name="qdrantApiKey">The API key for the Qdrant server.</param>
        /// <param name="qdrantCollectionName">The name of the Qdrant collection.</param>
        /// <param name="qdrantCollectionDistance">The distance metric for the Qdrant collection.</param>
        /// <param name="qdrantCollectionSize">The size of the Qdrant collection.</param>
        /// <param name="overwrite">Whether to overwrite existing files.</param>
        /// <param name="user">The user making the request.</param>
        public void Setup(
            string modelEmbedding,
            string modelEmbeddingApiKey,
            string qdrantUri,
            string qdrantApiKey,
            string qdrantCollectionName,
            Distance qdrantCollectionDistance,
            ulong qdrantCollectionSize,
            bool overwrite,
            ClaimsPrincipal user
            )
        {
            _modelEmbedding = modelEmbedding;
            _qdrantCollectionName = qdrantCollectionName;
            _qdrantCollectionSize = qdrantCollectionSize;
            _overwrite = overwrite;

            _user = user;

            _embeddingOpenAi.Setup(modelEmbedding, modelEmbeddingApiKey);

            _qdrantApi = new QdrantApi(_embeddingOpenAi, qdrantUri, qdrantApiKey, _qdrantCollectionSize, qdrantCollectionDistance);
        }

        /// <summary>
        /// Asynchronously adds a file to the Qdrant database.
        /// First, the file is uploaded to blob storage with metadata including FileId.
        /// Then, the file is added to Qdrant with the same FileId in the payload data.
        /// Embeddings and metadata with generated tags, summaries, and other useful information are included.
        /// </summary>
        /// <param name="document">The document to be added.</param>
        /// <param name="conversationUuid">The UUID of the conversation associated with the document.</param>
        /// <param name="maxTokensPerLine">Maximum number of tokens per line.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the UUID of the added file.</returns>
        /// <remarks>
        /// This method uses the Semantic Kernel to chunk the text and generate tags and summaries for each chunk.
        /// The generated tags and summaries are then added to the Qdrant database along with the embeddings.
        /// </remarks>
        /// <example>
        /// <code>
        /// public async Task ExampleUsage(IFormFile document, Guid conversationUuid)
        /// {
        ///     var qdrantFileService = new QdrantFileService(kernel, blobStorageService, documentReaderDocx, embeddingOpenAi);
        ///     qdrantFileService.Setup("modelEmbedding", "modelEmbeddingApiKey", "qdrantUri", "qdrantApiKey", "qdrantCollectionName", Distance.Cosine, 1000, true, user);
        ///     var fileUuid = await qdrantFileService.AddAsync(document, conversationUuid);
        ///     Console.WriteLine($"File added with UUID: {fileUuid}");
        /// }
        /// </code>
        /// </example>
        [Experimental("SKEXP0050")]  // for SemanticKernelTextChunker
        public async Task<Guid> AddAsync(IFormFile document, Guid conversationUuid, int maxTokensPerLine=200, int maxSummaryCharacters = 100)
        {
            var fileUuid = Guid.NewGuid();

            var uri = await _blobStorage.UploadToLibraryAsync(document, _overwrite, fileId: fileUuid.ToString());
            var reader = await _documentReaderDocx.ReadAsync(uri);

            var chunker = new SemanticKernelTextChunker(_modelEmbedding);
            var chunks = chunker.ChunkText(conversationUuid.ToString(), maxTokensPerLine, reader);

            var chat = _kernel.GetRequiredService<IChatCompletionService>();

            string authorName = DocumentReaderBase.GetAuthorNameFromDocument(document);

            await _qdrantApi.CheckCollectionExists(_qdrantCollectionName);

            var tasks = chunks.Select(async chunk =>
            {
                var tagAsString = await chat.GetChatMessageContentAsync(
                    @$"""Generate tags for the following text in a comma-separated list format.

                    <text>
                    {chunk.line}
                    </text>

                    Output must be a comma-separated list of tags. If there are no tags, return an empty list.

                    <output>
                    tag1, tag2
                    </output>

                    """);

                var summary = await chat.GetChatMessageContentAsync(
                    $@"""

                    Summarize the following text in no more than {maxSummaryCharacters} characters for use in a vector database. 
                    The summary must remain in the same language as the original text, focusing on semantic clarity and key ideas that 
                    can aid similarity search. 
                        
                    <text> 
                    {chunk.line}    
                    </text>

                    """);

                var tagList = tagAsString.Content.Split(", ").ToList();

                await _qdrantApi.AddEmbeddingToQdrantAsync(Guid.NewGuid(), _qdrantCollectionName, chunk.line, new Dictionary<string, object>
                {
                    { "Title", Path.GetFileNameWithoutExtension(document.FileName) },
                    { "Author", authorName },
                    { "Text", chunk.line },
                    { "CreatedAt", DateTime.Now.ToString("o") },
                    { "UploadedBy", _user.FindFirstValue(ClaimTypes.Name) ?? "Unknown" },
                    { "FileName", document.FileName },
                    { "ConversationId", conversationUuid },
                    { "BlobUri", uri.ToString() },
                    { "FileId", fileUuid.ToString() },
                    { "MimeType", document.ContentType },
                    { "Tags", string.Join(", ", tagList) },
                    { "Summary", summary.Content },
                    { "EmbeddingModel", _modelEmbedding },
                    { "StartPosition", chunk.startPosition },
                    { "EndPosition", chunk.endPosition },
                    { "DataType", QdrantDataType.Document }
                });
            });

            await Task.WhenAll(tasks);

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
        ///     var qdrantFileService = new QdrantFileService(kernel, blobStorageService, documentReaderDocx, embeddingOpenAi);
        ///     qdrantFileService.Setup("modelEmbedding", "modelEmbeddingApiKey", "qdrantUri", "qdrantApiKey", "qdrantCollectionName", Distance.Cosine, 1000, true, user);
        ///     var results = await qdrantFileService.SearchAsync("collectionName", new List<string> { "kto złamał nogę", "query2" }, null, 5);
        ///     foreach (var result in results)
        ///     {
        ///         Console.WriteLine($"Found result with ID: {result.Id}");
        ///     }
        /// }
        /// </code>
        /// </example>
        public async Task<List<QdrantFileSearchResultType>> SearchAsync(string collectionName, List<string> queries, Dictionary<string, string> filter = null, int limit = 5)
        {
            var searchResults = await Task.WhenAll(queries.Select(query =>
                _qdrantApi.SearchAsync(collectionName, query, filter, limit)
            ));

            var results = new List<QdrantFileSearchResultType>();

            foreach (var result in searchResults.SelectMany(r => r))
            {
                var searchResult = new QdrantFileSearchResultType
                {
                    Id = result.Id.Uuid,
                    Payload = new QdrantFilePayloadType
                    {
                        BlobUri = result.Payload["BlobUri"].StringValue,
                        Text = result.Payload["Text"].StringValue,
                        ConversationId = result.Payload["ConversationId"].StringValue,
                        EndPosition = result.Payload["EndPosition"].StringValue,
                        UploadedBy = result.Payload["UploadedBy"].StringValue,
                        EmbeddingModel = result.Payload["EmbeddingModel"].StringValue,
                        StartPosition = result.Payload["StartPosition"].StringValue,
                        Author = result.Payload["Author"].StringValue,
                        FileName = result.Payload["FileName"].StringValue,
                        CreatedAt = result.Payload["CreatedAt"].StringValue,
                        FileId = result.Payload["FileId"].StringValue,
                        Summary = result.Payload["Summary"].StringValue,
                        Tags = result.Payload["Tags"].StringValue,
                        Title = result.Payload["Title"].StringValue,
                        MimeType = result.Payload["MimeType"].StringValue,
                        DataType = Enum.Parse<QdrantDataType>(result.Payload["DataType"].StringValue)
                    },
                    Score = result.Score,
                    Version = result.Version.ToString()
                };

                results.Add(searchResult);
            }

            return results;
        }
    }
}

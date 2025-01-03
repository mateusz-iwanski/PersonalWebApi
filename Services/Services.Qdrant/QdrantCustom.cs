using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Newtonsoft.Json;
using PersonalWebApi.Services.Azure;
using PersonalWebApi.Services.Services.DocumentReaders;
using PersonalWebApi.Services.Services.LLMIntegrations;
using System.Diagnostics.CodeAnalysis;
using DocumentFormat.OpenXml.Packaging;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Google.Protobuf.Collections;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.KernelMemory;

namespace PersonalWebApi.Services.Services.Qdrant
{
    public record ExampleData(
    CollectionData Collection,
    List<VectorData> Vectors
    //SearchQueryData SearchQuery
    );

    public record CollectionData(
        string Name
        );

    public record VectorData(
        int Id,
        List<float> Vector,
        PayloadData Payload
    );

    public record PayloadData(
        string Title,
        string UserUpload,
        string Author,
        DateTime Date,
        string FileName,
        string ConversationId,
        string FileId,  // connectio with blob storage
        List<string> Tags,
        string Summary,
        string EmbeddingModel,
        DateTime EmbeddingDate,
        int StartPosition,
        int EndPosition
    );

    public record SearchQueryData(
        List<float> Vector,
        int Top,
        FilterData Filter
    );

    public record FilterData(
        string Author
    );


    public class QdrantCustom : IQdrant
    {
        private readonly IBlobStorageService _blobStorage;
        private readonly IDocumentReaderDocx _documentReaderDocx;
        private readonly Kernel _kernel;
        private readonly QdrantCloud _qdrantCloud;

        public record ChunkWithEmbedding(string Id, DateTime DateTime, string fileName, string ContentType, string ContentDisposition, string Headers, string Line, IList<ReadOnlyMemory<float>> Embeddings, int startPosition, int endPosition);

        public QdrantCustom(Kernel kernel, IBlobStorageService blobStorageService, IDocumentReaderDocx documentReaderDocx, QdrantCloud qdrantCloud)
        {
            _blobStorage = blobStorageService;
            _documentReaderDocx = documentReaderDocx;
            _kernel = kernel;
            _qdrantCloud = qdrantCloud;
        }


        /// <summary>
        /// Add file to qdrant database.
        /// First add to blob storage with metada included FileId, after add to qdrant with the same FileId in pyaload data.
        /// Embeddingd + metadata with generated tags, summaries and rest of usefull information 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="userName"></param>
        /// <param name="overwrite">If file can be overwrite on blob storare set to true</param>
        /// <returns></returns>
        [Experimental("SKEXP0050")]
        public async Task<string> AddToQdrant(
            IFormFile document,
            string userName,
            string collectionName,
            string collectionDistance,
            int collectionSize,
            string modelEmbedding,
            bool overwrite = true)
        {
            Guid fileUUID = Guid.NewGuid();
            var points = new List<object>();

            var uri = await _blobStorage.UploadToLibraryAsync(document, overwrite, fileId: fileUUID.ToString());
            var reader = await _documentReaderDocx.ReadAsync(uri);

            SemanticKernelTextChunker s = new SemanticKernelTextChunker(modelEmbedding);
            var chunked = s.ChunkText("newId", 100, reader);

            var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

            var chat = _kernel.GetRequiredService<IChatCompletionService>();

            var vectors = new List<VectorData>();

            string authorName = GetAuthorNameFromDocument(document);

            foreach (var chunk in chunked)
            {
                var embeddings = await embeddingGenerator.GenerateEmbeddingsAsync(new List<string> { chunk.line });

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
                    Give me summary. Summary must be short in the same language.
                        
                    <text> 
                    {chunk.line}    
                    </text> 
                    """);

                var tagList = tagAsString.Content.Split(", ").ToList();

                var payload = new PayloadData(
                    Path.GetFileNameWithoutExtension(document.FileName), 
                    userName,
                    authorName, // Replace with actual author if available
                    DateTime.Now,
                    document.FileName,
                    chunk.conversationId,
                    fileUUID.ToString(),
                    tagList, // Replace with actual tags if available
                    summary.Content,
                    modelEmbedding,
                    DateTime.Now,
                    chunk.startPosition,
                    chunk.endPosition
                );

                var vectorData = new VectorData(
                    chunk.conversationId.GetHashCode(), // Use a unique identifier
                    embeddings.SelectMany(e => e.ToArray()).ToList(),
                    payload
                );

                string serializedPoint = JsonConvert.SerializeObject(vectorData);

                points.Add(new
                {
                    id = (ulong)new Random().Next(), //new PointId { Uuid = fileUUID.ToString() },
                    vector = embeddings.SelectMany(e => e.ToArray()).ToList(),
                    payload = new
                    {
                        Title = payload.Title,
                        Author = payload.Author,
                        Date = payload.Date.ToString("o"),
                        FileName = payload.FileName,
                        ConversationId = payload.ConversationId,
                        FileId = payload.FileId,
                        Tags = string.Join(", ", payload.Tags),
                        Summary = payload.Summary,
                        EmbeddingModel = payload.EmbeddingModel,
                        EmbeddingDate = payload.EmbeddingDate.ToString("o"),
                        StartPosition = payload.StartPosition,
                        EndPosition = payload.EndPosition
                    }
                });
            }
            
            var response = await addToPointToQdrant(collectionName, collectionSize, collectionDistance, points);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent; //json;
           
        }

        public async Task<HttpResponseMessage> addToPointToQdrant(string collectionName, int collectionSize, string collectionDistance, List<object> pointsList)
        {
            var isExists = await _qdrantCloud.CollectionExistsAsync(collectionName);

            if (!isExists)
            {
                await _qdrantCloud.CreateCollectionAsync(collectionName, vectorSize: collectionSize, distance: collectionDistance);
            }

            var pointsObject = new { points = pointsList };
            var pointToJson = JsonConvert.SerializeObject(pointsObject);

            var response = await _qdrantCloud.UpsertPointsAsync(collectionName, pointToJson);

            return response;
        }

        private string GetAuthorNameFromDocument(IFormFile document)
        {
            using (var stream = document.OpenReadStream())
            {
                using (var wordDocument = WordprocessingDocument.Open(stream, false))
                {
                    var coreProperties = wordDocument.PackageProperties;
                    return coreProperties.Creator;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI.Embeddings;
using OpenAI;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PersonalWebApi.Exceptions;

namespace PersonalWebApi.Services.Services.Agent
{
    public class EmbeddingOpenAi : IEmbedding
    {
        private EmbeddingClient _client;

        public EmbeddingOpenAi(IConfiguration configuration)
        {
            var model = configuration.GetSection("Qdrant:Container:OpenAiModelEmbedding").Value ??
                throw new SettingsException("Qdrant:Container:Name can't find in appsettings");
            var apiKey = configuration.GetSection("Qdrant:Container:ModelEmbeddingApiKey").Value ??
                throw new SettingsException("Qdrant:Container:ModelEmbeddingApiKey can't find in appsettings");

            _client = new EmbeddingClient(
                model: model,
                apiKey: apiKey
            );
        }

        /// <summary>
        /// By default, the length of the embedding vector will be 1536 when using the text-embedding-3-small 
        /// model or 3072 when using the text-embedding-3-large model. 
        /// Generally, larger embeddings perform better, but using them also tends to cost more in terms 
        /// of compute, memory, and storage. 
        /// You can reduce the dimensions of the embedding by creating an instance of the EmbeddingGenerationOptions class, 
        /// setting the Dimensions property, and passing it as an argument in your call to the GenerateEmbedding
        /// </summary>
        /// <param name="input"></param>
        /// <param name="dimensions"></param>
        /// <returns></returns>
        public async Task<ReadOnlyMemory<float>> EmbeddingAsync(string input, ulong dimensions)
        {
            if (_client == default)
            {
                throw new InvalidOperationException("The Embedding client is not initialized. Call Setup method first.");
            }

            EmbeddingGenerationOptions options = new() { Dimensions = (int)dimensions };

            OpenAIEmbedding embedding = _client.GenerateEmbedding(input, options);
            ReadOnlyMemory<float> vector = embedding.ToFloats();

            return vector.ToArray();
        }
    }
}

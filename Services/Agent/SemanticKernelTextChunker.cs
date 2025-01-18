using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel.Text;
using Newtonsoft.Json;
using PersonalWebApi.Processes.Qdrant.Models;
using SQLitePCL;

namespace PersonalWebApi.Services.Agent
{
    

    /// <summary>
    /// Provides functionality to chunk text into smaller segments based on token count.
    /// </summary>
    [Experimental("SKEXP0050")]
    public class SemanticKernelTextChunker : ITextChunker
    {
        private Tokenizer _tokenizer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticKernelTextChunker"/> class.
        /// </summary>
        /// <param name="tiktokenizerModel">The model to be used by the tokenizer.</param>
        public SemanticKernelTextChunker() { }

        /// <summary>
        /// Object is injected, after initialization has to be setup
        /// </summary>
        /// <param name="tiktokenierModel"></param>
        public void Setup(string tiktokenierModel) => _tokenizer = TiktokenTokenizer.CreateForModel(tiktokenierModel);

        /// <summary>
        /// Checks if the client is set up.
        /// </summary>
        /// <returns>True if the client is set up, otherwise false.</returns>
        public bool IsSetup() => _tokenizer != default;

        /// <summary>
        /// Chunks the given text into smaller segments based on the maximum number of tokens per line.
        /// </summary>
        /// <param name="conversationId">The ID of the conversation.</param>
        /// <param name="maxTokensPerLine">The maximum number of tokens allowed per line.</param>
        /// <param name="text">The text to be chunked.</param>
        /// <returns>A list of <see cref="StringChunkerFormat"/> containing the chunked text segments.</returns>
        /// <example>
        /// <code>
        /// var chunker = new SemanticKernelTextChunker("model-name");
        /// var chunks = chunker.ChunkText("conversation123", 50, "This is a sample text to be chunked.");
        /// foreach (var chunk in chunks)
        /// {
        ///     Console.WriteLine($"{chunk.startPosition}-{chunk.endPosition}: {chunk.line}");
        /// }
        /// </code>
        /// </example>
        /// <remarks>
        /// This method splits the text into lines where each line contains a maximum number of tokens specified by <paramref name="maxTokensPerLine"/>.
        /// </remarks>
        public List<DocumentChunkerDto> ChunkText(int maxTokensPerLine, string text)
        {
            if (IsSetup() == false) throw new InvalidOperationException("The TextChunker is not initialized. Call Setup method first.");

            var lines = getChunk(text, maxTokensPerLine);

            var result = new List<DocumentChunkerDto>();

            int startPosition = 0;

            foreach (var line in lines)
            {
                int endPosition = startPosition + line.Length;
                result.Add(
                    new DocumentChunkerDto()
                    {
                        StartPosition = startPosition,
                        EndPosition = endPosition,
                        Content = line
                    }
                );

                startPosition = endPosition + 1; // +1 to account for the newline character
            }

            return result;
        }

        /// <summary>
        /// Splits the given text into lines based on the maximum number of tokens per line.
        /// </summary>
        /// <param name="chunkedText">The text to be chunked.</param>
        /// <param name="maxTokensPerLine">The maximum number of tokens allowed per line.</param>
        /// <returns>An enumerable of chunked text lines.</returns>
        private IEnumerable<string> getChunk(string chunkedText, int maxTokensPerLine)
        {
            if (IsSetup() == false) throw new InvalidOperationException("The TextChunker is not initialized. Call Setup method first.");

            var lines = TextChunker.SplitPlainTextLines(chunkedText, maxTokensPerLine, text => _tokenizer.CountTokens(text));

            foreach (var line in lines)
            {
                yield return line;
            }
        }
    }
}

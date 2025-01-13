﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel.Text;
using Newtonsoft.Json;
using SQLitePCL;

namespace PersonalWebApi.Utilities.Utilities.Models
{
    /// <summary>
    /// Provides functionality to chunk text into smaller segments based on token count.
    /// </summary>
    [Experimental("SKEXP0050")]
    public class SemanticKernelTextChunker
    {
        private readonly Tokenizer _tokenizer;

        /// <summary>
        /// Represents a chunk of text with metadata.
        /// </summary>
        /// <param name="conversationId">The ID of the conversation.</param>
        /// <param name="startPosition">The start position of the chunk in the original text.</param>
        /// <param name="endPosition">The end position of the chunk in the original text.</param>
        /// <param name="line">The chunked line of text.</param>
        public record StringChunkerFormat(string conversationId, int startPosition, int endPosition, string line);

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticKernelTextChunker"/> class.
        /// </summary>
        /// <param name="tiktokenizerModel">The model to be used by the tokenizer.</param>
        public SemanticKernelTextChunker(string tiktokenizerModel)
        {
            _tokenizer = TiktokenTokenizer.CreateForModel(tiktokenizerModel);
        }

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
        public List<StringChunkerFormat> ChunkText(string conversationId, int maxTokensPerLine, string text)
        {
            var lines = getChunk(text, maxTokensPerLine);

            var result = new List<StringChunkerFormat>();

            int startPosition = 0;

            foreach (var line in lines)
            {
                int endPosition = startPosition + line.Length;
                result.Add(
                    new StringChunkerFormat(conversationId, startPosition, endPosition, line)
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
            var lines = TextChunker.SplitPlainTextLines(chunkedText, maxTokensPerLine, text => _tokenizer.CountTokens(text));

            foreach (var line in lines)
            {
                yield return line;
            }
        }
    }
}
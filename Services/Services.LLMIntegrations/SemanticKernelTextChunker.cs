using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel.Text;
using Newtonsoft.Json;
using SQLitePCL;

namespace PersonalWebApi.Services.Services.LLMIntegrations
{
    [Experimental("SKEXP0050")]
    public class SemanticKernelTextChunker
    {
        private readonly Tokenizer _tokenizer;
        public record StringChunkerFormat(string conversationId, int startPosition, int endPosition, string line);

        public SemanticKernelTextChunker(string tiktokenizerModel)
        {
            _tokenizer = TiktokenTokenizer.CreateForModel(tiktokenizerModel);
        }
        
        public List<StringChunkerFormat> ChunkText(string conversationId, int maxTokensPerLine, string text)
        {
            Console.WriteLine("=== Text chunking with a custom token counter ===");

            var sw = new Stopwatch();
            sw.Start();

            var lines = getChunk(text, maxTokensPerLine);

            sw.Stop();
            Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");

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

        private IEnumerable<string> getChunk(string chunkedText, int maxTokensPerLine)
        {
            var lines = TextChunker.SplitPlainTextLines(chunkedText, maxTokensPerLine, text => _tokenizer.CountTokens(text));

            foreach (var line in lines)
            {
                yield return line;
            }
        }

        private const string Text = """
        The city of Venice, located in the northeastern part of Italy,
        is renowned for its unique geographical features. Built on more than 100 small islands in a lagoon in the
        Adriatic Sea, it has no roads, just canals including the Grand Canal thoroughfare lined with Renaissance and
        Gothic palaces. The central square, Piazza San Marco, contains St. Mark's Basilica, which is tiled with Byzantine
        mosaics, and the Campanile bell tower offering views of the city's red roofs.

        The Amazon Rainforest, also known as Amazonia, is a moist broadleaf tropical rainforest in the Amazon biome that
        covers most of the Amazon basin of South America. This basin encompasses 7 million square kilometers, of which
        5.5 million square kilometers are covered by the rainforest. This region includes territory belonging to nine nations
        and 3.4 million square kilometers of uncontacted tribes. The Amazon represents over half of the planet's remaining
        rainforests and comprises the largest and most biodiverse tract of tropical rainforest in the world.

        The Great Barrier Reef is the world's largest coral reef system composed of over 2,900 individual reefs and 900 islands
        stretching for over 2,300 kilometers over an area of approximately 344,400 square kilometers. The reef is located in the
        Coral Sea, off the coast of Queensland, Australia. The Great Barrier Reef can be seen from outer space and is the world's
        biggest single structure made by living organisms. This reef structure is composed of and built by billions of tiny organisms,
        known as coral polyps.
        """;
    }
}

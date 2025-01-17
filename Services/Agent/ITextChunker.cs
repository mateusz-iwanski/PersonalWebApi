
namespace PersonalWebApi.Services.Agent
{
    public interface ITextChunker
    {
        List<SemanticKernelTextChunker.StringChunkerFormat> ChunkText(string conversationId, int maxTokensPerLine, string text);
    }
}
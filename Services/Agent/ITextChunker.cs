
using PersonalWebApi.Processes.Qdrant.Models;

namespace PersonalWebApi.Services.Agent
{
    public interface ITextChunker
    {
        List<DocumentChunkerDto> ChunkText(int maxTokensPerLine, string text);
        void Setup(string tiktokenierModel);
        bool IsSetup();
    }
}
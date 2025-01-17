
namespace PersonalWebApi.Services.Agent
{
    public interface ITextChunker
    {
        List<StringChunkerFormat> ChunkText(int maxTokensPerLine, string text);
        void Setup(string tiktokenierModel);
        bool IsSetup();
    }
}
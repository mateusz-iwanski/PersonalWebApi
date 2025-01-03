
namespace PersonalWebApi.Services.Services.DocumentReaders
{
    public interface IDocumentReaderDocx
    {
        Task<string> ReadAsync(Uri fileUri);
    }
}
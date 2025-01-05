namespace PersonalWebApi.Utilities.Utilities.DocumentReaders
{
    public interface IDocumentReaderDocx
    {
        Task<string> ReadAsync(Uri fileUri);
    }
}
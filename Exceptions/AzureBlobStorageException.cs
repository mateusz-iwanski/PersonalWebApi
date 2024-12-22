namespace PersonalWebApi.Exceptions
{
    public class AzureBlobStorageException : Exception
    {
        public AzureBlobStorageException(string message) : base(message) { }
    }
    
}

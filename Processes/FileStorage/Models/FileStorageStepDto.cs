namespace PersonalWebApi.Processes.FileStorage.Models
{
    public class FileStorageStepDto
    {
        public string FileName { get; set; } = string.Empty;
        public Guid FileId { get; set; } = Guid.Empty;
        public Uri FileUri { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = true;
    }

}

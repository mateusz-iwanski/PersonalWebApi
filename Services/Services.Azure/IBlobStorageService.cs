
using Azure.Storage.Blobs.Models;

namespace PersonalWebApi.Services.Azure
{
    public interface IBlobStorageService
    {
        string TempContainerName();
        string LibraryContainerName();
        Task<Uri> UploadToTempAsync(IFormFile file, double ttlInDays, bool overwrite = true, Dictionary<string, string>? metadata = null);
        Task<Uri> UploadToLibraryAsync(IFormFile file, bool overwrite = false, Dictionary<string, string>? metadata = null, string fileId = "");
        Task DeleteFileFromTemp(string fileName);
        Task DeleteFileFromLibrary(string fileName);
        Task<Uri> UploadFromUriToTemp(string fileUri, string fileName, double ttlInDays, bool overwrite = false, Dictionary<string, string>? metadata = null);
        Task<Uri> UploadFromUriToLibrary(string fileUri, string fileName, bool overwrite = false, Dictionary<string, string>? metadata = null);
        Task<List<BlobItem>> GetFilesWithMetadataAsync(string containerName);
        Task<List<string>> GetContainersAsync();
        Task<Stream> DownloadFileAsync(Uri fileUri);
    }
}
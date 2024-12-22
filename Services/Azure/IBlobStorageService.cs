
namespace PersonalWebApi.Services.Azure
{
    public interface IBlobStorageService
    {
        Task<Uri> UploadToTempAsync(IFormFile file, double ttlInDays, bool overwrite = true);
        Task<Uri> UploadToLibrary(IFormFile file, bool overwrite = false);
        Task DeleteFileFromTemp(string fileName);
        Task DeleteFileFromLibrary(string fileName);
        Task<Uri> UploadFromUriToTemp(string fileUri, string fileName, double ttlInDays, bool overwrite = false);
        Task<Uri> UploadFromUriToLibrary(string fileUri, string fileName, bool overwrite = false);
    }
}
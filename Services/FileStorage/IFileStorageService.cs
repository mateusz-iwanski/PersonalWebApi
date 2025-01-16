﻿
using Azure.Storage.Blobs.Models;

namespace PersonalWebApi.Services.FileStorage
{
    public interface IFileStorageService
    {
        Task RemoveFromContainer(string fileName);
        Task<Stream> DownloadFileAsync(Uri fileUri);
        Task<List<BlobContainerItem>> GetContainersAsync();
        Task<List<BlobItem>> GetFilesWithMetadataAsync();
        Task<string> GetFileUrlAsync(string fileName);
        void SetContainer(string name);
        Task<Uri> UploadFromUriAsync(string fileUri, string fileName, bool overwrite = false, Dictionary<string, string>? metadata = null);
        Task<Uri> UploadToContainerAsync(IFormFile file, bool overwrite = false, Dictionary<string, string>? metadata = null, string fileId = "");
    }
}
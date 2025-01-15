using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.SemanticKernel;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Services.FileStorage;
using System.ComponentModel;

namespace PersonalWebApi.Agent.SemanticKernel.Plugins.StoragePlugins.AzureBlob
{
    public class AzureBlobPlugin
    {
        private readonly IFileStorageService _blobStorageService;
        private readonly string _containerName;

        public AzureBlobPlugin(IFileStorageService blobStorageService, IConfiguration configuration)
        {
            _blobStorageService = blobStorageService;

            _containerName = configuration.GetSection("FileStorage:Container:DefaultName").Value ?? 
                throw new SettingsException("FileStorage:Container:DefaultName not exists in appsettings.");

            _blobStorageService.SetContainer(_containerName);
        }

        [KernelFunction("upload_to_library")]
        [Description("Uploads a file to the library container in Azure Blob Storage")]
        [return: Description("The URI of the uploaded file")]
        public async Task<Uri> UploadToLibraryAsync(IFormFile file, bool overwrite = false, Dictionary<string, string>? metadata = null, string fileId = "")
        {
            return await _blobStorageService.UploadToContainerAsync(file, overwrite, metadata, fileId);
        }

        //[KernelFunction("delete_file_from_library")]
        //[Description("Deletes a file from the library container in Azure Blob Storage")]
        //public async Task DeleteFileFromLibrary(string fileName)
        //{
        //    await _blobStorageService.DeleteFileFromLibrary(fileName);
        //}

        [KernelFunction("upload_from_uri_to_library")]
        [Description("Uploads a file from a URI to the library container in Azure Blob Storage")]
        [return: Description("The URI of the uploaded file")]
        public async Task<Uri> UploadFromUriToLibrary(string fileUri, string fileName, bool overwrite = false, Dictionary<string, string>? metadata = null)
        {
            return await _blobStorageService.UploadFromUriAsync(fileUri, fileName, overwrite, metadata);
        }

        [KernelFunction("get_files_with_metadata")]
        [Description("Gets a list of files with metadata from the specified container in Azure Blob Storage")]
        [return: Description("A list of files with metadata")]
        public async Task<List<BlobItem>> GetFilesWithMetadataAsync(string containerName)
        {
            return await _blobStorageService.GetFilesWithMetadataAsync();
        }

        [KernelFunction("get_containers")]
        [Description("Gets a list of containers in Azure Blob Storage")]
        [return: Description("A list of container names")]
        public async Task<List<BlobContainerItem>> GetContainersAsync()
        {
            return await _blobStorageService.GetContainersAsync();
        }

        //[KernelFunction("download_file")]
        //[Description("Downloads a file from the specified URI and returns it as a stream")]
        //[return: Description("A stream containing the file data")]
        //public async Task<Stream> DownloadFileAsync(Uri fileUri)
        //{
        //    return await _blobStorageService.DownloadFileAsync(fileUri);
        //}
    }
}

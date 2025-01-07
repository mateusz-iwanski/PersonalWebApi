using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using PersonalWebApi.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace PersonalWebApi.Services.Azure
{
    /// <summary>
    /// Service for handling Azure Blob Storage operations.
    /// </summary>
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _tempContainerName;
        private readonly string _libraryContainerName;
        private BlobContainerClient _blobContainerClient { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <exception cref="SettingsException">Thrown when required settings are missing.</exception>
        public AzureBlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;

            // TODO: remove settings to appsettings.AzureService.json

            var blobStorageConnection = _configuration.GetSection("Azure::BlobStorage:Connection").Value ??
                throw new SettingsException("Azure::BlobStorage:Connection doesn't exists in azure appsettings");

            _blobServiceClient = new BlobServiceClient(blobStorageConnection);

            _tempContainerName = _configuration.GetSection("Azure:BlobStorage:TempContainerName").Value ??
                throw new SettingsException("Appsettings doesn't have AzureBlobStorage:TempContainerName.");

            _libraryContainerName = _configuration.GetSection("Azure:BlobStorage:LibraryContainerName").Value ??
                throw new SettingsException("Appsettings doesn't have AzureBlobStorage:LibraryContainerName.");
        }

        public string TempContainerName() => _tempContainerName;
        public string LibraryContainerName() => _libraryContainerName;

        /// <summary>
        /// Uploads a file to the specified container in Azure Blob Storage.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="ttlInDays">The time-to-live in days for the file.</param>
        /// <param name="overwrite">Whether to Overwrite the file if it already exists.</param>
        /// <param name="containerName">The name of the container to upload to.</param>
        /// <returns>The URI of the uploaded file.</returns>
        /// <exception cref="AzureBlobStorageException">Thrown when the blob already exists and Overwrite is set to false.</exception>
        /// <exception cref="RequestFailedException">Thrown when an error occurs during the upload.</exception>
        private async Task<Uri> upload(IFormFile file, double? ttlInDays, bool overwrite, string containerName, Dictionary<string, string>? metadata = null)
        {
            _blobContainerClient = await getContainerPublicAccessAsync(containerName, PublicAccessType.Blob);

            BlobClient blobClient = _blobContainerClient.GetBlobClient(file.FileName);

            Dictionary<string, string>? _metadata = new Dictionary<string, string> { };
            if (ttlInDays != null)
                _metadata.Add(nameof(ttlInDays), DateTime.UtcNow.AddDays(ttlInDays ?? 0d).ToString("o")); // ISO 8601 format

            if (metadata !=null)
                foreach (var item in metadata)
                {
                    _metadata.Add(item.Key, item.Value);
                }

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            try
            {
                await blobClient.UploadAsync(file.OpenReadStream(), new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                    Metadata = _metadata,
                    Conditions = overwrite ? null : new BlobRequestConditions { IfNoneMatch = new ETag("*") }
                });
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
            {
                throw new AzureBlobStorageException("Blob already exists and Overwrite is set to false.");
            }
            catch (RequestFailedException ex)
            {
                // Handle other RequestFailedException errors
                throw new RequestFailedException("An error occurred while uploading the blob.", ex);
            }

            var fileUrl = await GetFileUrlAsync(file.FileName, containerName);

            return new Uri(fileUrl);
        }

        /// <summary>
        /// Deletes a file from the specified container in Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <param name="containerName">The name of the container to delete from.</param>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist in the container.</exception>
        private async Task deleteFileAsync(string fileName, string containerName)
        {
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();
            }
            else
            {
                throw new FileNotFoundException($"The file '{fileName}' does not exist in the container '{containerName}'.");
            }
        }

        /// <summary>
        /// Deletes a file from the temporary container.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        public async Task DeleteFileFromTemp(string fileName) => await deleteFileAsync(fileName, _tempContainerName);

        /// <summary>
        /// Deletes a file from the library container.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        public async Task DeleteFileFromLibrary(string fileName) => await deleteFileAsync(fileName, _libraryContainerName);

        /// <summary>
        /// Uploads a file to the temporary container in Azure Blob Storage.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="ttlInDays">The time-to-live in days for the file.</param>
        /// <param name="overwrite">Whether to Overwrite the file if it already exists.</param>
        /// <param name="metadata">The metadata to add to the file.</param>
        /// <returns>The URI of the uploaded file.</returns>
        /// <remarks>Once you set ttl in the metadat, the other service will delete the file after the set time.</remarks>
        public async Task<Uri> UploadToTempAsync(IFormFile file, double ttlInDays, bool overwrite = true, Dictionary<string, string>? metadata = null)
        {
            var uri = await upload(file, ttlInDays, overwrite, _tempContainerName, metadata);
            return uri;
        }

        /// <summary>
        /// Uploads a file to the library container in Azure Blob Storage.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="overwrite">Whether to Overwrite the file if it already exists.</param>
        /// <returns>The URI of the uploaded file.</returns>
        /// <param name="metadata">The metadata to add to the file.</param>
        public async Task<Uri> UploadToLibraryAsync(IFormFile file, bool overwrite = false, Dictionary<string, string>? metadata = null, string fileId = "")
        {
            if (!string.IsNullOrEmpty(fileId))
                if (metadata != null) 
                    metadata["fileId"] = fileId;
                else
                    metadata = new Dictionary<string, string> { { "fileId", fileId } };

            var uri = await upload(file, null, overwrite, _libraryContainerName, metadata);
            return uri;
        }

        /// <summary>
        /// Gets the URL of a file in the specified container.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>The URL of the file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist in the container.</exception>
        public async Task<string> GetFileUrlAsync(string fileName, string containerName)
        {
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
            {
                return blobClient.Uri.ToString();
            }
            else
            {
                throw new FileNotFoundException($"The file '{fileName}' does not exist in the container '{containerName}'.");
            }
        }

        /// <summary>
        /// Uploads a file from a URI to the temporary container in Azure Blob Storage.
        /// </summary>
        /// <param name="fileUri">The URI of the file to upload.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="ttlInDays">The time-to-live in days for the file.</param>
        /// <param name="overwrite">Whether to Overwrite the file if it already exists.</param>
        /// <param name="metadata">The metadata to add to the file.</param>
        /// <returns>The URI of the uploaded file.</returns>
        public async Task<Uri> UploadFromUriToTemp(string fileUri, string fileName, double ttlInDays, bool overwrite = false, Dictionary<string, string>? metadata = null)
        {
            return await uploadFromUriAsync(fileUri, fileName, _tempContainerName, ttlInDays, overwrite, metadata);
        }

        /// <summary>
        /// Uploads a file from a URI to the library container in Azure Blob Storage.
        /// </summary>
        /// <param name="fileUri">The URI of the file to upload.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="overwrite">Whether to Overwrite the file if it already exists.</param>
        /// <param name="metadata">The metadata to add to the file.</param>
        /// <returns>The URI of the uploaded file.</returns>
        public async Task<Uri> UploadFromUriToLibrary(string fileUri, string fileName, bool overwrite = false, Dictionary<string, string>? metadata = null)
        {
            return await uploadFromUriAsync(fileUri, fileName, _libraryContainerName, null, overwrite, metadata);
        }

        /// <summary>
        /// Gets the container, if it does not exist, creates it with the specified public access type.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="accessType">The public access type for the container.</param>
        /// <returns>The <see cref="BlobContainerClient"/> for the container.</returns>
        private async Task<BlobContainerClient> getContainerPublicAccessAsync(string containerName, PublicAccessType accessType)
        {
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            var publicAccessType = accessType;
            await blobContainerClient.SetAccessPolicyAsync(publicAccessType);

            return blobContainerClient;
        }

        /// <summary>
        /// Uploads a file from a URI to the specified container in Azure Blob Storage.
        /// </summary>
        /// <param name="fileUri">The URI of the file to upload.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="containerName">The name of the container to upload to.</param>
        /// <param name="ttlInDays">The time-to-live in days for the file.</param>
        /// <param name="overwrite">Whether to Overwrite the file if it already exists.</param>
        /// <param name="metadata">The metadata to add to the file.</param>
        /// <returns>The URI of the uploaded file.</returns>
        private async Task<Uri> uploadFromUriAsync(string fileUri, string fileName, string containerName, double? ttlInDays, bool overwrite = false, Dictionary<string, string>? metadata = null)
        {
            string tempFilePath = Path.GetTempFileName();
            Uri uri = null;

            using (HttpClient httpClient = new HttpClient())
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(fileUri))
                {
                    response.EnsureSuccessStatusCode();
                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                }
            }

            IFormFile file;
            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
            {
                file = new FormFile(fileStream, 0, fileStream.Length, null, fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/octet-stream" // Set the appropriate content type if known
                };

                uri = await upload(file, ttlInDays, overwrite, containerName, metadata);

                // Ensure fileStream is disposed before deleting the file
            }

            // Delete the local file
            File.Delete(tempFilePath);

            return uri;
        }

        // get list of files with metadat
        public async Task<List<BlobItem>> GetFilesWithMetadataAsync(string containerName)
        {
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobs = new List<BlobItem>();
            await foreach (var blobItem in _blobContainerClient.GetBlobsAsync())
            {
                var blobClient = _blobContainerClient.GetBlobClient(blobItem.Name);
                var properties = await blobClient.GetPropertiesAsync();

                foreach (var metadataItem in properties.Value.Metadata)
                {
                    blobItem.Metadata.Add(metadataItem);
                }

                blobs.Add(blobItem);
            }
            return blobs;
        }

        // get list of containers
        public async Task<List<string>> GetContainersAsync()
        {
            var containers = new List<string>();
            await foreach (var container in _blobServiceClient.GetBlobContainersAsync())
            {
                containers.Add(container.Name);
            }
            return containers;
        }

        /// <summary>
        /// Downloads a file from the specified URI and returns it as a stream.
        /// </summary>
        /// <param name="fileUri">The URI of the file to download.</param>
        /// <returns>A stream containing the file data.</returns>
        public async Task<Stream> DownloadFileAsync(Uri fileUri)
        {
            BlobClient blobClient = new BlobClient(fileUri);
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            MemoryStream memoryStream = new MemoryStream();
            await download.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset the stream position to the beginning
            return memoryStream;
        }
    }
}

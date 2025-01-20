using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Models.Storage;
using PersonalWebApi.Services.Services.History;
using PersonalWebApi.Services.Services.Qdrant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PersonalWebApi.Services.FileStorage
{
    /// <summary>
    /// Service for handling Azure Blob Storage operations.
    /// 
    /// After initialize use SetContainer method to set container name.
    /// </summary>
    public class AzureBlobStorageService : IFileStorageService
    {
        private string _containerName { get; set; }
        private readonly BlobContainerClient _blobContainerClient;
        private readonly BlobServiceClient _blobServiceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <exception cref="SettingsException">Thrown when required settings are missing.</exception>
        public AzureBlobStorageService(IConfiguration configuration)
        {

            var blobStorageConnection = configuration.GetSection("Azure:BlobStorage:Connection").Value ??
                throw new SettingsException("Azure::BlobStorage:Connection doesn't exists in azure appsettings");

            _containerName = configuration.GetSection("Qdrant:Container:Name").Value ?? throw new SettingsException("Qdrant:Container:Name not exists");
            _blobServiceClient = new BlobServiceClient(blobStorageConnection);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        }

        /// <summary>
        /// Deletes a file from the specified container in Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist in the container.</exception>
        public async Task RemoveFromContainer(string fileName)
        {
            ensureContainerIsSet();
            
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();
            }
            else
            {
                throw new FileNotFoundException($"The file '{fileName}' does not exist in the container '{_containerName}'.");
            }
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

        /// <summary>
        /// Gets the list of containers in Azure Blob Storage.
        /// </summary>
        /// <returns>A list of blob container items.</returns>
        public async Task<List<BlobContainerItem>> GetContainersAsync()
        {
            var containers = new List<BlobContainerItem>();
            await foreach (var container in _blobServiceClient.GetBlobContainersAsync())
            {
                containers.Add(container);
            }
            return containers;
        }

        /// <summary>
        /// Gets the list of files with metadata in the specified container.
        /// </summary>
        /// <returns>A list of blob items with metadata.</returns>
        public async Task<List<BlobItem>> GetFilesWithMetadataAsync()
        {
            ensureContainerIsSet();
            
            var blobs = new List<BlobItem>();
            await foreach (var blobItem in _blobContainerClient.GetBlobsAsync())
            {
                var blobClient = _blobContainerClient.GetBlobClient(blobItem.Name);
                var properties = await blobClient.GetPropertiesAsync();

                foreach (var metadataItem in properties.Value.Metadata)
                {
                    blobItem.Metadata.Add(metadataItem);
                }

                // Add blob properties to metadata
                blobItem.Metadata.Add("Url", blobClient.Uri.ToString());
                blobItem.Metadata.Add("AcceptRanges", properties.Value.AcceptRanges);
                blobItem.Metadata.Add("AccessTier", properties.Value.AccessTier);
                blobItem.Metadata.Add("AccessTierChangedOn", properties.Value.AccessTierChangedOn.ToString());
                blobItem.Metadata.Add("AccessTierInferred", properties.Value.AccessTierInferred.ToString());
                blobItem.Metadata.Add("ArchiveStatus", properties.Value.ArchiveStatus);
                blobItem.Metadata.Add("BlobCommittedBlockCount", properties.Value.BlobCommittedBlockCount.ToString());
                blobItem.Metadata.Add("BlobCopyStatus", properties.Value.BlobCopyStatus.ToString());
                blobItem.Metadata.Add("BlobSequenceNumber", properties.Value.BlobSequenceNumber.ToString());
                blobItem.Metadata.Add("BlobType", properties.Value.BlobType.ToString());
                blobItem.Metadata.Add("CacheControl", properties.Value.CacheControl);
                blobItem.Metadata.Add("ContentDisposition", properties.Value.ContentDisposition);
                blobItem.Metadata.Add("ContentEncoding", properties.Value.ContentEncoding);
                blobItem.Metadata.Add("ContentHash", Convert.ToBase64String(properties.Value.ContentHash));
                blobItem.Metadata.Add("ContentLanguage", properties.Value.ContentLanguage);
                blobItem.Metadata.Add("ContentLength", properties.Value.ContentLength.ToString());
                blobItem.Metadata.Add("ContentType", properties.Value.ContentType);
                blobItem.Metadata.Add("CopyCompletedOn", properties.Value.CopyCompletedOn.ToString());
                blobItem.Metadata.Add("CopyId", properties.Value.CopyId);
                blobItem.Metadata.Add("CopyProgress", properties.Value.CopyProgress);
                blobItem.Metadata.Add("CopySource", properties.Value.CopySource?.ToString());
                blobItem.Metadata.Add("CopyStatus", properties.Value.CopyStatus.ToString());
                blobItem.Metadata.Add("CopyStatusDescription", properties.Value.CopyStatusDescription);
                blobItem.Metadata.Add("CreatedOn", properties.Value.CreatedOn.ToString());
                blobItem.Metadata.Add("DestinationSnapshot", properties.Value.DestinationSnapshot);
                blobItem.Metadata.Add("ETag", properties.Value.ETag.ToString());
                blobItem.Metadata.Add("EncryptionKeySha256", properties.Value.EncryptionKeySha256);
                blobItem.Metadata.Add("EncryptionScope", properties.Value.EncryptionScope);
                blobItem.Metadata.Add("ExpiresOn", properties.Value.ExpiresOn.ToString());
                blobItem.Metadata.Add("HasLegalHold", properties.Value.HasLegalHold.ToString());
                blobItem.Metadata.Add("ImmutabilityPolicy", properties.Value.ImmutabilityPolicy?.ToString());
                blobItem.Metadata.Add("IsIncrementalCopy", properties.Value.IsIncrementalCopy.ToString());
                blobItem.Metadata.Add("IsLatestVersion", properties.Value.IsLatestVersion.ToString());
                blobItem.Metadata.Add("IsSealed", properties.Value.IsSealed.ToString());
                blobItem.Metadata.Add("IsServerEncrypted", properties.Value.IsServerEncrypted.ToString());
                blobItem.Metadata.Add("LastAccessed", properties.Value.LastAccessed.ToString());
                blobItem.Metadata.Add("LastModified", properties.Value.LastModified.ToString());
                blobItem.Metadata.Add("LeaseDuration", properties.Value.LeaseDuration.ToString());
                blobItem.Metadata.Add("LeaseState", properties.Value.LeaseState.ToString());
                blobItem.Metadata.Add("LeaseStatus", properties.Value.LeaseStatus.ToString());
                blobItem.Metadata.Add("ObjectReplicationDestinationPolicyId", properties.Value.ObjectReplicationDestinationPolicyId);
                blobItem.Metadata.Add("ObjectReplicationSourceProperties", properties.Value.ObjectReplicationSourceProperties?.ToString());
                blobItem.Metadata.Add("RehydratePriority", properties.Value.RehydratePriority);
                blobItem.Metadata.Add("TagCount", properties.Value.TagCount.ToString());
                blobItem.Metadata.Add("VersionId", properties.Value.VersionId);

                blobs.Add(blobItem);
            }
            return blobs;
        }

        /// <summary>
        /// Gets the URL of a file in the specified container.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The URL of the file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist in the container.</exception>
        public async Task<string> GetFileUrlAsync(string fileName)
        {
            ensureContainerIsSet();
            
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
            {
                return blobClient.Uri.ToString();
            }
            else
            {
                throw new FileNotFoundException($"The file '{fileName}' does not exist in the container '{_containerName}'.");
            }
        }

        /// <summary>
        /// Sets the name of the container to use.
        /// </summary>
        /// <param name="name">The name of the container.</param>
        public void SetContainer(string name)
        {
            validateContainerName(name);
            _containerName = name;
        }

        /// <summary>
        /// Uploads a file from a URI to the container in Azure Blob Storage.
        /// </summary>
        /// <param name="fileUri">The URI of the file to upload.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
        /// <param name="metadata">The metadata to add to the file.</param>
        /// <returns>The URI of the uploaded file.</returns>
        public async Task<Uri> UploadFromUriAsync(Guid fileId, string fileUri, string fileName, bool overwrite = false, Dictionary<string, string>? metadata = null)
        {
            ensureContainerIsSet();
            return await uploadFromUriAsync(fileUri, fileName, _containerName, null, overwrite, metadata);
        }

        /// <summary>
        /// Uploads a file to the specified container in Azure Blob Storage.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
        /// <param name="metadata">The metadata to add to the file.</param>
        /// <param name="fileId">The file ID.</param>
        /// <returns>The URI of the uploaded file.</returns>
        public async Task<Uri> UploadToContainerAsync(Guid fileId, IFormFile file, bool overwrite = false, Dictionary<string, string>? metadata = null)
        {
            ensureContainerIsSet();
            return await uploadAsync(file, null, overwrite, _containerName, metadata);
        }

        /// <summary>
        /// Ensures the container name is set.
        /// </summary>
        private void ensureContainerIsSet()
        {
            if (string.IsNullOrEmpty(_containerName))
            {
                throw new InvalidOperationException("Container name must be set by calling SetContainer() method before using the service.");
            }
        }

        /// <summary>
        /// Validates the provided container name according to Azure Blob Storage naming rules.
        /// </summary>
        /// <param name="containerName">The name of the container to validate.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the container name does not meet the following criteria:
        /// - Length is not between 3 and 63 characters.
        /// - Contains characters other than lowercase letters, numbers, and dashes.
        /// - Does not start with a letter or number.
        /// - Contains uppercase letters.
        /// </exception>
        private void validateContainerName(string containerName)
        {
            if (containerName.Length < 3 || containerName.Length > 63)
            {
                throw new ArgumentException("Container name must be between 3 and 63 characters long.");
            }

            if (!Regex.IsMatch(containerName, @"^[a-z0-9]+(-[a-z0-9]+)*$"))
            {
                throw new ArgumentException("Container name must start with a letter or number, and can contain only letters, numbers, and the dash (-) character. Every dash (-) character must be immediately preceded and followed by a letter or number.");
            }

            if (containerName != containerName.ToLower())
            {
                throw new ArgumentException("Container name must be lowercase.");
            }
        }

        /// <summary>
        /// Uploads a file to the specified container in Azure Blob Storage.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="ttlInDays">The time-to-live in days for the file.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
        /// <param name="containerName">The name of the container to upload to.</param>
        /// <param name="metadata">The metadata to add to the file.</param>
        /// <returns>The URI of the uploaded file.</returns>
        /// <exception cref="AzureBlobStorageException">Thrown when the blob already exists and overwrite is set to false.</exception>
        /// <exception cref="RequestFailedException">Thrown when an error occurs during the upload.</exception>
        private async Task<Uri> uploadAsync(IFormFile file, double? ttlInDays, bool overwrite, string containerName, Dictionary<string, string>? metadata = null)
        {
            ensureContainerIsSet();

            BlobClient blobClient = _blobContainerClient.GetBlobClient(file.FileName);

            Dictionary<string, string>? _metadata = new Dictionary<string, string> { };
            if (ttlInDays != null)
                _metadata.Add(nameof(ttlInDays), DateTime.UtcNow.AddDays(ttlInDays ?? 0d).ToString("o")); // ISO 8601 format

            if (metadata != null)
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

            var fileUrl = await GetFileUrlAsync(file.FileName);

            return new Uri(fileUrl);
        }

        /// <summary>
        /// Uploads a file from a URI to the specified container in Azure Blob Storage.
        /// </summary>
        /// <param name="fileUri">The URI of the file to upload.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="containerName">The name of the container to upload to.</param>
        /// <param name="ttlInDays">The time-to-live in days for the file.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
        /// <param name="metadata">The metadata to add to the file.</param>
        /// <returns>The URI of the uploaded file.</returns>
        private async Task<Uri> uploadFromUriAsync(string fileUri, string fileName, string containerName, double? ttlInDays, bool overwrite = false, Dictionary<string, string>? metadata = null)
        {
            ensureContainerIsSet();

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

                uri = await uploadAsync(file, ttlInDays, overwrite, containerName, metadata);
            }

            File.Delete(tempFilePath); // Clean up the temporary file

            return uri;
        }
    }
}

﻿using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Models.Storage;
using PersonalWebApi.Services.Services.History;
using PersonalWebApi.Services.Services.System;
using SharpCompress.Common;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAssistantHistoryManager _assistantHistoryManager;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _tempContainerName;
        private readonly string _libraryContainerName;
        private BlobContainerClient _blobContainerClient { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <exception cref="SettingsException">Thrown when required settings are missing.</exception>
        public AzureBlobStorageService(
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor,
            IAssistantHistoryManager assistantHistoryManager)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _assistantHistoryManager = assistantHistoryManager;

            // TODO: remove settings to appsettings.AzureService.json

            var blobStorageConnection = _configuration.GetSection("Azure:BlobStorage:Connection").Value ??
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
        /// <param name="file">The file to uploadAsync.</param>
        /// <param name="ttlInDays">The time-to-live in days for the file.</param>
        /// <param name="overwrite">Whether to Overwrite the file if it already exists.</param>
        /// <param name="containerName">The name of the container to uploadAsync to.</param>
        /// <returns>The URI of the uploaded file.</returns>
        /// <exception cref="AzureBlobStorageException">Thrown when the blob already exists and Overwrite is set to false.</exception>
        /// <exception cref="RequestFailedException">Thrown when an error occurs during the uploadAsync.</exception>
        private async Task<Uri> uploadAsync(IFormFile file, double? ttlInDays, bool overwrite, string containerName, Dictionary<string, string>? metadata = null)
        {
            (Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(_httpContextAccessor);

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

            // ###### Log it
            var storageEvent = new StorageEventsDto(conversationUuid, sessionUuid)
            {
                EventName = "upload",
                ServiceName = "AzureBlobStorage",
                IsSuccess = true,
                ActionType = "Upload",
                FileUri = fileUrl,
                ErrorMessage = string.Empty,
            };

            await _assistantHistoryManager.SaveAsync(storageEvent);

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
        /// <param name="file">The file to uploadAsync.</param>
        /// <param name="ttlInDays">The time-to-live in days for the file.</param>
        /// <param name="overwrite">Whether to Overwrite the file if it already exists.</param>
        /// <param name="metadata">The metadata to add to the file.</param>
        /// <returns>The URI of the uploaded file.</returns>
        /// <remarks>Once you set ttl in the metadat, the other service will delete the file after the set time.</remarks>
        public async Task<Uri> UploadToTempAsync(IFormFile file, double ttlInDays, bool overwrite = true, Dictionary<string, string>? metadata = null)
        {
            var uri = await uploadAsync(file, ttlInDays, overwrite, _tempContainerName, metadata);
            return uri;
        }

        /// <summary>
        /// Uploads a file to the library container in Azure Blob Storage.
        /// </summary>
        /// <param name="file">The file to uploadAsync.</param>
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

            var uri = await uploadAsync(file, null, overwrite, _libraryContainerName, metadata);
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
        /// <param name="fileUri">The URI of the file to uploadAsync.</param>
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
        /// <param name="fileUri">The URI of the file to uploadAsync.</param>
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
        /// <param name="fileUri">The URI of the file to uploadAsync.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="containerName">The name of the container to uploadAsync to.</param>
        /// <param name="ttlInDays">The time-to-live in days for the file.</param>
        /// <param name="overwrite">Whether to Overwrite the file if it already exists.</param>
        /// <param name="metadata">The metadata to add to the file.</param>
        /// <returns>The URI of the uploaded file.</returns>
        private async Task<Uri> uploadFromUriAsync(string fileUri, string fileName, string containerName, double? ttlInDays, bool overwrite = false, Dictionary<string, string>? metadata = null)
        {
            (Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(_httpContextAccessor);

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

                // Ensure fileStream is disposed before deleting the file
            }

            // Delete the local file
            File.Delete(tempFilePath);

            // ###### Log it
            var storageEvent = new StorageEventsDto(conversationUuid, sessionUuid)
            {
                EventName = "upload",
                ServiceName = "AzureBlobStorage",
                IsSuccess = true,
                ActionType = "Upload",
                FileUri = uri.ToString(),
                ErrorMessage = string.Empty,
            };

            await _assistantHistoryManager.SaveAsync(storageEvent);

            return uri;
        }

        // get list of files with metadata
        // add all properties from properties.Value to metadata
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

        // get list of containers
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

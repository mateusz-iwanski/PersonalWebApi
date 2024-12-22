﻿using Azure;
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
    public class BlobStorageService : IBlobStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _tempContainerName;
        private readonly string _libraryContainerName;
        private BlobContainerClient _blobContainerClient { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <exception cref="SettingsException">Thrown when required settings are missing.</exception>
        public BlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;

            var blobStorageConnection = _configuration.GetConnectionString("AzureBlobStorageConnection") ??
                throw new SettingsException("Appsettings doesn't have ConnectionStrings:AzureBlobStorage");

            _blobServiceClient = new BlobServiceClient(blobStorageConnection);

            _tempContainerName = _configuration.GetSection("Azure:BlobStorage:TempContainerName").Value ??
                throw new SettingsException("Appsettings doesn't have AzureBlobStorage:TempContainerName.");

            _libraryContainerName = _configuration.GetSection("Azure:BlobStorage:LibraryContainerName").Value ??
                throw new SettingsException("Appsettings doesn't have AzureBlobStorage:LibraryContainerName.");
        }

        /// <summary>
        /// Uploads a file to the specified container in Azure Blob Storage.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="ttlInDays">The time-to-live in days for the file.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
        /// <param name="containerName">The name of the container to upload to.</param>
        /// <returns>The URI of the uploaded file.</returns>
        /// <exception cref="AzureBlobStorageException">Thrown when the blob already exists and overwrite is set to false.</exception>
        /// <exception cref="RequestFailedException">Thrown when an error occurs during the upload.</exception>
        private async Task<Uri> upload(IFormFile file, double? ttlInDays, bool overwrite, string containerName)
        {
            _blobContainerClient = await getContainerPublicAccessAsync(containerName, PublicAccessType.Blob);

            BlobClient blobClient = _blobContainerClient.GetBlobClient(file.FileName);

            Dictionary<string, string>? metadata = null;
            if (ttlInDays != null)
                metadata = new Dictionary<string, string>
                {
                    { "ttl", DateTime.UtcNow.AddDays(ttlInDays ?? 0d).ToString("o") } // ISO 8601 format
                };

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            try
            {
                await blobClient.UploadAsync(file.OpenReadStream(), new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                    Metadata = metadata,
                    Conditions = overwrite ? null : new BlobRequestConditions { IfNoneMatch = new ETag("*") }
                });
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
            {
                throw new AzureBlobStorageException("Blob already exists and overwrite is set to false.");
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
        /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
        /// <returns>The URI of the uploaded file.</returns>
        /// <remarks>Once you set ttl in the metadata, the other service will delete the file after the set time.</remarks>
        public async Task<Uri> UploadToTempAsync(IFormFile file, double ttlInDays, bool overwrite = true)
        {
            var uri = await upload(file, ttlInDays, overwrite, _tempContainerName);
            return uri;
        }

        /// <summary>
        /// Uploads a file to the library container in Azure Blob Storage.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
        /// <returns>The URI of the uploaded file.</returns>
        public async Task<Uri> UploadToLibrary(IFormFile file, bool overwrite = false)
        {
            var uri = await upload(file, null, overwrite, _libraryContainerName);
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
        /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
        /// <returns>The URI of the uploaded file.</returns>
        public async Task<Uri> UploadFromUriToTemp(string fileUri, string fileName, double ttlInDays, bool overwrite = false)
        {
            return await uploadFromUriAsync(fileUri, fileName, _tempContainerName, ttlInDays, overwrite);
        }

        /// <summary>
        /// Uploads a file from a URI to the library container in Azure Blob Storage.
        /// </summary>
        /// <param name="fileUri">The URI of the file to upload.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
        /// <returns>The URI of the uploaded file.</returns>
        public async Task<Uri> UploadFromUriToLibrary(string fileUri, string fileName, bool overwrite = false)
        {
            return await uploadFromUriAsync(fileUri, fileName, _libraryContainerName, null, overwrite);
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
        /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
        /// <returns>The URI of the uploaded file.</returns>
        private async Task<Uri> uploadFromUriAsync(string fileUri, string fileName, string containerName, double? ttlInDays, bool overwrite = false)
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

                uri = await upload(file, ttlInDays, overwrite, containerName);

                // Ensure fileStream is disposed before deleting the file
            }

            // Delete the local file
            File.Delete(tempFilePath);

            return uri;
        }
    }
}
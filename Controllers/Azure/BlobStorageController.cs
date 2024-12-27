using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using PersonalWebApi.Services.Azure;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PersonalWebApi.Models.Azure;
using Azure.Core;

namespace PersonalWebApi.Controllers.Azure
{
    [ApiController]
    [Route("api/azure/blob-storage")]
    public class BlobStorageController : ControllerBase
    {
        private readonly IBlobStorageService _service;

        public BlobStorageController(IBlobStorageService service)
        {
            _service = service;
        }

        /// <summary>
        /// Upload file to Azure Blob Storage Account to the `file` Container name
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ttlInDays">Set how many days it should be store.</param>i added, but still 
        /// <param name="overwrite">If file exists with the same name Overwrite it.</param>
        /// <returns>IActionResult with URI</returns>
        /// <remarks>The files will be automatically deleted after TtlInDays</remarks>
        [HttpPost("upload-to-temp")]
        public async Task<IActionResult> UploadToTempAsync([FromForm] UploadFileToTempRequestDto request)
        {
            var uri = await _service.UploadToTempAsync(request.File, request.TtlInDays, request.Overwrite, request.Metadata);

            return Ok(uri);
        }

        /// <summary>
        /// Upload file to Azure Blob Storage Account to the `library` Container name
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ttlInDays">Set how many days it should be store.</param>
        /// <param name="overwrite">If file exists with the same name Overwrite it.</param>
        /// <returns>IActionResult with URI</returns>
        /// <remark>File will not automatically delete</remark>
        [HttpPost("upload-to-library")]
        public async Task<IActionResult> UploadToLibrary([FromForm] UploadFileToLibraryRequestDto request)
        {
            var uri = await _service.UploadToLibrary(request.File, request.Overwrite, request.Metadata);
            return Ok(uri);
        }

        /// <summary>
        /// Delete file from Azure Blob Storage Account from the `file` Container name
        /// </summary>
        /// <param name="fileName">Name of the file to be deleted</param>
        /// <returns>IActionResult indicating the result of the delete operation</returns>
        [HttpDelete("delete-from-temp/{FileName}")]
        public async Task<IActionResult> DeleteFromTemp([Required][FromRoute] string fileName)
        {
            await _service.DeleteFileFromTemp(fileName);
            return Ok();
        }

        /// <summary>
        /// Delete file from Azure Blob Storage Account from the `library` Container name
        /// </summary>
        /// <param name="fileName">Name of the file to be deleted</param>,
        /// 
        /// 01<returns>IActionResult indicating the result of the delete operation</returns>
        [HttpDelete("delete-from-library/{FileName}")]
        public async Task<IActionResult> DeleteFromLibrary([Required][FromRoute] string fileName)
        {
            await _service.DeleteFileFromLibrary(fileName);
            return Ok();
        }

        /// <summary>
        /// Uploads a file from a given URI to the Azure Blob Storage Account in the `file` container.
        /// </summary>
        /// <param name="fileUri">The URI of the file to be uploaded.</param>
        /// <param name="fileName">The name to be assigned to the uploaded file.</param>
        /// <param name="ttlInDays">The number of days the file should be stored before automatic deletion.</param>
        /// <param name="overwrite">Indicates whether to Overwrite the file if it already exists.</param>
        /// <returns>IActionResult containing the URI of the uploaded file.</returns>
        /// <remarks>
        /// The file will be automatically deleted after the specified TtlInDays.
        /// First it is downloaded from uri locally and then uploaded to the blob storage.
        /// After uploading the file is deleted from the local storage.
        /// </remarks>
        [HttpPost("upload-from-uri-to-temp")]
        public async Task<IActionResult> UploadFromUriToTemp([FromBody] UploadFileFromUriToTempRequestDto request)
        {
            var uri = await _service.UploadFromUriToTemp(request.FileUri, request.FileName, request.TtlInDays, request.Overwrite, request.Metadata);
            return Ok(uri);
        }

        /// <summary>
        /// Uploads a file from a given URI to the Azure Blob Storage Account in the `library` container.
        /// </summary>
        /// <param name="fileUri">The URI of the file to be uploaded.</param>
        /// <param name="fileName">The name to be assigned to the uploaded file.</param>
        /// <param name="overwrite">Indicates whether to Overwrite the file if it already exists.</param>
        /// <returns>IActionResult containing the URI of the uploaded file.</returns>
        /// <remarks>
        /// The file will not be automatically deleted.
        /// First it is downloaded from uri locally and then uploaded to the blob storage.
        /// After uploading the file is deleted from the local storage.
        /// </remarks>
        [HttpPost("upload-from-uri-to-library")]
        public async Task<IActionResult> UploadFromUriToLibrary([FromBody] UploadFileFromUriToLibraryRequestDto request)
        {
            var uri = await _service.UploadFromUriToLibrary(request.FileUri, request.FileName, request.Overwrite, request.Metadata);
            return Ok(uri);
        }

        // GetFilesWithMetadataAsync
        [HttpGet("files-list-with-metadata/{containerName}")]
        public async Task<IActionResult> GetFilesWithMetadataAsync([Required] string containerName)
        {
            var files = await _service.GetFilesWithMetadataAsync(containerName);
            return Ok(files);
        }

        // GetContainersAsync
        [HttpGet("containers-list")]
        public async Task<IActionResult> GetContainersAsync()
        {
            var containers = await _service.GetContainersAsync();
            return Ok(containers);
        }
    }
}

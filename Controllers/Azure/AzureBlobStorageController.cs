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
    public class AzureBlobStorageController : ControllerBase
    {
        private readonly IBlobStorageService _service;

        public AzureBlobStorageController(IBlobStorageService service)
        {
            _service = service;
        }

        /// <summary>
        /// Uploads a file to the Azure Blob Storage Account in the `file` container.
        /// </summary>
        /// <param name="request">The request containing the file, TTL in days, overwrite flag, and metadata.</param>
        /// <returns>IActionResult with the URI of the uploaded file.</returns>
        /// <remarks>The file will be automatically deleted after the specified TTL in days.</remarks>
        [HttpPost("uploadAsync-to-temp")]
        public async Task<IActionResult> UploadToTempAsync([FromForm] UploadFileToTempRequestDto request)
        {
            var uri = await _service.UploadToTempAsync(request.File, request.TtlInDays, request.Overwrite, request.Metadata);
            return Ok(uri);
        }

        /// <summary>
        /// Uploads a file to the Azure Blob Storage Account in the `library` container.
        /// </summary>
        /// <param name="request">The request containing the file, overwrite flag, and metadata.</param>
        /// <returns>IActionResult with the URI of the uploaded file.</returns>
        /// <remarks>The file will not be automatically deleted.</remarks>
        [HttpPost("uploadAsync-to-library")]
        public async Task<IActionResult> UploadToLibrary([FromForm] UploadFileToLibraryRequestDto request)
        {
            var uri = await _service.UploadToLibraryAsync(request.File, request.Overwrite, request.Metadata);
            return Ok(uri);
        }

        /// <summary>
        /// Deletes a file from the Azure Blob Storage Account in the `file` container.
        /// </summary>
        /// <param name="fileName">The name of the file to be deleted.</param>
        /// <returns>IActionResult indicating the result of the delete operation.</returns>
        [HttpDelete("delete-from-temp/{fileName}")]
        public async Task<IActionResult> DeleteFromTemp([Required][FromRoute] string fileName)
        {
            await _service.DeleteFileFromTemp(fileName);
            return Ok();
        }

        /// <summary>
        /// Deletes a file from the Azure Blob Storage Account in the `library` container.
        /// </summary>
        /// <param name="fileName">The name of the file to be deleted.</param>
        /// <returns>IActionResult indicating the result of the delete operation.</returns>
        [HttpDelete("delete-from-library/{fileName}")]
        public async Task<IActionResult> DeleteFromLibrary([Required][FromRoute] string fileName)
        {
            await _service.DeleteFileFromLibrary(fileName);
            return Ok();
        }

        /// <summary>
        /// Uploads a file from a given URI to the Azure Blob Storage Account in the `file` container.
        /// </summary>
        /// <param name="request">The request containing the file URI, file name, TTL in days, overwrite flag, and metadata.</param>
        /// <returns>IActionResult with the URI of the uploaded file.</returns>
        /// <remarks>The file will be automatically deleted after the specified TTL in days.</remarks>
        [HttpPost("uploadAsync-from-uri-to-temp")]
        public async Task<IActionResult> UploadFromUriToTemp([FromBody] UploadFileFromUriToTempRequestDto request)
        {
            var uri = await _service.UploadFromUriToTemp(request.FileUri, request.FileName, request.TtlInDays, request.Overwrite, request.Metadata);
            return Ok(uri);
        }

        /// <summary>
        /// Uploads a file from a given URI to the Azure Blob Storage Account in the `library` container.
        /// </summary>
        /// <param name="request">The request containing the file URI, file name, overwrite flag, and metadata.</param>
        /// <returns>IActionResult with the URI of the uploaded file.</returns>
        /// <remarks>The file will not be automatically deleted.</remarks>
        [HttpPost("uploadAsync-from-uri-to-library")]
        public async Task<IActionResult> UploadFromUriToLibrary([FromBody] UploadFileFromUriToLibraryRequestDto request)
        {
            var uri = await _service.UploadFromUriToLibrary(request.FileUri, request.FileName, request.Overwrite, request.Metadata);
            return Ok(uri);
        }

        /// <summary>
        /// Retrieves a list of files with metadata from the specified container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>IActionResult with the list of files and their metadata.</returns>
        [HttpGet("files-list-with-metadata/{containerName}")]
        public async Task<IActionResult> GetFilesWithMetadataAsync([Required] string containerName)
        {
            var files = await _service.GetFilesWithMetadataAsync(containerName);
            return Ok(files);
        }

        /// <summary>
        /// Retrieves a list of all containers in the Azure Blob Storage Account.
        /// </summary>
        /// <returns>IActionResult with the list of container names.</returns>
        [HttpGet("containers-list")]
        public async Task<IActionResult> GetContainersAsync()
        {
            var containers = await _service.GetContainersAsync();
            return Ok(containers);
        }

        /// <summary>
        /// Downloads a file from the Azure Blob Storage Account.
        /// </summary>
        /// <param name="fileUri">The URI of the file to be downloaded.</param>
        /// <returns>IActionResult with the file stream.</returns>
        /// <remarks>The file is returned as an application/octet-stream.</remarks>
        [HttpGet("download-file-stream")]
        public async Task<IActionResult> DownloadFileAsync([Required] Uri fileUri)
        {
            var stream = await _service.DownloadFileAsync(fileUri);
            return File(stream, "application/octet-stream");
        }
    }
}

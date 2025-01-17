using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Azure.Core;
using PersonalWebApi.ActionFilters;
using PersonalWebApi.Services.FileStorage;
using PersonalWebApi.Models.Storage;

namespace PersonalWebApi.Controllers.Azure
{
    [ApiController]
    [Route("api/azure/blob-storage")]
    public class FileStorageController : ControllerBase
    {
        private readonly IFileStorageService _service;

        public FileStorageController(IFileStorageService service)
        {
            _service = service;
        }

        /// <summary>
        /// Uploads a file to the Azure Blob Storage Account in the `containerName` container.
        /// </summary>
        /// <param name="request">The request containing the file, overwrite flag, and metadata.</param>
        /// <returns>File id.</returns>
        /// <remarks>The file will not be automatically deleted.</remarks>
        [ServiceFilter(typeof(CheckConversationAccessFilter))]
        [HttpPost("container/{containerName}/upload")]
        public async Task<IActionResult> Upload([FromForm] UploadFileToFileStorageRequestDto request, [Required][FromRoute] string containerName)
        {
            var fileId = Guid.NewGuid();

            _service.SetContainer(containerName);
            var uri = await _service.UploadToContainerAsync(fileId, request.File, request.Overwrite, request.Metadata);
            return Ok(fileId);
        }

        /// <summary>
        /// Deletes a file from the Azure Blob Storage Account in the `containerName` container.
        /// </summary>
        /// <param name="fileName">The name of the file to be deleted.</param>
        /// <returns>IActionResult indicating the result of the delete operation.</returns>
        [HttpDelete("container/{containerName}/delete/{fileName}")]
        public async Task<IActionResult> Delete([Required][FromRoute] string containerName, [Required][FromRoute] string fileName)
        {
            _service.SetContainer(containerName);
            await _service.RemoveFromContainer(fileName);
            return Ok();
        }

        /// <summary>
        /// Uploads a file from a given URI to the Azure Blob Storage Account in the `containerName` container.
        /// </summary>
        /// <param name="request">The request containing the file URI, file name, TTL in days, overwrite flag, and metadata.</param>
        /// <returns>File ID.</returns>
        /// <remarks>The file will be automatically deleted after the specified TTL in days.</remarks>
        [HttpPost("container/{containerName}/upload/uri")]
        public async Task<IActionResult> UploadFromUri([FromBody][Required] UploadFileFromUriToFileStorageRequestDto request, [Required][FromRoute] string containerName)
        {
            var fileId = Guid.NewGuid();
            _service.SetContainer(containerName);
            var uri = await _service.UploadFromUriAsync(fileId , request.FileUri, request.FileName, request.Overwrite, request.Metadata);
            return Ok(fileId);
        }


        /// <summary>
        /// Retrieves a list of files with metadata from the specified container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>IActionResult with the list of files and their metadata.</returns>
        [HttpGet("container/{containerName}/files/list")]
        public async Task<IActionResult> GetFilesWithMetadataAsync([Required][FromRoute] string containerName)
        {
            _service.SetContainer(containerName);
            var files = await _service.GetFilesWithMetadataAsync();
            return Ok(files);
        }

        /// <summary>
        /// Retrieves a list of all containers in the Azure Blob Storage Account.
        /// </summary>
        /// <returns>IActionResult with the list of container names.</returns>
        [HttpGet("container/list")]
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
        [HttpGet("container/{containerName}/file/{fileUri}/download/stream")]
        public async Task<IActionResult> DownloadFileAsync([Required][FromRoute] string containerName, [Required][FromRoute] Uri fileUri)
        {
            _service.SetContainer(containerName);
            var stream = await _service.DownloadFileAsync(fileUri);
            return File(stream, "application/octet-stream");
        }
    }
}

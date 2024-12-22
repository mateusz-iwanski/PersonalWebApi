using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using PersonalWebApi.Services.Azure;
using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Controllers.Azure
{
    [ApiController]
    [Route("api/azure/[controller]")]
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
        /// <param name="ttlInDays">Set how many days it should be store.</param>
        /// <param name="overwrite">If file exists with the same name overwrite it.</param>
        /// <returns>IActionResult with URI</returns>
        /// <remarks>The files will be automatically deleted after ttlInDays</remarks>
        [HttpPost("upload-to-temp")]
        public async Task<IActionResult> UploadToTempAsync([Required] IFormFile file, [Range(0.1, double.MaxValue)][Required] double ttlInDays, bool overwrite = true)
        {
            var uri = await _service.UploadToTempAsync(file, ttlInDays, overwrite);
            return Ok(uri);
        }

        /// <summary>
        /// Upload file to Azure Blob Storage Account to the `library` Container name
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ttlInDays">Set how many days it should be store.</param>
        /// <param name="overwrite">If file exists with the same name overwrite it.</param>
        /// <returns>IActionResult with URI</returns>
        /// <remark>File will not automatically delete</remark>
        [HttpPost("upload-to-library")]
        public async Task<IActionResult> UploadToLibrary([Required] IFormFile file, bool overwrite = false)
        {
            var uri = await _service.UploadToLibrary(file, overwrite);
            return Ok(uri);
        }

        /// <summary>
        /// Delete file from Azure Blob Storage Account from the `file` Container name
        /// </summary>
        /// <param name="fileName">Name of the file to be deleted</param>
        /// <returns>IActionResult indicating the result of the delete operation</returns>
        [HttpDelete("delete-from-temp/{fileName}")]
        public async Task<IActionResult> DeleteFromTemp([Required][FromRoute] string fileName)
        {
            await _service.DeleteFileFromTemp(fileName);
            return Ok();
        }

        /// <summary>
        /// Delete file from Azure Blob Storage Account from the `library` Container name
        /// </summary>
        /// <param name="fileName">Name of the file to be deleted</param>
        /// <returns>IActionResult indicating the result of the delete operation</returns>
        [HttpDelete("delete-from-library/{fileName}")]
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
        /// <param name="overwrite">Indicates whether to overwrite the file if it already exists.</param>
        /// <returns>IActionResult containing the URI of the uploaded file.</returns>
        /// <remarks>
        /// The file will be automatically deleted after the specified ttlInDays.
        /// First it is downloaded from uri locally and then uploaded to the blob storage.
        /// After uploading the file is deleted from the local storage.
        /// </remarks>
        [HttpPost("upload-from-uri-to-temp")]
        public async Task<IActionResult> UploadFromUriToTemp([Required] string fileUri, [Required] string fileName, [Range(0.1, double.MaxValue)][Required] double ttlInDays, bool overwrite = false)
        {
            var uri = await _service.UploadFromUriToTemp(fileUri, fileName, ttlInDays, overwrite);
            return Ok(uri);
        }

        /// <summary>
        /// Uploads a file from a given URI to the Azure Blob Storage Account in the `library` container.
        /// </summary>
        /// <param name="fileUri">The URI of the file to be uploaded.</param>
        /// <param name="fileName">The name to be assigned to the uploaded file.</param>
        /// <param name="overwrite">Indicates whether to overwrite the file if it already exists.</param>
        /// <returns>IActionResult containing the URI of the uploaded file.</returns>
        /// <remarks>
        /// The file will not be automatically deleted.
        /// First it is downloaded from uri locally and then uploaded to the blob storage.
        /// After uploading the file is deleted from the local storage.
        /// </remarks>
        [HttpPost("upload-from-uri-to-library")]
        public async Task<IActionResult> UploadFromUriToLibrary([Required] string fileUri, [Required] string fileName, bool overwrite = false)
        {
            var uri = await _service.UploadFromUriToLibrary(fileUri, fileName, overwrite);
            return Ok(uri);
        }
    }
}

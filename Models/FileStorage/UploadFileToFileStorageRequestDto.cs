using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Models.Storage
{
    /// <summary>
    /// Using for uploading file to Azure Blob Storage Account.
    /// Metadata it's not working with swagger, it's always null
    /// </summary>
    public class UploadFileToFileStorageRequestDto
    {
        [Required]
        public IFormFile File { get; set; }

        public bool Overwrite { get; set; } = true;

        public Dictionary<string, string>? Metadata { get; set; }
    }

}

using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Models.Azure
{
    /// <summary>
    /// Using for uploading file from Url to Azure Blob Storage Account.
    /// Metadata it's not working with swagger, it's always null.
    /// </summary>
    public class UploadFileFromUriToLibraryRequestDto
    {
        [Required]
        public string FileUri { get; set; } = string.Empty;

        [Required]
        public string FileName { get; set; } = string.Empty;

        public bool Overwrite { get; set; } = true;

        public Dictionary<string, string>? Metadata { get; set; }
    }
}

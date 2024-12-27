using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Models.Azure
{
    /// <summary>
    /// Using for uploading file from Url to Azure Blob Storage Account.
    /// Metadata it's not working with swagger, it's always null.
    /// </summary>
    public class UploadFileFromUriToTempRequestDto
    {
        [Required]
        public string FileUri { get; set; } = string.Empty;

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Range(0.1, double.MaxValue)]
        [Required]
        public double TtlInDays { get; set; } = 7;

        public bool Overwrite { get; set; } = true;

        public Dictionary<string, string>? Metadata { get; set; }
    }
}

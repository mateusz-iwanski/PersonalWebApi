using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Models.Azure
{
    /// <summary>
    /// Using for uploading file to Azure Blob Storage Account.
    /// Metadata it's not working with swagger, it's always null
    /// </summary>
    public class UploadFileToTempRequestDto
    {
        [Required]
        public IFormFile File { get; set; }

        [Required]
        [Range(0.1, double.MaxValue)]
        public double TtlInDays { get; set; } = 7;

        public bool Overwrite { get; set; } = true;

        public Dictionary<string, string>? Metadata { get; set; }
    }

}

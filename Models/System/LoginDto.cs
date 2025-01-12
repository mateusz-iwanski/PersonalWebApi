using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Models.System
{
    /// <summary>
    /// User login Data Transfer Object for API
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// User email 
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User password
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

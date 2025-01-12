using Microsoft.Extensions.Diagnostics.HealthChecks;
using PersonalWebApi.Entities;
using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Models.System
{
    public class RegisterUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }
    }
}

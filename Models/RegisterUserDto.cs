using Microsoft.Extensions.Diagnostics.HealthChecks;
using PersonalWebApi.Entities;
using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Models
{
    public class RegisterUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}

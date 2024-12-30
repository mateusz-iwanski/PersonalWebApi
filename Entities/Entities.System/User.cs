using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace PersonalWebApi.Entities.System
{
    public class User
    {
        public int id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }

        public int RoleId { get; set; }

        [JsonIgnore]
        public virtual Role Role { get; set; }
    }
}

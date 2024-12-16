using Microsoft.AspNetCore.Identity;

namespace PersonalWebApi.Entities
{
    public class User
    {
        public int id { get; set; }
        public string Email { get; set; }                
        public string Name { get; set; }
        public string PasswordHash { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
    }
}

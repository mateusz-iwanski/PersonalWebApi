using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Entities
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}

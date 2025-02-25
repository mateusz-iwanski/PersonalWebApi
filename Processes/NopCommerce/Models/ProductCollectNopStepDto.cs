using nopCommerceApiHub.WebApi.DTOs;
using System.ComponentModel.DataAnnotations;

namespace PersonalWebApi.Processes.NopCommerce.Models
{
    public class ProductCollectNopStepDto
    {
        [Required]
        public string Sku { get; set; }
        public ProductDto Product { get; set; }
        public CategoryDto Category { get; set; }
    }
}

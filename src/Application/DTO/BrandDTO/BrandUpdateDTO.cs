
using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.BrandDTO
{
    public class BrandUpdateDTO
    {
        [Required]
        [MinLength(2)]
        [MaxLength(80)]
        public string Name { get; set; } = null!;

        [MaxLength(200)]
        public string? Description { get; set; }
    }
}

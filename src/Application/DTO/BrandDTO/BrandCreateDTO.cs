
using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.BrandDTO
{
    /// <summary>
    /// DTO utilizado para crear una nueva marca.
    /// </summary>
    public class BrandCreateDTO
    {
        /// <summary>
        /// Nombre de la marca.
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(80)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Descripción opcional de la marca.
        /// </summary>
        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
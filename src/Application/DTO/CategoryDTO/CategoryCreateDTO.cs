using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.CategoryDTO
{
    /// <summary>
    /// DTO utilizado para crear una nueva categoría.
    /// </summary>
    public class CategoryCreateDTO
    {
        /// <summary>
        /// Nombre de la categoría.
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(80)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Descripción opcional de la categoría.
        /// </summary>
        [MaxLength(200)]
        public string? Description { get; set; }
    }
}

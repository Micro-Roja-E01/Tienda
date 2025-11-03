using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.CategoryDTO
{
    /// <summary>
    /// DTO utilizado para actualizar la información de una categoría existente.
    /// </summary>
    public class CategoryUpdateDTO
    {
        /// <summary>
        /// Nombre actualizado de la categoría.
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(80)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Descripción actualizada de la categoría.
        /// </summary>
        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
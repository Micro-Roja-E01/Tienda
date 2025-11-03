
using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.BrandDTO
{
    /// <summary>
    /// DTO utilizado para actualizar la información de una marca existente.
    /// </summary>
    public class BrandUpdateDTO
    {
        /// <summary>
        /// Nombre actualizado de la marca.
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(80)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Descripción actualizada de la marca.
        /// </summary>
        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
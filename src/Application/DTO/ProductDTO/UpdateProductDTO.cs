using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Tienda.src.Application.Services.Validators;

namespace Tienda.src.Application.DTO.ProductDTO
{
    /// <summary>
    /// DTO para actualizar un producto existente.
    /// </summary>
    public class UpdateProductDTO
    {
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
        [SanitizeHtml]
        public string? Title { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres.")]
        [MinLength(10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
        [SanitizeHtml]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public int? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int? Stock { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100.")]
        public int? Discount { get; set; }

        [RegularExpression("^(Nuevo|Usado)$", ErrorMessage = "El estado debe ser 'Nuevo' o 'Usado'.")]
        public string? Status { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de categoría debe tener entre 3 y 50 caracteres.")]
        public string? CategoryName { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de marca debe tener entre 3 y 50 caracteres.")]
        public string? BrandName { get; set; }

        /// <summary>
        /// Nuevas imágenes a agregar al producto (se subirán a Cloudinary)
        /// </summary>
        public List<IFormFile>? NewImages { get; set; }

        /// <summary>
        /// IDs de imágenes a eliminar del producto (se eliminarán de Cloudinary y BD)
        /// </summary>
        public List<int>? ImageIdsToDelete { get; set; }
    }
}
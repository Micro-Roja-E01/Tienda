using System.ComponentModel.DataAnnotations;
using Tienda.src.Application.Services.Validators;

namespace Tienda.src.Application.DTO.ProductDTO
{
    /// <summary>
    /// DTO para la creación de un nuevo producto via JSON.
    /// </summary>
    public class CreateProductJsonDTO
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
        [SanitizeHtml]
        public required string Title { get; set; }

        [Required(ErrorMessage = "La descripción del producto es obligatoria.")]
        [StringLength(100, ErrorMessage = "La descripción no puede tener más de 100 caracteres.")]
        [MinLength(10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
        [SanitizeHtml]
        public required string Description { get; set; }

        [Required(ErrorMessage = "El precio del producto es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El precio debe ser un valor entero positivo.")]
        public required int Price { get; set; }

        [Required(ErrorMessage = "El stock del producto es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public required int Stock { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100.")]
        public int Discount { get; set; } = 0;

        [Required(ErrorMessage = "El estado del producto es obligatorio.")]
        [RegularExpression("^(Nuevo|Usado)$", ErrorMessage = "El estado debe ser 'Nuevo' o 'Usado'.")]
        public required string Status { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría del producto es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de la categoría no puede tener más de 50 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre de la categoría debe tener al menos 3 caracteres.")]
        public required string CategoryName { get; set; }

        [Required(ErrorMessage = "El nombre de la marca del producto es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de la marca no puede tener más de 50 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre de la marca debe tener al menos 3 caracteres.")]
        public required string BrandName { get; set; }

        /// <summary>
        /// Lista de URLs de imágenes para el producto
        /// </summary>
        public List<ImageUrlDTO>? ImageUrls { get; set; } = new List<ImageUrlDTO>();
    }

    /// <summary>
    /// DTO para URLs de imágenes
    /// </summary>
    public class ImageUrlDTO
    {
        [Required(ErrorMessage = "La URL de la imagen es obligatoria.")]
        [Url(ErrorMessage = "Debe ser una URL válida.")]
        public required string Url { get; set; }

        [StringLength(100, ErrorMessage = "El texto alternativo no puede tener más de 100 caracteres.")]
        public string? Alt { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.ProductDTO
{
    /// <summary>
    /// DTO para la creación de un nuevo producto.
    /// </summary>
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "La descripción del producto es obligatoria.")]
        [StringLength(100, ErrorMessage = "La descripción no puede tener más de 100 caracteres.")]
        [MinLength(10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "El precio del producto es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El precio debe ser un valor entero positivo.")]
        public required int Price { get; set; }

        [Required(ErrorMessage = "El stock del producto es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public required int Stock { get; set; }

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

        [Required(ErrorMessage = "La imagen del producto es obligatoria.")]
        public required List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
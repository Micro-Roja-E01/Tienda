namespace Tienda.src.Application.Domain.Models
{
    /// <summary>
    /// Representa una imagen asociada a un producto.
    /// Implementa soft delete para permitir restauración.
    /// </summary>
    public class Image
    {
        /// <summary>
        /// Identificador único de la imagen.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// URL de la imagen en Cloudinary.
        /// </summary>
        public required string ImageUrl { get; set; }

        /// <summary>
        /// Identificador público de la imagen en Cloudinary.
        /// </summary>
        public required string PublicId { get; set; }

        /// <summary>
        /// Identificador del producto asociado a la imagen.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Fecha de creación de la imagen.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica si la imagen ha sido marcada como eliminada (soft delete).
        /// Las imágenes eliminadas no se borran de Cloudinary para permitir restauración.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Fecha y hora en que la imagen fue eliminada (si IsDeleted es true).
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}
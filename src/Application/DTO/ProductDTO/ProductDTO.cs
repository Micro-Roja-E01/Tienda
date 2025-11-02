namespace tienda.src.Application.DTO.ProductDTO
{
    /// <summary>
    /// DTO genérico que representa un producto completo en el sistema.
    /// </summary>
    public class ProductDTO
    {
        /// <summary>
        /// Identificador único del producto.
        /// </summary>
        public required int Id { get; set; }

        /// <summary>
        /// Título o nombre del producto.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Descripción del producto.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// URL de la imagen principal del producto.
        /// </summary>
        public required string? ImageUrl { get; set; }

        /// <summary>
        /// Precio del producto (puede incluir decimales).
        /// </summary>
        public required decimal Price { get; set; }

        /// <summary>
        /// Cantidad disponible en stock.
        /// </summary>
        public required int Stock { get; set; }

        /// <summary>
        /// Identificador de la categoría asociada.
        /// </summary>
        public required int CategoryId { get; set; }

        /// <summary>
        /// Identificador de la marca asociada.
        /// </summary>
        public required int BrandId { get; set; }

        /// <summary>
        /// Indica si el producto está disponible.
        /// </summary>
        public required bool IsAvailable { get; set; }

        /// <summary>
        /// Fecha de creación del producto.
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha de última actualización del producto.
        /// </summary>
        public required DateTime UpdatedAt { get; set; }
    }
}
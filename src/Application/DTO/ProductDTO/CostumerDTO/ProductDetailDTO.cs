namespace tienda.src.Application.DTO.ProductDTO.CostumerDTO
{
    /// <summary>
    /// DTO que representa el detalle completo de un producto visible para clientes.
    /// </summary>
    public class ProductDetailDTO
    {
        /// <summary>
        /// Identificador único del producto.
        /// </summary>
        public required int Id { get; set; }

        /// <summary>
        /// Nombre o título del producto.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Descripción detallada del producto.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// URL de la imagen principal del producto.
        /// </summary>
        public required string? MainImageURL { get; set; }

        /// <summary>
        /// Lista de URLs de imágenes adicionales del producto.
        /// </summary>
        public required List<string> ImageUrls { get; set; } = new();

        /// <summary>
        /// Precio original del producto (en CLP).
        /// </summary>
        public required int Price { get; set; }

        /// <summary>
        /// Precio final del producto después del descuento.
        /// </summary>
        public required int FinalPrice { get; set; }

        /// <summary>
        /// Porcentaje de descuento aplicado.
        /// </summary>
        public required int DiscountPercentage { get; set; }

        /// <summary>
        /// Cantidad disponible en stock.
        /// </summary>
        public required int Stock { get; set; }

        /// <summary>
        /// Indicador textual del estado del stock (por ejemplo: “Disponible”, “Agotado”).
        /// </summary>
        public required string StockIndicator { get; set; }

        /// <summary>
        /// Nombre de la categoría asociada al producto.
        /// </summary>
        public required string CategoryName { get; set; }

        /// <summary>
        /// Nombre de la marca asociada al producto.
        /// </summary>
        public required string BrandName { get; set; }

        /// <summary>
        /// Indica si el producto está disponible para la venta.
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
using Bogus.DataSets;

namespace tienda.src.Application.DTO.ProductDTO.AdminDTO
{
    /// <summary>
    /// DTO que representa la información de un producto dentro del panel de administración.
    /// </summary>
    public class ProductForAdminDTO
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
        /// URL de la imagen principal del producto.
        /// </summary>
        public required string? MainImageURL { get; set; }

        /// <summary>
        /// Precio actual del producto (como string formateado para mostrar).
        /// </summary>
        public required string Price { get; set; }

        /// <summary>
        /// Cantidad disponible en stock.
        /// </summary>
        public required int Stock { get; set; }

        /// <summary>
        /// Indicador textual del estado de stock (por ejemplo: “En stock”, “Poco stock”).
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
        /// Estado del producto (por ejemplo: “Nuevo” o “Usado”).
        /// </summary>
        public required string StatusName { get; set; }

        /// <summary>
        /// Indica si el producto está disponible para la venta.
        /// </summary>
        public required bool IsAvailable { get; set; }

        /// <summary>
        /// Fecha de última actualización del producto.
        /// </summary>
        public required DateTime UpdateAt { get; set; }
    }
}
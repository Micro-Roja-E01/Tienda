namespace tienda.src.Application.DTO.ProductDTO.CostumerDTO
{
    /// <summary>
    /// DTO resumido que representa un producto visible para clientes en listados o catálogos.
    /// </summary>
    public class ProductForCostumerDTO
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
        /// URL principal de la imagen del producto.
        /// </summary>
        public string? MainImageURL { get; set; }

        /// <summary>
        /// Precio original del producto (en pesos chilenos).
        /// </summary>
        public required int Price { get; set; }

        /// <summary>
        /// Porcentaje de descuento aplicado (0-100)
        /// </summary>
        public required int Discount { get; set; }

        /// <summary>
        /// Precio final calculado en servidor: Price - ceil(Price * Discount/100)
        /// </summary>
        public required int FinalPrice { get; set; }

        /// <summary>
        /// Cantidad de unidades disponibles en inventario.
        /// </summary>
        public required int Stock { get; set; }

        /// <summary>
        /// Indicador textual del nivel de stock.
        /// </summary>
        public required string StockIndicator { get; set; }

        /// <summary>
        /// Nombre de la categoría del producto.
        /// </summary>
        public required string CategoryName { get; set; }

        /// <summary>
        /// Nombre de la marca del producto.
        /// </summary>
        public required string BrandName { get; set; }

        /// <summary>
        /// Indica si el producto está disponible para su compra.
        /// </summary>
        public required bool IsAvailable { get; set; }

        /// <summary>
        /// Indica si el producto tiene descuento activo
        /// </summary>
        public bool HasDiscount => Discount > 0;
    }
}
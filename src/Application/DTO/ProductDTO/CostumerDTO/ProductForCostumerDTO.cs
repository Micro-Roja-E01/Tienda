namespace tienda.src.Application.DTO.ProductDTO.CostumerDTO
{
    /// <summary>
    /// DTO para mostrar productos en el catálogo público (clientes)
    /// </summary>
    public class ProductForCostumerDTO
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public string? MainImageURL { get; set; }

        /// <summary>
        /// Precio original antes de descuento (CLP)
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

        public required int Stock { get; set; }

        /// <summary>
        /// Indicador de disponibilidad: "Sin stock", "Últimas unidades", "" (stock normal)
        /// </summary>
        public required string StockIndicator { get; set; }

        public required string CategoryName { get; set; }
        public required string BrandName { get; set; }

        /// <summary>
        /// Indica si el producto está activo para venta
        /// </summary>
        public required bool IsAvailable { get; set; }

        /// <summary>
        /// Indica si el producto tiene descuento activo
        /// </summary>
        public bool HasDiscount => Discount > 0;
    }
}
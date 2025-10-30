namespace tienda.src.Application.DTO.ProductDTO.CostumerDTO
{
    public class ProductForCostumerDTO
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public string? MainImageURL { get; set; }

        // CLP enteros (consistente con DB)
        public required int Price { get; set; }

        // % descuento aplicado (0 si no hay)
        public required int DiscountPercentage { get; set; }

        // Price - ceil(Price * DiscountPercentage/100)
        public required int FinalPrice { get; set; }

        public required int Stock { get; set; }
        public required string StockIndicator { get; set; }

        public required string CategoryName { get; set; }
        public required string BrandName { get; set; }

        public required bool IsAvailable { get; set; }
    }
}

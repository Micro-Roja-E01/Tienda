namespace tienda.src.Application.DTO.ProductDTO.CostumerDTO
{
public class ProductDetailDTO
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string? MainImageURL { get; set; }
        public required List<string> ImageUrls { get; set; } = new();
        public required int Price { get; set; }                // ← cambiar a int
        public required int FinalPrice { get; set; }           // ← NUEVO (CLP)
        public required int DiscountPercentage { get; set; }
        public required int Stock { get; set; }
        public required string StockIndicator { get; set; }
        public required string CategoryName { get; set; }
        public required string BrandName { get; set; }
        public required bool IsAvailable { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
    }
}
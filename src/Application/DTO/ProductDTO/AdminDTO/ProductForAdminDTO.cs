using Bogus.DataSets;

namespace tienda.src.Application.DTO.ProductDTO.AdminDTO
{
    public class ProductForAdminDTO
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string? MainImageURL { get; set; }
        public required string Price { get; set; }
        public required int Stock { get; set; }
        public required string StockIndicator { get; set; }
        public required string CategoryName { get; set; }
        public required string BrandName { get; set; }
        public required string StatusName { get; set; }
        public required bool IsAvailable { get; set; }
        public required DateTime UpdateAt { get; set; }
    }
}
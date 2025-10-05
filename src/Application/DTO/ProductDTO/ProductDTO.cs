namespace tienda.src.Application.DTO.ProductDTO
{
    public class ProductDTO
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string? ImageUrl { get; set; }
        public required decimal Price { get; set; }
        public required int Stock { get; set; }
        public required int CategoryId { get; set; }
        public required int BrandId { get; set; }
        public required bool IsAvailable { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
    }
}
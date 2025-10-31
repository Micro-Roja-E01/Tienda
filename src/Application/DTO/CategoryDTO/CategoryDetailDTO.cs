namespace Tienda.src.Application.DTO.CategoryDTO
{
    public class CategoryDetailDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string? Description { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
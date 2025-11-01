
namespace Tienda.src.Application.DTO.BrandDTO
{
    public class BrandListItemDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

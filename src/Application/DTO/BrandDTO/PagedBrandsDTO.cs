
namespace Tienda.src.Application.DTO.BrandDTO
{
    public class PagedBrandsDTO
    {
        public required List<BrandListItemDTO> Brands { get; set; }
        public required int TotalCount { get; set; }
        public required int TotalPages { get; set; }
        public required int CurrentPage { get; set; }
        public required int PageSize { get; set; }
    }
}

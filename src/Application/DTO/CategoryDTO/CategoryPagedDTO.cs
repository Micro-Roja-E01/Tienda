namespace Tienda.src.Application.DTO.CategoryDTO
{
    public class PagedCategoriesDTO
    {
        public required List<CategoryListItemDTO> Categories { get; set; }
        public required int TotalCount { get; set; }
        public required int TotalPages { get; set; }
        public required int CurrentPage { get; set; }
        public required int PageSize { get; set; }
    }
}
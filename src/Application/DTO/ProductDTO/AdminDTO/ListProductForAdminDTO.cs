namespace tienda.src.Application.DTO.ProductDTO.AdminDTO
{
    public class ListedProductsForAdminDTO
    {
        public required List<ProductForAdminDTO> Products { get; set; }
        public required int TotalCount { get; set; }
        public required int TotalPages { get; set; }
        public required int CurrentPage { get; set; }
        public required int PageSize { get; set; }
    }
}
namespace tienda.src.Application.DTO.ProductDTO.CostumerDTO
{
    public class ListedProductsForCostumerDTO
    {
        public required List<ProductForCostumerDTO> Products { get; set; }
        public required int TotalCount { get; set; }
        public required int TotalPages { get; set; }
        public required int CurrentPage { get; set; }
        public required int PageSize { get; set; }
    }
}
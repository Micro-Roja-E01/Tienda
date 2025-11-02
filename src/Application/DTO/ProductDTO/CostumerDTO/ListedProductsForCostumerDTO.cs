namespace tienda.src.Application.DTO.ProductDTO.CostumerDTO
{
    /// <summary>
    /// DTO que representa una lista paginada de productos visibles para clientes.
    /// </summary>
    public class ListedProductsForCostumerDTO
    {
        /// <summary>
        /// Lista de productos disponibles en la página actual.
        /// </summary>
        public required List<ProductForCostumerDTO> Products { get; set; }

        /// <summary>
        /// Total de productos encontrados.
        /// </summary>
        public required int TotalCount { get; set; }

        /// <summary>
        /// Total de páginas disponibles.
        /// </summary>
        public required int TotalPages { get; set; }

        /// <summary>
        /// Número de página actual.
        /// </summary>
        public required int CurrentPage { get; set; }

        /// <summary>
        /// Tamaño de página (cantidad de productos por página).
        /// </summary>
        public required int PageSize { get; set; }
    }
}

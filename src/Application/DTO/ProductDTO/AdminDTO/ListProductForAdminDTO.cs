namespace tienda.src.Application.DTO.ProductDTO.AdminDTO
{
    /// <summary>
    /// DTO que representa una lista paginada de productos para el panel de administración.
    /// </summary>
    public class ListedProductsForAdminDTO
    {
        /// <summary>
        /// Lista de productos obtenidos en la página actual.
        /// </summary>
        public required List<ProductForAdminDTO> Products { get; set; }

        /// <summary>
        /// Número total de productos encontrados.
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
        /// Cantidad de productos por página.
        /// </summary>
        public required int PageSize { get; set; }
    }
}
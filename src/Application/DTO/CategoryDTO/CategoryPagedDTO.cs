namespace Tienda.src.Application.DTO.CategoryDTO
{
    /// <summary>
    /// DTO que representa una lista paginada de categorías.
    /// </summary>
    public class PagedCategoriesDTO
    {
        /// <summary>
        /// Lista de categorías en la página actual.
        /// </summary>
        public required List<CategoryListItemDTO> Categories { get; set; }

        /// <summary>
        /// Total de registros encontrados.
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
        /// Cantidad de registros por página.
        /// </summary>
        public required int PageSize { get; set; }
    }
}

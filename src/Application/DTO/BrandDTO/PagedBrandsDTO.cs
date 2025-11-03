
namespace Tienda.src.Application.DTO.BrandDTO
{
    /// <summary>
    /// DTO que encapsula un listado paginado de marcas.
    /// </summary>
    public class PagedBrandsDTO
    {
        /// <summary>
        /// Lista de marcas obtenidas en la página actual.
        /// </summary>
        public required List<BrandListItemDTO> Brands { get; set; }

        /// <summary>
        /// Número total de registros encontrados.
        /// </summary>
        public required int TotalCount { get; set; }

        /// <summary>
        /// Número total de páginas disponibles.
        /// </summary>
        public required int TotalPages { get; set; }

        /// <summary>
        /// Número de la página actual.
        /// </summary>
        public required int CurrentPage { get; set; }

        /// <summary>
        /// Tamaño de página (cantidad de registros por página).
        /// </summary>
        public required int PageSize { get; set; }
    }
}
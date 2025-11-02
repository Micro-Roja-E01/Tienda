namespace Tienda.src.Application.DTO.AdminUserDTO
{
    /// <summary>
    /// DTO que encapsula un resultado paginado de usuarios para administración.
    /// </summary>
    public class PagedAdminUsersDTO
    {
        /// <summary>
        /// Lista de usuarios de la página actual.
        /// </summary>
        public required List<AdminUserListItemDTO> Users { get; set; }

        /// <summary>
        /// Total de usuarios encontrados según los filtros.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total de páginas disponibles.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Página actual.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Tamaño de página (cantidad de usuarios por página).
        /// </summary>
        public int PageSize { get; set; }
    }
}

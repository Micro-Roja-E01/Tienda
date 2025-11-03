namespace Tienda.src.Application.DTO.OrderDTO
{
    /// <summary>
    /// DTO que representa una lista paginada de órdenes con metadatos de paginación.
    /// Utilizado para retornar el historial de órdenes del usuario con soporte de búsqueda y navegación por páginas.
    /// </summary>
    public class ListedOrderDetailDTO
    {
        /// <summary>
        /// Lista de órdenes de la página actual.
        /// Contiene los detalles completos de cada orden incluyendo sus items.
        /// </summary>
        public required List<OrderDetailDTO> Orders { get; set; } = new List<OrderDetailDTO>();

        /// <summary>
        /// Cantidad total de órdenes que coinciden con los criterios de búsqueda.
        /// Este número puede ser mayor que la cantidad de items en la lista actual si hay múltiples páginas.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Número total de páginas disponibles basado en TotalCount y PageSize.
        /// Se calcula como: Math.Ceiling(TotalCount / PageSize)
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Número de la página actual (basado en 1).
        /// Por ejemplo, 1 para la primera página, 2 para la segunda, etc.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Cantidad de órdenes por página.
        /// Define cuántos elementos se incluyen en la propiedad Orders.
        /// </summary>
        public int PageSize { get; set; }
    }
}
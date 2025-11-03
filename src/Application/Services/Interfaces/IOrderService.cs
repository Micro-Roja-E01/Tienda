using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.DTO.OrderDTO;

namespace Tienda.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz del servicio de órdenes de compra.
    /// Define las operaciones de lógica de negocio para crear y consultar órdenes.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Crea una nueva orden a partir del carrito del usuario autenticado.
        /// Proceso:
        /// 1. Valida que el carrito tenga items
        /// 2. Genera un código único para la orden
        /// 3. Convierte los items del carrito en items de orden (snapshot histórico)
        /// 4. Actualiza el stock de los productos
        /// 5. Vacía el carrito del usuario
        /// Usa transacciones con rollback automático en caso de error.
        /// </summary>
        /// <param name="userId">ID del usuario autenticado</param>
        /// <returns>Código único de la orden creada (formato: ORD-YYMMDDHHMMSS-XXX)</returns>
        /// <exception cref="KeyNotFoundException">Si el carrito no existe</exception>
        /// <exception cref="InvalidOperationException">Si el carrito está vacío</exception>
        Task<string> CreateAsync(int userId);

        /// <summary>
        /// Obtiene los detalles completos de una orden por su código único.
        /// Incluye todos los items con información histórica del momento de la compra.
        /// </summary>
        /// <param name="orderCode">Código único de la orden</param>
        /// <returns>Detalles completos de la orden incluyendo items, totales y fecha de compra</returns>
        /// <exception cref="KeyNotFoundException">Si la orden no existe</exception>
        Task<OrderDetailDTO> GetDetailAsync(string orderCode);

        /// <summary>
        /// Obtiene el historial de órdenes de un usuario con paginación y búsqueda.
        /// Permite filtrar por código de orden, título o descripción de productos.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y paginación (SearchTerm, PageNumber, PageSize)</param>
        /// <param name="userId">ID del usuario al que pertenecen las órdenes</param>
        /// <returns>Lista paginada de órdenes con metadata de paginación</returns>
        /// <exception cref="ArgumentOutOfRangeException">Si el número de página está fuera de rango</exception>
        Task<ListedOrderDetailDTO> GetByUserIdAsync(SearchParamsDTO searchParams, int userId);
    }
}
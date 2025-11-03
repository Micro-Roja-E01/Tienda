using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.Domain.Models;

namespace Tienda.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz del repositorio de órdenes.
    /// Define las operaciones de acceso a datos para la gestión de órdenes de compra.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Obtiene una orden completa por su código único.
        /// Incluye todos los items de la orden.
        /// </summary>
        /// <param name="orderCode">Código único de la orden (formato: ORD-YYMMDDHHMMSS-XXX)</param>
        /// <returns>La orden completa con sus items, o null si no se encuentra</returns>
        Task<Order?> GetByCodeAsync(string orderCode);

        /// <summary>
        /// Crea una nueva orden en la base de datos.
        /// Verifica que no exista una orden con el mismo código antes de crear.
        /// </summary>
        /// <param name="order">Orden a crear con todos sus items</param>
        /// <returns>True si la creación fue exitosa, false si ya existe una orden con ese código</returns>
        Task<bool> CreateAsync(Order order);

        /// <summary>
        /// Verifica si existe una orden con el código especificado.
        /// Útil para validar unicidad de códigos al generar nuevas órdenes.
        /// </summary>
        /// <param name="orderCode">Código de la orden a verificar</param>
        /// <returns>True si el código existe, false en caso contrario</returns>
        Task<bool> CodeExistsAsync(string orderCode);

        /// <summary>
        /// Obtiene las órdenes de un usuario con paginación y búsqueda.
        /// Permite filtrar por código de orden, título o descripción de productos.
        /// Retorna tanto las órdenes de la página como el conteo total para calcular paginación.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda (SearchTerm, PageNumber, PageSize)</param>
        /// <param name="userId">ID del usuario propietario de las órdenes</param>
        /// <returns>Tupla con la lista de órdenes de la página actual y el conteo total de órdenes</returns>
        Task<(IEnumerable<Order> orders, int totalCount)> GetByUserIdAsync(SearchParamsDTO searchParams, int userId);
    }
}
using Tienda.src.Application.Domain.Models;

namespace Tienda.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz del repositorio de carritos.
    /// Define las operaciones de acceso a datos para la gestión de carritos y sus items.
    /// </summary>
    public interface ICartRepository
    {
        /// <summary>
        /// Busca un carrito por buyerId y opcionalmente por userId.
        /// Incluye los items del carrito con sus productos relacionados.
        /// </summary>
        /// <param name="buyerId">Identificador del comprador</param>
        /// <param name="userId">ID del usuario autenticado (opcional)</param>
        /// <returns>El carrito encontrado o null si no existe</returns>
        Task<Cart?> FindAsync(string buyerId, int? userId);

        /// <summary>
        /// Obtiene el carrito de un usuario autenticado por su ID.
        /// Incluye los items del carrito con sus productos relacionados.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>El carrito del usuario o null si no existe</returns>
        Task<Cart?> GetByUserIdAsync(int userId);

        /// <summary>
        /// Obtiene un carrito anónimo por su buyerId.
        /// Incluye los items del carrito con sus productos relacionados.
        /// </summary>
        /// <param name="buyerId">Identificador del comprador anónimo</param>
        /// <returns>El carrito anónimo o null si no existe</returns>
        Task<Cart?> GetAnonymousAsync(string buyerId);

        /// <summary>
        /// Crea un nuevo carrito en la base de datos.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="userId">ID del usuario autenticado (opcional, null para anónimos)</param>
        /// <returns>El carrito recién creado</returns>
        Task<Cart> CreateAsync(string buyerId, int? userId = null);

        /// <summary>
        /// Actualiza un carrito existente en la base de datos.
        /// Guarda cambios en totales, items y demás propiedades.
        /// </summary>
        /// <param name="cart">Carrito con los cambios a guardar</param>
        Task UpdateAsync(Cart cart);

        /// <summary>
        /// Elimina un carrito de la base de datos.
        /// Útil al fusionar carritos anónimos con carritos de usuario.
        /// </summary>
        /// <param name="cart">Carrito a eliminar</param>
        Task DeleteAsync(Cart cart);

        /// <summary>
        /// Agrega un nuevo item al carrito en la base de datos.
        /// </summary>
        /// <param name="cart">Carrito al que se agregará el item</param>
        /// <param name="cartItem">Item a agregar</param>
        Task AddItemAsync(Cart cart, CartItem cartItem);

        /// <summary>
        /// Elimina un item específico del carrito en la base de datos.
        /// </summary>
        /// <param name="cartItem">Item a eliminar</param>
        Task RemoveItemAsync(CartItem cartItem);

        /// <summary>
        /// Obtiene los carritos de usuarios registrados que no han sido modificados
        /// en el número de días especificado y que tienen items.
        /// </summary>
        /// <param name="inactiveDays">Número de días sin modificaciones</param>
        /// <returns>Lista de tuplas con UserId, Email, UserName y LastModified</returns>
        Task<List<(int UserId, string Email, string UserName, DateTime LastModified)>> GetInactiveCartsAsync(int inactiveDays);
    }
}
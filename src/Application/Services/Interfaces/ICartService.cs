using Tienda.src.Application.DTO.CartDTO;

namespace Tienda.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz del servicio de carrito de compras.
    /// Define las operaciones de lógica de negocio para gestionar carritos tanto de usuarios autenticados como anónimos.
    /// </summary>
    public interface ICartService
    {
        /// <summary>
        /// Agrega un producto al carrito o incrementa su cantidad si ya existe.
        /// Valida stock disponible antes de agregar.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="productId">ID del producto a agregar</param>
        /// <param name="quantity">Cantidad de unidades a agregar</param>
        /// <param name="userId">ID del usuario autenticado (opcional, null para anónimos)</param>
        /// <returns>El carrito actualizado con el nuevo item</returns>
        /// <exception cref="KeyNotFoundException">Si el producto no existe</exception>
        /// <exception cref="ArgumentException">Si no hay suficiente stock</exception>
        Task<CartDTO> AddItemAsync(string buyerId, int productId, int quantity, int? userId = null);

        /// <summary>
        /// Elimina completamente un producto del carrito.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="productId">ID del producto a eliminar</param>
        /// <param name="userId">ID del usuario autenticado (opcional, null para anónimos)</param>
        /// <returns>El carrito actualizado sin el producto eliminado</returns>
        /// <exception cref="KeyNotFoundException">Si el carrito o el producto no existen</exception>
        Task<CartDTO> RemoveItemAsync(string buyerId, int productId, int? userId = null);

        /// <summary>
        /// Vacía completamente el carrito, eliminando todos los items.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="userId">ID del usuario autenticado (opcional, null para anónimos)</param>
        /// <returns>El carrito vacío con totales en cero</returns>
        /// <exception cref="KeyNotFoundException">Si el carrito no existe</exception>
        Task<CartDTO> ClearAsync(string buyerId, int? userId = null);

        /// <summary>
        /// Actualiza la cantidad de un producto específico en el carrito.
        /// Si la cantidad es 0, elimina el producto del carrito.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="productId">ID del producto a actualizar</param>
        /// <param name="quantity">Nueva cantidad (0 para eliminar)</param>
        /// <param name="userId">ID del usuario autenticado (opcional, null para anónimos)</param>
        /// <returns>El carrito actualizado con la nueva cantidad</returns>
        /// <exception cref="KeyNotFoundException">Si el carrito o el producto no existen</exception>
        /// <exception cref="ArgumentException">Si no hay suficiente stock</exception>
        Task<CartDTO> UpdateItemQuantityAsync(string buyerId, int productId, int quantity, int? userId = null);

        /// <summary>
        /// Asocia un carrito anónimo con un usuario autenticado.
        /// Si el usuario ya tiene un carrito, fusiona ambos carritos sumando cantidades de productos duplicados.
        /// </summary>
        /// <param name="buyerId">Identificador del comprador anónimo</param>
        /// <param name="userId">ID del usuario autenticado</param>
        Task AssociateWithUserAsync(string buyerId, int userId);

        /// <summary>
        /// Valida y ajusta el carrito antes del checkout.
        /// Elimina productos sin stock y ajusta cantidades que excedan el stock disponible.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="userId">ID del usuario autenticado (opcional)</param>
        /// <returns>El carrito validado y ajustado</returns>
        /// <exception cref="KeyNotFoundException">Si el carrito no existe</exception>
        /// <exception cref="InvalidOperationException">Si el carrito está vacío</exception>
        Task<CartDTO> CheckoutAsync(string buyerId, int? userId);

        /// <summary>
        /// Obtiene un carrito existente o crea uno nuevo si no existe.
        /// Busca primero por buyerId y userId, luego solo por userId para usuarios autenticados.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="userId">ID del usuario autenticado (opcional, null para anónimos)</param>
        /// <returns>El carrito existente o uno nuevo</returns>
        Task<CartDTO> CreateOrGetCartAsync(string buyerId, int? userId = null);
    }
}
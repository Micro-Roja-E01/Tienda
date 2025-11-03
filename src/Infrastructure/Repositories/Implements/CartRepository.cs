using Microsoft.EntityFrameworkCore;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Infrastructure.Data;
using Tienda.src.Infrastructure.Repositories.Interfaces;

namespace Tienda.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Implementación del repositorio de carritos.
    /// Maneja todas las operaciones de acceso a datos para carritos y sus items,
    /// incluyendo la gestión de relaciones con productos e imágenes.
    /// </summary>
    public class CartRepository : ICartRepository
    {
        public readonly DataContext _context;

        /// <summary>
        /// Constructor que inyecta el contexto de base de datos.
        /// </summary>
        /// <param name="context">Contexto de Entity Framework para acceso a datos</param>
        public CartRepository(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Agrega un nuevo item al carrito y persiste los cambios.
        /// </summary>
        /// <param name="cart">Carrito al que se agregará el item</param>
        /// <param name="cartItem">Item a agregar</param>
        public async Task AddItemAsync(Cart cart, CartItem cartItem)
        {
            cart.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Crea un nuevo carrito en la base de datos con valores iniciales en cero.
        /// Retorna el carrito completo con todas sus relaciones cargadas.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="userId">ID del usuario autenticado (opcional)</param>
        /// <returns>El carrito recién creado con sus relaciones cargadas</returns>
        public async Task<Cart> CreateAsync(string buyerId, int? userId = null)
        {
            var cart = new Cart
            {
                BuyerId = buyerId,
                UserId = userId,
                SubTotal = 0,
                Total = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return await _context.Carts.Include(c => c.CartItems)
                                            .ThenInclude(ci => ci.Product)
                                                .ThenInclude(p => p.Images)
                                        .FirstAsync(c => c.Id == cart.Id);
        }

        /// <summary>
        /// Elimina un carrito de la base de datos.
        /// </summary>
        /// <param name="cart">Carrito a eliminar</param>
        public async Task DeleteAsync(Cart cart)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Busca un carrito por buyerId y opcionalmente por userId.
        /// Lógica de búsqueda:
        /// 1. Si userId es proporcionado, busca primero por userId
        /// 2. Si no se encuentra, busca por buyerId para carritos anónimos
        /// 3. Si se encuentra un carrito anónimo y se proporciona userId, lo asocia automáticamente
        /// Incluye todos los items con sus productos e imágenes.
        /// </summary>
        /// <param name="buyerId">Identificador del comprador</param>
        /// <param name="userId">ID del usuario autenticado (opcional)</param>
        /// <returns>El carrito encontrado con todas sus relaciones, o null</returns>
        public async Task<Cart?> FindAsync(string buyerId, int? userId)
        {
            Cart? cart = null;
            if (userId.HasValue)
            {
                cart = await _context.Carts.Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart != null)
                {
                    if (cart.BuyerId != buyerId)
                    {
                        cart.BuyerId = buyerId;
                        cart.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    return cart;
                }
            }
            cart = await _context.Carts.Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.BuyerId == buyerId && c.UserId == null);
            if (cart != null && userId.HasValue)
            {
                cart.UserId = userId;
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        /// <summary>
        /// Obtiene un carrito anónimo por su buyerId.
        /// Solo retorna carritos sin userId asociado.
        /// Incluye todos los items con sus productos e imágenes.
        /// </summary>
        /// <param name="buyerId">Identificador del comprador anónimo</param>
        /// <returns>El carrito anónimo o null si no existe</returns>
        public async Task<Cart?> GetAnonymousAsync(string buyerId)
        {
            return await _context.Carts.Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.BuyerId == buyerId && c.UserId == null);
        }

        /// <summary>
        /// Obtiene el carrito de un usuario autenticado por su ID.
        /// Incluye todos los items con sus productos.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>El carrito del usuario o null si no existe</returns>
        public async Task<Cart?> GetByUserIdAsync(int userId)
        {
            return await _context.Carts.Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        /// <summary>
        /// Elimina un item específico del carrito.
        /// </summary>
        /// <param name="cartItem">Item a eliminar</param>
        public async Task RemoveItemAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Actualiza un carrito existente en la base de datos.
        /// Actualiza automáticamente la fecha de modificación (UpdatedAt).
        /// Recarga las relaciones después de actualizar para mantener consistencia.
        /// </summary>
        /// <param name="cart">Carrito con los cambios a guardar</param>
        public async Task UpdateAsync(Cart cart)
        {
            cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            // TODO: Esto lo agrego Copilot para corregir las relaciones. Habria que probar si funciona bien.
            // Recargar el carrito con sus relaciones para que el mapeo funcione correctamente
            await _context.Entry(cart)
                .Collection(c => c.CartItems)
                .Query()
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                .LoadAsync();
        }
    }
}
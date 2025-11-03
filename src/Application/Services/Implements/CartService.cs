using Mapster;
using Serilog;
using tienda.src.Infrastructure.Repositories.Interfaces;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.CartDTO;
using Tienda.src.Application.Services.Interfaces;
using Tienda.src.Infrastructure.Repositories.Interfaces;

namespace Tienda.src.Application.Services.Implements
{
    /// <summary>
    /// Implementación del servicio de carrito de compras.
    /// Gestiona la lógica de negocio para operaciones de carrito, validaciones de stock,
    /// cálculo de totales y manejo de carritos anónimos y autenticados.
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        /// <summary>
        /// Constructor que inyecta las dependencias necesarias.
        /// </summary>
        /// <param name="cartRepository">Repositorio para acceso a datos de carritos</param>
        /// <param name="productRepository">Repositorio para validación de productos y stock</param>
        public CartService(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        /// <summary>
        /// Agrega un producto al carrito o incrementa su cantidad si ya existe.
        /// Realiza validaciones de existencia del producto y disponibilidad de stock.
        /// Crea un nuevo carrito si no existe uno para el buyerId especificado.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="productId">ID del producto a agregar</param>
        /// <param name="quantity">Cantidad de unidades a agregar</param>
        /// <param name="userId">ID del usuario autenticado (opcional)</param>
        /// <returns>El carrito actualizado con el nuevo item</returns>
        /// <exception cref="KeyNotFoundException">Si el producto no existe</exception>
        /// <exception cref="ArgumentException">Si no hay suficiente stock disponible</exception>
        public async Task<CartDTO> AddItemAsync(string buyerId, int productId, int quantity, int? userId = null)
        {
            Cart? cart = await _cartRepository.FindAsync(buyerId, userId);
            Product? product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                Log.Information("El producto con ID {ProductId} no existe.", productId);
                throw new KeyNotFoundException("El producto no existe.");
            }
            // Verificar stock real en la base de datos
            int realStock = await _productRepository.GetRealStockAsync(productId);
            Log.Information("Verificando stock real del producto {ProductId}. Stock disponible: {RealStock}, Cantidad solicitada: {Quantity}",
                productId, realStock, quantity);
            if (realStock < quantity)
            {
                Log.Information("El producto con ID {ProductId} no tiene suficiente stock. Stock disponible: {RealStock}, Solicitado: {Quantity}",
                    productId, realStock, quantity);
                throw new ArgumentException($"No hay suficiente stock del producto. Stock disponible: {realStock}");
            }
            if (cart == null)
            {
                Log.Information("Creando nuevo carrito para buyerId: {BuyerId}", buyerId);
                cart = await _cartRepository.CreateAsync(buyerId, userId);
            }
            var existingProduct = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (existingProduct != null)
            {
                // Si el producto ya existe en el carrito, verificar que la cantidad total no exceda el stock
                int newTotalQuantity = existingProduct.Quantity + quantity;
                if (realStock < newTotalQuantity)
                {
                    Log.Information("La cantidad total del producto {ProductId} excedería el stock. Stock: {RealStock}, Cantidad actual en carrito: {CurrentQuantity}, Cantidad a agregar: {AddQuantity}",
                        productId, realStock, existingProduct.Quantity, quantity);
                    throw new ArgumentException($"La cantidad total ({newTotalQuantity}) excedería el stock disponible ({realStock}).");
                }
                existingProduct.Quantity = newTotalQuantity;
                Log.Information("Actualizando cantidad del producto {ProductId} en el carrito. Nueva cantidad: {NewQuantity}",
                    productId, newTotalQuantity);
            }
            else
            {
                var newCartItem = new CartItem
                {
                    ProductId = product.Id,
                    Product = product,
                    CartId = cart.Id,
                    Quantity = quantity,
                };
                await _cartRepository.AddItemAsync(cart, newCartItem);
                Log.Information("Nuevo producto agregado al carrito. ProductId: {ProductId}, Cantidad: {Quantity}", productId, quantity);
            }
            RecalculateCartTotals(cart);
            await _cartRepository.UpdateAsync(cart);
            Log.Information("Carrito guardado exitosamente. CartId: {CartId}", cart.Id);
            return cart.Adapt<CartDTO>();
        }

        /// <summary>
        /// Asocia un carrito anónimo con un usuario que acaba de autenticarse.
        /// Si el usuario ya tiene un carrito:
        /// - Fusiona ambos carritos sumando cantidades de productos duplicados
        /// - Elimina el carrito anónimo después de la fusión
        /// Si el usuario no tiene carrito:
        /// - Simplemente asocia el carrito anónimo con el userId
        /// </summary>
        /// <param name="buyerId">Identificador del carrito anónimo</param>
        /// <param name="userId">ID del usuario autenticado</param>
        public async Task AssociateWithUserAsync(string buyerId, int userId)
        {
            Cart? cart = await _cartRepository.GetAnonymousAsync(buyerId);
            if (cart == null)
            {
                Log.Information("No hay carrito para asociar con buyerId: {BuyerId}", buyerId);
                return;
            }
            var existingUserCart = await _cartRepository.GetByUserIdAsync(userId);
            if (existingUserCart != null)
            {
                foreach (var anonymousItem in cart.CartItems)
                {
                    var existingItem = existingUserCart.CartItems.FirstOrDefault(i => i.ProductId == anonymousItem.ProductId);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += anonymousItem.Quantity;
                    }
                    else
                    {
                        anonymousItem.CartId = existingUserCart.Id;
                        existingUserCart.CartItems.Add(anonymousItem);
                    }
                }
                RecalculateCartTotals(existingUserCart);
                await _cartRepository.UpdateAsync(existingUserCart);
                await _cartRepository.DeleteAsync(cart);
                Log.Information("Carritos fusionados. Carrito anónimo {AnonymousCartId} → Carrito usuario {UserCartId}", cart.Id, existingUserCart.Id);
            }
            else
            {
                cart.UserId = userId;
                await _cartRepository.UpdateAsync(cart);
                Log.Information("Carrito anónimo asociado con usuario. BuyerId: {BuyerId} → UserId: {UserId}", buyerId, userId);
            }
        }

        /// <summary>
        /// Valida y ajusta el carrito antes del checkout.
        /// Realiza las siguientes acciones:
        /// - Verifica el stock real de cada producto
        /// - Elimina productos que estén agotados (stock = 0)
        /// - Ajusta cantidades que excedan el stock disponible
        /// - Recalcula los totales del carrito
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="userId">ID del usuario autenticado (opcional)</param>
        /// <returns>El carrito validado y ajustado</returns>
        /// <exception cref="KeyNotFoundException">Si el carrito no existe</exception>
        /// <exception cref="InvalidOperationException">Si el carrito está vacío</exception>
        public async Task<CartDTO> CheckoutAsync(string buyerId, int? userId)
        {
            Cart? cart = await _cartRepository.FindAsync(buyerId, userId);
            if (cart == null)
            {
                Log.Information("El carrito no existe para el comprador especificado {BuyerId}.", buyerId);
                throw new KeyNotFoundException("El carrito no existe para el comprador especificado.");
            }
            if (cart.CartItems.Count == 0)
            {
                Log.Information("El carrito está vacío para el comprador especificado {BuyerId}.", buyerId);
                throw new InvalidOperationException("El carrito está vacío.");
            }
            var itemsToRemove = new List<CartItem>();
            var itemsToUpdate = new List<(CartItem item, int newQuantity)>();
            bool hasChanges = false;
            foreach (var item in cart.CartItems.ToList())
            {
                int productStock = await _productRepository.GetRealStockAsync(item.ProductId);
                if (productStock == 0)
                {
                    Log.Information("El producto con ID {ProductId} está agotado.", item.ProductId);
                    itemsToRemove.Add(item);
                    hasChanges = true;
                }
                else if (item.Quantity > productStock)
                {
                    Log.Information("Ajustando cantidad del producto {ProductId} con cantidad actualizada {NewQuantity}", item.ProductId, productStock);
                    itemsToUpdate.Add((item, productStock));
                    hasChanges = true;
                }
            }
            if (hasChanges)
            {
                foreach (var itemToRemove in itemsToRemove)
                {
                    cart.CartItems.Remove(itemToRemove);
                    await _cartRepository.RemoveItemAsync(itemToRemove);
                }
                foreach (var (item, newQuantity) in itemsToUpdate)
                {
                    item.Quantity = newQuantity;
                }
                RecalculateCartTotals(cart);
                await _cartRepository.UpdateAsync(cart);
                Log.Information("Carrito actualizado tras checkout. ItemsEliminados: {RemovedCount}, ItemsActualizados: {UpdatedCount}", itemsToRemove.Count, itemsToUpdate.Count);
            }
            else
            {
                Log.Information("No se requirieron ajustes en el carrito durante checkout. CartId: {CartId}", cart.Id);
            }
            return cart.Adapt<CartDTO>();
        }

        /// <summary>
        /// Vacía completamente el carrito eliminando todos sus items.
        /// Establece todos los totales en cero.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="userId">ID del usuario autenticado (opcional)</param>
        /// <returns>El carrito vacío</returns>
        /// <exception cref="KeyNotFoundException">Si el carrito no existe</exception>
        public async Task<CartDTO> ClearAsync(string buyerId, int? userId = null)
        {
            Cart? cart = await _cartRepository.FindAsync(buyerId, userId);
            if (cart == null)
            {
                Log.Information("El carrito no existe para el comprador especificado {BuyerId}.", buyerId);
                throw new KeyNotFoundException("El carrito no existe para el comprador especificado.");
            }

            cart.CartItems.Clear();
            RecalculateCartTotals(cart);
            await _cartRepository.UpdateAsync(cart);
            Log.Information("Carrito limpiado exitosamente. CartId: {CartId}", cart.Id);

            return cart.Adapt<CartDTO>();
        }

        /// <summary>
        /// Obtiene un carrito existente o crea uno nuevo si no existe.
        /// Para usuarios autenticados, busca primero por buyerId y userId,
        /// luego solo por userId para recuperar carritos existentes del usuario.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="userId">ID del usuario autenticado (opcional)</param>
        /// <returns>El carrito existente o uno nuevo recién creado</returns>
        public async Task<CartDTO> CreateOrGetCartAsync(string buyerId, int? userId)
        {
            Cart? cart = await _cartRepository.FindAsync(buyerId, userId);
            if (cart == null)
            {
                if (userId.HasValue)
                {
                    var existingUserCart = await _cartRepository.GetByUserIdAsync(userId.Value);
                    if (existingUserCart != null)
                    {
                        Log.Information("Se encontró un carrito existente para el usuario autenticado. UserId: {UserId}", userId.Value);
                        return existingUserCart.Adapt<CartDTO>();
                    }
                    cart = await _cartRepository.CreateAsync(buyerId, userId);
                    Log.Information("Se creó un nuevo carrito para el usuario autenticado. UserId: {UserId}, BuyerId: {BuyerId}", userId.Value, buyerId);
                }
            }
            return cart.Adapt<CartDTO>();
        }

        /// <summary>
        /// Elimina completamente un producto del carrito.
        /// Recalcula los totales después de eliminar el item.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="productId">ID del producto a eliminar</param>
        /// <param name="userId">ID del usuario autenticado (opcional)</param>
        /// <returns>El carrito actualizado sin el producto</returns>
        /// <exception cref="KeyNotFoundException">Si el carrito o el producto no existen</exception>
        public async Task<CartDTO> RemoveItemAsync(string buyerId, int productId, int? userId = null)
        {
            Cart? cart = await _cartRepository.FindAsync(buyerId, userId);
            if (cart == null)
            {
                Log.Information("El carrito no existe para el comprador especificado {BuyerId}.", buyerId);
                throw new KeyNotFoundException("El carrito no existe para el comprador especificado.");
            }

            CartItem? itemToRemove = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (itemToRemove == null)
            {
                Log.Information("El artículo no existe en el carrito para el comprador especificado {BuyerId}.", buyerId);
                throw new KeyNotFoundException("El artículo no existe en el carrito.");
            }

            cart.CartItems.Remove(itemToRemove);
            await _cartRepository.RemoveItemAsync(itemToRemove);
            RecalculateCartTotals(cart);
            await _cartRepository.UpdateAsync(cart);

            return cart.Adapt<CartDTO>();
        }

        /// <summary>
        /// Actualiza la cantidad de un producto específico en el carrito.
        /// Si la cantidad es 0, elimina el producto del carrito.
        /// Valida que haya suficiente stock disponible para la nueva cantidad.
        /// </summary>
        /// <param name="buyerId">Identificador único del comprador</param>
        /// <param name="productId">ID del producto a actualizar</param>
        /// <param name="quantity">Nueva cantidad (0 para eliminar)</param>
        /// <param name="userId">ID del usuario autenticado (opcional)</param>
        /// <returns>El carrito actualizado</returns>
        /// <exception cref="KeyNotFoundException">Si el carrito, producto o item no existen</exception>
        /// <exception cref="ArgumentException">Si no hay suficiente stock</exception>
        public async Task<CartDTO> UpdateItemQuantityAsync(string buyerId, int productId, int quantity, int? userId = null)
        {
            Cart? cart = await _cartRepository.FindAsync(buyerId, userId);
            if (cart == null)
            {
                Log.Information("El carrito no existe para el comprador especificado {BuyerId}.", buyerId);
                throw new KeyNotFoundException("El carrito no existe para el comprador especificado.");
            }
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                Log.Information("El producto no existe para el ID especificado {ProductId}.", productId);
                throw new KeyNotFoundException("El producto no existe para el ID especificado.");
            }
            var itemToUpdate = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (itemToUpdate == null)
            {
                throw new KeyNotFoundException("Producto del carrito no encontrado");
            }
            if (quantity == 0)
            {
                cart.CartItems.Remove(itemToUpdate);
                await _cartRepository.RemoveItemAsync(itemToUpdate);
                RecalculateCartTotals(cart);
                await _cartRepository.UpdateAsync(cart);
                Log.Information("Producto con ID {ProductId} eliminado del carrito debido a cantidad 0", productId);
                return cart.Adapt<CartDTO>();
            }
            // Verificar stock real en la base de datos
            int realStock = await _productRepository.GetRealStockAsync(productId);
            Log.Information("Verificando stock real del producto {ProductId} para actualización. Stock disponible: {RealStock}, Cantidad solicitada: {Quantity}",
                productId, realStock, quantity);
            if (realStock < quantity)
            {
                Log.Information("El producto con ID {ProductId} no tiene suficiente stock para la cantidad solicitada. Stock disponible: {RealStock}, Solicitado: {Quantity}",
                    productId, realStock, quantity);
                throw new ArgumentException($"Stock insuficiente. Stock disponible: {realStock}");
            }
            itemToUpdate.Quantity = quantity;
            RecalculateCartTotals(cart);
            await _cartRepository.UpdateAsync(cart);
            Log.Information("Cantidad del producto {ProductId} actualizada exitosamente a {Quantity}", productId, quantity);
            return cart.Adapt<CartDTO>();
        }
        /// <summary>
        /// Recalcula los totales del carrito (SubTotal, Total, TotalUniqueItemsCount y TotalSavedAmount) basándose en los artículos actuales y sus descuentos.
        /// Valida la consistencia de los datos para prevenir errores de cálculo.
        /// </summary>
        /// <param name="cart"></param>
        /// <exception cref="InvalidOperationException">Si los cálculos resultan en valores inconsistentes</exception>
        private static void RecalculateCartTotals(Cart cart)
        {
            // Carrito vacio
            if (!cart.CartItems.Any())
            {
                cart.SubTotal = 0;
                cart.Total = 0;
                cart.TotalUniqueItemsCount = 0;
                cart.TotalSavedAmount = 0;
                return;
            }
            // Validar que todos los items tengan valores válidos
            foreach (var item in cart.CartItems)
            {
                if (item.Product.Price < 0)
                {
                    Log.Error("Precio negativo detectado en el producto {ProductId}: {Price}", item.ProductId, item.Product.Price);
                    throw new InvalidOperationException($"El producto {item.ProductId} tiene un precio inválido.");
                }
                if (item.Quantity <= 0)
                {
                    Log.Error("Cantidad inválida detectada en el producto {ProductId}: {Quantity}", item.ProductId, item.Quantity);
                    throw new InvalidOperationException($"El producto {item.ProductId} tiene una cantidad inválida.");
                }
                if (item.Product.Discount < 0 || item.Product.Discount > 100)
                {
                    Log.Warning("Descuento fuera de rango en el producto {ProductId}: {Discount}%. Se ajustará a 0.", item.ProductId, item.Product.Discount);
                    item.Product.Discount = Math.Clamp(item.Product.Discount, 0, 100);
                }
            }
            // Calcular cantidad de artículos únicos
            cart.TotalUniqueItemsCount = cart.CartItems.Count;
            // Calcular subtotal
            cart.SubTotal = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity);
            // Calcular total con descuentos usando redondeo apropiado
            var totalWithDiscounts = cart.CartItems.Sum(ci =>
            {
                var itemTotal = ci.Product.Price * ci.Quantity;
                var discount = ci.Product.Discount;
                var discountedAmount = itemTotal * (1 - (decimal)discount / 100);
                return (int)Math.Round(discountedAmount, MidpointRounding.AwayFromZero);
            });
            cart.Total = totalWithDiscounts;
            // Calcular el total ahorrado (diferencia entre subtotal y total)
            cart.TotalSavedAmount = cart.SubTotal - cart.Total;
            // Validaciones
            if (cart.SubTotal < 0)
            {
                Log.Error("SubTotal negativo calculado: {SubTotal}", cart.SubTotal);
                throw new InvalidOperationException("El subtotal calculado es negativo.");
            }
            if (cart.Total < 0)
            {
                Log.Error("Total negativo calculado: {Total}", cart.Total);
                throw new InvalidOperationException("El total calculado es negativo.");
            }
            if (cart.Total > cart.SubTotal)
            {
                Log.Error("Total ({Total}) es mayor que SubTotal ({SubTotal}). Esto indica un error de cálculo.", cart.Total, cart.SubTotal);
                throw new InvalidOperationException("El total no puede ser mayor que el subtotal.");
            }
            if (cart.TotalSavedAmount < 0)
            {
                Log.Error("TotalSavedAmount negativo calculado: {TotalSavedAmount}", cart.TotalSavedAmount);
                throw new InvalidOperationException("El total ahorrado no puede ser negativo.");
            }
            Log.Information("Totales recalculados. SubTotal: {SubTotal}, Total: {Total}, Descuento aplicado: {TotalSaved}, Items únicos: {UniqueItems}",
                cart.SubTotal, cart.Total, cart.TotalSavedAmount, cart.TotalUniqueItemsCount);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.CartDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión del carrito de compras.
    /// Permite operaciones CRUD sobre el carrito tanto para usuarios autenticados como anónimos.
    /// </summary>
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;

        /// <summary>
        /// Constructor del controlador de carrito.
        /// </summary>
        /// <param name="cartService">Servicio de lógica de negocio para el carrito</param>
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Obtiene el carrito actual del comprador (usuario autenticado o anónimo).
        /// Si no existe un carrito, se crea uno nuevo.
        /// </summary>
        /// <returns>El carrito con todos sus items y totales.</returns>
        /// <response code="200">Carrito obtenido exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCart()
        {
            var buyerId = GetBuyerId();
            var userId = User.Identity?.IsAuthenticated == true ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value : null;
            int? parsedUserId = userId != null && int.TryParse(userId, out int id) ? id : null;
            var cart = await _cartService.CreateOrGetCartAsync(buyerId, parsedUserId);
            return Ok(new GenericResponse<CartDTO>("Carrito obtenido exitosamente", cart));
        }

        /// <summary>
        /// Agrega un producto al carrito o incrementa su cantidad si ya existe.
        /// </summary>
        /// <param name="addCartItemDTO">DTO con el ID del producto y la cantidad a agregar</param>
        /// <returns>El carrito actualizado con el nuevo item.</returns>
        /// <response code="200">Item agregado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">Producto no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("items")]
        [AllowAnonymous]
        public async Task<IActionResult> AddItem([FromForm] AddCartItemDTO addCartItemDTO)
        {
            var buyerId = GetBuyerId();
            var userId = User.Identity?.IsAuthenticated == true ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value : null;
            var parsedUserId = userId != null && int.TryParse(userId, out int id) ? id : (int?)null;
            var result = await _cartService.AddItemAsync(buyerId, addCartItemDTO.ProductId, addCartItemDTO.Quantity, parsedUserId);
            return Ok(new GenericResponse<CartDTO>("Item agregado exitosamente", result));
        }

        /// <summary>
        /// Elimina completamente un producto del carrito.
        /// </summary>
        /// <param name="productId">ID del producto a eliminar</param>
        /// <returns>El carrito actualizado sin el producto eliminado.</returns>
        /// <response code="200">Item eliminado exitosamente</response>
        /// <response code="404">Producto no encontrado en el carrito</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpDelete("items/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var buyerId = GetBuyerId();
            var userId = User.Identity?.IsAuthenticated == true ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value : null;
            var parsedUserId = userId != null && int.TryParse(userId, out int id) ? id : (int?)null;
            var result = await _cartService.RemoveItemAsync(buyerId, productId, parsedUserId);
            return Ok(new GenericResponse<CartDTO>("Item eliminado exitosamente", result));
        }

        /// <summary>
        /// Vacía completamente el carrito, eliminando todos los items.
        /// </summary>
        /// <returns>El carrito vacío con totales en cero.</returns>
        /// <response code="200">Carrito limpiado exitosamente</response>
        /// <response code="404">Carrito no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("clear")]
        [AllowAnonymous]
        public async Task<IActionResult> ClearCart()
        {
            var buyerId = GetBuyerId();
            var userId = User.Identity?.IsAuthenticated == true ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value : null;
            var parsedUserId = userId != null && int.TryParse(userId, out int id) ? id : (int?)null;
            var result = await _cartService.ClearAsync(buyerId, parsedUserId);
            return Ok(new GenericResponse<CartDTO>("Carrito limpiado exitosamente", result));
        }

        /// <summary>
        /// Actualiza la cantidad de un producto específico en el carrito.
        /// Si la cantidad es 0 o negativa, el producto se elimina del carrito.
        /// </summary>
        /// <param name="changeItemQuantityDTO">DTO con el ID del producto y la nueva cantidad</param>
        /// <returns>El carrito actualizado con la nueva cantidad.</returns>
        /// <response code="200">Cantidad actualizada exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">Producto no encontrado en el carrito</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPatch("items")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateItemQuantity([FromForm] ChangeItemQuantityDTO changeItemQuantityDTO)
        {
            var buyerId = GetBuyerId();
            var userId = User.Identity?.IsAuthenticated == true ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value : null;
            var parsedUserId = userId != null && int.TryParse(userId, out int id) ? id : (int?)null;
            var result = await _cartService.UpdateItemQuantityAsync(buyerId, changeItemQuantityDTO.ProductId, changeItemQuantityDTO.Quantity, parsedUserId);
            return Ok(new GenericResponse<CartDTO>("Cantidad de item actualizada exitosamente", result));
        }

        /// <summary>
        /// Realiza el checkout del carrito. Solo disponible para usuarios autenticados con rol "Customer".
        /// Transfiere los items del carrito anónimo al carrito del usuario si corresponde.
        /// </summary>
        /// <returns>El carrito después del checkout.</returns>
        /// <response code="200">Checkout realizado exitosamente</response>
        /// <response code="401">Usuario no autenticado</response>
        /// <response code="403">Usuario no tiene el rol necesario</response>
        /// <response code="404">Carrito no encontrado o vacío</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("checkout")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CheckoutAsync()
        {
            var buyerId = GetBuyerId();
            var userId = User.Identity?.IsAuthenticated == true ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value : null;
            var parsedUserId = userId != null && int.TryParse(userId, out int id) ? id : (int?)null;
            var result = await _cartService.CheckoutAsync(buyerId, parsedUserId);
            return Ok(new GenericResponse<CartDTO>("Checkout realizado exitosamente", result));
        }

        /// <summary>
        /// Obtiene el identificador del comprador desde el contexto HTTP.
        /// Este ID puede ser una cookie de sesión para usuarios anónimos o el ID del usuario autenticado.
        /// </summary>
        /// <returns>El identificador único del comprador.</returns>
        /// <exception cref="Exception">Se lanza si no se encuentra el ID del comprador en el contexto.</exception>
        private string GetBuyerId()
        {
            var buyerId = HttpContext.Items["BuyerId"]?.ToString();
            Log.Information("buyerid es: {buyerId}", buyerId);

            if (string.IsNullOrEmpty(buyerId))
            {
                throw new Exception("No se encontró el id del comprador.");
            }
            return buyerId;
        }
    }
}
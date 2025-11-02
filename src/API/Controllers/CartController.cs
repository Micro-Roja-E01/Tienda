using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.CartDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
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
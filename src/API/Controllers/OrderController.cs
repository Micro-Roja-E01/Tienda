using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.OrderDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Crea una nueva orden.
        /// </summary>
        /// <returns>Detalles de la orden creada.</returns>
        [HttpPost("create")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CreateOrder()
        {
            var userId = (User.Identity?.IsAuthenticated == true ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value : null) ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
            int.TryParse(userId, out int parsedUserId);
            var result = await _orderService.CreateAsync(parsedUserId);
            return Created($"api/order/detail/{result}", new GenericResponse<string>("Orden creada exitosamente", result));
        }
        /// <summary>
        /// Obtiene los detalles de una orden.
        /// </summary>
        /// <param name="orderCode">Código de la orden</param>
        /// <returns>Detalles de la orden encontrada.</returns>
        [HttpGet("detail/{orderCode}")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> GetOrderDetail(string orderCode)
        {
            var result = await _orderService.GetDetailAsync(orderCode);
            return Ok(new GenericResponse<OrderDetailDTO>("Detalle de orden obtenido exitosamente", result));
        }

        /// <summary>
        /// Obtiene las órdenes de un usuario.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda</param>
        /// <returns>Órdenes del usuario.</returns>
        [HttpGet("user-orders")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> GetUserOrders([FromQuery] SearchParamsDTO searchParams)
        {
            var userId = (User.Identity?.IsAuthenticated == true ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value : null) ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
            int.TryParse(userId, out int parsedUserId);
            var result = _orderService.GetByUserIdAsync(searchParams, parsedUserId);
            return Ok(new GenericResponse<ListedOrderDetailDTO>("Órdenes del usuario obtenidas exitosamente", await result));
        }
    }
}
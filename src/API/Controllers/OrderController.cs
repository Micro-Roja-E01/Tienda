using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.OrderDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de órdenes de compra.
    /// Permite crear órdenes desde el carrito y consultar el historial de órdenes del usuario.
    /// Todos los endpoints requieren autenticación con rol "Cliente".
    /// </summary>
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;

        /// <summary>
        /// Constructor del controlador de órdenes.
        /// </summary>
        /// <param name="orderService">Servicio de lógica de negocio para órdenes</param>
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Crea una nueva orden a partir del carrito de compras del usuario autenticado.
        /// Esta operación:
        /// - Convierte todos los items del carrito en items de la orden
        /// - Actualiza el stock de los productos
        /// - Vacía el carrito del usuario
        /// - Usa transacciones para garantizar consistencia (rollback automático en caso de error)
        /// </summary>
        /// <returns>Código único de la orden creada y URL para consultar sus detalles.</returns>
        /// <response code="201">Orden creada exitosamente. Retorna el código de la orden.</response>
        /// <response code="400">Carrito vacío o datos inválidos</response>
        /// <response code="401">Usuario no autenticado</response>
        /// <response code="403">Usuario no tiene el rol "Cliente"</response>
        /// <response code="404">Carrito no encontrado</response>
        /// <response code="500">Error interno del servidor (la transacción se revierte automáticamente)</response>
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
        /// Obtiene los detalles completos de una orden específica por su código único.
        /// Incluye información de todos los items, precios, cantidades y datos guardados al momento de la compra.
        /// </summary>
        /// <param name="orderCode">Código único de la orden (formato: ORD-YYMMDDHHMMSS-XXX)</param>
        /// <returns>Detalles completos de la orden incluyendo items, totales y metadata.</returns>
        /// <response code="200">Orden encontrada y detalles retornados exitosamente</response>
        /// <response code="401">Usuario no autenticado</response>
        /// <response code="403">Usuario no tiene el rol "Cliente"</response>
        /// <response code="404">Orden no encontrada o no pertenece al usuario</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("detail/{orderCode}")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> GetOrderDetail(string orderCode)
        {
            var result = await _orderService.GetDetailAsync(orderCode);
            return Ok(new GenericResponse<OrderDetailDTO>("Detalle de orden obtenido exitosamente", result));
        }

        /// <summary>
        /// Obtiene el historial de órdenes del usuario autenticado con paginación y búsqueda.
        /// Permite filtrar órdenes por código o por items comprados.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y paginación:
        /// - SearchTerm: Busca por código de orden, título o descripción de productos
        /// - PageNumber: Número de página (por defecto: 1)
        /// - PageSize: Cantidad de órdenes por página (por defecto: valor configurado)
        /// </param>
        /// <returns>Lista paginada de órdenes del usuario con información de paginación.</returns>
        /// <response code="200">Lista de órdenes obtenida exitosamente</response>
        /// <response code="400">Parámetros de búsqueda inválidos o página fuera de rango</response>
        /// <response code="401">Usuario no autenticado</response>
        /// <response code="403">Usuario no tiene el rol "Cliente"</response>
        /// <response code="500">Error interno del servidor</response>
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
using Mapster;
using Serilog;
using tienda.src.Application.DTO.ProductDTO;
using tienda.src.Infrastructure.Repositories.Interfaces;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.OrderDTO;
using Tienda.src.Application.Services.Interfaces;
using Tienda.src.Infrastructure.Repositories.Interfaces;

namespace Tienda.src.Application.Services.Implements
{
    /// <summary>
    /// Implementación del servicio de órdenes de compra.
    /// Gestiona la lógica de negocio para crear órdenes a partir de carritos,
    /// consultar historial de órdenes y generar códigos únicos.
    /// Utiliza el patrón Unit of Work para garantizar transacciones ACID.
    /// </summary>
    public class OrderService : IOrderService
    {

        private readonly IConfiguration _configuration;
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly int _defaultPageSize;

        /// <summary>
        /// Constructor que inyecta todas las dependencias necesarias.
        /// </summary>
        /// <param name="orderRepository">Repositorio para acceso a datos de órdenes</param>
        /// <param name="cartRepository">Repositorio para acceso a datos de carritos</param>
        /// <param name="productRepository">Repositorio para actualización de stock de productos</param>
        /// <param name="unitOfWork">Unidad de trabajo para manejo de transacciones</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <exception cref="InvalidOperationException">Si DefaultPageSize no está configurado</exception>
        public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository, IProductRepository productRepository, IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _defaultPageSize = int.Parse(_configuration["Products:DefaultPageSize"] ?? throw new InvalidOperationException("La configuración 'DefaultPageSize' no está definida."));
        }

        /// <summary>
        /// Crea una nueva orden de compra a partir del carrito del usuario.
        /// Proceso transaccional completo:
        /// 1. Valida que el carrito exista y contenga items
        /// 2. Genera un código único para la orden
        /// 3. Convierte el carrito en orden (snapshot histórico de productos)
        /// 4. Actualiza el stock de cada producto
        /// 5. Vacía el carrito del usuario
        /// 
        /// Utiliza transacciones de base de datos para garantizar atomicidad:
        /// - Si cualquier paso falla, todos los cambios se revierten automáticamente (rollback)
        /// - Solo confirma cambios si todo el proceso es exitoso (commit)
        /// </summary>
        /// <param name="userId">ID del usuario autenticado que realiza la compra</param>
        /// <returns>Código único de la orden creada (formato: ORD-YYMMDDHHMMSS-XXX)</returns>
        /// <exception cref="KeyNotFoundException">Si el carrito del usuario no existe</exception>
        /// <exception cref="InvalidOperationException">Si el carrito está vacío</exception>
        /// <exception cref="Exception">Cualquier error durante el proceso causa un rollback automático</exception>
        public async Task<string> CreateAsync(int userId)
        {
            // Iniciar una transacción para garantizar consistencia de datos
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                Cart? cart = await _cartRepository.GetByUserIdAsync(userId) ?? throw new KeyNotFoundException("Carrito no encontrado");
                if (cart.CartItems.Count == 0)
                {
                    Log.Information("El carrito del usuario con id {userId} está vacío.", userId);
                    throw new InvalidOperationException("El carrito del usuario está vacío.");
                }
                string code = await GenerateOrderCodeAsync();
                Order order = cart.Adapt<Order>();
                order.Code = code;
                order.UserId = userId;
                await _orderRepository.CreateAsync(order);
                foreach (var item in cart.CartItems)
                {
                    item.Product.Stock -= item.Quantity;
                    await _productRepository.UpdateStockAsync(item.ProductId, item.Product.Stock);
                }
                cart.CartItems.Clear();
                cart.Total = 0;
                cart.SubTotal = 0;
                await _cartRepository.UpdateAsync(cart);

                // Confirmar la transacción si todo fue exitoso
                await _unitOfWork.CommitAsync();
                Log.Information("Orden {OrderCode} creada exitosamente para el usuario {UserId}.", code, userId);
                return code;
            }
            catch (Exception ex)
            {
                // Revertir todos los cambios en caso de error
                await _unitOfWork.RollbackAsync();
                Log.Error(ex, "Error al crear la orden para el usuario {UserId}. La transacción fue revertida.", userId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene el historial de órdenes de un usuario con paginación y búsqueda.
        /// Permite filtrar órdenes por:
        /// - Código de orden
        /// - Título de productos comprados
        /// - Descripción de productos comprados
        /// 
        /// Valida que el número de página esté dentro del rango válido.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y paginación:
        /// - SearchTerm: Texto a buscar (opcional)
        /// - PageNumber: Número de página (requerido, basado en 1)
        /// - PageSize: Cantidad de items por página (opcional, usa default si no se especifica)
        /// </param>
        /// <param name="userId">ID del usuario propietario de las órdenes</param>
        /// <returns>Lista paginada de órdenes con metadata de paginación (total de páginas, página actual, etc.)</returns>
        /// <exception cref="ArgumentOutOfRangeException">Si el número de página está fuera del rango válido (menor a 1 o mayor al total de páginas)</exception>
        public async Task<ListedOrderDetailDTO> GetByUserIdAsync(SearchParamsDTO searchParams, int userId)
        {
            var (orders, totalCount) = await _orderRepository.GetByUserIdAsync(searchParams, userId);
            var totalPages = (int)Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
            int currentPage = (int)searchParams.PageNumber!; //TODO: pequeño fix, validar después
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            if (currentPage < 1 || currentPage > totalPages)
            {
                throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");
            }
            var listedOrders = new ListedOrderDetailDTO
            {
                Orders = orders.Adapt<List<OrderDetailDTO>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize
            };
            return listedOrders;
        }

        /// <summary>
        /// Obtiene los detalles completos de una orden específica por su código único.
        /// Incluye toda la información histórica de la orden:
        /// - Código de orden
        /// - Totales (con y sin descuentos)
        /// - Fecha de compra
        /// - Lista de items con información del producto al momento de la compra (snapshot histórico)
        /// </summary>
        /// <param name="orderCode">Código único de la orden (formato: ORD-YYMMDDHHMMSS-XXX)</param>
        /// <returns>Detalles completos de la orden con todos sus items</returns>
        /// <exception cref="KeyNotFoundException">Si no existe una orden con el código especificado</exception>
        public async Task<OrderDetailDTO> GetDetailAsync(string orderCode)
        {
            Order? order = await _orderRepository.GetByCodeAsync(orderCode) ?? throw new KeyNotFoundException("Orden no encontrada");
            return order.Adapt<OrderDetailDTO>();
        }

        /// <summary>
        /// Genera un código único para identificar una orden.
        /// Formato del código: ORD-YYMMDDHHMMSS-XXX donde:
        /// - ORD: Prefijo fijo
        /// - YYMMDDHHMMSS: Timestamp con año (2 dígitos), mes, día, hora, minuto y segundo
        /// - XXX: Número aleatorio entre 100 y 999
        /// 
        /// Verifica en la base de datos que el código generado no exista previamente.
        /// Si existe, genera un nuevo código hasta encontrar uno único.
        /// </summary>
        /// <returns>Código único de orden que no existe en la base de datos</returns>
        private async Task<string> GenerateOrderCodeAsync()
        {
            string code;
            do
            {
                var timestamp = DateTime.UtcNow.ToString("yyMMddHHmmss");
                var random = Random.Shared.Next(100, 999);
                code = $"ORD-{timestamp}-{random}";
            }
            while (await _orderRepository.CodeExistsAsync(code));
            return code;
        }
    }
}
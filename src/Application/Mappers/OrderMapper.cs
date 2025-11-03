using Mapster;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.OrderDTO;

namespace Tienda.src.Application.Mappers
{
    /// <summary>
    /// Clase responsable de configurar los mapeos entre entidades del dominio de órdenes y sus DTOs.
    /// Utiliza Mapster para definir transformaciones entre Order/OrderItem, Cart/CartItem y sus respectivos DTOs.
    /// Incluye lógica para:
    /// - Conversión de carrito a orden (snapshot histórico)
    /// - Formateo de precios y fechas
    /// - Manejo de zona horaria para fechas de compra
    /// </summary>
    public class OrderMapper
    {
        private readonly IConfiguration _configuration;
        private readonly string _defaultImageURL;
        private readonly TimeZoneInfo _timeZone;

        /// <summary>
        /// Constructor que inicializa el mapper con configuración y zona horaria.
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación para obtener valores como la URL de imagen por defecto</param>
        /// <exception cref="InvalidOperationException">Se lanza si la URL de imagen por defecto no está configurada</exception>
        public OrderMapper(IConfiguration configuration)
        {
            _configuration = configuration;
            _defaultImageURL = _configuration["Products:DefaultImageUrl"] ?? throw new InvalidOperationException("La configuración de DefaultImageUrl es necesaria.");
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id);
        }

        /// <summary>
        /// Configura todos los mapeos relacionados con órdenes.
        /// Debe llamarse al inicio de la aplicación para registrar las configuraciones en Mapster.
        /// </summary>
        public void ConfigureAllMappings()
        {
            ConfigureOrderItemsMappings();
            ConfigureOrderMappings();
        }

        /// <summary>
        /// Configura los mapeos de órdenes:
        /// 1. Order → OrderDetailDTO: Convierte la orden a DTO con formato de moneda y conversión de zona horaria
        /// 2. Cart → Order: Transforma el carrito en una orden, copiando totales y convirtiendo CartItems a OrderItems
        /// </summary>
        public void ConfigureOrderMappings()
        {
            TypeAdapterConfig<Order, OrderDetailDTO>.NewConfig()
                .Map(dest => dest.Items, src => src.OrderItems)
                .Map(dest => dest.PurchasedAt, src => TimeZoneInfo.ConvertTimeFromUtc(src.CreatedAt, _timeZone))
                .Map(dest => dest.Code, src => src.Code)
                .Map(dest => dest.Total, src => src.Total.ToString("C"))
                .Map(dest => dest.SubTotal, src => src.SubTotal.ToString("C"));

            TypeAdapterConfig<Cart, Order>.NewConfig()
                .Map(dest => dest.Total, src => src.Total)
                .Map(dest => dest.SubTotal, src => src.SubTotal)
                .Map(dest => dest.OrderItems, src => src.CartItems)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.Code)
                .Ignore(dest => dest.CreatedAt);
        }

        /// <summary>
        /// Configura los mapeos de items de órdenes:
        /// 1. OrderItem → OrderItemDTO: Convierte el item de orden a DTO formateando el precio como moneda
        /// 2. CartItem → OrderItem: Transforma items del carrito en items de orden, creando un snapshot histórico
        ///    - Captura título, descripción, imagen y precio del producto en el momento de la compra
        ///    - Usa imagen por defecto si el producto no tiene imágenes
        ///    - Ignora propiedades autogeneradas (Id, OrderId, relaciones)
        /// </summary>
        public void ConfigureOrderItemsMappings()
        {
            TypeAdapterConfig<OrderItem, OrderItemDTO>.NewConfig()
                .Map(dest => dest.ProductTitle, src => src.TitleAtMoment)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.ProductDescription, src => src.DescriptionAtMoment)
                .Map(dest => dest.MainImageURL, src => src.ImageUrlAtMoment)
                .Map(dest => dest.PriceAtMoment, src => src.PriceAtMoment.ToString("C"));

            TypeAdapterConfig<CartItem, OrderItem>.NewConfig()
                .Map(dest => dest.TitleAtMoment, src => src.Product.Title)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.DescriptionAtMoment, src => src.Product.Description)
                .Map(dest => dest.ImageUrlAtMoment, src => src.Product.Images != null && src.Product.Images.Any() ? src.Product.Images.First().ImageUrl : _defaultImageURL)
                .Map(dest => dest.PriceAtMoment, src => src.Product.Price)
                .Map(dest => dest.DiscountAtMoment, src => src.Product.Discount)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.OrderId)
                .Ignore(dest => dest.Order);
        }

    }
}
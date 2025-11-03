using Mapster;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.CartDTO;

namespace Tienda.src.Application.Mappers
{
    /// <summary>
    /// Clase responsable de configurar los mapeos entre entidades del dominio del carrito y sus DTOs.
    /// Utiliza Mapster para definir las transformaciones entre Cart/CartItem y CartDTO/CartItemDTO.
    /// </summary>
    public class CartMapper
    {
        private readonly IConfiguration _configuration;
        private readonly string? _defaultImageURL;

        /// <summary>
        /// Constructor que inicializa el mapper con la configuración de la aplicación.
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación para obtener valores como la URL de imagen por defecto</param>
        /// <exception cref="InvalidOperationException">Se lanza si la URL de imagen por defecto no está configurada</exception>
        public CartMapper(IConfiguration configuration)
        {
            _configuration = configuration;
            _defaultImageURL = _configuration.GetValue<string>("Products:DefaultImageUrl") ?? throw new InvalidOperationException("La URL de la imagen por defecto no puede ser nula.");
        }

        /// <summary>
        /// Configura todos los mapeos relacionados con el carrito.
        /// Debe llamarse al inicio de la aplicación para registrar las configuraciones en Mapster.
        /// </summary>
        public void ConfigureAllMappings()
        {
            ConfigureCartItemMappings();
            ConfigureCartMappings();
        }

        /// <summary>
        /// Configura el mapeo de Cart (entidad del dominio) a CartDTO.
        /// Transforma los totales a formato de moneda y mapea la colección de items.
        /// </summary>
        public void ConfigureCartMappings()
        {
            TypeAdapterConfig<Cart, CartDTO>.NewConfig()
                .Map(dest => dest.BuyerId, src => src.BuyerId)
                .Map(dest => dest.UserId, src => src.UserId)
                .Map(dest => dest.SubTotalPrice, src => src.SubTotal.ToString("C"))
                .Map(dest => dest.Items, src => src.CartItems)
                .Map(dest => dest.TotalPrice, src => src.Total.ToString("C"))
                .Map(dest => dest.TotalUniqueItemsCount, src => src.TotalUniqueItemsCount)
                .Map(dest => dest.TotalSavedAmount, src => src.TotalSavedAmount);
        }

        /// <summary>
        /// Configura el mapeo de CartItem (entidad del dominio) a CartItemDTO.
        /// Realiza las siguientes transformaciones:
        /// - Extrae información del producto relacionado (título, precio, descuento)
        /// - Selecciona la primera imagen del producto o usa una imagen por defecto
        /// - Calcula el subtotal (precio × cantidad) formateado como moneda
        /// - Calcula el total aplicando el descuento: (precio × cantidad × (1 - descuento%/100))
        /// </summary>
        public void ConfigureCartItemMappings()
        {
            TypeAdapterConfig<CartItem, CartItemDTO>.NewConfig()
                .Map(dest => dest.ProductId, src => src.ProductId)
                .Map(dest => dest.ProductTitle, src => src.Product.Title)
                .Map(dest => dest.ProductImageUrl, src => src.Product.Images != null && src.Product.Images.Any() ? src.Product.Images.First().ImageUrl : _defaultImageURL)
                .Map(dest => dest.Price, src => src.Product.Price)
                .Map(dest => dest.Discount, src => src.Product.Discount)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.SubTotalPrice, src => (src.Product.Price * src.Quantity).ToString("C"))
                .Map(dest => dest.TotalPrice, src => (src.Product.Price * src.Quantity * (1 - (decimal)src.Product.Discount / 100)).ToString("C"));
        }
    }
}
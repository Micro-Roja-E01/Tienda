using Mapster;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.CartDTO;

namespace Tienda.src.Application.Mappers
{
    /// <summary>
    /// Clase para mapear objetos de tipo DTO a Cart y viceversa.
    /// </summary>
    public class CartMapper
    {
        private readonly IConfiguration _configuration;
        private readonly string? _defaultImageUrl;
        public CartMapper(IConfiguration configuration)
        {
            _configuration = configuration;
            _defaultImageUrl = configuration.GetValue<string>("Products:DefaultImageUrl") ?? throw new InvalidOperationException("La URL de la imagen no está configurada.");
        }
        public void ConfigureAllMappings()
        {
            ConfigureCartItemMappings();
            ConfigureCartMappings();
        }
        public void ConfigureCartItemMappings()
        {
            TypeAdapterConfig<CartItem, CartItemDTO>.NewConfig()
                .Map(dest => dest.ProductId, src => src.ProductId)
                .Map(dest => dest.ProductTitle, src => src.Product.Title)
                .Map(dest => dest.ProductImageUrl, src => src.Product.Images != null && src.Product.Images.Any() ? src.Product.Images.First().ImageUrl : _defaultImageUrl)
                .Map(dest => dest.Price, src => src.Product.Price)
                .Map(dest => dest.Discount, src => src.Product.Discount)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.SubTotalPrice, src => (src.Product.Price * src.Quantity).ToString("C"))
                .Map(dest => dest.TotalPrice, src => (src.Product.Price * src.Quantity * (1 - (decimal)src.Product.Discount / 100)).ToString("C"));
        }
        public void ConfigureCartMappings()
        {
            TypeAdapterConfig<Cart, CartDTO>.NewConfig()
                .Map(dest => dest.BuyerId, src => src.BuyerId)
                .Map(dest => dest.UserId, src => src.UserId)
                .Map(dest => dest.SubTotalPrice, src => src.SubTotal.ToString("C"))
                .Map(dest => dest.Items, src => src.CartItems)
                .Map(dest => dest.TotalPrice, src => src.Total.ToString("C"));
        }

    }
}
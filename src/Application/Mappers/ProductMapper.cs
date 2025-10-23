using Mapster;
using tienda.src.Application.DTO.ProductDTO;
using tienda.src.Application.DTO.ProductDTO.CostumerDTO;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.Services.Implements;

namespace Tienda.src.Application.Mappers
{
    /// <summary>
    /// Clase para mapear objetos de tipo DTO a Product y viceversa.
    /// </summary>
    public class ProductMapper
    {
        private readonly IConfiguration _configuration;
        private readonly string? _defaultImageUrl;
        private readonly int _fewUnitsAvailable;
        public ProductMapper(IConfiguration configuration)
        {
            _configuration = configuration;
            _defaultImageUrl = _configuration.GetValue<string>("Products:DefaultImageUrl") ?? throw new InvalidOperationException("La URL de la imagen no pudo ser obtenida o es invalida");
            _fewUnitsAvailable = _configuration.GetValue<int?>("Products:FewUnitsAvailable") ?? throw new InvalidOperationException("La configuración 'FewUnitsAvailable' no puede ser nula.");
        }

        /// <summary>
        /// Configura el mapeo de ProductDTO a Product.
        /// </summary>
        public void ConfigureAllMappings()
        {
            ConfigureProductMappings();
            ConfigureProductDetailMappings();
        }

        public void ConfigureProductMappings()
        {
            TypeAdapterConfig<Product, ProductDTO>.NewConfig()
                .Map(dest => dest.ImageUrl, src => src.Images.Any() ? src.Images.First().ImageUrl : _defaultImageUrl)
                .Map(dest => dest.Price, src => (decimal)src.Price);
        }

        public void ConfigureProductDetailMappings()
        {
            TypeAdapterConfig<Product, ProductDetailDTO>.NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.MainImageURL, src => src.Images.Any() ? src.Images.First().ImageUrl : _defaultImageUrl)
                .Map(dest => dest.ImageUrls, src => src.Images.Select(img => img.ImageUrl).ToList())
                .Map(dest => dest.Price, src => src.Price.ToString())
                .Map(dest => dest.Stock, src => src.Stock)
                .Map(dest => dest.StockIndicator, src => src.Stock <= _fewUnitsAvailable ? "Pocas unidades disponibles" : "Disponible")
                .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : "Sin categoría")
                .Map(dest => dest.BrandName, src => src.Brand != null ? src.Brand.Name : "Sin marca")
                .Map(dest => dest.IsAvailable, src => src.IsAvailable)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);
        }
    }
}
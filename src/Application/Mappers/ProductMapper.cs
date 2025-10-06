using Mapster;
using tienda.src.Application.DTO.ProductDTO;
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
        }

        public void ConfigureProductMappings()
        {
            TypeAdapterConfig<Product, ProductDTO>.NewConfig()
                .Map(dest => dest.ImageUrl, src => src.Images.Any() ? src.Images.First().ImageUrl : _defaultImageUrl)
                .Map(dest => dest.Price, src => (decimal)src.Price);
        }
    }
}
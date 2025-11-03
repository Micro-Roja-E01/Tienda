using Mapster;
using tienda.src.Application.DTO.ProductDTO;
using tienda.src.Application.DTO.ProductDTO.AdminDTO;
using tienda.src.Application.DTO.ProductDTO.CostumerDTO;
using Tienda.src.Application.Domain.Models;

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

        /// <summary>
        /// Crea una nueva instancia del mapper de productos usando la configuración de la aplicación.
        /// </summary>
        /// <param name="configuration">Configuración desde donde se obtienen valores por defecto para productos.</param>
        /// <exception cref="InvalidOperationException">Se lanza si no se puede obtener la URL de imagen por defecto o el umbral de pocas unidades.</exception>
        public ProductMapper(IConfiguration configuration)
        {
            _configuration = configuration;
            _defaultImageUrl = _configuration.GetValue<string>("Products:DefaultImageUrl")
                ?? throw new InvalidOperationException("La URL de la imagen no pudo ser obtenida o es invalida");
            _fewUnitsAvailable = _configuration.GetValue<int?>("Products:FewUnitsAvailable")
                ?? throw new InvalidOperationException("La configuración 'FewUnitsAvailable' no puede ser nula.");
        }

        /// <summary>
        /// Configura todos los mapeos relacionados con productos.
        /// </summary>
        public void ConfigureAllMappings()
        {
            ConfigureProductMappings();
            ConfigureProductDetailMappings();
            ConfigureProductDetailForAdminMappings();
        }

        /// <summary>
        /// Configura los mapeos generales de Product → ProductDTO.
        /// </summary>
        public void ConfigureProductMappings()
        {
            TypeAdapterConfig<Product, ProductDTO>.NewConfig()
                .Map(dest => dest.ImageUrl,
                    src => src.Images.Any()
                        ? src.Images.First().ImageUrl
                        : _defaultImageUrl)
                .Map(dest => dest.Price, src => (decimal)src.Price);
        }

        /// <summary>
        /// Configura los mapeos de Product → ProductDetailDTO y Product → ProductForCostumerDTO (detalle público).
        /// Incluye imágenes, stock y nombres de categoría/marca.
        /// </summary>
        public void ConfigureProductDetailMappings()
        {
            TypeAdapterConfig<Product, ProductDetailDTO>.NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.MainImageURL,
                    src => src.Images.Any()
                        ? src.Images.First().ImageUrl
                        : _defaultImageUrl)
                .Map(dest => dest.ImageUrls, src => src.Images.Select(img => img.ImageUrl).ToList())
                .Map(dest => dest.Price, src => src.Price.ToString())
                .Map(dest => dest.Stock, src => src.Stock)
                .Map(dest => dest.StockIndicator,
                    src => src.Stock <= _fewUnitsAvailable
                        ? "Pocas unidades disponibles"
                        : "Disponible")
                .Map(dest => dest.CategoryName,
                    src => src.Category != null ? src.Category.Name : "Sin categoría")
                .Map(dest => dest.BrandName,
                    src => src.Brand != null ? src.Brand.Name : "Sin marca")
                .Map(dest => dest.IsAvailable, src => src.IsAvailable)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);
        }

        /// <summary>
        /// Configura el mapeo de Product a ProductDetailForAdminDTO con toda la información de auditoría
        /// </summary>
        public void ConfigureProductDetailForAdminMappings()
        {
            TypeAdapterConfig<Product, ProductDetailForAdminDTO>.NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.Discount, src => src.Discount)
                .Map(dest => dest.FinalPrice, src => src.FinalPrice) // Propiedad calculada
                .Map(dest => dest.Stock, src => src.Stock)
                .Map(dest => dest.Status, src => src.Status.ToString())
                .Map(dest => dest.IsAvailable, src => src.IsAvailable)
                .Map(dest => dest.IsDeleted, src => src.IsDeleted)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.UpdatedAt, src => src.UpdatedAt)
                .Map(dest => dest.DeletedAt, src => src.DeletedAt)
                .Map(dest => dest.CategoryId, src => src.CategoryId)
                .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : "Sin categoría")
                .Map(dest => dest.BrandId, src => src.BrandId)
                .Map(dest => dest.BrandName, src => src.Brand != null ? src.Brand.Name : "Sin marca")
                .Map(dest => dest.Images, src => src.Images.Select(img => new ImageDetailDTO
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    PublicId = img.PublicId,
                    CreatedAt = img.CreatedAt
                }).ToList())
                .Map(dest => dest.StockIndicator, src =>
                    src.Stock <= 0 ? "Sin stock" :
                    src.Stock <= _fewUnitsAvailable ? "Últimas unidades" :
                    "En stock");
        }
    }
}
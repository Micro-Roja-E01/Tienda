using Mapster;
using Serilog;
using tienda.src.Application.DTO.ProductDTO;
using tienda.src.Application.DTO.ProductDTO.AdminDTO;
using tienda.src.Application.DTO.ProductDTO.CostumerDTO;
using tienda.src.Application.Services.Interfaces;
using tienda.src.Infrastructure.Repositories.Interfaces;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.ProductDTO;

namespace Tienda.src.Application.Services.Implements
{
    /// <summary>
    /// Implementación del servicio de productos.
    /// Maneja la lógica de negocio relacionada con productos.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;
        private readonly int _defaultPageSize;

        public ProductService(
            IProductRepository productRepository,
            IConfiguration configuration,
            IFileService fileService
        )
        {
            _productRepository = productRepository;
            _configuration = configuration;
            _fileService = fileService;
            _defaultPageSize = int.Parse(
                _configuration["Products:DefaultPageSize"]
                    ?? throw new InvalidOperationException(
                        "La configuración 'DefaultPageSize' no está definida."
                    )
            );
        }

        public async Task<string> CreateProductAsync(CreateProductDTO createProductDTO)
        {
            try
            {
                // Validar que el DTO no sea nulo
                if (createProductDTO == null)
                    throw new ArgumentNullException(
                        nameof(createProductDTO),
                        "Los datos del producto no pueden ser nulos."
                    );

                // Adaptar DTO a entidad Product
                Product product = createProductDTO.Adapt<Product>();

                // Crear o obtener la categoría
                Category category =
                    await _productRepository.CreateOrGetCategoryAsync(createProductDTO.CategoryName)
                    ?? throw new Exception("Error al crear o obtener la categoría del producto.");

                // Crear o obtener la marca
                Brand brand =
                    await _productRepository.CreateOrGetBrandAsync(createProductDTO.BrandName)
                    ?? throw new Exception("Error al crear o obtener la marca del producto.");

                // Asignar IDs de categoría y marca al producto
                product.CategoryId = category.Id;
                product.BrandId = brand.Id;
                product.Images = new List<Image>();

                // Crear el producto en la base de datos
                int productId = await _productRepository.CreateAsync(product);
                Log.Information("Producto creado: {@Product}", product);

                // Validar que se proporcionen imágenes
                if (createProductDTO.Images == null || !createProductDTO.Images.Any())
                {
                    Log.Information(
                        "No se proporcionaron imágenes. Se asignará la imagen por defecto."
                    );
                    throw new InvalidOperationException(
                        "Debe proporcionar al menos una imagen para el producto."
                    );
                }

                // Subir las imágenes asociadas al producto
                foreach (var image in createProductDTO.Images)
                {
                    Log.Information("Imagen asociada al producto: {@Image}", image);
                    await _fileService.UploadAsync(image, productId);
                }

                Log.Information("Producto creado exitosamente con ID: {ProductId}", productId);
                return productId.ToString();
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex, "Error de validación al crear el producto: {Message}", ex.Message);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, "Error de operación al crear el producto: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inesperado al crear el producto: {Message}", ex.Message);
                throw new Exception($"Error al crear el producto: {ex.Message}", ex);
            }
        }

        public async Task<ProductDetailDTO> GetByIdForCostumerAsync(int productId)
        {
            var product =
                await _productRepository.GetByIdAsync(productId)
                ?? throw new KeyNotFoundException($"Producto con ID {productId} no encontrado.");
            Log.Information("Producto obtenido para el cliente: {@Product}", product);
            return product.Adapt<ProductDetailDTO>();
        }

        public async Task<ProductDetailDTO> GetByIdForAdminAsync(int productId)
        {
            var product =
                await _productRepository.GetByIdAsync(productId)
                ?? throw new KeyNotFoundException($"Producto con ID {productId} no encontrado.");
            Log.Information("Producto obtenido para el administrador: {@Product}", product);
            return product.Adapt<ProductDetailDTO>();
        }

        public async Task<ListedProductsForAdminDTO> GetFilteredForAdminAsync(
            SearchParamsDTO searchParams
        )
        {
            try
            {
                Log.Information(
                    "Obteniendo productos para administrador con parámetros de búsqueda: {@SearchParams}",
                    searchParams
                );

                var (products, totalCount) = await _productRepository.GetFilteredForAdminAsync(
                    searchParams
                );
                var totalPages = (int)
                    Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
                int currentPage = searchParams.PageNumber;
                int pageSize = searchParams.PageSize ?? _defaultPageSize;

                if (currentPage < 1 || currentPage > totalPages)
                {
                    throw new ArgumentOutOfRangeException(
                        "El número de página está fuera de rango."
                    );
                }

                Log.Information(
                    "Total de productos encontrados: {TotalCount}, Total de páginas: {TotalPages}, Página actual: {CurrentPage}, Tamaño de página: {PageSize}",
                    totalCount,
                    totalPages,
                    currentPage,
                    pageSize
                );

                // Convertimos los productos filtrados a DTOs para la respuesta
                return new ListedProductsForAdminDTO
                {
                    Products = products.Adapt<List<ProductForAdminDTO>>(),
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = currentPage,
                    PageSize = products.Count(),
                };
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Error(ex, "Error de rango en paginación: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Error inesperado al obtener productos para administrador: {Message}",
                    ex.Message
                );
                throw new Exception(
                    $"Error al obtener productos para administrador: {ex.Message}",
                    ex
                );
            }
        }

        public async Task<ListedProductsForCostumerDTO> GetFilteredForCostumerAsync(
            SearchParamsDTO searchParams
        )
        {
            try
            {
                Log.Information(
                    "Obteniendo productos para cliente con parámetros de búsqueda: {@SearchParams}",
                    searchParams
                );

                var (products, totalCount) = await _productRepository.GetFilteredForCustomerAsync(
                    searchParams
                );
                var totalPages = (int)
                    Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
                int currentPage = searchParams.PageNumber;
                int pageSize = searchParams.PageSize ?? _defaultPageSize;

                if (currentPage < 1 || currentPage > totalPages)
                {
                    throw new ArgumentOutOfRangeException(
                        "El número de página está fuera de rango."
                    );
                }

                Log.Information(
                    "Total de productos encontrados: {TotalCount}, Total de páginas: {TotalPages}, Página actual: {CurrentPage}, Tamaño de página: {PageSize}",
                    totalCount,
                    totalPages,
                    currentPage,
                    pageSize
                );

                // Convertimos los productos filtrados a DTOs para la respuesta
                return new ListedProductsForCostumerDTO
                {
                    Products = products.Adapt<List<ProductForCostumerDTO>>(),
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = currentPage,
                    PageSize = products.Count(),
                };
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Error(ex, "Error de rango en paginación para cliente: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Error inesperado al obtener productos para cliente: {Message}",
                    ex.Message
                );
                throw new Exception($"Error al obtener productos para cliente: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica si un producto existe.
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Task que representa la operación</returns>
        public async Task ToggleActiveAsync(int productId)
        {
            await _productRepository.ToggleActiveAsync(productId);
        }
    }
}
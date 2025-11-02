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
        private readonly IFileRepository _fileRepository;
        private readonly int _defaultPageSize;

        public ProductService(IProductRepository productRepository, IConfiguration configuration, IFileService fileService, IFileRepository fileRepository)
        {
            _productRepository = productRepository;
            _configuration = configuration;
            _fileService = fileService;
            _fileRepository = fileRepository;
            _defaultPageSize = int.Parse(_configuration["Products:DefaultPageSize"] ?? throw new InvalidOperationException("La configuración 'DefaultPageSize' no está definida."));
        }

        /// <summary>
        /// Crea un nuevo producto a partir de un DTO que incluye archivos de imagen.
        /// Valida categoría, marca e imágenes antes de persistir.
        /// </summary>
        /// <param name="createProductDTO">Datos del producto a crear.</param>
        /// <returns>Identificador del producto creado en formato string.</returns>
        /// <exception cref="ArgumentNullException">Si el DTO es nulo.</exception>
        /// <exception cref="InvalidOperationException">Si no se envía al menos una imagen.</exception>
        public async Task<string> CreateProductAsync(CreateProductDTO createProductDTO)
        {
            try
            {
                // Validar que el DTO no sea nulo
                if (createProductDTO == null)
                    throw new ArgumentNullException(nameof(createProductDTO), "Los datos del producto no pueden ser nulos.");

                // Adaptar DTO a entidad Product
                Product product = createProductDTO.Adapt<Product>();

                // Configurar propiedades adicionales del producto
                product.IsAvailable = true; // Los productos nuevos están disponibles por defecto

                // Crear o obtener la categoría
                Category category = await _productRepository.CreateOrGetCategoryAsync(createProductDTO.CategoryName)
                    ?? throw new Exception("Error al crear o obtener la categoría del producto.");

                // Crear o obtener la marca
                Brand brand = await _productRepository.CreateOrGetBrandAsync(createProductDTO.BrandName)
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
                    Log.Information("No se proporcionaron imágenes. Se asignará la imagen por defecto.");
                    throw new InvalidOperationException("Debe proporcionar al menos una imagen para el producto.");
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

        /// <summary>
        /// Crea un nuevo producto a partir de un DTO que viene en JSON (sin archivos),
        /// permitiendo asociar imágenes por URL.
        /// </summary>
        /// <param name="createProductJsonDTO">Datos del producto, incluida la categoría, marca y URLs de imágenes.</param>
        /// <returns>Identificador del producto creado en formato string.</returns>
        /// <exception cref="ArgumentNullException">Si el DTO es nulo.</exception>
        /// <exception cref="InvalidOperationException">Si ocurre un error al crear el producto.</exception>
        public async Task<string> CreateProductJsonAsync(CreateProductJsonDTO createProductJsonDTO)
        {
            try
            {
                // Validar que el DTO no sea nulo
                if (createProductJsonDTO == null)
                    throw new ArgumentNullException(nameof(createProductJsonDTO), "Los datos del producto no pueden ser nulos.");

                // Adaptar DTO a entidad Product
                Product product = createProductJsonDTO.Adapt<Product>();

                // Configurar propiedades adicionales del producto
                product.IsAvailable = true; // Los productos nuevos están disponibles por defecto

                // Crear o obtener la categoría
                Category category = await _productRepository.CreateOrGetCategoryAsync(createProductJsonDTO.CategoryName)
                    ?? throw new Exception("Error al crear o obtener la categoría del producto.");

                // Crear o obtener la marca
                Brand brand = await _productRepository.CreateOrGetBrandAsync(createProductJsonDTO.BrandName)
                    ?? throw new Exception("Error al crear o obtener la marca del producto.");

                // Asignar IDs de categoría y marca al producto
                product.CategoryId = category.Id;
                product.BrandId = brand.Id;
                product.Images = new List<Image>();

                // Crear el producto en la base de datos
                int productId = await _productRepository.CreateAsync(product);
                Log.Information("Producto creado: {@Product}", product);

                // Si se proporcionan URLs de imágenes, crearlas en la base de datos
                if (createProductJsonDTO.ImageUrls != null && createProductJsonDTO.ImageUrls.Any())
                {
                    foreach (var imageUrl in createProductJsonDTO.ImageUrls)
                    {
                        var image = new Image
                        {
                            ImageUrl = imageUrl.Url,
                            ProductId = productId,
                            PublicId = $"product_{productId}_{Guid.NewGuid()}" // Generar un PublicId único
                        };

                        await _fileRepository.CreateAsync(image);
                        Log.Information("Imagen agregada al producto: {@Image}", image);
                    }
                }
                else
                {
                    Log.Information("No se proporcionaron URLs de imágenes. El producto se creó sin imágenes.");
                }

                Log.Information("Producto creado exitosamente con ID: {ProductId}", productId);
                return productId.ToString();
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex, "Error de validación al crear el producto: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inesperado al crear el producto: {Message}", ex.Message);
                throw new InvalidOperationException("Error al crear el producto. Por favor, inténtelo de nuevo más tarde.");
            }
        }

        /// <summary>
        /// Obtiene el detalle de un producto para mostrarlo al cliente final,
        /// incluyendo imágenes, precio final y estado de stock.
        /// </summary>
        /// <param name="productId">Identificador del producto.</param>
        /// <returns>DTO con el detalle del producto.</returns>
        /// <exception cref="KeyNotFoundException">Si el producto no existe.</exception>
        public async Task<ProductDetailDTO> GetByIdForCostumerAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId)
                ?? throw new KeyNotFoundException($"Producto con ID {productId} no encontrado.");

            // Map básico
            var dto = product.Adapt<ProductDetailDTO>();


            var discountPct = product.Discount;
            var priceCLP = product.Price;
            var discountVal = (int)Math.Ceiling(priceCLP * (discountPct / 100.0));
            var finalPrice = Math.Max(0, priceCLP - discountVal);

            // Poblar imágenes
            var fallback = _configuration["Products:DefaultImageUrl"]
                        ?? "https://shop.songprinting.com/global/images/PublicShop/ProductSearch/prodgr_default_300.png";

            var urls = product.Images?.OrderBy(i => i.CreatedAt)
                                    .Select(i => i.ImageUrl)
                                    .Where(u => !string.IsNullOrWhiteSpace(u))
                                    .ToList() ?? new List<string>();

            dto.MainImageURL = urls.FirstOrDefault() ?? fallback;

            dto.ImageUrls = urls.Count > 0 ? urls : new List<string> { dto.MainImageURL };


            dto.FinalPrice = finalPrice;
            dto.DiscountPercentage = discountPct;

            //indicador de stock en detalle
            int fewUnits = int.Parse(_configuration["Products:FewUnitsAvailable"] ?? "5");
            dto.StockIndicator = product.Stock <= 0 ? "Sin stock"
                            : product.Stock <= fewUnits ? "Últimas unidades"
                            : "";

            return dto;
        }

        /// <summary>
        /// Obtiene el detalle de un producto para la vista de administrador.
        /// </summary>
        /// <param name="productId">Identificador del producto.</param>
        /// <returns>DTO con la información del producto.</returns>
        /// <exception cref="KeyNotFoundException">Si el producto no existe.</exception>
        public async Task<ProductDetailDTO> GetByIdForAdminAsync(int productId)
        {
            var product = await _productRepository.GetByIdForAdminAsync(productId) ?? throw new KeyNotFoundException($"Producto con ID {productId} no encontrado.");
            Log.Information("Producto obtenido para el administrador: {@Product}", product);
            return product.Adapt<ProductDetailDTO>();
        }

        /// <summary>
        /// Obtiene una lista paginada de productos para el panel de administración,
        /// aplicando filtros, orden y paginación.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y paginación.</param>
        /// <returns>Listado de productos para administrador con metadatos.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Si el número de página está fuera de rango.</exception>
        public async Task<ListedProductsForAdminDTO> GetFilteredForAdminAsync(SearchParamsDTO searchParams)
        {
            try
            {
                Log.Information("Obteniendo productos para administrador con parámetros de búsqueda: {@SearchParams}", searchParams);

                var (products, totalCount) = await _productRepository.GetFilteredForAdminAsync(searchParams);
                var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
                int currentPage = searchParams.PageNumber ?? 1;
                int pageSize = searchParams.PageSize ?? _defaultPageSize;

                if (currentPage < 1 || (totalCount > 0 && currentPage > totalPages))
                {
                    throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");
                }

                Log.Information("Total de productos encontrados: {TotalCount}, Total de páginas: {TotalPages}, Página actual: {CurrentPage}, Tamaño de página: {PageSize}",
                    totalCount, totalPages, currentPage, pageSize);

                // Convertimos los productos filtrados a DTOs para la respuesta
                return new ListedProductsForAdminDTO
                {
                    Products = products.Adapt<List<ProductForAdminDTO>>(),
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = currentPage,
                    PageSize = pageSize
                };
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Error(ex, "Error de rango en paginación: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inesperado al obtener productos para administrador: {Message}", ex.Message);
                throw new Exception($"Error al obtener productos para administrador: {ex.Message}", ex);
            }
        }

         /// <summary>
        /// Obtiene una lista paginada de productos para el cliente (tienda),
        /// considerando disponibilidad, filtros y orden.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda (texto, categoría, marca, precio, orden).</param>
        /// <returns>Listado de productos para cliente con metadatos de paginación.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Si la página solicitada no existe.</exception>
        public async Task<ListedProductsForCostumerDTO> GetFilteredForCostumerAsync(SearchParamsDTO searchParams)
        {
            try
            {
                Log.Information("Obteniendo productos para cliente con parámetros de búsqueda: {@SearchParams}", searchParams);

                var (items, totalCount) = await _productRepository.GetFilteredForCustomerAsync(searchParams);

                int currentPage = searchParams.PageNumber ?? 1;
                int pageSize    = searchParams.PageSize   ?? _defaultPageSize;
                int totalPages  = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);

                if (totalCount > 0 && (currentPage < 1 || currentPage > totalPages))
                    throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");

                return new ListedProductsForCostumerDTO
                {
                    Products   = items.ToList(),       // ProductForCostumerDTO
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage= currentPage,
                    PageSize   = pageSize
                };
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Error(ex, "Error de rango en paginación para cliente: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inesperado al obtener productos para cliente: {Message}", ex.Message);
                throw new Exception($"Error al obtener productos para cliente: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Cambia el estado de un producto a activo/inactivo.
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Task que representa la operación</returns>
        public async Task ToggleActiveAsync(int productId)
        {
            await _productRepository.ToggleActiveAsync(productId);
        }

        /// <summary>
        /// Activa todos los productos en la base de datos (método temporal para desarrollo)
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task ActivateAllProductsAsync()
        {
            Log.Information("Activando todos los productos en la base de datos");
            await _productRepository.ActivateAllAsync();
            Log.Information("Todos los productos han sido activados");
        }

    }
}
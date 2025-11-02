using Mapster;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
            int productId = 0;
            var uploadedPublicIds = new List<string>(); // Para tracking de rollback

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
                productId = await _productRepository.CreateAsync(product);
                Log.Information("Producto creado con ID: {ProductId}", productId);

                // Validar que se proporcionen imágenes
                if (createProductDTO.Images == null || !createProductDTO.Images.Any())
                {
                    Log.Warning("No se proporcionaron imágenes para el producto {ProductId}", productId);
                    throw new InvalidOperationException("Debe proporcionar al menos una imagen para el producto.");
                }

                // Subir las imágenes asociadas al producto con tracking para rollback
                foreach (var image in createProductDTO.Images)
                {
                    try
                    {
                        Log.Information("Subiendo imagen para el producto {ProductId}", productId);

                        // El FileService internamente sube a Cloudinary y guarda en BD
                        // Necesitamos obtener el PublicId para el tracking
                        await _fileService.UploadAsync(image, productId);

                        // Obtener la última imagen subida para tracking
                        var images = await _fileRepository.GetByProductIdAsync(productId);
                        var lastImage = images.OrderByDescending(i => i.CreatedAt).FirstOrDefault();

                        if (lastImage != null)
                        {
                            uploadedPublicIds.Add(lastImage.PublicId);
                            Log.Information("Imagen subida exitosamente: {PublicId}", lastImage.PublicId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error al subir imagen para el producto {ProductId}", productId);
                        throw; // Lanzar para activar el rollback
                    }
                }

                Log.Information("Producto {ProductId} creado exitosamente con {Count} imágenes",
                    productId, uploadedPublicIds.Count);
                return productId.ToString();
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex, "Error de validación al crear el producto: {Message}", ex.Message);
                await RollbackProductCreation(productId, uploadedPublicIds);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, "Error de operación al crear el producto: {Message}", ex.Message);
                await RollbackProductCreation(productId, uploadedPublicIds);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inesperado al crear el producto: {Message}", ex.Message);
                await RollbackProductCreation(productId, uploadedPublicIds);
                throw new Exception($"Error al crear el producto: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Realiza rollback eliminando el producto y todas las imágenes subidas a Cloudinary
        /// </summary>
        private async Task RollbackProductCreation(int productId, List<string> uploadedPublicIds)
        {
            if (uploadedPublicIds.Any())
            {
                Log.Warning("Iniciando rollback: eliminando {Count} imágenes de Cloudinary", uploadedPublicIds.Count);

                foreach (var publicId in uploadedPublicIds)
                {
                    try
                    {
                        await _fileService.DeleteAsync(publicId);
                        await _fileRepository.DeleteAsync(publicId);
                        Log.Information("Imagen {PublicId} eliminada durante rollback", publicId);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error al eliminar imagen {PublicId} durante rollback", publicId);
                    }
                }
            }

            if (productId > 0)
            {
                try
                {
                    Log.Warning("Eliminando producto {ProductId} durante rollback", productId);
                    await _productRepository.DeleteAsync(productId);
                    Log.Information("Producto {ProductId} eliminado durante rollback", productId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al eliminar producto {ProductId} durante rollback", productId);
                }
            }
        }

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
        /// Obtiene el detalle completo de un producto con toda la información de auditoría (admin)
        /// </summary>
        public async Task<ProductDetailForAdminDTO> GetDetailedByIdForAdminAsync(int productId)
        {
            var product = await _productRepository.GetByIdForAdminAsync(productId)
                ?? throw new KeyNotFoundException($"Producto con ID {productId} no encontrado.");

            Log.Information("Producto detallado obtenido para el administrador: ID {ProductId}", productId);
            return product.Adapt<ProductDetailForAdminDTO>();
        }

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
                int pageSize = searchParams.PageSize ?? _defaultPageSize;
                int totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);

                if (totalCount > 0 && (currentPage < 1 || currentPage > totalPages))
                    throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");

                return new ListedProductsForCostumerDTO
                {
                    Products = items.ToList(),       // ProductForCostumerDTO
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = currentPage,
                    PageSize = pageSize
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

        public async Task<string> DeleteProductAsync(int id)
        {
            try
            {
                Log.Information("Iniciando eliminación (soft delete) del producto con ID: {ProductId}", id);

                // Usar GetByIdWithoutRelationsAsync para evitar problemas con tracking y relaciones
                var product = await _productRepository.GetByIdWithoutRelationsAsync(id);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
                }
                if (product.IsDeleted)
                {
                    throw new InvalidOperationException($"El producto con ID {id} ya ha sido eliminado.");
                }

                // Obtener todas las imágenes del producto (solo las activas)
                var images = await _fileRepository.GetByProductIdAsync(id);

                Log.Information("Marcando {Count} imágenes como eliminadas (soft delete) sin borrar de Cloudinary", images.Count());

                // ✅ SOFT DELETE: Solo marcar imágenes como eliminadas, NO borrar de Cloudinary
                foreach (var image in images)
                {
                    try
                    {
                        await _fileRepository.SoftDeleteAsync(image.Id);
                        Log.Information("Imagen {ImageId} con PublicId {PublicId} marcada como eliminada", image.Id, image.PublicId);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error al marcar imagen {ImageId} como eliminada: {Message}", image.Id, ex.Message);
                    }
                }

                // Soft delete del producto
                product.IsDeleted = true;
                product.DeletedAt = DateTime.UtcNow;
                product.IsAvailable = false; // También marcar como no disponible
                product.UpdatedAt = DateTime.UtcNow;

                await _productRepository.UpdateAsync(product);

                Log.Information("Producto {ProductId} y {Count} imágenes marcados como eliminados (soft delete). Imágenes preservadas en Cloudinary.", id, images.Count());

                return $"Producto con ID {id} eliminado exitosamente.";
            }
            catch (KeyNotFoundException)
            {
                Log.Warning("Intento de eliminar producto inexistente: {ProductId}", id);
                throw;
            }

            catch (InvalidOperationException)
            {
                Log.Warning("Intento de eliminar producto ya eliminado: {ProductId}", id);
                throw;
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Error inesperado al eliminar producto {ProductId}", id);
                throw new Exception($"Error al eliminar producto: {ex.Message}", ex);
            }

        }

        public async Task<string> UpdateProductAsync(int id, UpdateProductDTO updateProductDTO)
        {
            try
            {
                Log.Information("Actualizando producto con ID: {ProductId}", id);

                // 1. Verificar que el producto existe
                // Usar GetByIdWithoutRelationsAsync para evitar problemas con tracking y relaciones
                var product = await _productRepository.GetByIdWithoutRelationsAsync(id);

                if (product == null)
                {
                    throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
                }

                if (product.IsDeleted)
                {
                    throw new InvalidOperationException("No se puede actualizar un producto eliminado.");
                }

                // 2. Actualizar solo los campos proporcionados (partial update)
                if (!string.IsNullOrWhiteSpace(updateProductDTO.Title))
                {
                    product.Title = updateProductDTO.Title;
                    Log.Information("Título actualizado a: {Title}", updateProductDTO.Title);
                }

                if (!string.IsNullOrWhiteSpace(updateProductDTO.Description))
                {
                    product.Description = updateProductDTO.Description;
                    Log.Information("Descripción actualizada");
                }

                if (updateProductDTO.Price.HasValue)
                {
                    product.Price = updateProductDTO.Price.Value;
                    Log.Information("Precio actualizado a: {Price}", updateProductDTO.Price.Value);
                }

                if (updateProductDTO.Stock.HasValue)
                {
                    product.Stock = updateProductDTO.Stock.Value;
                    Log.Information("Stock actualizado a: {Stock}", updateProductDTO.Stock.Value);
                }

                if (updateProductDTO.Discount.HasValue)
                {
                    // Validar rango 0-100
                    if (updateProductDTO.Discount.Value < 0 || updateProductDTO.Discount.Value > 100)
                    {
                        throw new ArgumentException("El descuento debe estar entre 0 y 100.");
                    }
                    product.Discount = updateProductDTO.Discount.Value;
                    Log.Information("Descuento actualizado a: {Discount}%", updateProductDTO.Discount.Value);
                }

                if (!string.IsNullOrWhiteSpace(updateProductDTO.Status))
                {
                    // Convertir string a enum Status
                    if (Enum.TryParse<Status>(updateProductDTO.Status, true, out var statusEnum))
                    {
                        product.Status = statusEnum;
                        Log.Information("Estado actualizado a: {Status}", updateProductDTO.Status);
                    }
                    else
                    {
                        throw new ArgumentException($"Estado inválido: {updateProductDTO.Status}. Debe ser 'Nuevo' o 'Usado'.");
                    }
                }

                // 3. Actualizar marca si se proporciona
                if (!string.IsNullOrWhiteSpace(updateProductDTO.BrandName))
                {
                    var brand = await _productRepository.CreateOrGetBrandAsync(updateProductDTO.BrandName);
                    if (brand != null)
                    {
                        product.BrandId = brand.Id;
                        Log.Information("Marca actualizada a: {BrandName}", updateProductDTO.BrandName);
                    }
                }

                // 4. Actualizar categoría si se proporciona
                if (!string.IsNullOrWhiteSpace(updateProductDTO.CategoryName))
                {
                    var category = await _productRepository.CreateOrGetCategoryAsync(updateProductDTO.CategoryName);
                    if (category != null)
                    {
                        product.CategoryId = category.Id;
                        Log.Information("Categoría actualizada a: {CategoryName}", updateProductDTO.CategoryName);
                    }
                }

                // 5. Eliminar imágenes si se especifican
                if (updateProductDTO.ImageIdsToDelete != null && updateProductDTO.ImageIdsToDelete.Any())
                {
                    Log.Information("Eliminando {Count} imágenes del producto {ProductId}",
                        updateProductDTO.ImageIdsToDelete.Count, id);

                    foreach (var imageId in updateProductDTO.ImageIdsToDelete)
                    {
                        try
                        {
                            var image = await _fileRepository.GetByIdAsync(imageId);

                            if (image == null)
                            {
                                Log.Warning("Imagen con ID {ImageId} no encontrada, omitiendo...", imageId);
                                continue;
                            }

                            if (image.ProductId != id)
                            {
                                Log.Warning("Imagen {ImageId} no pertenece al producto {ProductId}, omitiendo...",
                                    imageId, id);
                                continue;
                            }

                            // Eliminar de Cloudinary
                            await _fileService.DeleteAsync(image.PublicId);
                            Log.Information("Imagen {PublicId} eliminada de Cloudinary", image.PublicId);

                            // Eliminar de BD
                            await _fileRepository.DeleteAsync(image.PublicId);
                            Log.Information("Imagen con ID {ImageId} eliminada de la base de datos", imageId);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error al eliminar imagen {ImageId}", imageId);
                            // Continuar con las demás imágenes
                        }
                    }
                }

                // 6. Agregar nuevas imágenes si se proporcionan
                if (updateProductDTO.NewImages != null && updateProductDTO.NewImages.Any())
                {
                    Log.Information("Agregando {Count} nuevas imágenes al producto {ProductId}",
                        updateProductDTO.NewImages.Count, id);

                    foreach (var imageFile in updateProductDTO.NewImages)
                    {
                        try
                        {
                            // Subir a Cloudinary (el método internamente guarda en BD)
                            await _fileService.UploadAsync(imageFile, id);
                            Log.Information("Nueva imagen agregada al producto {ProductId}", id);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error al agregar nueva imagen al producto {ProductId}", id);
                            throw new Exception($"Error al subir imagen: {ex.Message}", ex);
                        }
                    }
                }

                // 7. Actualizar timestamp (IMPORTANTE: NO modificar CreatedAt)
                product.UpdatedAt = DateTime.UtcNow;

                // 8. Guardar cambios en BD
                await _productRepository.UpdateAsync(product);

                Log.Information("Producto {ProductId} actualizado exitosamente", id);
                return $"Producto con ID {id} actualizado exitosamente";
            }
            catch (KeyNotFoundException)
            {
                Log.Warning("Producto no encontrado: {ProductId}", id);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Log.Warning("Operación no válida al actualizar producto {ProductId}: {Message}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inesperado al actualizar producto {ProductId}", id);
                throw new Exception($"Error al actualizar producto: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Restaura un producto eliminado, marcándolo como disponible nuevamente.
        /// </summary>
        /// <param name="id">El ID del producto a restaurar.</param>
        /// <returns>Mensaje de confirmación.</returns>
        public async Task<string> RestoreProductAsync(int id)
        {
            try
            {
                // Verificar que el producto existe
                var product = await _productRepository.GetByIdWithoutRelationsAsync(id)
                    ?? throw new KeyNotFoundException($"El producto con ID {id} no existe.");

                // Validar que el producto esté eliminado
                if (!product.IsDeleted)
                {
                    throw new InvalidOperationException($"El producto con ID {id} no está eliminado.");
                }

                // Restaurar el producto
                await _productRepository.RestoreAsync(id);

                // ✅ RESTAURAR IMÁGENES: Obtener todas las imágenes eliminadas y restaurarlas
                var allImages = await _fileRepository.GetAllByProductIdAsync(id);
                var deletedImages = allImages.Where(i => i.IsDeleted).ToList();

                if (deletedImages.Any())
                {
                    Log.Information("Restaurando {Count} imágenes del producto {ProductId}", deletedImages.Count, id);

                    foreach (var image in deletedImages)
                    {
                        try
                        {
                            await _fileRepository.RestoreAsync(image.Id);
                            Log.Information("Imagen {ImageId} con PublicId {PublicId} restaurada", image.Id, image.PublicId);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error al restaurar imagen {ImageId}: {Message}", image.Id, ex.Message);
                        }
                    }
                }

                Log.Information("Producto {ProductId} y {Count} imágenes restaurados exitosamente", id, deletedImages.Count);
                return $"Producto con ID {id} y {deletedImages.Count} imagen(es) restaurados exitosamente";
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Producto {ProductId} no encontrado: {Message}", id, ex.Message);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Log.Warning("Operación no válida al restaurar producto {ProductId}: {Message}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inesperado al restaurar producto {ProductId}", id);
                throw new Exception($"Error al restaurar producto: {ex.Message}", ex);
            }
        }
    }
}
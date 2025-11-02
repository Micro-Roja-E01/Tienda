using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using tienda.src.Application.DTO.ProductDTO;
using tienda.src.Application.DTO.ProductDTO.CostumerDTO;
using tienda.src.Infrastructure.Repositories.Interfaces;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Infrastructure.Data;

namespace tienda.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Implementación del repositorio de productos que interactúa con la base de datos.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        private readonly int _defaultPageSize;

        public ProductRepository(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _defaultPageSize = _configuration.GetValue<int?>("Products:DefaultPageSize") ?? throw new ArgumentNullException("El tamaño de página por defecto no puede ser nulo.");
        }

        /// <summary>
        /// Crea un nuevo producto en el repositorio.
        /// </summary>
        /// <param name="product">El producto a crear.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el id del producto creado</returns>
        public async Task<int> CreateAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product.Id;
        }

        /// <summary>
        /// Crea o obtiene una marca por su nombre.
        /// </summary>
        /// <param name="brandName">El nombre de la marca.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con la marca creada o encontrada.</returns>
        public async Task<Brand?> CreateOrGetBrandAsync(string brandName)
        {
            var normalized = brandName.Trim();

            var brand = await _context.Brands
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Name.ToLower() == normalized.ToLower());

            if (brand != null)
                return brand;

            var slug = GenerateSlug(normalized);

            // por si acaso ya existe una con el mismo slug (raro, pero seguro)
            var slugExists = await _context.Brands
                .AnyAsync(b => b.Slug.ToLower() == slug.ToLower());
            if (slugExists)
                slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";

            brand = new Brand
            {
                Name = normalized,
                Slug = slug,
                Description = null
            };

            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
            return brand;
        }


        /// <summary>
        /// Crea o obtiene una categoría por su nombre.
        /// </summary>
        /// <param name="categoryName">El nombre de la categoría.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con la categoría creada o encontrada.</returns>
        public async Task<Category?> CreateOrGetCategoryAsync(string categoryName)
        {
            var lowerName = categoryName.Trim().ToLower();

            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == lowerName);

            if (category != null)
                return category;

            // crear nueva categoría con slug
            var newCategory = new Category
            {
                Name = categoryName.Trim(),
                Slug = GenerateSlug(categoryName),
                Description = null
            };

            await _context.Categories.AddAsync(newCategory);
            await _context.SaveChangesAsync();
            return newCategory;


        }
        /// <summary>
        /// Retorna un producto específico por su ID.
        /// </summary>
        /// <param name="id">El ID del producto a buscar.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el producto encontrado o null si no se encuentra.</returns>
        public async Task<Product?> GetByIdAsync(int id)
        {
            var result = await _context.Products.
                                        AsNoTracking().
                                        Where(p => p.Id == id && !p.IsAvailable).
                                        Include(p => p.Category).
                                        Include(p => p.Brand).
                                        Include(p => p.Images)
                                        .FirstOrDefaultAsync();
            Log.Information("Resultado de los productos obtenidos: {@result}", result);
            return result;
        }

        /// <summary>
        /// Retorna un producto específico por su ID desde el punto de vista de un admin.
        /// </summary>
        /// <param name="id">El ID del producto a buscar.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el producto encontrado o null si no se encuentra.</returns>
        public async Task<Product?> GetByIdForAdminAsync(int id)
        {
            return await _context.Products.
                                        AsNoTracking().
                                        Where(p => p.Id == id).
                                        Include(p => p.Category).
                                        Include(p => p.Brand).
                                        Include(p => p.Images)
                                        .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtiene un producto por su ID sin relaciones de navegación (para operaciones de actualización/eliminación)
        /// No usa AsNoTracking() ni Include() para evitar problemas con actualizaciones
        /// </summary>
        /// <param name="id">El ID del producto a buscar.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el producto encontrado o null si no se encuentra.</returns>
        public async Task<Product?> GetByIdWithoutRelationsAsync(int id)
        {
            return await _context.Products
                                        .AsNoTracking()
                                        .Where(p => p.Id == id)
                                        .FirstOrDefaultAsync();
        }

        // <summary>
        /// Retorna una lista de productos para el administrador con los parámetros de búsqueda especificados.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar los productos.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con una lista de productos para el administrador y el conteo total de productos.</returns>
        public async Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForAdminAsync(SearchParamsDTO searchParams)
        {
            int fewUnitsThreshold = _configuration.GetValue<int>("Products:FewUnitsAvailable");

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images.OrderBy(i => i.CreatedAt).Take(1)) // Cargamos la URL de la imagen principal a la hora de crear el producto
                .AsNoTracking();

            // Filtro de disponibilidad (para admin puede ver todos o solo disponibles)
            if (searchParams.IsAvailable.HasValue)
            {
                query = query.Where(p => p.IsAvailable == searchParams.IsAvailable.Value);
            }

            // Filtro de productos eliminados (por defecto excluidos, admin puede incluirlos)
            if (!searchParams.IncludeDeleted.HasValue || !searchParams.IncludeDeleted.Value)
            {
                query = query.Where(p => !p.IsDeleted);
            }

            // Búsqueda por texto
            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            {
                var searchTerm = searchParams.SearchTerm.Trim().ToLower();

                query = query.Where(p =>
                    p.Title.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.Category.Name.ToLower().Contains(searchTerm) ||
                    p.Brand.Name.ToLower().Contains(searchTerm) ||
                    p.Status.ToString().ToLower().Contains(searchTerm) ||
                    p.Price.ToString().ToLower().Contains(searchTerm) ||
                    p.Stock.ToString().ToLower().Contains(searchTerm)
                );
            }

            // Filtro por categoría
            if (!string.IsNullOrWhiteSpace(searchParams.Category))
            {
                var category = searchParams.Category.Trim().ToLower();
                query = query.Where(p => p.Category.Name.ToLower() == category);
            }

            // Filtro por marca
            if (!string.IsNullOrWhiteSpace(searchParams.Brand))
            {
                var brand = searchParams.Brand.Trim().ToLower();
                query = query.Where(p => p.Brand.Name.ToLower() == brand);
            }

            // Filtro por rango de precio
            if (searchParams.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= searchParams.MinPrice.Value);
            }

            if (searchParams.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= searchParams.MaxPrice.Value);
            }

            // Filtro por estado (Nuevo/Usado)
            if (!string.IsNullOrWhiteSpace(searchParams.Status))
            {
                var status = searchParams.Status.Trim();
                query = query.Where(p => p.Status.ToString() == status);
            }

            // Filtro por descuento
            if (searchParams.HasDiscount.HasValue && searchParams.HasDiscount.Value)
            {
                query = query.Where(p => p.Discount > 0);
            }

            // Filtro por stock bajo
            if (searchParams.LowStock.HasValue && searchParams.LowStock.Value)
            {
                query = query.Where(p => p.Stock > 0 && p.Stock <= fewUnitsThreshold);
            }

            // Contar total ANTES de paginar
            int totalCount = await query.CountAsync();

            // Ordenamiento
            bool asc = (searchParams.SortDir ?? "asc").ToLower() == "asc";
            switch ((searchParams.SortBy ?? "").ToLower())
            {
                case "price":
                    query = asc ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                    break;
                case "title":
                    query = asc ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title);
                    break;
                case "createdat":
                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            // Paginación
            var products = await query
                .Skip(((searchParams.PageNumber ?? 1) - 1) * (searchParams.PageSize ?? _defaultPageSize))
                .Take(searchParams.PageSize ?? _defaultPageSize)
                .ToArrayAsync();

            return (products, totalCount);
        }

        /// <summary>
        /// Retorna una lista de productos para el cliente con los parámetros de búsqueda especificados.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar los productos.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con una lista de productos para el cliente y el conteo total de productos.</returns>
        public async Task<(IEnumerable<ProductForCostumerDTO> products, int totalCount)> GetFilteredForCustomerAsync(SearchParamsDTO searchParams)
        {
            int pageNumber = searchParams.PageNumber ?? 1;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;

            int fewUnitsThreshold = _configuration.GetValue<int>("Products:FewUnitsAvailable");
            string fallbackImage = _configuration["Products:DefaultImageUrl"]
                ?? "https://shop.songprinting.com/global/images/PublicShop/ProductSearch/prodgr_default_300.png";

            // Base query (solo disponibles)
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.IsAvailable)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    CategoryName = p.Category.Name,
                    BrandName = p.Brand.Name,
                    p.Price,
                    p.Discount,     // %
                    p.Stock,
                    p.Status,       // Enum
                    p.IsAvailable,
                    p.CreatedAt,
                    MainImageURL = p.Images
                        .OrderBy(i => i.CreatedAt)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),
                });

            // Filtros
            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            {
                var term = searchParams.SearchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(term) ||
                    p.CategoryName.ToLower().Contains(term) ||
                    p.BrandName.ToLower().Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(searchParams.Category))
            {
                var cat = searchParams.Category.Trim().ToLower();
                query = query.Where(p => p.CategoryName.ToLower() == cat);
            }
            if (!string.IsNullOrWhiteSpace(searchParams.Brand))
            {
                var br = searchParams.Brand.Trim().ToLower();
                query = query.Where(p => p.BrandName.ToLower() == br);
            }
            if (searchParams.MinPrice.HasValue)
                query = query.Where(p => p.Price >= searchParams.MinPrice.Value);
            if (searchParams.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= searchParams.MaxPrice.Value);

            // Filtros avanzados
            if (!string.IsNullOrWhiteSpace(searchParams.Status))
            {
                var status = searchParams.Status.Trim();
                // Comparar con el enum directamente
                query = query.Where(p => p.Status.ToString() == status);
            }
            if (searchParams.HasDiscount.HasValue && searchParams.HasDiscount.Value)
            {
                query = query.Where(p => p.Discount > 0);
            }
            if (searchParams.LowStock.HasValue && searchParams.LowStock.Value)
            {
                query = query.Where(p => p.Stock > 0 && p.Stock <= fewUnitsThreshold);
            }

            // Orden seguro
            bool asc = (searchParams.SortDir ?? "asc").ToLower() == "asc";
            switch ((searchParams.SortBy ?? "").ToLower())
            {
                case "price":
                    query = asc ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                    break;
                case "title":
                    query = asc ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title);
                    break;
                case "createdat":
                default:
                    // por defecto: más nuevos primero
                    query = asc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt);
                    break;
            }

            int totalCount = await query.CountAsync();

            var page = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var rows = page.Select(p =>
            {
                int discountPct = p.Discount;
                int finalPrice = p.Price;
                if (discountPct > 0)
                {
                    var discountValue = (int)Math.Ceiling(p.Price * (discountPct / 100.0));
                    finalPrice = Math.Max(0, p.Price - discountValue);
                }

                string indicator =
                    p.Stock <= 0 ? "Sin stock" :
                    (p.Stock <= fewUnitsThreshold ? "Últimas unidades" : "");

                return new ProductForCostumerDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    MainImageURL = string.IsNullOrWhiteSpace(p.MainImageURL) ? fallbackImage : p.MainImageURL,
                    Price = p.Price,
                    Discount = discountPct,
                    FinalPrice = finalPrice,
                    Stock = p.Stock,
                    StockIndicator = indicator,
                    CategoryName = p.CategoryName,
                    BrandName = p.BrandName,
                    IsAvailable = p.IsAvailable
                };
            }).ToList();

            return (rows, totalCount);
        }

        /// <summary>
        /// Obtiene el stock real de un producto por su ID.
        /// </summary>
        /// <param name="productId">El ID del producto cuyo stock se obtendrá.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el stock real del producto.</returns>
        public async Task<int> GetRealStockAsync(int productId)
        {
            return await _context.Products.AsNoTracking().Where(p => p.Id == productId).Select(p => p.Stock).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Cambia el estado activo de un producto por su ID.
        /// </summary>
        /// <param name="id">El ID del producto cuyo estado se cambiará.</param>
        public async Task ToggleActiveAsync(int id)
        {
            await _context.Products.Where(p => p.Id == id).ExecuteUpdateAsync(p => p.SetProperty(p => p.IsAvailable, p => !p.IsAvailable));
        }

        /// <summary>
        /// Actualiza el stock de un producto por su ID.
        /// </summary>
        /// <param name="productId">El ID del producto cuyo stock se actualizará.</param>
        /// <param name="stock">El nuevo stock del producto.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task UpdateStockAsync(int productId, int stock)
        {
            Product? product = await _context.Products.FindAsync(productId) ?? throw new KeyNotFoundException("Producto no encontrado");
            product.Stock = stock;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        /// <param name="product">El producto con los datos actualizados.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina un producto por su ID.
        /// </summary>
        /// <param name="id">El ID del producto a eliminar.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Verifica si existe un producto con el ID especificado.
        /// </summary>
        /// <param name="id">El ID del producto a verificar.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con true si existe, false en caso contrario.</returns>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        /// <summary>
        /// Retorna una lista de productos para el cliente con los parámetros de búsqueda especificados.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar los productos.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con una lista de productos para el cliente y el conteo total de productos.</returns>
        public async Task<(IEnumerable<object> products, int totalCount)> GetFilteredForCostumerAsync(SearchParamsDTO searchParams)
        {
            return await GetFilteredForCustomerAsync(searchParams);
        }

        /// <summary>
        /// Activa todos los productos en la base de datos (método temporal para desarrollo)
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task ActivateAllAsync()
        {
            await _context.Products
                .Where(p => !p.IsAvailable)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsAvailable, true));
        }

        /// <summary>
        /// Restaura un producto eliminado, marcándolo como disponible nuevamente.
        /// </summary>
        /// <param name="id">El ID del producto a restaurar.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task RestoreAsync(int id)
        {
            await _context.Products
                .Where(p => p.Id == id)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(x => x.IsDeleted, false)
                    .SetProperty(x => x.DeletedAt, (DateTime?)null)
                    .SetProperty(x => x.IsAvailable, true)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
        }

        private static string GenerateSlug(string text)
        {
            text = text.Trim().ToLower();
            text = text
                .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");
            text = string.Join("-", text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return text;
        }

    }
}
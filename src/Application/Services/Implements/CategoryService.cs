using Mapster;
using Microsoft.EntityFrameworkCore;
using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.CategoryDTO;
using Tienda.src.Application.Services.Interfaces;
using Tienda.src.Infrastructure.Data;

namespace Tienda.src.Application.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly DataContext _context;

        public CategoryService(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene una lista paginada de categorías, permitiendo filtrar por nombre.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y paginación.</param>
        /// <returns>DTO con las categorías y datos de paginación.</returns>
        public async Task<PagedCategoriesDTO> GetAllAsync(SearchParamsDTO searchParams)
        {
            int page = searchParams.PageNumber ?? 1;
            int pageSize = searchParams.PageSize ?? 10;
            if (pageSize > 50) pageSize = 50;

            var query = _context.Categories
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            // búsqueda por nombre
            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            {
                string term = searchParams.SearchTerm.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(term));
            }

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // acá sí usamos Mapster
            var categories = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ProjectToType<CategoryListItemDTO>()   // usa lo del CategoryMapper
                .ToListAsync();

            // como en el mapper pusiste ProductCount = 0, acá lo rellenamos real
            var categoryIds = categories.Select(c => c.Id).ToList();
            var productCounts = await _context.Products
                .Where(p => categoryIds.Contains(p.CategoryId))
                .GroupBy(p => p.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var cat in categories)
            {
                var count = productCounts.FirstOrDefault(pc => pc.CategoryId == cat.Id)?.Count ?? 0;
                cat.ProductCount = count;
            }

            return new PagedCategoriesDTO
            {
                Categories = categories,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Obtiene el detalle de una categoría específica.
        /// </summary>
        /// <param name="id">Identificador de la categoría.</param>
        /// <returns>DTO con el detalle de la categoría o <c>null</c> si no existe.</returns>
        public async Task<CategoryDetailDTO?> GetByIdAsync(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null)
                return null;

            int productCount = await _context.Products.CountAsync(p => p.CategoryId == id);

            // mapeamos y luego seteamos el count
            var dto = category.Adapt<CategoryDetailDTO>();
            dto.ProductCount = productCount;
            return dto;
        }

        /// <summary>
        /// Crea una nueva categoría a partir de los datos proporcionados.
        /// </summary>
        /// <param name="dto">Datos de la categoría a crear.</param>
        /// <returns>DTO con la categoría creada.</returns>
        /// <exception cref="InvalidOperationException">Si ya existe una categoría con el mismo nombre.</exception>
        public async Task<CategoryDetailDTO> CreateAsync(CategoryCreateDTO dto)
        {
            string normalizedName = dto.Name.Trim();

            // 1) nombre único
            bool exists = await _context.Categories
                .AnyAsync(c => !c.IsDeleted &&
                               c.Name.ToLower() == normalizedName.ToLower());
            if (exists)
                throw new InvalidOperationException("Ya existe una categoría con ese nombre.");

            // 2) mapeamos DTO → entity usando el mapper
            var category = dto.Adapt<Category>(); // esto ya setea Slug y CreatedAt porque lo pusiste en el mapper

            // pero ojo: igual validamos slug único
            bool slugExists = await _context.Categories
                .AnyAsync(c => !c.IsDeleted &&
                               c.Slug.ToLower() == category.Slug.ToLower());
            if (slugExists)
            {
                category.Slug = $"{category.Slug}-{Guid.NewGuid().ToString("N")[..6]}";
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // entity → DTO
            var result = category.Adapt<CategoryDetailDTO>();
            result.ProductCount = 0;
            return result;
        }

        /// <summary>
        /// Actualiza una categoría existente validando la unicidad del nombre y del slug.
        /// </summary>
        /// <param name="id">Identificador de la categoría.</param>
        /// <param name="dto">Datos actualizados.</param>
        /// <returns>DTO actualizado o <c>null</c> si la categoría no existe.</returns>
        /// <exception cref="InvalidOperationException">Si el nombre propuesto ya está en uso.</exception>
        public async Task<CategoryDetailDTO?> UpdateAsync(int id, CategoryUpdateDTO dto)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null)
                return null;

            string normalizedName = dto.Name.Trim();

            // 1) validar nombre único (excluyéndome)
            bool nameTaken = await _context.Categories
                .AnyAsync(c => c.Id != id &&
                               !c.IsDeleted &&
                               c.Name.ToLower() == normalizedName.ToLower());
            if (nameTaken)
                throw new InvalidOperationException("Ya existe otra categoría con ese nombre.");

            // 2) aplicamos los cambios del DTO sobre la entidad usando Mapster
            dto.Adapt(category); // esto va a tocar Name, Description y Slug (porque lo mapeaste así)

            // 3) validar que el nuevo slug no choque con otros
            bool slugTaken = await _context.Categories
                .AnyAsync(c => c.Id != id &&
                               !c.IsDeleted &&
                               c.Slug.ToLower() == category.Slug.ToLower());
            if (slugTaken)
            {
                category.Slug = $"{category.Slug}-{Guid.NewGuid().ToString("N")[..6]}";
            }

            await _context.SaveChangesAsync();

            int productCount = await _context.Products.CountAsync(p => p.CategoryId == id);

            var result = category.Adapt<CategoryDetailDTO>();
            result.ProductCount = productCount;
            return result;
        }

        /// <summary>
        /// Elimina lógicamente una categoría si no tiene productos asociados.
        /// </summary>
        /// <param name="id">Identificador de la categoría.</param>
        /// <exception cref="InvalidOperationException">
        /// Se lanza si la categoría no existe o tiene productos asociados.
        /// </exception>
        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                throw new InvalidOperationException("La categoría no existe.");

            // integridad: no borrar si hay productos
            bool hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
                throw new InvalidOperationException("No se puede eliminar la categoría porque tiene productos asociados.");

            category.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Genera un slug normalizado a partir de un nombre, reemplazando tildes y espacios.
        /// </summary>
        /// <param name="text">Texto a normalizar.</param>
        /// <returns>Slug en minúsculas separado por guiones.</returns>
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
using Mapster;
using Microsoft.EntityFrameworkCore;
using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.BrandDTO;
using Tienda.src.Application.Services.Interfaces;
using Tienda.src.Infrastructure.Data;

namespace Tienda.src.Application.Services.Implements
{
    public class BrandService : IBrandService
    {
        private readonly DataContext _context;

        public BrandService(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene una lista paginada de marcas filtradas por nombre.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y paginación (término, página, tamaño de página).</param>
        /// <returns>Objeto con la página de marcas y metadatos de paginación.</returns>
        public async Task<PagedBrandsDTO> GetAllAsync(SearchParamsDTO searchParams)
        {
            int page = searchParams.PageNumber ?? 1;
            int pageSize = searchParams.PageSize ?? 10;
            if (pageSize > 50) pageSize = 50;

            var query = _context.Brands
                .Where(b => !b.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            {
                string term = searchParams.SearchTerm.Trim().ToLower();
                query = query.Where(b => b.Name.ToLower().Contains(term));
            }

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var brands = await query
                .OrderBy(b => b.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BrandListItemDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    Slug = b.Slug,
                    CreatedAt = b.CreatedAt,
                    ProductCount = _context.Products.Count(p => p.BrandId == b.Id)
                })
                .ToListAsync();

            return new PagedBrandsDTO
            {
                Brands = brands,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Obtiene el detalle de una marca por su identificador.
        /// </summary>
        /// <param name="id">Identificador único de la marca.</param>
        /// <returns>DTO con el detalle de la marca o <c>null</c> si no existe.</returns>
        public async Task<BrandDetailDTO?> GetByIdAsync(int id)
        {
            var brand = await _context.Brands
                .Where(b => b.Id == id && !b.IsDeleted)
                .FirstOrDefaultAsync();

            if (brand == null) return null;

            int productCount = await _context.Products.CountAsync(p => p.BrandId == id);

            // aquí SÍ usamos mapster
            var dto = brand.Adapt<BrandDetailDTO>();
            dto.ProductCount = productCount;
            return dto;
        }

        /// <summary>
        /// Crea una nueva marca validando que no exista otra con el mismo nombre o slug.
        /// </summary>
        /// <param name="dto">Datos de la marca a crear.</param>
        /// <returns>DTO con el detalle de la marca creada.</returns>
        /// <exception cref="InvalidOperationException">Si ya existe una marca con el mismo nombre.</exception>
        public async Task<BrandDetailDTO> CreateAsync(BrandCreateDTO dto)
        {
            string normalizedName = dto.Name.Trim();
            string slug = GenerateSlug(normalizedName);

            bool exists = await _context.Brands
                .AnyAsync(b => b.Name.ToLower() == normalizedName.ToLower() && !b.IsDeleted);
            if (exists)
                throw new InvalidOperationException("Ya existe una marca con ese nombre.");

            bool slugExists = await _context.Brands
                .AnyAsync(b => b.Slug.ToLower() == slug.ToLower() && !b.IsDeleted);
            if (slugExists)
                slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";

            var brand = new Brand
            {
                Name = normalizedName,
                Slug = slug,
                Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim()
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            var result = brand.Adapt<BrandDetailDTO>();
            result.ProductCount = 0;
            return result;
        }

        /// <summary>
        /// Actualiza una marca existente validando unicidad de nombre y slug.
        /// </summary>
        /// <param name="id">Identificador de la marca a actualizar.</param>
        /// <param name="dto">Nuevos datos de la marca.</param>
        /// <returns>DTO actualizado o <c>null</c> si la marca no existe.</returns>
        /// <exception cref="InvalidOperationException">Si el nuevo nombre ya está en uso por otra marca.</exception>
        public async Task<BrandDetailDTO?> UpdateAsync(int id, BrandUpdateDTO dto)
        {
            var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
            if (brand == null) return null;

            string normalizedName = dto.Name.Trim();
            string slug = GenerateSlug(normalizedName);

            bool exists = await _context.Brands
                .AnyAsync(b => b.Id != id &&
                               b.Name.ToLower() == normalizedName.ToLower() &&
                               !b.IsDeleted);
            if (exists)
                throw new InvalidOperationException("Ya existe otra marca con ese nombre.");

            bool slugExists = await _context.Brands
                .AnyAsync(b => b.Id != id &&
                               b.Slug.ToLower() == slug.ToLower() &&
                               !b.IsDeleted);
            if (slugExists)
                slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";

            brand.Name = normalizedName;
            brand.Slug = slug;
            brand.Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();

            await _context.SaveChangesAsync();

            int productCount = await _context.Products.CountAsync(p => p.BrandId == id);

            var result = brand.Adapt<BrandDetailDTO>();
            result.ProductCount = productCount;
            return result;
        }

        /// <summary>
        /// Elimina lógicamente una marca, siempre que no tenga productos asociados.
        /// </summary>
        /// <param name="id">Identificador de la marca.</param>
        /// <exception cref="InvalidOperationException">
        /// Se lanza si la marca no existe o si tiene productos asociados.
        /// </exception>
        public async Task DeleteAsync(int id)
        {
            var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);
            if (brand == null)
                throw new InvalidOperationException("La marca no existe.");

            bool hasProducts = await _context.Products.AnyAsync(p => p.BrandId == id);
            if (hasProducts)
                throw new InvalidOperationException("No se puede eliminar la marca porque tiene productos asociados.");

            brand.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Genera un slug URL-friendly a partir de un texto, normalizando tildes y espacios.
        /// </summary>
        /// <param name="text">Texto de entrada.</param>
        /// <returns>Slug normalizado.</returns>
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

using Microsoft.EntityFrameworkCore;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Infrastructure.Data;
using Tienda.src.Infrastructure.Repositories.Interfaces;

namespace Tienda.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Repositorio para la entidad categoría.
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DataContext _context;
        public CategoryRepository(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Devuelve una consulta sobre categorías para que el servicio pueda aplicar filtros.
        /// </summary>
        public IQueryable<Category> Query()
        {
            return _context.Categories.AsQueryable();
        }

        /// <summary>
        /// Comprueba si existe una categoría con el nombre dado.
        /// </summary>
        /// <param name="name">Nombre a validar.</param>
        /// <param name="excludeId">ID a excluir de la comprobación.</param>
        /// <returns><c>true</c> si existe.</returns>
        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var query = _context.Categories.Where(c => !c.IsDeleted && c.Name.ToLower() == name.ToLower());
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        /// <summary>
        /// Comprueba si existe una categoría con el slug dado.
        /// </summary>
        /// <param name="slug">Slug a validar.</param>
        /// <param name="excludeId">ID a excluir de la comprobación.</param>
        /// <returns><c>true</c> si existe.</returns>
        public async Task<bool> ExistsBySlugAsync(string slug, int? excludeId = null)
        {
            var query = _context.Categories.Where(c => !c.IsDeleted && c.Slug.ToLower() == slug.ToLower());
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        /// <summary>
        /// Obtiene una categoría por su ID.
        /// </summary>
        /// <param name="id">ID de la categoría.</param>
        /// <returns>La categoría o <c>null</c>.</returns>
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Agrega una categoría al contexto.
        /// </summary>
        /// <param name="category">Categoría a agregar.</param>
        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        /// <summary>
        /// Guarda los cambios pendientes en la base de datos.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
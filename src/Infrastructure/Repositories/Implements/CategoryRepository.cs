using Microsoft.EntityFrameworkCore;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Infrastructure.Data;
using Tienda.src.Infrastructure.Repositories.Interfaces;

namespace Tienda.src.Infrastructure.Repositories.Implements
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DataContext _context;
        public CategoryRepository(DataContext context)
        {
            _context = context;
        }

        public IQueryable<Category> Query()
        {
            return _context.Categories.AsQueryable();
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var query = _context.Categories.Where(c => !c.IsDeleted && c.Name.ToLower() == name.ToLower());
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> ExistsBySlugAsync(string slug, int? excludeId = null)
        {
            var query = _context.Categories.Where(c => !c.IsDeleted && c.Slug.ToLower() == slug.ToLower());
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

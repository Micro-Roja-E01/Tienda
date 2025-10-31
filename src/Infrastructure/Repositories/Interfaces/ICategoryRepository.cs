using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.Domain.Models;

namespace Tienda.src.Infrastructure.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
        Task<bool> ExistsBySlugAsync(string slug, int? excludeId = null);
        Task<Category?> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task SaveChangesAsync();
        IQueryable<Category> Query();
    }
}

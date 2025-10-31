using Tienda.src.Application.Domain.Models;

namespace Tienda.src.Infrastructure.Repositories.Interfaces
{
    public interface IBrandRepository
    {
        Task<Brand?> GetByIdAsync(int id);
        Task<Brand?> GetByNameAsync(string name);
        Task AddAsync(Brand brand);
        Task SaveChangesAsync();
    }
}
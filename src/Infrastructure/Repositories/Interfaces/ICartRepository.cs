using Tienda.src.Application.Domain.Models;

namespace Tienda.src.Infrastructure.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> FindAsync(string buyerId, int? userId);
        Task<Cart?> GetByUserIdAsync(int userId);
        Task<Cart?> GetAnonymousAsync(string buyerId);
        Task<Cart> CreateAsync(string buyerId, int? userId = null);
        Task UpdateAsync(Cart cart);
        Task DeleteAsync(Cart cart);
        Task AddItemAsync(Cart cart, CartItem cartItem);
        Task RemoveItemAsync(CartItem cartItem);
    }
}
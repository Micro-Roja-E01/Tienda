using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.Domain.Models;

namespace Tienda.src.Infrastructure.Repositories.Interfaces
{
    // TODO: Terminar documentación
    public interface IUserRepository
    {
        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="id"> ID del usuario </param>
        /// <returns> Usuario encontrado o null </returns>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);

        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByRutAsync(string rut);
        Task<User?> GetByRutAsync(string rut, bool trackChanges = false);
        Task<bool> CreateAsync(User user, string password);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<string> GetUserRoleAsync(User user);
        Task<bool> DeleteAsync(int userId);
        Task<bool> ConfirmEmailAsync(string email);
        Task<int> DeleteUnconfirmedAsync();
    }
}
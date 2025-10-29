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
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);

        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByRutAsync(string rut);

        // NUEVOS: validación de unicidad excluyendo al propio usuario
        Task<bool> EmailExistsForOtherUserAsync(string email, int currentUserId);
        Task<bool> RutExistsForOtherUserAsync(string rut, int currentUserId);

        Task<User?> GetByRutAsync(string rut, bool trackChanges = false);
        Task<bool> CreateAsync(User user, string password);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<string> GetUserRoleAsync(User user);
        Task<bool> DeleteAsync(int userId);
        Task<bool> ConfirmEmailAsync(string email);
        Task<int> DeleteUnconfirmedAsync();
        Task<bool> UpdatePasswordAsync(User user, string newPassword);

        // NUEVO: guardar cambios de perfil (nombre, rut, etc.)
        Task<bool> UpdateProfileAsync(User user);
    }
}
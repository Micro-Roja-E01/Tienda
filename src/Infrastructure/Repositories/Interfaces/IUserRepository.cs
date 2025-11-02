using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.AdminUserDTO;

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

        // NUEVOS para flujo 9
        Task<(List<User> users, Dictionary<int,string> roles, int totalCount)> GetPagedForAdminAsync(AdminUserSearchParamsDTO search);
        Task<(User user, string role)?> GetByIdWithRoleAsync(int userId);
        Task<int> CountAdminsAsync();
        Task UpdateStatusAsync(User user, UserStatus status);
        Task UpdateRoleAsync(User user, string roleName);
        Task UpdateLastLoginAtAsync(User user);
    }
}
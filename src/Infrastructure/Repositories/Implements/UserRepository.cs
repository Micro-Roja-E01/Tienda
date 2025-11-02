using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Infrastructure.Data;
using Tienda.src.Infrastructure.Repositories.Interfaces;
using Tienda.src.Application.DTO.AdminUserDTO;

namespace Tienda.src.Infrastructure.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly int _daysOfDeleteUnconfirmedUsers;
        private readonly UserManager<User> _userManager;
        private readonly IVerificationCodeRepository _verificationCodeRepository;

        public UserRepository(
            DataContext context,
            UserManager<User> userManager,
            IConfiguration configuration,
            IVerificationCodeRepository verificationCodeRepository
        )
        {
            _context = context;
            _userManager = userManager;
            _verificationCodeRepository = verificationCodeRepository;
            _daysOfDeleteUnconfirmedUsers =
                configuration.GetValue<int?>("Jobs:DaysOfDeleteUnconfirmedUsers")
                ?? throw new InvalidOperationException(
                    "La configuración 'Jobs:DaysOfDeleteUnconfirmedUsers' no está definida."
                );
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<bool> ConfirmEmailAsync(string email)
        {
            var result = await _context
                .Users.Where(u => u.Email == email)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.EmailConfirmed, true));
            return result > 0;
        }

        public async Task<bool> CreateAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Cliente");
                return roleResult.Succeeded;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var result = await _userManager.DeleteAsync(user!);
            return result.Succeeded;
        }

        // TODO: Revisar que funcione con los metdoso de VerificationCode
        public async Task<int> DeleteUnconfirmedAsync()
        {
            Log.Information("Iniciando eliminacion de usuarios no confirmados");
            var cutoffDate = DateTime.UtcNow.AddDays(-_daysOfDeleteUnconfirmedUsers);
            var unconfirmedUsers = await _context
                .Users.Where(u => !u.EmailConfirmed && u.RegisteredAt < cutoffDate)
                .Include(u => u.VerificationCodes)
                .ToListAsync();

            if (!unconfirmedUsers.Any())
            {
                Log.Information("No se encontraron usuarios no confirmados para eliminar");
                return 0;
            }

            foreach (var user in unconfirmedUsers)
            {
                if (user.VerificationCodes.Any())
                {
                    await _verificationCodeRepository.DeleteByUserIdAsync(user.Id);
                }
            }

            _context.Users.RemoveRange(unconfirmedUsers);
            await _context.SaveChangesAsync();

            Log.Information($"Eliminados {unconfirmedUsers.Count} usuarios no confirmados");
            return unconfirmedUsers.Count;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByRutAsync(string rut)
        {
            return await _context.Users.AnyAsync(u => u.Rut == rut);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _userManager.FindByIdAsync(id.ToString());
        }

        
        public async Task<User?> GetByRutAsync(string rut, bool trackChanges = false)
        {
            if (trackChanges)
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Rut == rut);
            }

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Rut == rut);
        }


        public async Task<string> GetUserRoleAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault()!; // Se obtiene el primer rol del usuario. No puede ser nulo.
            // El usuario ya deberia tener un rol en la Base de Datos, por eso no se pone el ?? "User".
        }

        public async Task<bool> UpdatePasswordAsync(User user, string newPassword)
        {
            // Generar el token de reinicio de contraseña
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Reiniciar la contraseña usando el token
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (result.Succeeded)
            {
                Log.Information(
                    $"Contraseña actualizada exitosamente para el usuario: {user.Email}"
                );
                return true;
            }
            // Error al actualizar la contraseña
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            Log.Warning($"Error al actualizar contraseña para {user.Email}: {errors}");
            throw new InvalidOperationException($"No se pudo actualizar la contraseña: {errors}");
        }
        public async Task<bool> EmailExistsForOtherUserAsync(string email, int currentUserId)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email && u.Id != currentUserId);
        }

        public async Task<bool> RutExistsForOtherUserAsync(string rut, int currentUserId)
        {
            return await _context.Users
                .AnyAsync(u => u.Rut == rut && u.Id != currentUserId);
        }

        public async Task<bool> UpdateProfileAsync(User user)
        {
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                Log.Information("Perfil actualizado para usuario {UserId}", user.Id);
                return true;
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            Log.Warning("Error al actualizar perfil de usuario {UserId}: {Errors}", user.Id, errors);
            throw new InvalidOperationException($"No se pudo actualizar el perfil: {errors}");
        }
        public async Task<(List<User> users, Dictionary<int,string> roles, int totalCount)> GetPagedForAdminAsync(AdminUserSearchParamsDTO search)
        {
            var query = _context.Users.AsQueryable();

            // filtros
            if (!string.IsNullOrWhiteSpace(search.Email))
            {
                string emailLike = search.Email.ToLower();
                query = query.Where(u => u.Email!.ToLower().Contains(emailLike));
            }

            if (!string.IsNullOrWhiteSpace(search.Status))
            {
                if (search.Status == "active")
                    query = query.Where(u => u.Status == UserStatus.Active);
                else if (search.Status == "blocked")
                    query = query.Where(u => u.Status == UserStatus.Blocked);
            }

            if (search.CreatedFrom.HasValue)
            {
                query = query.Where(u => u.RegisteredAt >= search.CreatedFrom.Value);
            }

            if (search.CreatedTo.HasValue)
            {
                query = query.Where(u => u.RegisteredAt <= search.CreatedTo.Value);
            }

            // ojo: filtrado por rol requiere join con AspNetUserRoles
            // si viene Role → solo esos
            if (!string.IsNullOrWhiteSpace(search.Role))
            {
                // obtenemos ids de usuarios con ese rol
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == search.Role);
                if (role != null)
                {
                    var userIdsWithRole = await _context.UserRoles
                        .Where(ur => ur.RoleId == role.Id)
                        .Select(ur => ur.UserId)
                        .ToListAsync();

                    query = query.Where(u => userIdsWithRole.Contains(u.Id));
                }
                else
                {
                    // no existe el rol -> lista vacía
                    return (new List<User>(), new Dictionary<int,string>(), 0);
                }
            }

            // total antes de paginar
            int totalCount = await query.CountAsync();

            // orden seguro
            string orderBy = search.OrderBy?.ToLower() ?? "createdat";
            bool desc = (search.OrderDir?.ToLower() == "desc");

            query = orderBy switch
            {
                "email" => (desc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email)),
                "lastlogin" => (desc ? query.OrderByDescending(u => u.LastLoginAt) : query.OrderBy(u => u.LastLoginAt)),
                _ => (desc ? query.OrderByDescending(u => u.RegisteredAt) : query.OrderBy(u => u.RegisteredAt))
            };

            // paginación
            int skip = (search.Page - 1) * search.PageSize;
            var users = await query
                .Skip(skip)
                .Take(search.PageSize)
                .ToListAsync();

            // obtener roles de todos los usuarios de la página
            var userIds = users.Select(u => u.Id).ToList();

            var userRoles = await _context.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .ToListAsync();

            var rolesDict = await _context.Roles.ToDictionaryAsync(r => r.Id, r => r.Name!);

            var resultRoles = new Dictionary<int, string>();
            foreach (var ur in userRoles)
            {
                if (rolesDict.TryGetValue(ur.RoleId, out var roleName))
                {
                    resultRoles[ur.UserId] = roleName;
                }
            }

            return (users, resultRoles, totalCount);
        }

        public async Task<(User user, string role)?> GetByIdWithRoleAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId);
            string roleName = "Cliente";
            if (userRole != null)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == userRole.RoleId);
                if (role != null) roleName = role.Name!;
            }

            return (user, roleName);
        }

        public async Task<int> CountAdminsAsync()
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null) return 0;

            return await _context.UserRoles.CountAsync(ur => ur.RoleId == adminRole.Id);
        }

        public async Task UpdateStatusAsync(User user, UserStatus status)
        {
            user.Status = status;

            // también podemos sincronizar con Identity lockout:
            if (status == UserStatus.Blocked)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
            else
            {
                user.LockoutEnd = null;
                user.LockoutEnabled = false;
            }

            await _userManager.UpdateAsync(user);
        }

        public async Task UpdateRoleAsync(User user, string roleName)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            await _userManager.AddToRoleAsync(user, roleName);
        }
        public async Task UpdateLastLoginAtAsync(User user)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }
    
}
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.AdminUserDTO;
using Tienda.src.Infrastructure.Data;
using Tienda.src.Infrastructure.Repositories.Interfaces;

namespace Tienda.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Repositorio para operaciones de persistencia sobre usuarios
    /// </summary>
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

        /// <summary>
        /// Comprueba si la contraseña indicada coincide con la del usuario.
        /// </summary>
        /// <param name="user">Usuario a validar.</param>
        /// <param name="password">Contraseña en texto plano.</param>
        /// <returns><c>true</c> si la contraseña es correcta.</returns>
        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        /// <summary>
        /// Marca el correo de un usuario como confirmado.
        /// </summary>
        /// <param name="email">Correo del usuario.</param>
        /// <returns>Número de filas afectadas &gt; 0 si tuvo éxito.</returns>
        public async Task<bool> ConfirmEmailAsync(string email)
        {
            var result = await _context
                .Users.Where(u => u.Email == email)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.EmailConfirmed, true));
            return result > 0;
        }

        /// <summary>
        /// Crea un nuevo usuario en Identity y lo asigna al rol "Cliente".
        /// </summary>
        /// <param name="user">Entidad de usuario.</param>
        /// <param name="password">Contraseña inicial.</param>
        /// <returns><c>true</c> si se creó y se asignó el rol correctamente.</returns>
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

        /// <summary>
        /// Elimina un usuario por su identificador.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns><c>true</c> si se eliminó correctamente.</returns>
        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var result = await _userManager.DeleteAsync(user!);
            return result.Succeeded;
        }

        /// <summary>
        /// Elimina todos los usuarios que no han confirmado su correo dentro del plazo configurado
        /// y borra también sus códigos de verificación asociados.
        /// </summary>
        /// <returns>Cantidad de usuarios eliminados.</returns>
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

        /// <summary>
        /// Indica si existe un usuario con el correo proporcionado.
        /// </summary>
        /// <param name="email">Correo a buscar.</param>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        /// <summary>
        /// Indica si existe un usuario con el RUT proporcionado.
        /// </summary>
        /// <param name="rut">RUT a buscar.</param>
        public async Task<bool> ExistsByRutAsync(string rut)
        {
            return await _context.Users.AnyAsync(u => u.Rut == rut);
        }

        /// <summary>
        /// Obtiene un usuario por correo electrónico usando el <see cref="UserManager{TUser}"/>.
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        /// <summary>
        /// Obtiene un usuario por su identificador usando el <see cref="UserManager{TUser}"/>.
        /// </summary>
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _userManager.FindByIdAsync(id.ToString());
        }

        /// <summary>
        /// Obtiene un usuario por RUT, con opción de hacer tracking del registro.
        /// </summary>
        /// <param name="rut">RUT a buscar.</param>
        /// <param name="trackChanges">Si es <c>true</c> se habilita seguimiento de cambios.</param>
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

        /// <summary>
        /// Obtiene el primer rol asociado al usuario.
        /// </summary>
        /// <param name="user">Usuario objetivo.</param>
        /// <returns>Nombre del rol.</returns>

        public async Task<string> GetUserRoleAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault()!; // Se obtiene el primer rol del usuario. No puede ser nulo.
            // El usuario ya deberia tener un rol en la Base de Datos, por eso no se pone el ?? "User".
        }

        /// <summary>
        /// Actualiza la contraseña del usuario usando el flujo de reseteo de Identity.
        /// </summary>
        /// <param name="user">Usuario objetivo.</param>
        /// <param name="newPassword">Nueva contraseña.</param>
        /// <returns><c>true</c> si se actualizó correctamente.</returns>
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

        /// <summary>
        /// Verifica si el correo indicado ya existe en otro usuario distinto al dado.
        /// </summary>
        public async Task<bool> EmailExistsForOtherUserAsync(string email, int currentUserId)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email && u.Id != currentUserId);
        }

        /// <summary>
        /// Verifica si el RUT indicado ya existe en otro usuario distinto al dado.
        /// </summary>
        public async Task<bool> RutExistsForOtherUserAsync(string rut, int currentUserId)
        {
            return await _context.Users
                .AnyAsync(u => u.Rut == rut && u.Id != currentUserId);
        }

        /// <summary>
        /// Actualiza los datos de perfil de un usuario en Identity.
        /// </summary>
        /// <param name="user">Usuario con los cambios.</param>
        /// <returns><c>true</c> si se actualizó correctamente.</returns>
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

        /// <summary>
        /// Obtiene usuarios paginados para el panel de administración,
        /// permitiendo filtrar por correo, estado, fechas y rol.
        /// </summary>
        /// <param name="search">Parámetros de búsqueda.</param>
        /// <returns>Tupla con usuarios de la página, roles de esos usuarios y total de registros.</returns>
        public async Task<(List<User> users, Dictionary<int, string> roles, int totalCount)> GetPagedForAdminAsync(AdminUserSearchParamsDTO search)
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
                    return (new List<User>(), new Dictionary<int, string>(), 0);
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

        /// <summary>
        /// Obtiene un usuario con su rol asociado a partir del ID.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Tupla con usuario y nombre del rol, o <c>null</c> si no existe.</returns>
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

        /// <summary>
        /// Cuenta la cantidad de usuarios que tienen rol de administrador.
        /// </summary>
        public async Task<int> CountAdminsAsync()
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null) return 0;

            return await _context.UserRoles.CountAsync(ur => ur.RoleId == adminRole.Id);
        }

        /// <summary>
        /// Actualiza el estado (activo/bloqueado) del usuario y sincroniza el lockout de Identity.
        /// </summary>
        /// <param name="user">Usuario a actualizar.</param>
        /// <param name="status">Nuevo estado.</param>
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

        /// <summary>
        /// Reemplaza los roles actuales del usuario y asigna el nuevo rol indicado.
        /// </summary>
        /// <param name="user">Usuario objetivo.</param>
        /// <param name="roleName">Nuevo rol a asignar.</param>
        public async Task UpdateRoleAsync(User user, string roleName)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            await _userManager.AddToRoleAsync(user, roleName);
        }

        /// <summary>
        /// Actualiza la fecha y hora del último inicio de sesión del usuario.
        /// </summary>
        /// <param name="user">Usuario a actualizar.</param>
        public async Task UpdateLastLoginAtAsync(User user)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }

}
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
            var cutoffDate = DateTime.UtcNow.AddDays(_daysOfDeleteUnconfirmedUsers);
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
    }
    
}
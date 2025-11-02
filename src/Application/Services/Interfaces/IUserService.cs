using Tienda.src.Application.DTO.AuthDTO;
using Tienda.src.Application.DTO.UserDTO;
using Tienda.src.Application.DTO.AdminUserDTO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Tienda.src.Application.Services.Interfaces
{
    /// <summary>
    /// Define las operaciones relacionadas con los usuarios del sistema,
    /// incluyendo autenticación, gestión de perfil y administración.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Inicia sesión con las credenciales del usuario y genera un token JWT.
        /// </summary>
        /// <param name="loginDTO">Credenciales de acceso.</param>
        /// <param name="httpContext">Contexto HTTP actual para registrar sesión y cookies.</param>
        /// <returns>Tupla con el token generado y el ID del usuario autenticado.</returns>
        Task<(string token, int userId)> LoginAsync(LoginDTO loginDTO, HttpContext httpContext);

        /// <summary>
        /// Registra un nuevo usuario en el sistema y envía un código de verificación al correo electrónico.
        /// </summary>
        /// <param name="registerDTO">Datos del usuario a registrar.</param>
        /// <param name="httpContext">Contexto HTTP actual.</param>
        /// <returns>Mensaje indicando el resultado del registro.</returns>
        Task<string> RegisterAsync(RegisterDTO registerDTO, HttpContext httpContext);

        /// <summary>
        /// Verifica el correo electrónico del usuario usando el código recibido.
        /// </summary>
        /// <param name="verifyEmailDTO">Datos del correo y código de verificación.</param>
        /// <returns>Mensaje indicando el resultado de la verificación.</returns>
        Task<string> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO);

        /// <summary>
        /// Reenvía el código de verificación por correo electrónico.
        /// </summary>
        /// <param name="resendEmailVerificationCodeDTO">Datos del correo al que reenviar el código.</param>
        /// <returns>Mensaje indicando el resultado del reenvío.</returns>
        Task<string> ResendEmailVerificationCodeAsync(ResendEmailVerificationCodeDTO resendEmailVerificationCodeDTO);

        /// <summary>
        /// Elimina usuarios no confirmados dentro del tiempo límite configurado.
        /// </summary>
        /// <returns>Cantidad de usuarios eliminados.</returns>
        Task<int> DeleteUnconfirmedAsync();

        /// <summary>
        /// Envía un código de recuperación de contraseña al correo del usuario.
        /// </summary>
        /// <param name="recoverPasswordDTO">Correo electrónico del usuario.</param>
        /// <returns>Mensaje indicando el resultado del envío.</returns>
        Task<string> RecoverPasswordAsync(RecoverPasswordDTO recoverPasswordDTO);

        /// <summary>
        /// Restablece la contraseña del usuario utilizando el código de recuperación.
        /// </summary>
        /// <param name="resetPasswordDTO">Datos para restablecer la contraseña.</param>
        /// <returns>Mensaje indicando el resultado del proceso.</returns>
        Task<string> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);

        /// <summary>
        /// Obtiene la información del perfil del usuario autenticado.
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Datos del perfil del usuario.</returns>
        Task<UserProfileDTO> GetProfileAsync(int userId);

        /// <summary>
        /// Actualiza la información del perfil del usuario autenticado.
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <param name="dto">Datos actualizados del perfil.</param>
        /// <returns>El perfil actualizado.</returns>
        Task<UserProfileDTO> UpdateProfileAsync(int userId, UpdateProfileDTO dto);

        /// <summary>
        /// Cambia la contraseña del usuario autenticado.
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <param name="dto">Datos de la contraseña actual y la nueva.</param>
        /// <param name="httpContext">Contexto HTTP actual.</param>
        Task ChangePasswordAsync(int userId, ChangePasswordDTO dto, HttpContext httpContext);

        // --- Sección de administración ---

        /// <summary>
        /// Obtiene una lista paginada de usuarios para el panel de administración.
        /// </summary>
        /// <param name="search">Parámetros de búsqueda y filtros administrativos.</param>
        /// <returns>Usuarios paginados con información resumida.</returns>
        Task<PagedAdminUsersDTO> GetAllForAdminAsync(AdminUserSearchParamsDTO search);

        /// <summary>
        /// Obtiene los datos detallados de un usuario específico (modo administrador).
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Detalle del usuario solicitado.</returns>
        Task<AdminUserDetailDTO> GetByIdForAdminAsync(int userId);

        /// <summary>
        /// Actualiza el estado (activo o bloqueado) de un usuario desde el panel de administración.
        /// </summary>
        /// <param name="targetUserId">Usuario objetivo a modificar.</param>
        /// <param name="adminId">Administrador que realiza la acción.</param>
        /// <param name="dto">Datos de actualización del estado.</param>
        Task UpdateStatusForAdminAsync(int targetUserId, int adminId, UpdateUserStatusDTO dto);

        /// <summary>
        /// Actualiza el rol de un usuario (Admin o Cliente) desde el panel de administración.
        /// </summary>
        /// <param name="targetUserId">Usuario objetivo a modificar.</param>
        /// <param name="adminId">Administrador que realiza la acción.</param>
        /// <param name="dto">Datos de actualización del rol.</param>
        Task UpdateRoleForAdminAsync(int targetUserId, int adminId, UpdateUserRoleDTO dto);
    }
}
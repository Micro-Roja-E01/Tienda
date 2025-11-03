using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.AdminUserDTO;

namespace Tienda.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Define las operaciones de acceso a datos relacionadas con usuarios,
    /// incluyendo autenticación, verificación, administración y paginación.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Obtiene un usuario por su identificador único.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        /// <returns>El usuario encontrado o <c>null</c> si no existe.</returns>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene un usuario por su correo electrónico.
        /// </summary>
        /// <param name="email">Correo a buscar.</param>
        /// <returns>El usuario encontrado o <c>null</c> si no existe.</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Indica si ya existe un usuario registrado con el correo dado.
        /// </summary>
        /// <param name="email">Correo a comprobar.</param>
        /// <returns><c>true</c> si existe, de lo contrario <c>false</c>.</returns>
        Task<bool> ExistsByEmailAsync(string email);

        /// <summary>
        /// Indica si ya existe un usuario registrado con el RUT dado.
        /// </summary>
        /// <param name="rut">RUT a comprobar.</param>
        /// <returns><c>true</c> si existe, de lo contrario <c>false</c>.</returns>
        Task<bool> ExistsByRutAsync(string rut);

        /// <summary>
        /// Verifica si un correo está en uso por otro usuario distinto al actual.
        /// Útil para validaciones en actualización de perfil.
        /// </summary>
        /// <param name="email">Correo a comprobar.</param>
        /// <param name="currentUserId">ID del usuario que está actualizando su perfil.</param>
        /// <returns><c>true</c> si ya está en uso por otro usuario.</returns>
        Task<bool> EmailExistsForOtherUserAsync(string email, int currentUserId);

        /// <summary>
        /// Verifica si un RUT está en uso por otro usuario distinto al actual.
        /// </summary>
        /// <param name="rut">RUT a comprobar.</param>
        /// <param name="currentUserId">ID del usuario que está actualizando su perfil.</param>
        /// <returns><c>true</c> si ya está en uso por otro usuario.</returns>
        Task<bool> RutExistsForOtherUserAsync(string rut, int currentUserId);

        /// <summary>
        /// Obtiene un usuario por su RUT.
        /// </summary>
        /// <param name="rut">RUT del usuario.</param>
        /// <param name="trackChanges">Si es <c>true</c>, el usuario se rastrea en el contexto.</param>
        /// <returns>El usuario encontrado o <c>null</c>.</returns>
        Task<User?> GetByRutAsync(string rut, bool trackChanges = false);

        /// <summary>
        /// Crea un usuario en el sistema y le asigna una contraseña.
        /// </summary>
        /// <param name="user">Entidad de usuario a crear.</param>
        /// <param name="password">Contraseña en texto plano.</param>
        /// <returns><c>true</c> si se creó correctamente.</returns>
        Task<bool> CreateAsync(User user, string password);

        /// <summary>
        /// Verifica que la contraseña proporcionada coincida con la del usuario.
        /// </summary>
        /// <param name="user">Usuario a validar.</param>
        /// <param name="password">Contraseña a comprobar.</param>
        /// <returns><c>true</c> si la contraseña es correcta.</returns>
        Task<bool> CheckPasswordAsync(User user, string password);

        /// <summary>
        /// Obtiene el primer rol asignado a un usuario.
        /// </summary>
        /// <param name="user">Usuario del que se quiere obtener el rol.</param>
        /// <returns>Nombre del rol.</returns>
        Task<string> GetUserRoleAsync(User user);

        /// <summary>
        /// Elimina un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario a eliminar.</param>
        /// <returns><c>true</c> si se eliminó correctamente.</returns>
        Task<bool> DeleteAsync(int userId);

        /// <summary>
        /// Marca el correo de un usuario como confirmado.
        /// </summary>
        /// <param name="email">Correo del usuario.</param>
        /// <returns><c>true</c> si se actualizó algún registro.</returns>
        Task<bool> ConfirmEmailAsync(string email);

        /// <summary>
        /// Elimina todos los usuarios que no han confirmado su correo dentro del plazo configurado.
        /// </summary>
        /// <returns>Número de usuarios eliminados.</returns>
        Task<int> DeleteUnconfirmedAsync();

        /// <summary>
        /// Actualiza la contraseña de un usuario.
        /// </summary>
        /// <param name="user">Usuario objetivo.</param>
        /// <param name="newPassword">Nueva contraseña.</param>
        /// <returns><c>true</c> si se actualizó correctamente.</returns>
        Task<bool> UpdatePasswordAsync(User user, string newPassword);

        /// <summary>
        /// Actualiza los datos generales de un usuario (perfil).
        /// </summary>
        /// <param name="user">Usuario con la información modificada.</param>
        /// <returns><c>true</c> si se actualizó correctamente.</returns>
        Task<bool> UpdateProfileAsync(User user);

        /// <summary>
        /// Obtiene una lista paginada de usuarios para administración,
        /// junto con un diccionario de roles.
        /// </summary>
        /// <param name="search">Parámetros de búsqueda y paginación.</param>
        /// <returns>Tupla con usuarios, roles por usuario y total de registros.</returns>
        Task<(List<User> users, Dictionary<int, string> roles, int totalCount)> GetPagedForAdminAsync(AdminUserSearchParamsDTO search);

        /// <summary>
        /// Obtiene un usuario y su rol por ID.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Tupla con el usuario y el nombre del rol, o <c>null</c> si no existe.</returns>
        Task<(User user, string role)?> GetByIdWithRoleAsync(int userId);

        /// <summary>
        /// Cuenta cuántos usuarios tienen el rol de administrador.
        /// </summary>
        /// <returns>Número de administradores.</returns>
        Task<int> CountAdminsAsync();

        /// <summary>
        /// Actualiza el estado de un usuario (activo/bloqueado).
        /// </summary>
        /// <param name="user">Usuario objetivo.</param>
        /// <param name="status">Nuevo estado.</param>
        Task UpdateStatusAsync(User user, UserStatus status);

        /// <summary>
        /// Cambia el rol asignado a un usuario.
        /// </summary>
        /// <param name="user">Usuario objetivo.</param>
        /// <param name="roleName">Nombre del nuevo rol.</param>
        Task UpdateRoleAsync(User user, string roleName);

        /// <summary>
        /// Actualiza el campo de último inicio de sesión del usuario.
        /// </summary>
        /// <param name="user">Usuario a actualizar.</param>
        Task UpdateLastLoginAtAsync(User user);
    }
}
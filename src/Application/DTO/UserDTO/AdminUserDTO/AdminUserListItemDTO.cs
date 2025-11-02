
namespace Tienda.src.Application.DTO.AdminUserDTO
{
    /// <summary>
    /// DTO resumido para listar usuarios en la vista de administración.
    /// </summary>
    public class AdminUserListItemDTO
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public required string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        public required string LastName { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Rol actual del usuario.
        /// </summary>
        public required string Role { get; set; }

        /// <summary>
        /// Estado del usuario (active, blocked).
        /// </summary>
        public required string Status { get; set; }

        /// <summary>
        /// Fecha en que el usuario se registró.
        /// </summary>
        public DateTime RegisteredAt { get; set; }

        /// <summary>
        /// Fecha del último inicio de sesión (si existe).
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }
}

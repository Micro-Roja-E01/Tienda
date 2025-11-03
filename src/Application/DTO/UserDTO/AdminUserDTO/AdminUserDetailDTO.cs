namespace Tienda.src.Application.DTO.AdminUserDTO
{
    /// <summary>
    /// DTO que representa el detalle completo de un usuario visto desde el panel de administración.
    /// Incluye datos personales, estado y fechas relevantes.
    /// </summary>
    public class AdminUserDetailDTO
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
        /// RUT del usuario.
        /// </summary>
        public required string Rut { get; set; }

        /// <summary>
        /// Rol actual del usuario (por ejemplo: Admin, Cliente).
        /// </summary>
        public required string Role { get; set; }

        /// <summary>
        /// Estado actual del usuario (por ejemplo: active, blocked).
        /// </summary>
        public required string Status { get; set; }

        /// <summary>
        /// Número de teléfono del usuario (opcional).
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Fecha en que el usuario se registró en la plataforma.
        /// </summary>
        public DateTime RegisteredAt { get; set; }

        /// <summary>
        /// Fecha de última actualización de los datos del usuario.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Fecha del último inicio de sesión del usuario (si existe).
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }
}
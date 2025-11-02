using Microsoft.AspNetCore.Identity;

namespace Tienda.src.Application.Domain.Models
{
    /// <summary>
    /// Representa el género de un usuario.
    /// </summary>
    public enum Gender
    {
        Masculino,
        Femenino,
        Otro,
    }

    /// <summary>
    /// Estado actual del usuario dentro del sistema (flujo 9).
    /// </summary>
    public enum UserStatus
    {
        Active,
        Blocked
    }

    /// <summary>
    /// Representa un usuario registrado en el sistema.
    /// Hereda de IdentityUser con tipo de clave entera.
    /// </summary>
    public class User : IdentityUser<int>
    {
        /// <summary>
        /// Rol Único Tributario del usuario (identificación chilena).
        /// </summary>
        public required string Rut { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public required string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        public required string LastName { get; set; }

        /// <summary>
        /// Género del usuario.
        /// </summary>
        public required Gender Gender { get; set; }

        /// <summary>
        /// Fecha de nacimiento del usuario.
        /// </summary>
        public required DateTime BirthDate { get; set; }

        /// <summary>
        /// Fecha de registro en la plataforma.
        /// </summary>
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización de datos del usuario.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Estado del usuario (activo o bloqueado).
        /// </summary>
        public UserStatus Status { get; set; } = UserStatus.Active;

        /// <summary>
        /// Fecha y hora del último inicio de sesión.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Lista de códigos de verificación asociados al usuario.
        /// </summary>
        public ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.AdminUserDTO
{
    /// <summary>
    /// DTO utilizado por el administrador para cambiar el estado de un usuario.
    /// </summary>
    public class UpdateUserStatusDTO
    {
        /// <summary>
        /// Nuevo estado del usuario. Debe ser "active" o "blocked".
        /// </summary>
        [Required]
        [RegularExpression("^(active|blocked)$", ErrorMessage = "El estado debe ser 'active' o 'blocked'.")]
        public required string Status { get; set; }

        /// <summary>
        /// Motivo del cambio de estado (opcional).
        /// </summary>
        public string? Reason { get; set; }
    }
}

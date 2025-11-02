using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.AdminUserDTO
{
    /// <summary>
    /// DTO utilizado por el administrador para cambiar el rol de un usuario.
    /// </summary>
    public class UpdateUserRoleDTO
    {
        /// <summary>
        /// Nuevo rol del usuario. Debe ser "Admin" o "Cliente".
        /// </summary>
        [Required]
        [RegularExpression("^(Admin|Cliente)$", ErrorMessage = "El rol debe ser 'Admin' o 'Cliente'.")]
        public required string Role { get; set; }

        /// <summary>
        /// Motivo del cambio de rol (opcional, útil para auditoría).
        /// </summary>
        public string? Reason { get; set; }
    }
}

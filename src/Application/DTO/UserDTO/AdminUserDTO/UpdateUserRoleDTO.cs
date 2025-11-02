
using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.AdminUserDTO
{
    public class UpdateUserRoleDTO
    {
        [Required]
        [RegularExpression("^(Admin|Cliente)$", ErrorMessage = "El rol debe ser 'Admin' o 'Cliente'.")]
        public required string Role { get; set; }

        public string? Reason { get; set; }
    }
}

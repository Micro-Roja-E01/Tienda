
using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.AdminUserDTO
{
    public class UpdateUserStatusDTO
    {
        [Required]
        [RegularExpression("^(active|blocked)$", ErrorMessage = "El estado debe ser 'active' o 'blocked'.")]
        public required string Status { get; set; }

        public string? Reason { get; set; }
    }
}

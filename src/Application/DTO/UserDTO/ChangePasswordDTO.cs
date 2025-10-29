using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.UserDTO
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        public required string CurrentPassword { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\[\]{};':""\\|,.<>/?-]).*$",
            ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial."
        )]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [MaxLength(20, ErrorMessage = "La contraseña no puede exceder los 20 caracteres.")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Debe confirmar la nueva contraseña.")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public required string ConfirmNewPassword { get; set; }
    }
}

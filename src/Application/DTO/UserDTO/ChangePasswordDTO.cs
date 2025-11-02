using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.UserDTO
{
    /// <summary>
    /// DTO utilizado por el usuario autenticado para cambiar su contraseña.
    /// </summary>
    public class ChangePasswordDTO
    {
        /// <summary>
        /// Contraseña actual del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        public required string CurrentPassword { get; set; }

        /// <summary>
        /// Nueva contraseña que se desea establecer.
        /// </summary>
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\[\]{};':""\\|,.<>/?-]).*$",
            ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial."
        )]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [MaxLength(20, ErrorMessage = "La contraseña no puede exceder los 20 caracteres.")]
        public required string NewPassword { get; set; }

        /// <summary>
        /// Confirmación de la nueva contraseña. Debe coincidir con <see cref="NewPassword"/>.
        /// </summary>
        [Required(ErrorMessage = "Debe confirmar la nueva contraseña.")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public required string ConfirmNewPassword { get; set; }
    }
}

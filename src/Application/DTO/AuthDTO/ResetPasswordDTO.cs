using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.AuthDTO
{
    /// <summary>
    /// DTO utilizado para restablecer la contraseña mediante código de recuperación.
    /// </summary>
    public class ResetPasswordDTO
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        public required string Email { get; set; }

        /// <summary>
        /// Código de recuperación enviado al correo electrónico.
        /// </summary>
        [Required(ErrorMessage = "El código de recuperación es obligatorio.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "El código de recuperación debe tener 6 dígitos.")]
        public required string RecoveryCode { get; set; }

        /// <summary>
        /// Nueva contraseña que reemplazará la anterior.
        /// </summary>
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\[\]{};':""\\|,.<>/?-]).*$",
            ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial."
        )]
        public required string NewPassword { get; set; }
    }
}
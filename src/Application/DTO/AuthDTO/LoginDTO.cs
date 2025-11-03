using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.AuthDTO
{
    /// <summary>
    /// DTO utilizado para iniciar sesión en el sistema.
    /// </summary>
    public class LoginDTO
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El campo Email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public required string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public required string Password { get; set; }

        /// <summary>
        /// Indica si el usuario desea mantener la sesión iniciada.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
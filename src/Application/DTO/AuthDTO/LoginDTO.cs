using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.AuthDTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "El campo Email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electronico no es valido.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public required string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
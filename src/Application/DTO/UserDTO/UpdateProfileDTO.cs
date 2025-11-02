using System.ComponentModel.DataAnnotations;
using Tienda.src.Application.Services.Validators;

namespace Tienda.src.Application.DTO.UserDTO
{
    /// <summary>
    /// DTO utilizado para que el usuario actualice su información de perfil.
    /// Aplica validaciones de nombre, rut, fecha de nacimiento y teléfono.
    /// </summary>
    public class UpdateProfileDTO
    {
        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$",
            ErrorMessage = "El Nombre solo puede contener carácteres del abecedario español."
        )]
        [MinLength(2, ErrorMessage = "El nombre debe tener mínimo 2 letras.")]
        [MaxLength(50, ErrorMessage = "El nombre debe tener máximo 50 letras.")]
        public required string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$",
            ErrorMessage = "El Apellido solo puede contener carácteres del abecedario español."
        )]
        [MinLength(2, ErrorMessage = "El apellido debe tener mínimo 2 letras.")]
        [MaxLength(50, ErrorMessage = "El apellido debe tener máximo 50 letras.")]
        public required string LastName { get; set; }

        /// <summary>
        /// Género del usuario. Debe ser Masculino, Femenino u Otro.
        /// </summary>
        [Required(ErrorMessage = "El género es obligatorio.")]
        [RegularExpression(
            @"^(Masculino|Femenino|Otro)$",
            ErrorMessage = "El género debe ser Masculino, Femenino u Otro."
        )]
        public required string Gender { get; set; }

        /// <summary>
        /// Fecha de nacimiento del usuario.
        /// </summary>
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [BirthDateValidation]
        public required DateTime BirthDate { get; set; }

        /// <summary>
        /// Número de teléfono chileno (9 dígitos, comienza con 9).
        /// </summary>
        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [RegularExpression(
            @"^9\d{8}$",
            ErrorMessage = "El número de teléfono debe tener 9 dígitos y comenzar con 9."
        )]
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// RUT del usuario en formato XXXXXXXX-X.
        /// </summary>
        [Required(ErrorMessage = "El campo RUT es obligatorio.")]
        [RegularExpression(
            @"^\d{7,8}-[0-9kK]$",
            ErrorMessage = "El Rut debe tener formato XXXXXXXX-X"
        )]
        [RutValidation(ErrorMessage = "El Rut no es válido.")]
        public required string Rut { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El campo Email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public required string Email { get; set; }
    }
}

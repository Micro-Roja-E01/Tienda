using System;
using System.ComponentModel.DataAnnotations;
using Tienda.src.Application.Services.Validators;

namespace Tienda.src.Application.DTO.UserDTO
{
    public class UpdateProfileDTO
    {
        // Nombre
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$",
            ErrorMessage = "El Nombre solo puede contener carácteres del abecedario español."
        )]
        [MinLength(2, ErrorMessage = "El nombre debe tener mínimo 2 letras.")]
        [MaxLength(50, ErrorMessage = "El nombre debe tener máximo 50 letras.")]
        public required string FirstName { get; set; }

        // Apellido
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$",
            ErrorMessage = "El Apellido solo puede contener carácteres del abecedario español."
        )]
        [MinLength(2, ErrorMessage = "El apellido debe tener mínimo 2 letras.")]
        [MaxLength(50, ErrorMessage = "El apellido debe tener máximo 50 letras.")]
        public required string LastName { get; set; }

        // Género
        [Required(ErrorMessage = "El género es obligatorio.")]
        [RegularExpression(
            @"^(Masculino|Femenino|Otro)$",
            ErrorMessage = "El género debe ser Masculino, Femenino u Otro."
        )]
        public required string Gender { get; set; }

        // Fecha de nacimiento
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [BirthDateValidation]
        public required DateTime BirthDate { get; set; }

        // Teléfono
        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [RegularExpression(
            @"^9\d{8}$",
            ErrorMessage = "El número de teléfono debe tener 9 dígitos y comenzar con 9."
        )]
        public required string PhoneNumber { get; set; }

        // RUT
        [Required(ErrorMessage = "El campo RUT es obligatorio.")]
        [RegularExpression(
            @"^\d{7,8}-[0-9kK]$",
            ErrorMessage = "El Rut debe tener formato XXXXXXXX-X"
        )]
        [RutValidation(ErrorMessage = "El Rut no es válido.")]
        public required string Rut { get; set; }

        // Email
        [Required(ErrorMessage = "El campo Email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public required string Email { get; set; }
    }
}

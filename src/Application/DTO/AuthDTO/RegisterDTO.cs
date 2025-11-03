using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.Services.Validators;

namespace Tienda.src.Application.DTO.AuthDTO
{
    public class RegisterDTO
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = "El campo Email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electronico no es valido.")]
        public required string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        /// <returns></returns>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\[\]{};':""\\|,.<>/?-]).*$",
            ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial."
        )]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [MaxLength(20, ErrorMessage = "La contraseña no puede exceder los 20 caracteres.")]
        public required string Password { get; set; }

        /// <summary>
        /// Confirmación de la contraseña del usuario.
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public required string ConfirmPassword { get; set; }

        /// <summary>
        /// RUT del usuario.
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = "El campo RUT es obligatorio.")]
        [RegularExpression(
            @"^\d{7,8}-[0-9kK]$",
            ErrorMessage = "El Rut debe tener formato XXXXXXXX-X"
        )]
        [RutValidation(ErrorMessage = "El Rut no es válido.")]
        public required string Rut { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        /// <value></value>
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
        /// <value></value>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$",
            ErrorMessage = "El Apellido solo puede contener carácteres del abecedario español."
        )]
        [MinLength(2, ErrorMessage = "El apellido debe tener mínimo 2 letras.")]
        [MaxLength(50, ErrorMessage = "El apellido debe tener máximo 50 letras.")]
        public required string LastName { get; set; }

        /// <summary>
        /// Fecha de nacimiento del usuario.
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [BirthDateValidation]
        public required DateTime BirthDate { get; set; }

        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [RegularExpression(
            @"^9\d{8}$",
            ErrorMessage = "El número de teléfono debe tener 9 dígitos y comenzar con 9."
        )]
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// Género del usuario.
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = "El género es obligatorio.")]
        [RegularExpression(
            @"^(Masculino|Femenino|Otro)$",
            ErrorMessage = "El género debe ser Masculino, Femenino u Otro."
        )]
        public required string Gender { get; set; }
    }
}
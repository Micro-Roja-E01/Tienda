using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.Application.Services.Validators
{
    public class BirthDateValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            string valueString = value?.ToString()!;
            if (!string.IsNullOrEmpty(valueString))
            {
                if (!DateTime.TryParse(valueString, out DateTime date))
                {
                    return new ValidationResult(
                        "El formato de la Fecha de Nacimiento no es valido."
                    );
                }
                if (date > DateTime.Today)
                {
                    return new ValidationResult("La fecha de nacimiento no puede ser futura.");
                }
                if (date < DateTime.Today.AddYears(-120))
                {
                    return new ValidationResult(
                        "La fecha de nacimiento no puede ser mayor a 120 años."
                    );
                }
                if (date > DateTime.Today.AddYears(-18))
                {
                    return new ValidationResult(
                        "La fecha de nacimiento debe ser de una persona mayor de edad."
                    );
                }
            }
            return ValidationResult.Success;
        }
    }
}
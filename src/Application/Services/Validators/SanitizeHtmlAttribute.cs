using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Tienda.src.Application.Services.Validators
{
    /// <summary>
    /// Atributo de validación que previene inyección de HTML/JavaScript.
    /// Sanitiza el contenido eliminando caracteres potencialmente peligrosos.
    /// </summary>
    public class SanitizeHtmlAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            string input = value.ToString()!;

            // Patrones peligrosos que debemos rechazar
            var dangerousPatterns = new[]
            {
                @"<script[\s\S]*?</script>",  // Script tags
                @"javascript:",                 // JavaScript protocol
                @"on\w+\s*=",                  // Event handlers (onclick, onerror, etc.)
                @"<iframe[\s\S]*?>",           // Iframes
                @"<object[\s\S]*?>",           // Objects
                @"<embed[\s\S]*?>",            // Embeds
                @"<link[\s\S]*?>",             // Link tags
                @"<meta[\s\S]*?>",             // Meta tags
                @"<style[\s\S]*?</style>",    // Style tags
            };

            foreach (var pattern in dangerousPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    return new ValidationResult(
                        $"El campo {validationContext.DisplayName} contiene contenido HTML no permitido. " +
                        "Por favor, elimine etiquetas HTML, scripts o código JavaScript.");
                }
            }

            // Validar caracteres permitidos (letras, números, espacios, puntuación básica)
            var allowedPattern = @"^[a-zA-Z0-9\s\-.,;:¿?¡!áéíóúÁÉÍÓÚñÑüÜ()\[\]""'/@\n\r]+$";

            if (!Regex.IsMatch(input, allowedPattern))
            {
                return new ValidationResult(
                    $"El campo {validationContext.DisplayName} contiene caracteres no permitidos.");
            }

            return ValidationResult.Success;
        }
    }
}
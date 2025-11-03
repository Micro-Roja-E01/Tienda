using Resend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.Application.Services.Implements
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailService(
            IResend resend,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _resend = resend;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Envía un código de verificación al correo electrónico del usuario.
        /// </summary>
        /// <param name="email">El correo electrónico del usuario.</param>
        /// <param name="code">El código de verificación a enviar.</param>
        public async Task SendVerificationCodeEmailAsync(string email, string code)
        {
            var htmlBody = await LoadTemplate("VerificationCode", code);

            var message = new EmailMessage
            {
                To = email,
                Subject =
                    _configuration["EmailConfiguration:VerificationSubject"]
                    ?? throw new ArgumentNullException(
                        "El asunto del correo de verificación no puede ser nulo."
                    ),
                From =
                    _configuration["EmailConfiguration:From"]
                    ?? throw new ArgumentNullException(
                        "La configuración de 'From' no puede ser nula."
                    ),
                HtmlBody = htmlBody,
            };

            Console.WriteLine("Sending email from: ");
            Console.WriteLine(_configuration["EmailConfiguration:From"]);

            await _resend.EmailSendAsync(message);
        }

        /// <summary>
        /// Envía un correo electrónico de bienvenida al usuario.
        /// </summary>
        /// <param name="email">El correo electrónico del usuario.</param>
        public async Task SendWelcomeEmailAsync(string email)
        {
            var htmlBody = await LoadTemplate("Welcome", null);

            var message = new EmailMessage
            {
                To = email,
                Subject =
                    _configuration["EmailConfiguration:WelcomeSubject"]
                    ?? throw new ArgumentNullException(
                        "El asunto del correo de bienvenida no puede ser nulo."
                    ),
                From =
                    _configuration["EmailConfiguration:From"]
                    ?? throw new ArgumentNullException(
                        "La configuración de 'From' no puede ser nula."
                    ),
                HtmlBody = htmlBody,
            };

            await _resend.EmailSendAsync(message);
        }

        /// <summary>
        /// Envía un correo con el código de recuperación de contraseña.
        /// </summary>
        /// <param name="email">Correo electrónico del usuario que solicitó la recuperación.</param>
        /// <param name="code">Código de recuperación que debe ingresar el usuario.</param>
        public async Task SendPasswordRecoveryEmailAsync(string email, string code)
        {
            var htmlBody = await LoadTemplate("PasswordRecoveryCode", code);

            var message = new EmailMessage
            {
                To = email,
                Subject =
                    // TODO: Cambiar asunto a codigo de recuperacion
                    _configuration["EmailConfiguration:VerificationSubject"]
                    ?? throw new ArgumentNullException(
                        "El asunto del correo de recuperación de contraseña no puede ser nulo."
                    ),
                From =
                    _configuration["EmailConfiguration:From"]
                    ?? throw new ArgumentNullException(
                        "La configuración de 'From' no puede ser nula."
                    ),
                HtmlBody = htmlBody,
            };

            await _resend.EmailSendAsync(message);
        }

        /// <summary>
        /// Envía un recordatorio por correo electrónico sobre el carrito abandonado.
        /// </summary>
        /// <param name="email">El correo electrónico del usuario.</param>
        /// <param name="userName">El nombre del usuario.</param>
        public async Task SendCartReminderEmailAsync(string email, string userName)
        {
            var htmlBody = await LoadCartReminderTemplate(userName);

            var message = new EmailMessage
            {
                To = email,
                Subject =
                    _configuration["EmailConfiguration:CartReminderSubject"]
                    ?? throw new ArgumentNullException(
                        "El asunto del correo de recordatorio de carrito no puede ser nulo."
                    ),
                From =
                    _configuration["EmailConfiguration:From"]
                    ?? throw new ArgumentNullException(
                        "La configuración de 'From' no puede ser nula."
                    ),
                HtmlBody = htmlBody,
            };

            await _resend.EmailSendAsync(message);
        }

        /// <summary>
        /// Carga una plantilla de correo electrónico desde el sistema de archivos y reemplaza el marcador de código.
        /// </summary>
        /// <param name="templateName">El nombre de la plantilla sin extensión.</param>
        /// <param name="code">El código a insertar en la plantilla.</param>
        /// <returns>El contenido HTML de la plantilla con el código reemplazado.</returns
        private async Task<string> LoadTemplate(string templateName, string? code)
        {
            var templatePath = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                "src",
                "Application",
                "Templates",
                "Email",
                $"{templateName}.html"
            );
            var html = await File.ReadAllTextAsync(templatePath);
            return html.Replace("{{CODE}}", code);
        }

        /// <summary>
        /// Carga la plantilla de recordatorio de carrito y reemplaza el nombre del usuario.
        /// </summary>
        /// <param name="userName">El nombre del usuario.</param>
        /// <returns>El contenido HTML de la plantilla con el nombre reemplazado.</returns>
        private async Task<string> LoadCartReminderTemplate(string userName)
        {
            var templatePath = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                "src",
                "Application",
                "Templates",
                "Email",
                "CartReminder.html"
            );
            var html = await File.ReadAllTextAsync(templatePath);
            return html.Replace("{{USER_NAME}}", userName);
        }
    }
}
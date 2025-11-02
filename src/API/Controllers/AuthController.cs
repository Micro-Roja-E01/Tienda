using Microsoft.AspNetCore.Mvc;
using Serilog;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.AuthDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    /// <summary>
    /// Controlador responsable de la autenticación de usuarios:
    /// login, registro, verificación de correo y recuperación de contraseña.
    /// Expone los endpoints del Flujo 1 de la rúbrica (auth).
    /// </summary>
    public class AuthController(IUserService userService) : BaseController
    {
        private readonly IUserService _userService = userService;

        private readonly ICartService _cartService = cartService;

        /// <summary>
        /// Inicia sesión con las credenciales indicadas y retorna el token JWT.
        /// </summary>
        /// <param name="loginDTO">Credenciales del usuario (email y contraseña).</param>
        /// <returns>Token de autenticación y mensaje de éxito.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var (token, userId) = await _userService.LoginAsync(loginDTO, HttpContext);
            var buyerId = HttpContext.Items["BuyerId"]?.ToString();
            // TODO: Actualizar buyerId
            if (!string.IsNullOrEmpty(buyerId))
            {
                Log.Information("BuyerId encontrado en el contexto: {BuyerId}", buyerId);
                await _cartService.AssociateWithUserAsync(buyerId, userId);
                Log.Information("Carrito con buyerId {BuyerId} asociado al usuario con ID: {UserId}", buyerId, userId);
            }
            return Ok(new GenericResponse<string>("Inicio de sesión exitoso", token));
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// Según la implementación, puede dejar al usuario en estado pendiente de verificación.
        /// </summary>
        /// <param name="registerDTO">Datos del usuario a registrar.</param>
        /// <returns>Mensaje indicando que el registro fue exitoso.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var result = await _userService.RegisterAsync(registerDTO, HttpContext);
            return Ok(new GenericResponse<string>("Registro exitoso", result));
        }

        /// <summary>
        /// Verifica el correo electrónico de un usuario usando el código enviado por email.
        /// </summary>
        /// <param name="verifyEmailDTO">DTO con email y código de verificación.</param>
        /// <returns>Mensaje de confirmación si el código es válido.</returns>
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO verifyEmailDTO)
        {
            var result = await _userService.VerifyEmailAsync(verifyEmailDTO);
            return Ok(new GenericResponse<string>("Correo verificado exitosamente", result));
        }

        /// <summary>
        /// Reenvía el código de verificación de correo a un usuario que todavía no lo ha validado.
        /// </summary>
        /// <param name="resendEmailVerificationCodeDTO">DTO con el correo del usuario.</param>
        /// <returns>Mensaje indicando que el código fue reenviado.</returns>
        [HttpPost("resend-email-verification-code")]
        public async Task<IActionResult> ResendEmailVerificationCode(
            [FromBody] ResendEmailVerificationCodeDTO resendEmailVerificationCodeDTO
        )
        {
            var message = await _userService.ResendEmailVerificationCodeAsync(
                resendEmailVerificationCodeDTO
            );
            return Ok(
                new GenericResponse<string>(
                    "Código de verificación reenviado exitosamente",
                    message
                )
            );
        }

        /// <summary>
        /// Inicia el flujo de recuperación de contraseña enviando un código al correo del usuario.
        /// </summary>
        /// <param name="recoverPasswordDTO">DTO con el correo a recuperar.</param>
        /// <returns>Mensaje indicando que el código fue enviado.</returns>
        [HttpPost("recover-password")]
        public async Task<IActionResult> RecoverPassword(
            [FromBody] RecoverPasswordDTO recoverPasswordDTO
        )
        {
            var result = await _userService.RecoverPasswordAsync(recoverPasswordDTO);
            return Ok(new GenericResponse<string>("Recuperación de contraseña exitosa", result));
        }

        /// <summary>
        /// Completa el flujo de recuperación de contraseña usando el código enviado previamente.
        /// </summary>
        /// <param name="resetPasswordDTO">DTO con email, código y nueva contraseña.</param>
        /// <returns>Mensaje de confirmación del restablecimiento.</returns>
        [HttpPatch("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            var result = await _userService.ResetPasswordAsync(resetPasswordDTO);
            return Ok(
                new GenericResponse<string>("Restablecimiento de contraseña exitoso", result)
            );
        }
    }
}
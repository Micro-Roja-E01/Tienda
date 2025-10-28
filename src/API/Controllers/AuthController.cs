using Microsoft.AspNetCore.Mvc;
using Serilog;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.AuthDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    public class AuthController(IUserService userService, ICartService cartService) : BaseController
    {
        private readonly IUserService _userService = userService;

        private readonly ICartService _cartService = cartService;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            Log.Information("Intento de inicio de sesión para el usuario: {Email}", loginDTO.Email);
            var (token, userId) = await _userService.LoginAsync(loginDTO, HttpContext);
            return Ok(new GenericResponse<string>("Inicio de sesión exitoso", token));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var result = await _userService.RegisterAsync(registerDTO, HttpContext);
            return Ok(new GenericResponse<string>("Registro exitoso", result));
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO verifyEmailDTO)
        {
            var result = await _userService.VerifyEmailAsync(verifyEmailDTO);
            return Ok(new GenericResponse<string>("Correo verificado exitosamente", result));
        }

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

        [HttpPost("recover-password")]
        public async Task<IActionResult> RecoverPassword(
            [FromBody] RecoverPasswordDTO recoverPasswordDTO
        )
        {
            var result = await _userService.RecoverPasswordAsync(recoverPasswordDTO);
            return Ok(new GenericResponse<string>("Recuperación de contraseña exitosa", result));
        }

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
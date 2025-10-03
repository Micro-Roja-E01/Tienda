using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.AuthDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    public class AuthController(IUserService userService) : BaseController
    {
        private readonly IUserService _userService = userService;

        //private readonly ICartService _cartService = cartService;

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

        [HttpPost("verify-email")]
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
    }
}
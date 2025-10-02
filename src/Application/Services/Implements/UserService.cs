using Mapster;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.AuthDTO;
using Tienda.src.Application.Services.Interfaces;
using Tienda.src.Infrastructure.Repositories.Interfaces;

namespace Tienda.src.Application.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;

        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        private readonly IVerificationCodeRepository _verificationCodeRepository;
        private readonly int _verificationCodeExpirationTimeInMinutes;

        public UserService(
            ITokenService tokenService,
            IUserRepository userRepository,
            IEmailService emailService,
            IVerificationCodeRepository verificationCodeRepository,
            IConfiguration configuration
        )
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
            _emailService = emailService;
            _verificationCodeRepository = verificationCodeRepository;
            _configuration = configuration;
            _verificationCodeExpirationTimeInMinutes = _configuration.GetValue<int>(
                "VerificationCode:ExpirationTimeInMinutes"
            );
        }

        public Task<int> DeleteUnconfirmedAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<(string token, int userId)> LoginAsync(
            LoginDTO loginDTO,
            HttpContext httpContext
        )
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var user = await _userRepository.GetByEmailAsync(loginDTO.Email);
            if (user == null)
            {
                Log.Warning(
                    $"Intento de inicio de sesion fallido para el email {loginDTO.Email} desde la IP {ipAddress}. - Usuario no encontrado"
                );
                throw new UnauthorizedAccessException("Credenciales invalidas.");
            }
            if (!user.EmailConfirmed)
            {
                Log.Warning(
                    $"Intento de inicio de sesion fallido para el email {loginDTO.Email} desde la IP {ipAddress} - Email no confirmado"
                );
                throw new InvalidOperationException("El email no ha sido confirmado.");
            }
            var result = await _userRepository.CheckPasswordAsync(user, loginDTO.Password);
            if (!result)
            {
                Log.Warning(
                    $"Intento de inicio de sesión fallido para el usuario: {loginDTO.Email} desde la IP: {ipAddress}"
                );
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }
            string roleName = await _userRepository.GetUserRoleAsync(user);
            Log.Information(
                $"Inicio de sesión exitoso para el usuario: {loginDTO.Email} desde la IP: {ipAddress}"
            );
            var token = _tokenService.GenerateToken(user, roleName, loginDTO.RememberMe);
            return (token, user.Id);
        }

        public async Task<string> RegisterAsync(RegisterDTO registerDTO, HttpContext httpContext)
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "IP desconocida";
            Log.Information(
                $"Intento de registro de nuevo usuario: {registerDTO.Email} desde la IP: {ipAddress}"
            );

            bool isRegistered = await _userRepository.ExistsByEmailAsync(registerDTO.Email);
            if (isRegistered)
            {
                Log.Warning($"El usuario con el correo {registerDTO.Email} ya está registrado.");
                throw new InvalidOperationException("El usuario ya está registrado.");
            }
            isRegistered = await _userRepository.ExistsByRutAsync(registerDTO.Rut);
            if (isRegistered)
            {
                Log.Warning($"El usuario con el RUT {registerDTO.Rut} ya está registrado.");
                throw new InvalidOperationException("El RUT ya está registrado.");
            }
            var user = registerDTO.Adapt<User>();
            var result = await _userRepository.CreateAsync(user, registerDTO.Password);
            if (!result)
            {
                Log.Warning($"Error al registrar el usuario: {registerDTO.Email}");
                throw new Exception("Error al registrar el usuario.");
            }
            Log.Information(
                $"Registro exitoso para el usuario: {registerDTO.Email} desde la IP: {ipAddress}"
            );
            string code = new Random().Next(100000, 999999).ToString();
            var verificationCode = new VerificationCode
            {
                UserId = user.Id,
                Code = code,
                CodeType = CodeType.EmailVerification,
                ExpiryDate = DateTime.UtcNow.AddMinutes(_verificationCodeExpirationTimeInMinutes),
            };
            var createdVerificationCode = await _verificationCodeRepository.CreateAsync(
                verificationCode
            );
            Log.Information(
                $"Código de verificación generado para el usuario: {registerDTO.Email} - Código: {createdVerificationCode.Code}"
            );

            await _emailService.SendVerificationCodeEmailAsync(
                registerDTO.Email,
                createdVerificationCode.Code
            );
            Log.Information(
                $"Se ha enviado un código de verificación al correo electrónico: {registerDTO.Email}"
            );
            return "Se ha enviado un código de verificación a su correo electrónico.";
        }

        public Task<string> ResendEmailVerificationCodeAsync(
            ResendEmailVerificationCodeDTO resendEmailVerificationCodeDTO
        )
        {
            throw new NotImplementedException();
        }

        public Task<string> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO)
        {
            throw new NotImplementedException();
        }
    }
}
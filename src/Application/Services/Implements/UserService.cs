using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.DTO.AuthDTO;
using Tienda.src.Application.Services.Interfaces;
using Tienda.src.Infrastructure.Repositories.Interfaces;

namespace Tienda.src.Application.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;

        //private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        //private readonly IVerificationCodeRepository _verificationCodeRepository;
        private readonly int _verificationCodeExpirationTimeInMinutes;

        public UserService(
            ITokenService tokenService,
            IUserRepository userRepository,
            //IEmailService emailService,
            //IVerificationCodeRepository verificationCodeRepository,
            IConfiguration configuration
        )
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
            //_emailService = emailService;
            //_verificationCodeRepository = verificationCodeRepository;
            _configuration = configuration;
            _verificationCodeExpirationTimeInMinutes = _configuration.GetValue<int>(
                "VerificationCode:ExpirationTimeInMinutes"
            );
        }

        public Task<int> DeleteUnconfirmedAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<string> LoginAsync(LoginDTO loginDTO, HttpContext httpContext)
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
            return _tokenService.GenerateToken(user, roleName, loginDTO.RememberMe);
        }

        public Task<string> RegisterAsync(RegisterDTO registerDTO, HttpContext httpContext)
        {
            throw new NotImplementedException();
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

        Task<(string token, int userId)> IUserService.LoginAsync(
            LoginDTO loginDTO,
            HttpContext httpContext
        )
        {
            throw new NotImplementedException();
        }
    }
}
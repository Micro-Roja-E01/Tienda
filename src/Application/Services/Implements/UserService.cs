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
            // Por alguna razon, mapea todo menos el username.
            user.UserName = registerDTO.Email;
            var result = await _userRepository.CreateAsync(user, registerDTO.Password);
            if (!result)
            {
                Log.Warning($"Error al registrar el usuario: {registerDTO.Email}");
                throw new InvalidOperationException("Error al registrar el usuario.");
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

        public async Task<string> ResendEmailVerificationCodeAsync(
            ResendEmailVerificationCodeDTO resendEmailVerificationCodeDTO
        )
        {
            var currentTime = DateTime.UtcNow;
            User? user = await _userRepository.GetByEmailAsync(
                resendEmailVerificationCodeDTO.Email
            );
            if (user == null)
            {
                Log.Warning(
                    $"El usuario con el correo {resendEmailVerificationCodeDTO.Email} no existe."
                );
                throw new KeyNotFoundException("El usuario no existe.");
            }
            if (user.EmailConfirmed)
            {
                Log.Warning(
                    $"El usuario con el correo {resendEmailVerificationCodeDTO.Email} ya ha verificado su correo electrónico."
                );
                throw new InvalidOperationException("El correo electrónico ya ha sido verificado.");
            }
            VerificationCode? verificationCode =
                await _verificationCodeRepository.GetLatestByUserIdAsync(
                    user.Id,
                    CodeType.EmailVerification
                );
            var expirationTime = verificationCode!.CreatedAt.AddMinutes(
                _verificationCodeExpirationTimeInMinutes
            );
            if (expirationTime > currentTime)
            {
                int remainingSeconds = (int)(expirationTime - currentTime).TotalSeconds;
                Log.Warning(
                    $"El usuario {resendEmailVerificationCodeDTO.Email} ha solicitado un reenvío del código de verificación antes de los {_verificationCodeExpirationTimeInMinutes} minutos."
                );
                throw new TimeoutException(
                    $"Debe esperar {remainingSeconds} segundos para solicitar un nuevo código de verificación."
                );
            }
            string newCode = new Random().Next(100000, 999999).ToString();
            verificationCode.Code = newCode;
            verificationCode.ExpiryDate = DateTime.UtcNow.AddMinutes(
                _verificationCodeExpirationTimeInMinutes
            );
            await _verificationCodeRepository.UpdateAsync(verificationCode);
            Log.Information(
                $"Nuevo código de verificación generado para el usuario: {resendEmailVerificationCodeDTO.Email} - Código: {newCode}"
            );
            await _emailService.SendVerificationCodeEmailAsync(user.Email!, newCode);
            Log.Information(
                $"Se ha reenviado un nuevo código de verificación al correo electrónico: {resendEmailVerificationCodeDTO.Email}"
            );
            return "Se ha reenviado un nuevo código de verificación a su correo electrónico.";
        }

        public async Task<string> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO)
        {
            User? user = await _userRepository.GetByEmailAsync(verifyEmailDTO.Email);
            if (user == null)
            {
                Log.Warning($"El usuario con el correo {verifyEmailDTO.Email} no existe.");
                throw new KeyNotFoundException("El usuario no existe.");
            }
            if (user.EmailConfirmed)
            {
                Log.Warning(
                    $"El usuario con el correo {verifyEmailDTO.Email} ya ha verificado su correo electrónico."
                );
                throw new InvalidOperationException("El correo electrónico ya ha sido verificado.");
            }
            CodeType codeType = CodeType.EmailVerification;

            VerificationCode? verificationCode =
                await _verificationCodeRepository.GetLatestByUserIdAsync(user.Id, codeType);
            if (verificationCode == null)
            {
                Log.Warning(
                    $"No se encontró un código de verificación para el usuario: {verifyEmailDTO.Email}"
                );
                throw new KeyNotFoundException("El código de verificación no existe.");
            }
            if (
                verificationCode.Code != verifyEmailDTO.VerificationCode
                || DateTime.UtcNow >= verificationCode.ExpiryDate
            )
            {
                int attempsCountUpdated = await _verificationCodeRepository.IncreaseAttemptsAsync(
                    user.Id,
                    codeType
                );
                Log.Warning(
                    $"Código de verificación incorrecto o expirado para el usuario: {verifyEmailDTO.Email}. Intentos actuales: {attempsCountUpdated}"
                );
                if (attempsCountUpdated >= 5)
                {
                    Log.Warning(
                        $"Se ha alcanzado el límite de intentos para el usuario: {verifyEmailDTO.Email}"
                    );
                    bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(
                        user.Id,
                        codeType
                    );
                    if (codeDeleteResult)
                    {
                        Log.Warning(
                            $"Se ha eliminado el código de verificación para el usuario: {verifyEmailDTO.Email}"
                        );
                        bool userDeleteResult = await _userRepository.DeleteAsync(user.Id);
                        if (userDeleteResult)
                        {
                            Log.Warning($"Se ha eliminado el usuario: {verifyEmailDTO.Email}");
                            throw new ArgumentException(
                                "Se ha alcanzado el límite de intentos. El usuario ha sido eliminado."
                            );
                        }
                    }
                }
                if (DateTime.UtcNow >= verificationCode.ExpiryDate)
                {
                    Log.Warning(
                        $"El código de verificación ha expirado para el usuario: {verifyEmailDTO.Email}"
                    );
                    throw new ArgumentException("El código de verificación ha expirado.");
                }
                else
                {
                    Log.Warning(
                        $"El código de verificación es incorrecto para el usuario: {verifyEmailDTO.Email}"
                    );
                    throw new ArgumentException(
                        $"El código de verificación es incorrecto, quedan {5 - attempsCountUpdated} intentos."
                    );
                }
            }
            bool emailConfirmed = await _userRepository.ConfirmEmailAsync(user.Email!);
            if (emailConfirmed)
            {
                bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(
                    user.Id,
                    codeType
                );
                if (codeDeleteResult)
                {
                    Log.Warning(
                        $"Se ha eliminado el código de verificación para el usuario: {verifyEmailDTO.Email}"
                    );
                    await _emailService.SendWelcomeEmailAsync(user.Email!);
                    Log.Information(
                        $"El correo electrónico del usuario {verifyEmailDTO.Email} ha sido confirmado exitosamente."
                    );
                    return "!Ya puedes iniciar sesión y disfrutar de todos los beneficios de Tienda UCN!";
                }
                throw new Exception("Error al confirmar el correo electrónico.");
            }
            throw new Exception("Error al verificar el correo electrónico.");
        }
    }
}
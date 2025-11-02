using Mapster;
using Serilog;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.AuthDTO;
using Tienda.src.Application.Services.Interfaces;
using Tienda.src.Infrastructure.Repositories.Interfaces;
using Tienda.src.Application.DTO.UserDTO;
using Tienda.src.Application.DTO.AdminUserDTO;


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

        /// <summary>
        /// Elimina de la base de datos a los usuarios que no han confirmado su correo dentro del plazo configurado.
        /// </summary>
        /// <returns>Número de usuarios eliminados.</returns>
        public async Task<int> DeleteUnconfirmedAsync()
        {
            return await _userRepository.DeleteUnconfirmedAsync();
        }

        /// <summary>
        /// Inicia sesión con email y contraseña, valida estado del usuario y devuelve el JWT.
        /// </summary>
        /// <param name="loginDTO">Credenciales del usuario.</param>
        /// <param name="httpContext">Contexto HTTP para registrar IP y auditoría.</param>
        /// <returns>Tupla con el token JWT y el ID del usuario.</returns>
        /// <exception cref="UnauthorizedAccessException">Si las credenciales son inválidas.</exception>
        /// <exception cref="InvalidOperationException">Si el correo no ha sido confirmado.</exception>
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
            await _userRepository.UpdateLastLoginAtAsync(user);
            return (token, user.Id);
        }

        /// <summary>
        /// Registra un nuevo usuario, valida que el correo y el RUT no estén en uso
        /// y envía un código de verificación por correo.
        /// </summary>
        /// <param name="registerDTO">Datos para el registro.</param>
        /// <param name="httpContext">Contexto HTTP para registrar la IP de origen.</param>
        /// <returns>Mensaje indicando que se envió el código de verificación.</returns>
        /// <exception cref="InvalidOperationException">Si el correo o el RUT ya están registrados.</exception>
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
            user.UserName ??= registerDTO.Email;
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

        /// <summary>
        /// Reenvía el último código de verificación de correo siempre que no haya expirado el tiempo mínimo.
        /// </summary>
        /// <param name="resendEmailVerificationCodeDTO">Correo al que se reenviará el código.</param>
        /// <returns>Mensaje de confirmación.</returns>
        /// <exception cref="KeyNotFoundException">Si el usuario no existe.</exception>
        /// <exception cref="InvalidOperationException">Si el correo ya estaba verificado.</exception>
        /// <exception cref="TimeoutException">Si aún no ha pasado el tiempo mínimo para reenviar.</exception>
        public async Task<string> ResendEmailVerificationCodeAsync(ResendEmailVerificationCodeDTO resendEmailVerificationCodeDTO)
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

        /// <summary>
        /// Verifica el correo electrónico de un usuario comparando el código enviado.
        /// Si el código expira o se ingresa mal varias veces, puede eliminarse al usuario.
        /// </summary>
        /// <param name="verifyEmailDTO">Correo y código de verificación.</param>
        /// <returns>Mensaje de éxito.</returns>
        /// <exception cref="KeyNotFoundException">Si el usuario o el código no existen.</exception>
        /// <exception cref="ArgumentException">Si el código es incorrecto o expiró.</exception>
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

         /// <summary>
        /// Inicia el flujo de recuperación de contraseña generando y enviando un código al correo.
        /// </summary>
        /// <param name="recoverPasswordDTO">Correo del usuario.</param>
        /// <returns>Mensaje indicando que se envió el código.</returns>
        /// <exception cref="InvalidOperationException">Si el usuario no existe.</exception>
        public async Task<string> RecoverPasswordAsync(RecoverPasswordDTO recoverPasswordDTO)
        {
            Log.Information(
                $"Iniciando proceso de recuperación de contraseña para: {recoverPasswordDTO.Email}"
            );
            var user = await _userRepository.GetByEmailAsync(recoverPasswordDTO.Email);
            if (user == null)
            {
                Log.Warning($"El usuario con el correo {recoverPasswordDTO.Email} no existe.");
                throw new InvalidOperationException("El usuario no existe.");
            }

            // Generar un nuevo código de verificación para la recuperación de contraseña
            string code = new Random().Next(100000, 999999).ToString();
            var verificationCode = new VerificationCode
            {
                UserId = user.Id,
                Code = code,
                CodeType = CodeType.PasswordChange,
                ExpiryDate = DateTime.UtcNow.AddMinutes(_verificationCodeExpirationTimeInMinutes),
            };
            var createdVerificationCode = await _verificationCodeRepository.CreateAsync(
                verificationCode
            );
            Log.Information(
                $"Código de recuperación de contraseña generado para el usuario: {recoverPasswordDTO.Email} - Código: {createdVerificationCode.Code}"
            );

            // Enviar el código de recuperación por correo electrónico
            await _emailService.SendPasswordRecoveryEmailAsync(
                recoverPasswordDTO.Email,
                createdVerificationCode.Code
            );
            Log.Information(
                $"Se ha enviado un código de recuperación de contraseña al correo electrónico: {recoverPasswordDTO.Email}"
            );
            return "Se ha enviado un código de recuperación de contraseña a su correo electrónico.";
        }

        /// <summary>
        /// Restablece la contraseña de un usuario validando el código de recuperación y el número de intentos.
        /// </summary>
        /// <param name="resetPasswordDTO">Correo, código y nueva contraseña.</param>
        /// <returns>Mensaje de éxito.</returns>
        /// <exception cref="KeyNotFoundException">Si el usuario o el código no existen.</exception>
        /// <exception cref="ArgumentException">
        /// Si el código es inválido, expiró, o se superó el número máximo de intentos.
        /// </exception>
        public async Task<string> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {
            // 1. Buscar usuario por email
            var user = await _userRepository.GetByEmailAsync(resetPasswordDTO.Email);
            if (user == null)
            {
                Log.Warning("ResetPassword: usuario {Email} no existe", resetPasswordDTO.Email);
                throw new KeyNotFoundException("El usuario no existe.");
            }

            var codeType = CodeType.PasswordChange;

            // 2. Obtener el último código asociado a usuario
            var verificationCode = await _verificationCodeRepository.GetLatestByUserIdAsync(user.Id, codeType);
            if (verificationCode == null)
            {
                Log.Warning("ResetPassword: no hay código registrado para {Email}", resetPasswordDTO.Email);
                throw new KeyNotFoundException("El código de verificación no existe.");
            }

            bool codeMatch = verificationCode.Code == resetPasswordDTO.RecoveryCode;
            bool codeExpired = DateTime.UtcNow >= verificationCode.ExpiryDate;

            if (!codeMatch || codeExpired)
            {

                int attemptsCountUpdated = await _verificationCodeRepository.IncreaseAttemptsAsync(
                    user.Id,
                    codeType
                );

                Log.Warning(
                    "ResetPassword: código inválido/expirado para {Email}. Expirado={Expired} Intentos={Attempts}",
                    resetPasswordDTO.Email,
                    codeExpired,
                    attemptsCountUpdated
                );


                if (attemptsCountUpdated >= 5)
                {
                    Log.Warning(
                        "ResetPassword: límite de intentos alcanzado para {Email}. Eliminando usuario...",
                        resetPasswordDTO.Email
                    );

                    bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(
                        user.Id,
                        codeType
                    );

                    if (codeDeleteResult)
                    {
                        Log.Warning(
                            "ResetPassword: código eliminado para {Email} tras demasiados intentos",
                            resetPasswordDTO.Email
                        );

                        bool userDeleteResult = await _userRepository.DeleteAsync(user.Id);
                        if (userDeleteResult)
                        {
                            Log.Warning(
                                "ResetPassword: usuario {Email} eliminado tras demasiados intentos fallidos",
                                resetPasswordDTO.Email
                            );
                            throw new ArgumentException(
                                "Se alcanzó el límite de intentos fallidos. La cuenta ha sido eliminada."
                            );
                        }
                    }


                    throw new ArgumentException(
                        "Se alcanzó el límite de intentos fallidos. La cuenta ha sido bloqueada."
                    );
                }


                if (codeExpired)
                {
                    throw new ArgumentException("El código de verificación ha expirado.");
                }

                // código incorrecto pero no expirado
                int intentosRestantes = 5 - attemptsCountUpdated;
                throw new ArgumentException(
                    $"El código de verificación es incorrecto. Quedan {intentosRestantes} intentos."
                );
            }

            // 3. Código válido, actualizar la contraseña
            bool passwordReset = await _userRepository.UpdatePasswordAsync(
                user,
                resetPasswordDTO.NewPassword
            );

            if (!passwordReset)
            {
                Log.Error("ResetPassword: error al actualizar contraseña para {Email}", resetPasswordDTO.Email);
                throw new Exception("Error al restablecer la contraseña.");
            }

            // 4. Limpiar el código usado
            bool deleted = await _verificationCodeRepository.DeleteByUserIdAsync(user.Id, codeType);
            if (deleted)
            {
                Log.Information(
                    "ResetPassword: código eliminado tras uso correcto para {Email}",
                    resetPasswordDTO.Email
                );
            }

            Log.Information(
                "ResetPassword: contraseña restablecida exitosamente para {Email}",
                resetPasswordDTO.Email
            );

            // await _emailService.SendPasswordChangedConfirmationEmailAsync(user.Email!);

            return "¡La contraseña ha sido restablecida exitosamente!";
        }


        /// <summary>
        /// Obtiene el perfil del usuario autenticado en base a su identificador.
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Perfil del usuario.</returns>
        public async Task<UserProfileDTO> GetProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Usuario no encontrado.");

            Log.Information("Obteniendo perfil para usuario {UserId}", userId);

            var profile = new UserProfileDTO
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender.ToString(), // enum -> "Masculino"/"Femenino"/"Otro"
                BirthDate = user.BirthDate,
                Rut = user.Rut,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                RegisteredAt = user.RegisteredAt,
                UpdatedAt = user.UpdatedAt
            };

            return profile;
        }

        /// <summary>
        /// Actualiza los datos del perfil del usuario y valida que el nuevo email o RUT no estén en uso.
        /// Si cambia el email, se envía un código de verificación al nuevo correo.
        /// </summary>
        /// <param name="userId">Identificador del usuario que actualiza su perfil.</param>
        /// <param name="dto">Datos actualizados.</param>
        /// <returns>Perfil actualizado.</returns>
        /// <exception cref="InvalidOperationException">Si el RUT o el email ya están en uso.</exception>
        public async Task<UserProfileDTO> UpdateProfileAsync(int userId, UpdateProfileDTO dto)
        {
            // 1. Buscar el usuario actual
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Usuario no encontrado.");

            Log.Information("Usuario {UserId} solicita actualización de perfil", userId);

            // 2. Validar unicidad del RUT si cambió
            if (!string.Equals(user.Rut, dto.Rut, StringComparison.OrdinalIgnoreCase))
            {
                var rutEnUso = await _userRepository.RutExistsForOtherUserAsync(dto.Rut, userId);
                if (rutEnUso)
                {

                    throw new InvalidOperationException("El RUT ya está en uso por otro usuario.");
                }
            }

            // 3. Validar unicidad del Email si cambió
            bool emailCambio = !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase);
            if (emailCambio)
            {
                var emailEnUso = await _userRepository.EmailExistsForOtherUserAsync(dto.Email, userId);
                if (emailEnUso)
                {
                    throw new InvalidOperationException("El correo electrónico ya está en uso por otro usuario.");
                }


                string verificationCode = GenerarCodigoDe6Digitos();


                var expiryMinutes = _verificationCodeExpirationTimeInMinutes;

                var codeEntity = new VerificationCode
                {
                    UserId = user.Id,
                    Code = verificationCode,
                    CodeType = CodeType.EmailVerification,
                    AttemptCount = 0,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(expiryMinutes),
                    CreatedAt = DateTime.UtcNow,
                };

                await _verificationCodeRepository.CreateAsync(codeEntity);

                // Enviamos el código AL NUEVO EMAIL que quiere usar
                await _emailService.SendVerificationCodeEmailAsync(
                    dto.Email,
                    verificationCode
                );

                Log.Information(
                    "Usuario {UserId} solicitó cambio de email de {OldEmail} a {NewEmail}. Código enviado.",
                    userId,
                    user.Email,
                    dto.Email
                );



            }

            // 4. Actualizar campos normales del perfil
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;

            // Convertir string ("Masculino"/"Femenino"/"Otro") a enum Gender
            user.Gender = Enum.Parse<Gender>(dto.Gender, ignoreCase: true);

            user.BirthDate = dto.BirthDate;
            user.Rut = dto.Rut;
            user.PhoneNumber = dto.PhoneNumber;

            // Si el email NO cambió, se puede mantener sincronizado Email/UserName
            if (!emailCambio)
            {
                user.Email = dto.Email;
                user.UserName = dto.Email;
            }

            user.UpdatedAt = DateTime.UtcNow;

            // 5. Guardar cambios 
            await _userRepository.UpdateProfileAsync(user);

            Log.Information("Perfil de usuario {UserId} actualizado correctamente", userId);

            // 6. Respuesta de vuelta al cliente
            var response = new UserProfileDTO
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender.ToString(),
                BirthDate = user.BirthDate,
                Rut = user.Rut,
                Email = user.Email!, // si pidió cambiar email, este es el viejo hasta que verifique
                PhoneNumber = user.PhoneNumber!,
                RegisteredAt = user.RegisteredAt,
                UpdatedAt = user.UpdatedAt
            };

            return response;
        }

        /// <summary>
        /// Cambia la contraseña del usuario comprobando primero la contraseña actual.
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <param name="dto">Contraseña actual y nueva contraseña.</param>
        /// <param name="httpContext">Contexto HTTP para auditoría.</param>
        /// <exception cref="UnauthorizedAccessException">Si la contraseña actual no coincide.</exception>
        public async Task ChangePasswordAsync(int userId, ChangePasswordDTO dto, HttpContext httpContext)
        {
            // 1. Obtener el usuario
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Usuario no encontrado.");

            // 2. Validar contraseña actual
            var passOk = await _userRepository.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!passOk)
            {

                throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");
            }

            // 3. Actualizar contraseña 
            await _userRepository.UpdatePasswordAsync(user, dto.NewPassword);

            Log.Information(
                "Usuario {UserId} cambió su contraseña desde la IP {IpAddress}",
                userId,
                httpContext.Connection.RemoteIpAddress?.ToString()
            );

        }

        /// <summary>
        /// Obtiene una lista paginada de usuarios para el panel de administración,
        /// incluyendo su rol y estado.
        /// </summary>
        /// <param name="search">Parámetros de búsqueda (rol, estado, correo, fechas).</param>
        /// <returns>DTO paginado con usuarios.</returns>
        public async Task<PagedAdminUsersDTO> GetAllForAdminAsync(AdminUserSearchParamsDTO search)
        {
            var (users, rolesDict, totalCount) = await _userRepository.GetPagedForAdminAsync(search);

            var dtoList = users.Select(u => new AdminUserListItemDTO
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email!,
                Role = rolesDict.TryGetValue(u.Id, out var r) ? r : "Cliente",
                Status = u.Status.ToString(),
                RegisteredAt = u.RegisteredAt,
                LastLoginAt = u.LastLoginAt
            }).ToList();

            int totalPages = (int)Math.Ceiling((double)totalCount / search.PageSize);

            return new PagedAdminUsersDTO
            {
                Users = dtoList,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = search.Page,
                PageSize = search.PageSize
            };
        }

        /// <summary>
        /// Obtiene el detalle completo de un usuario específico para el panel de administración.
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <returns>Detalle del usuario.</returns>
        /// <exception cref="KeyNotFoundException">Si el usuario no existe.</exception>
        public async Task<AdminUserDetailDTO> GetByIdForAdminAsync(int userId)
        {
            var (user, role) = await _userRepository.GetByIdWithRoleAsync(userId)
                ?? throw new KeyNotFoundException("El usuario no existe.");

            return new AdminUserDetailDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Rut = user.Rut,
                Role = role,
                Status = user.Status.ToString(),
                PhoneNumber = user.PhoneNumber,
                RegisteredAt = user.RegisteredAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        /// <summary>
        /// Actualiza el estado (activo/bloqueado) de un usuario desde el panel de administración.
        /// Valida que no sea el mismo admin y que no se bloquee al último admin.
        /// </summary>
        /// <param name="targetUserId">Usuario objetivo.</param>
        /// <param name="adminId">Administrador que realiza la acción.</param>
        /// <param name="dto">Nuevo estado y motivo opcional.</param>
        /// <exception cref="InvalidOperationException">Si se intenta bloquearse a sí mismo o al último admin.</exception>
        public async Task UpdateStatusForAdminAsync(int targetUserId, int adminId, UpdateUserStatusDTO dto)
        {
            // 1. no me puedo bloquear a mí mismo
            if (targetUserId == adminId)
                throw new InvalidOperationException("No puedes cambiar tu propio estado.");

            // 2. obtener usuario
            var (user, role) = await _userRepository.GetByIdWithRoleAsync(targetUserId)
                ?? throw new KeyNotFoundException("El usuario no existe.");

            // 3. si es admin y lo quiero BLOQUEAR → verificar que no sea el único
            if (role == "Admin" && dto.Status == "blocked")
            {
                int adminCount = await _userRepository.CountAdminsAsync();
                if (adminCount <= 1)
                    throw new InvalidOperationException("No puedes bloquear al último administrador.");
            }

            // 4. aplicar estado
            var newStatus = dto.Status == "active" ? UserStatus.Active : UserStatus.Blocked;
            await _userRepository.UpdateStatusAsync(user, newStatus);


        }

        /// <summary>
        /// Actualiza el rol de un usuario (Admin/Cliente) desde el panel de administración.
        /// Valida que no se modifique el propio rol ni se deje al sistema sin administradores.
        /// </summary>
        /// <param name="targetUserId">Usuario objetivo.</param>
        /// <param name="adminId">Administrador que realiza la acción.</param>
        /// <param name="dto">Nuevo rol a asignar.</param>
        /// <exception cref="InvalidOperationException">Si se intenta modificar el propio rol o quitar el último admin.</exception>
        public async Task UpdateRoleForAdminAsync(int targetUserId, int adminId, UpdateUserRoleDTO dto)
        {
            // 1. no me puedo degradar a mí mismo
            if (targetUserId == adminId)
                throw new InvalidOperationException("No puedes modificar tu propio rol desde aquí.");

            var (user, currentRole) = await _userRepository.GetByIdWithRoleAsync(targetUserId)
                ?? throw new KeyNotFoundException("El usuario no existe.");

            // 2. si estoy quitando Admin → validar que no sea el último
            if (currentRole == "Admin" && dto.Role != "Admin")
            {
                int adminCount = await _userRepository.CountAdminsAsync();
                if (adminCount <= 1)
                    throw new InvalidOperationException("No puedes quitar el último administrador.");
            }

            // 3. aplicar nuevo rol
            await _userRepository.UpdateRoleAsync(user, dto.Role);



        }
        /// <summary>
        /// Genera un código aleatorio de 6 dígitos con ceros a la izquierda.
        /// </summary>
        /// <returns>Código de 6 dígitos en formato string.</returns>
        private static string GenerarCodigoDe6Digitos()
        {
            var random = new Random();
            int num = random.Next(0, 1000000);
            return num.ToString("D6");
        }
        


    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.UserDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // helper privado para sacar el userId del JWT
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("No se pudo determinar el usuario autenticado.");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("El identificador de usuario no es válido.");
            }

            return userId;
        }

        /// <summary>
        /// 2.1 Obtener perfil del usuario autenticado
        /// </summary>
        [HttpGet("profile")]
        [Authorize] // cualquier usuario logueado
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            Log.Information("GetProfile solicitado por UserId={UserId}", userId);

            var profile = await _userService.GetProfileAsync(userId);

            return Ok(profile);
        }

        /// <summary>
        /// 2.2 Editar datos del perfil (nombre, rut, teléfono, etc)
        /// Puede gatillar verificación de nuevo email.
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
        {
            var userId = GetCurrentUserId();
            Log.Information("UpdateProfile solicitado por UserId={UserId}", userId);

            var updatedProfile = await _userService.UpdateProfileAsync(userId, dto);

            return Ok(
                new GenericResponse<UserProfileDTO>(
                    "Perfil actualizado exitosamente (si cambiaste el correo, revisa tu bandeja para confirmarlo)",
                    updatedProfile
                )
            );
        }

        /// <summary>
        /// 2.3 Cambiar contraseña del usuario autenticado
        /// </summary>
        [HttpPatch("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var userId = GetCurrentUserId();
            Log.Information("ChangePassword solicitado por UserId={UserId}", userId);

            await _userService.ChangePasswordAsync(userId, dto, HttpContext);

            return Ok(
                new GenericResponse<object>(
                    "Contraseña actualizada exitosamente",
                    new
                    {
                        detail = "Se cerrarán las sesiones activas."
                    }
                )
            );
        }
    }
}

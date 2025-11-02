// Tienda.src.API.Controllers/AdminUserController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.AdminUserDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    /// <summary>
    /// Controlador que gestiona las operaciones administrativas sobre usuarios.
    /// Accesible únicamente por usuarios con rol "Admin".
    /// </summary>
    [ApiController]
    [Route("api")]
    public class AdminUserController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Inicializa una nueva instancia del controlador con el servicio de usuarios.
        /// </summary>
        public AdminUserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Obtiene el ID del administrador autenticado a partir del token JWT.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Si no se encuentra el claim del identificador.</exception>
        private int GetCurrentAdminId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) throw new UnauthorizedAccessException("No se pudo obtener el admin.");
            return int.Parse(claim.Value);
        }

        /// <summary>
        /// Devuelve un listado paginado de todos los usuarios registrados,
        /// accesible solo por administradores.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y filtrado (paginación, estado, rol, etc.).</param>
        /// <returns>Respuesta genérica con la lista paginada de usuarios.</returns>
        [HttpGet("admin/users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAsync([FromQuery] AdminUserSearchParamsDTO searchParams)
        {
            Log.Information("Admin solicitó listado de usuarios");
            var result = await _userService.GetAllForAdminAsync(searchParams);
            return Ok(new GenericResponse<PagedAdminUsersDTO>("Usuarios obtenidos exitosamente", result));
        }

        /// <summary>
        /// Obtiene el detalle completo de un usuario específico según su ID.
        /// </summary>
        /// <param name="id">Identificador del usuario.</param>
        /// <returns>Detalle del usuario solicitado.</returns>
        [HttpGet("admin/users/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _userService.GetByIdForAdminAsync(id);
            return Ok(new GenericResponse<AdminUserDetailDTO>("Usuario obtenido exitosamente", result));
        }

        /// <summary>
        /// Actualiza el estado (activo/bloqueado) de un usuario específico.
        /// </summary>
        /// <param name="id">ID del usuario a modificar.</param>
        /// <param name="dto">DTO con el nuevo estado.</param>
        /// <returns>Confirmación del cambio de estado.</returns>
        [HttpPatch("admin/users/{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatusAsync(int id, [FromBody] UpdateUserStatusDTO dto)
        {
            var adminId = GetCurrentAdminId();
            await _userService.UpdateStatusForAdminAsync(id, adminId, dto);
            return Ok(new GenericResponse<string>("Estado de usuario actualizado correctamente"));
        }

        /// <summary>
        /// Actualiza el rol de un usuario determinado (por ejemplo, de "Cliente" a "Admin").
        /// </summary>
        /// <param name="id">ID del usuario a actualizar.</param>
        /// <param name="dto">DTO con el nuevo rol asignado.</param>
        /// <returns>Confirmación del cambio de rol.</returns>
        [HttpPatch("admin/users/{id:int}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRoleAsync(int id, [FromBody] UpdateUserRoleDTO dto)
        {
            var adminId = GetCurrentAdminId();
            await _userService.UpdateRoleForAdminAsync(id, adminId, dto);
            return Ok(new GenericResponse<string>("Rol de usuario actualizado correctamente"));
        }
    }
}

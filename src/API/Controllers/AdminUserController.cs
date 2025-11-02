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
    [ApiController]
    [Route("api")]
    public class AdminUserController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminUserController(IUserService userService)
        {
            _userService = userService;
        }

        private int GetCurrentAdminId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) throw new UnauthorizedAccessException("No se pudo obtener el admin.");
            return int.Parse(claim.Value);
        }

        // GET /api/admin/users
        [HttpGet("admin/users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAsync([FromQuery] AdminUserSearchParamsDTO searchParams)
        {
            Log.Information("Admin solicitó listado de usuarios");
            var result = await _userService.GetAllForAdminAsync(searchParams);
            return Ok(new GenericResponse<PagedAdminUsersDTO>("Usuarios obtenidos exitosamente", result));
        }

        // GET /api/admin/users/{id}
        [HttpGet("admin/users/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _userService.GetByIdForAdminAsync(id);
            return Ok(new GenericResponse<AdminUserDetailDTO>("Usuario obtenido exitosamente", result));
        }

        // PATCH /api/admin/users/{id}/status
        [HttpPatch("admin/users/{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatusAsync(int id, [FromBody] UpdateUserStatusDTO dto)
        {
            var adminId = GetCurrentAdminId();
            await _userService.UpdateStatusForAdminAsync(id, adminId, dto);
            return Ok(new GenericResponse<string>("Estado de usuario actualizado correctamente"));
        }

        // PATCH /api/admin/users/{id:int}/role
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

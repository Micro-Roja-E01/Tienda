using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.API.Controllers;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.CategoryDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    /// <summary>
    /// Controlador para la administración de categorías.
    /// Implementa los endpoints del flujo 7.1 de la rúbrica (categorías, solo Admin).
    /// </summary>
    [ApiController]
    [Route("api")]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Lista todas las categorías en forma paginada para el panel admin.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda/paginación.</param>
        /// <returns>Listado paginado de categorías.</returns>
        // GET /api/admin/categories
        [HttpGet("admin/categories")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAsync([FromQuery] SearchParamsDTO searchParams)
        {
            var result = await _categoryService.GetAllAsync(searchParams);
            return Ok(new GenericResponse<PagedCategoriesDTO>(
                "Categorías obtenidas exitosamente",
                result
            ));
        }

        /// <summary>
        /// Obtiene el detalle de una categoría por su ID.
        /// </summary>
        /// <param name="id">ID de la categoría.</param>
        /// <returns>Detalle de la categoría o 404 si no existe.</returns>
        // GET /api/admin/categories/{id}
        [HttpGet("admin/categories/{id:int}", Name = "GetCategoryById")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category is null)
                return NotFound(new GenericResponse<string>($"No se encontró la categoría con ID {id}."));

            return Ok(new GenericResponse<CategoryDetailDTO>(
                "Categoría obtenida exitosamente",
                category
            ));
        }

        /// <summary>
        /// Crea una nueva categoría.
        /// </summary>
        /// <param name="dto">Datos de la categoría.</param>
        /// <returns>Categoría creada con su ubicación.</returns>
        // POST /api/admin/categories
        [HttpPost("admin/categories")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromBody] CategoryCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new GenericResponse<object>("Datos inválidos", ModelState));

            var created = await _categoryService.CreateAsync(dto);

            return CreatedAtRoute(
                "GetCategoryById",
                new { id = created.Id },
                new GenericResponse<CategoryDetailDTO>(
                    "Categoría creada exitosamente",
                    created
                )
            );
        }

        /// <summary>
        /// Actualiza los datos de una categoría existente.
        /// </summary>
        /// <param name="id">ID de la categoría a editar.</param>
        /// <param name="dto">Nuevos datos.</param>
        /// <returns>Categoría actualizada o 404.</returns>
        // PUT /api/admin/categories/{id}
        [HttpPut("admin/categories/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] CategoryUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new GenericResponse<object>("Datos inválidos", ModelState));

            var updated = await _categoryService.UpdateAsync(id, dto);
            if (updated is null)
                return NotFound(new GenericResponse<string>($"No se encontró la categoría con ID {id}."));

            return Ok(new GenericResponse<CategoryDetailDTO>(
                "Categoría actualizada exitosamente",
                updated
            ));
        }

        /// <summary>
        /// Elimina lógicamente una categoría o la marca como no disponible.
        /// Respeta la integridad: si tiene productos asociados, responde 409.
        /// </summary>
        /// <param name="id">ID de la categoría.</param>
        /// <returns>Mensaje de confirmación o conflicto.</returns>
        // DELETE /api/admin/categories/{id}
        [HttpDelete("admin/categories/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                return Ok(new GenericResponse<string>("Categoría eliminada (o marcada) exitosamente."));
            }
            catch (InvalidOperationException ex)
            {
                // aquí es donde el service detecta que hay productos asociados
                return Conflict(new GenericResponse<string>(ex.Message));
            }
        }
    }
}
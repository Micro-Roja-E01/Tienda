using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.API.Controllers;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.CategoryDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

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
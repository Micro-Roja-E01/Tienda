using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.BrandDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        // GET /api/admin/brands
        [HttpGet("admin/brands")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAsync([FromQuery] SearchParamsDTO searchParams)
        {
            var result = await _brandService.GetAllAsync(searchParams);
            return Ok(new GenericResponse<PagedBrandsDTO>(
                "Marcas obtenidas exitosamente",
                result
            ));
        }

        // GET /api/admin/brands/{id}
        [HttpGet("admin/brands/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var brand = await _brandService.GetByIdAsync(id);
            if (brand is null)
                return NotFound(new GenericResponse<string>($"No se encontró la marca con ID {id}."));

            return Ok(new GenericResponse<BrandDetailDTO>(
                "Marca obtenida exitosamente",
                brand
            ));
        }

        // POST /api/admin/brands
        [HttpPost("admin/brands")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromBody] BrandCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new GenericResponse<object>("Datos inválidos", ModelState));

            try
            {
                var created = await _brandService.CreateAsync(dto);

                
                return StatusCode(
                    StatusCodes.Status201Created,
                    new GenericResponse<BrandDetailDTO>(
                        "Marca creada exitosamente",
                        created
                    )
                );
            }
            catch (InvalidOperationException ex)
            {
                // nombre/slug duplicado
                return Conflict(new GenericResponse<string>(ex.Message));
            }
        }

        // PUT /api/admin/brands/{id}
        [HttpPut("admin/brands/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] BrandUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new GenericResponse<object>("Datos inválidos", ModelState));

            try
            {
                var updated = await _brandService.UpdateAsync(id, dto);
                if (updated is null)
                    return NotFound(new GenericResponse<string>($"No se encontró la marca con ID {id}."));

                return Ok(new GenericResponse<BrandDetailDTO>(
                    "Marca actualizada exitosamente",
                    updated
                ));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new GenericResponse<string>(ex.Message));
            }
        }

        // DELETE /api/admin/brands/{id}
        [HttpDelete("admin/brands/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                await _brandService.DeleteAsync(id);
                return Ok(new GenericResponse<string>("Marca eliminada (o deshabilitada) exitosamente."));
            }
            catch (InvalidOperationException ex)
            {
                // tiene productos asociados
                return Conflict(new GenericResponse<string>(ex.Message));
            }
        }
    }
}

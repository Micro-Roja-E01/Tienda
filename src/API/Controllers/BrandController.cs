using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.BrandDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Controllers
{
    /// <summary>
    /// Controlador para la administración de marcas.
    /// Todos los endpoints están protegidos para rol "Admin" según la rúbrica (flujo 7.2).
    /// </summary>
    [ApiController]
    [Route("api")]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        /// <summary>
        /// Obtiene el listado paginado de marcas para el panel de administración.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y paginación.</param>
        /// <returns>Listado paginado de marcas.</returns>
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

        /// <summary>
        /// Obtiene el detalle de una marca específica por su ID.
        /// </summary>
        /// <param name="id">ID de la marca.</param>
        /// <returns>Detalle de la marca o 404 si no existe.</returns>
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

        /// <summary>
        /// Crea una nueva marca.
        /// Valida nombre/slug duplicados en el servicio.
        /// </summary>
        /// <param name="dto">Datos de la marca a crear.</param>
        /// <returns>Marca creada con código 201.</returns>
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

        /// <summary>
        /// Actualiza los datos de una marca existente.
        /// </summary>
        /// <param name="id">ID de la marca a actualizar.</param>
        /// <param name="dto">Datos a modificar.</param>
        /// <returns>Marca actualizada o 404 si no existe.</returns>
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

        /// <summary>
        /// Elimina lógicamente una marca o la deshabilita,
        /// respetando las reglas de integridad (si tiene productos asociados se responde 409).
        /// </summary>
        /// <param name="id">ID de la marca a eliminar.</param>
        /// <returns>Mensaje de confirmación o conflicto.</returns>
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
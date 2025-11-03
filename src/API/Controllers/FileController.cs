using Microsoft.AspNetCore.Mvc;
using tienda.src.Application.Services.Interfaces;
using Tienda.src.API.Controllers;

namespace tienda.src.API.Controllers
{
    /// <summary>
    /// Controlador para manejar operaciones con archivos de imagen.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : BaseController
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Subir una imagen para un producto específico.
        /// </summary>
        /// <param name="file">El archivo de imagen a subir</param>
        /// <param name="productId">El ID del producto</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("upload/{productId}")]
        public async Task<IActionResult> UploadImage(IFormFile file, int productId)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No se proporcionó ningún archivo" });
                }

                if (productId <= 0)
                {
                    return BadRequest(new { message = "ProductId debe ser mayor a 0" });
                }

                var result = await _fileService.UploadAsync(file, productId);

                if (result)
                {
                    return Ok(new { message = "Imagen subida exitosamente", success = true });
                }
                else
                {
                    return BadRequest(new { message = "La imagen ya existe", success = false });
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message, success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", success = false, details = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar una imagen por su PublicId.
        /// </summary>
        /// <param name="publicId">El ID público de la imagen a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{publicId}")]
        public async Task<IActionResult> DeleteImage(string publicId)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                {
                    return BadRequest(new { message = "PublicId es requerido" });
                }

                var result = await _fileService.DeleteAsync(publicId);

                if (result)
                {
                    return Ok(new { message = "Imagen eliminada exitosamente", success = true });
                }
                else
                {
                    return NotFound(new { message = "La imagen no existe", success = false });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", success = false, details = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint de prueba para verificar que el controlador esté funcionando.
        /// </summary>
        /// <returns>Mensaje de confirmación</returns>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "FileController está funcionando correctamente", timestamp = DateTime.UtcNow });
        }
    }
}
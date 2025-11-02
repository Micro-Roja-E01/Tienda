using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using tienda.src.Application.DTO.ProductDTO;
using tienda.src.Application.DTO.ProductDTO.AdminDTO;
using tienda.src.Application.DTO.ProductDTO.CostumerDTO;
using tienda.src.Application.Services.Interfaces;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.ProductDTO;

namespace Tienda.src.API.Controllers
{
    // 👇 ojo: este prefix es correcto
    [Route("api")]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // ============================================================
        //    GET /api/products
        // ============================================================
        /// <summary>
        /// Catálogo público de productos (rúbrica flujo 5).
        /// Solo productos activos.
        /// Soporta paginación, filtros, búsqueda y orden.
        /// </summary>
        [HttpGet("products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProductsAsync([FromQuery] SearchParamsDTO searchParams)
        {

            var page = await _productService.GetFilteredForCostumerAsync(searchParams);

            var message = page.TotalCount == 0
                ? "No se encontraron productos con los criterios especificados"
                : "Productos obtenidos exitosamente";

            return Ok(new GenericResponse<ListedProductsForCostumerDTO>(message, page));
        }

        // ============================================================
        //    GET /api/products/{id}
        // ============================================================
        /// <summary>
        /// Detalle público de producto (rúbrica flujo 5).
        /// Solo productos activos.
        /// </summary>
        [HttpGet("products/{productId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProductByIdAsync(int productId)
        {
            var result = await _productService.GetByIdForCostumerAsync(productId);
            if (result == null)
                return NotFound(new { message = $"No se encontró el producto con ID {productId} o está inactivo." });

            return Ok(new GenericResponse<ProductDetailDTO>("Producto obtenido exitosamente", result));
        }

        /// <summary>
        /// Listado para cliente que ya existía: /api/costumer/products
        /// </summary>
        [HttpGet("costumer/products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllForCostumerAsync([FromQuery] SearchParamsDTO searchParams)
        {
            var page = await _productService.GetFilteredForCostumerAsync(searchParams);

            var message = page.TotalCount == 0
                ? "No se encontraron productos con los criterios especificados"
                : "Productos obtenidos exitosamente";

            return Ok(new GenericResponse<ListedProductsForCostumerDTO>(message, page));
        }

        // ============================================================
        //    ADMINISTRADOR
        // ============================================================
        /// <summary>
        /// Obtiene lista paginada de productos para administradores con filtros
        /// </summary>
        [HttpGet("admin/products")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(GenericResponse<ListedProductsForAdminDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllForAdminAsync([FromQuery] SearchParamsDTO searchParams)
        {
            var result = await _productService.GetFilteredForAdminAsync(searchParams);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene detalle completo de un producto (admin)
        /// </summary>
        [HttpGet("admin/{productId:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(GenericResponse<ProductDetailDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdForAdminAsync(int productId)
        {
            var result = await _productService.GetByIdForAdminAsync(productId);
            return Ok(new GenericResponse<ProductDetailDTO>("Producto obtenido exitosamente", result));
        }

        /// <summary>
        /// Obtiene detalle COMPLETO de un producto con información de auditoría (admin)
        /// Incluye: IsDeleted, DeletedAt, CreatedAt, UpdatedAt, FinalPrice, etc.
        /// </summary>
        [HttpGet("admin/{productId:int}/detailed")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(GenericResponse<ProductDetailForAdminDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDetailedByIdForAdminAsync(int productId)
        {
            var result = await _productService.GetDetailedByIdForAdminAsync(productId);
            return Ok(new GenericResponse<ProductDetailForAdminDTO>("Detalle completo del producto obtenido exitosamente", result));
        }

        /// <summary>
        /// Crea un nuevo producto con URLs de imágenes (JSON)
        /// </summary>
        [HttpPost("admin/create")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProductAsync([FromBody] CreateProductJsonDTO createProductDTO)
        {
            var result = await _productService.CreateProductJsonAsync(createProductDTO);
            return Created($"/api/product/{result}", new GenericResponse<string>("Producto creado exitosamente", result));
        }

        /// <summary>
        /// Crea un nuevo producto con archivos de imagen que se subirán a Cloudinary
        /// Incluye rollback automático si falla la subida de imágenes
        /// </summary>
        [HttpPost("admin/create-with-files")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProductWithFilesAsync([FromForm] CreateProductDTO createProductDTO)
        {
            try
            {
                var result = await _productService.CreateProductAsync(createProductDTO);
                return Created($"/api/product/{result}", new GenericResponse<string>("Producto creado exitosamente con imágenes subidas a Cloudinary", result));
            }
            catch (ArgumentException ex)
            {
                // 400 - Validaciones de métodos no validos
                return BadRequest(new GenericResponse<string>($"Error de validación: {ex.Message}", null));
            }
            catch (InvalidOperationException ex)
            {
                // 500 - Operación invalida
                return StatusCode(500, new GenericResponse<string>($"Error del servidor: {ex.Message}", null));
            }
            catch (Exception ex)
            {
                // 500 - Error inesperado
                Log.Error(ex, "Error inesperado al crear producto");
                return StatusCode(500, new GenericResponse<string>(
                    "Error interno del servidor",
                    "Ocurrió un error al crear el producto. Por favor, intente nuevamente."
                ));
            }
        }

        /// <summary>
        /// Activa o desactiva un producto (toggle IsAvailable)
        /// </summary>
        [HttpPatch("admin/{id}/toggle-availability")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ToggleAvailabilityAsync(int id)
        {
            await _productService.ToggleActiveAsync(id);
            return Ok(new GenericResponse<string>("Disponibilidad del producto cambiada exitosamente", $"La disponibilidad del producto con ID {id} ha sido cambiada."));
        }

        /// <summary>
        /// Endpoint temporal para activar todos los productos (solo para desarrollo)
        /// </summary>
        [HttpPost("admin/activate-all")]
        [AllowAnonymous] // Temporal para facilitar el uso
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActivateAllProductsAsync()
        {
            await _productService.ActivateAllProductsAsync();
            return Ok(new GenericResponse<string>("Todos los productos han sido activados", "Operación completada exitosamente"));
        }

        [HttpDelete("admin/products/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProductAsync(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);

                // 204 No Content es el código estándar para DELETE exitoso
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                // 404 - Producto no encontrado
                Log.Warning("Producto no encontrado: {ProductId}", id);
                return NotFound(new GenericResponse<string>(
                    "Producto no encontrado",
                    ex.Message
                ));
            }
            catch (InvalidOperationException ex)
            {
                // 400 - Producto ya eliminado u operación no válida
                return BadRequest(new GenericResponse<string>(
                    "Operación no válida",
                    ex.Message
                ));
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                Log.Error(ex, "Error al eliminar producto {ProductId}", id);
                return StatusCode(500, new GenericResponse<string>(
                    "Error interno del servidor",
                    "No se pudo eliminar el producto. Por favor, intente nuevamente."
                ));
            }
        }

        /// <summary>
        /// Restaura un producto eliminado, marcándolo como disponible nuevamente.
        /// Solo Admin puede restaurar productos.
        /// </summary>
        /// <param name="id">ID del producto a restaurar</param>
        /// <returns>200 OK con mensaje de confirmación</returns>
        [HttpPatch("admin/products/{id:int}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RestoreProductAsync(int id)
        {
            try
            {
                var result = await _productService.RestoreProductAsync(id);

                return Ok(new GenericResponse<string>(
                    "Producto restaurado exitosamente",
                    result
                ));
            }
            catch (KeyNotFoundException ex)
            {
                // 404 - Producto no encontrado
                Log.Warning("Producto no encontrado: {ProductId}", id);
                return NotFound(new GenericResponse<string>(
                    "Producto no encontrado",
                    ex.Message
                ));
            }
            catch (InvalidOperationException ex)
            {
                // 400 - Producto no está eliminado
                return BadRequest(new GenericResponse<string>(
                    "Operación no válida",
                    ex.Message
                ));
            }
            catch (Exception ex)
            {
                // 500 - Error interno
                Log.Error(ex, "Error al restaurar producto {ProductId}", id);
                return StatusCode(500, new GenericResponse<string>(
                    "Error interno del servidor",
                    "No se pudo restaurar el producto. Por favor, intente nuevamente."
                ));
            }
        }

        /// <summary>
        /// Actualiza un producto existente (actualización parcial)
        /// Solo Admin puede actualizar productos
        /// </summary>
        /// <param name="id">ID del producto a actualizar</param>
        /// <param name="updateProductDTO">Datos a actualizar (solo los campos proporcionados)</param>
        /// <returns>200 OK con mensaje de confirmación</returns>
        [HttpPut("admin/products/{id:int}")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(GenericResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProductAsync(int id, [FromForm] UpdateProductDTO updateProductDTO)
        {
            try
            {
                var result = await _productService.UpdateProductAsync(id, updateProductDTO);

                return Ok(new GenericResponse<string>(
                    "Producto actualizado exitosamente",
                    result
                ));
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Intento de actualizar producto inexistente: {ProductId}", id);
                return NotFound(new GenericResponse<string>(
                    "Producto no encontrado",
                    ex.Message
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new GenericResponse<string>(
                    "Operación no válida",
                    ex.Message
                ));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new GenericResponse<string>(
                    "Error de validación",
                    ex.Message
                ));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al actualizar producto {ProductId}", id);
                return StatusCode(500, new GenericResponse<string>(
                    "Error interno del servidor",
                    "No se pudo actualizar el producto. Por favor, intente nuevamente."
                ));
            }
        }
    }
}
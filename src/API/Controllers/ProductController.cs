using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tienda.src.Application.DTO.ProductDTO;
using tienda.src.Application.DTO.ProductDTO.CostumerDTO;
using tienda.src.Application.Services.Interfaces;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.ProductDTO;

namespace Tienda.src.API.Controllers
{
    [Route("api")]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // ----- COSTUMERS ------

        /// <summary>
        /// Obtiene todos los productos para clientes
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        [HttpGet("costumer/products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllForCostumerAsync([FromQuery] SearchParamsDTO searchParams)
        {
            var result = await _productService.GetFilteredForCostumerAsync(searchParams);
            var message = result.Products.Count == 0 ? "No se encontraron productos con los criterios especificados" : "Productos obtenidos exitosamente";
            return Ok(new GenericResponse<ListedProductsForCostumerDTO>(message, result));
        }

        /// <summary>
        /// Obtiene un producto por id para clientes
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet("costumer/products/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByIdForCostumerAsync(int productId)
        {
            var result = await _productService.GetByIdForCostumerAsync(productId);
            if (result == null) { throw new KeyNotFoundException($"No se encontró el producto con ID {productId}."); }
            return Ok(new GenericResponse<ProductDetailDTO>("Producto obtenido exitosamente", result));
        }

        // ----- ADMINS ------ 

        [HttpGet("admin/products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdminAsync([FromQuery] SearchParamsDTO searchParams)
        {
            try
            {
                var result = await _productService.GetFilteredForAdminAsync(searchParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }

        [HttpGet("admin/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByIdForAdminAsync(int productId)
        {
            var result = await _productService.GetByIdForAdminAsync(productId);
            if (result == null) { throw new KeyNotFoundException($"No se encontró el producto con ID {productId}."); }
            return Ok(new GenericResponse<ProductDetailDTO>("Producto obtenido exitosamente", result));
        }

        [HttpPost("admin/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProductAsync([FromBody] CreateProductJsonDTO createProductDTO)
        {
            var result = await _productService.CreateProductJsonAsync(createProductDTO);
            return Created($"/api/product/{result}", new GenericResponse<string>("Producto creado exitosamente", result));
        }

        /// <summary>
        /// Crea un nuevo producto con archivos de imagen que se subirán a Cloudinary
        /// </summary>
        [HttpPost("admin/create-with-files")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProductWithFilesAsync([FromForm] CreateProductDTO createProductDTO)
        {
            var result = await _productService.CreateProductAsync(createProductDTO);
            return Created($"/api/product/{result}", new GenericResponse<string>("Producto creado exitosamente con imágenes subidas a Cloudinary", result));
        }

        [HttpPatch("admin/{id}/toggle-availability")]
        [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> ActivateAllProductsAsync()
        {
            await _productService.ActivateAllProductsAsync();
            return Ok(new GenericResponse<string>("Todos los productos han sido activados", "Operación completada exitosamente"));
        }
    }
}
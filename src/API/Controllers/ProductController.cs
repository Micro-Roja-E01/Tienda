using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tienda.src.Application.DTO.ProductDTO;
using tienda.src.Application.DTO.ProductDTO.CostumerDTO;
using tienda.src.Application.Services.Interfaces;
using Tienda.src.Application.DTO;
using Tienda.src.Application.DTO.ProductDTO;

namespace Tienda.src.API.Controllers
{
    /// <summary>
    /// Controlador de productos.
    /// Expone catálogo público y endpoints administrativos para crear, editar y activar productos.
    /// </summary>
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
        /// Lista productos para el panel admin, incluyendo inactivos o eliminados según los filtros.
        /// </summary>
        /// <param name="searchParams">Parámetros de filtrado y paginación.</param>
        [HttpGet("admin/products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdminAsync([FromQuery] SearchParamsDTO searchParams)
        {
            var result = await _productService.GetFilteredForAdminAsync(searchParams);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene el detalle interno de un producto para administración.
        /// Incluye banderas de estado e información adicional para gestión.
        /// </summary>
        /// <param name="productId">ID del producto.</param>
        [HttpGet("admin/{productId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByIdForAdminAsync(int productId)
        {
            var result = await _productService.GetByIdForAdminAsync(productId);
            return Ok(new GenericResponse<ProductDetailDTO>("Producto obtenido exitosamente", result));
        }

        /// <summary>
        /// Crea un nuevo producto enviando el JSON del producto.
        /// Este endpoint está pensado para pruebas o para paneles que no suben archivos.
        /// </summary>
        /// <param name="createProductDTO">Datos del producto a crear.</param>
        [HttpPost("admin/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProductAsync([FromBody] CreateProductJsonDTO createProductDTO)
        {
            var result = await _productService.CreateProductJsonAsync(createProductDTO);
            return Created($"/api/product/{result}", new GenericResponse<string>("Producto creado exitosamente", result));
        }

        /// <summary>
        /// Crea un nuevo producto con archivos de imagen que se subirán a Cloudinary.
        /// </summary>
        [HttpPost("admin/create-with-files")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProductWithFilesAsync([FromForm] CreateProductDTO createProductDTO)
        {
            var result = await _productService.CreateProductAsync(createProductDTO);
            return Created($"/api/product/{result}", new GenericResponse<string>("Producto creado exitosamente con imágenes subidas a Cloudinary", result));
        }

        /// <summary>
        /// Activa o desactiva un producto específico.
        /// Útil para el flujo 6.3 (estado del producto).
        /// </summary>
        /// <param name="id">ID del producto.</param>
        [HttpPatch("admin/{id}/toggle-availability")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleAvailabilityAsync(int id)
        {
            await _productService.ToggleActiveAsync(id);
            return Ok(new GenericResponse<string>("Disponibilidad del producto cambiada exitosamente", $"La disponibilidad del producto con ID {id} ha sido cambiada."));
        }

        /// <summary>
        /// Endpoint temporal para activar todos los productos.
        /// Solo debe usarse en desarrollo o pruebas.
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
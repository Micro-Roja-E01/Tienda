
using tienda.src.Application.DTO.ProductDTO;
using tienda.src.Application.DTO.ProductDTO.AdminDTO;
using tienda.src.Application.DTO.ProductDTO.CostumerDTO;
using Tienda.src.Application.DTO.ProductDTO;

namespace tienda.src.Application.Services.Interfaces
{

    /// <summary>
    /// Interfaz para el servicio de productos.
    /// Define métodos para manejar operaciones relacionadas con productos.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        /// <param name="createProductDTO">Datos del producto a crear</param>
        /// <returns>ID del producto creado</returns>
        Task<string> CreateProductAsync(CreateProductDTO createProductDTO); // TODO: FALTA

        /// <summary>
        /// Crea un nuevo producto a partir de un DTO JSON (sin archivos)
        /// </summary>
        /// <param name="createProductJsonDTO">Datos del producto a crear con URLs de imágenes</param>
        /// <returns>ID del producto creado</returns>
        Task<string> CreateProductJsonAsync(CreateProductJsonDTO createProductJsonDTO);

        /// <summary>
        /// Retorna un producto por id para administradores.
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Detalle del producto para administradores</returns>
        Task<ProductDetailDTO> GetByIdForAdminAsync(int productId);

        /// <summary>
        /// Retorna un producto por id para clientes.
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Detalle del producto para clientes</returns>
        Task<ProductDetailDTO> GetByIdForCostumerAsync(int productId);

        /// <summary>
        /// Obtiene productos filtrados para administradores.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y filtrado</param>
        /// <returns>Lista paginada de productos para administradores</returns>
        Task<ListedProductsForAdminDTO> GetFilteredForAdminAsync(SearchParamsDTO searchParams);

        /// <summary>
        /// Obtiene productos filtrados para clientes.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y filtrado</param>
        /// <returns>Lista paginada de productos para clientes</returns>
        Task<ListedProductsForCostumerDTO> GetFilteredForCostumerAsync(SearchParamsDTO searchParams);

        /// <summary>
        /// Cambia el estado de un producto a activo/inactivo.
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>True si existe, false en caso contrario</returns>
        Task ToggleActiveAsync(int productId);

        /// <summary>
        /// Activa todos los productos en la base de datos (método temporal para desarrollo)
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task ActivateAllProductsAsync();
    }
}
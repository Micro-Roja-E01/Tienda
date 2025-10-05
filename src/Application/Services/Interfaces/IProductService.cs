
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
        /// Obtiene el detalle de un producto para clientes.
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Detalle del producto para clientes</returns>
        Task<ProductDetailDTO> GetByIdForAdminAsync(int productId);

        Task<string> CreateProductAsync(CreateProductDTO createProductDTO);

        /// <summary>
        /// Verifica si un producto existe.
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>True si existe, false en caso contrario</returns>
        Task ToggleActiveAsync(int productId);
    }
}
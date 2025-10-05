using tienda.src.Application.DTO.ProductDTO;
using tienda.src.Application.DTO.ProductDTO.AdminDTO;
using tienda.src.Application.DTO.ProductDTO.CostumerDTO;
using Tienda.src.Application.Domain.Models;

namespace tienda.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de productos.
    /// Define métodos para manejar operaciones de datos relacionadas con productos.
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        /// <param name="product">Producto a crear</param>
        /// <returns>ID del producto creado</returns>
        Task<int> CreateAsync(Product product);
        /// <summary>
        /// Obtiene productos filtrados para administradores.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda</param>
        /// <returns>Tupla con lista de productos y conteo total</returns>
        Task<(IEnumerable<ProductForAdminDTO> products, int totalCount)> GetFilteredForAdminAsync(SearchParamsDTO searchParams);

        /// <summary>
        /// Obtiene productos filtrados para clientes.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda</param>
        /// <returns>Tupla con lista de productos y conteo total</returns>
        Task<(IEnumerable<ProductForCostumerDTO> products, int totalCount)> GetFilteredForCostumerAsync(SearchParamsDTO searchParams);

        /// <summary>
        /// Obtiene un producto por su ID.
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>El producto encontrado o null si no existe</returns>
        Task<Product?> GetByIdAsync(int productId);

        /// <summary>
        /// Crea una nueva categoría u obtiene una existente por nombre.
        /// </summary>
        /// <param name="categoryName">Nombre de la categoría</param>
        /// <returns>La categoría creada o existente</returns>
        Task<Category?> CreateOrGetCategoryAsync(string categoryName);

        /// <summary>
        /// Crea una nueva marca o obtiene una existente por nombre.
        /// </summary>
        /// <param name="brandName">Nombre de la marca</param>
        /// <returns>La marca creada o existente</returns>
        Task<Brand?> CreateOrGetBrandAsync(string brandName);

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        /// <param name="product">Producto a actualizar</param>
        /// <returns>True si se actualizó correctamente</returns>
        Task<bool> UpdateAsync(Product product);

        /// <summary>
        /// Elimina un producto.
        /// </summary>
        /// <param name="productId">ID del producto a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> DeleteAsync(int productId);

        /// <summary>
        /// Verifica si un producto existe.
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>True si existe</returns>
        Task<bool> ExistsAsync(int productId);
    }
}
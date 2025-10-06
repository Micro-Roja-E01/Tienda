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
        /// Retorna una lista de productos para el administrador con los parámetros de búsqueda especificados.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar los productos.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con una lista de productos para el administrador y el conteo total de productos.</returns>
        Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForAdminAsync(
            SearchParamsDTO searchParams
        );

        /// <summary>
        /// Retorna una lista de productos para el cliente con los parámetros de búsqueda especificados.
        /// </summary>
        /// /// <param name="searchParams">Parámetros de búsqueda para filtrar los productos.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con una lista de productos para el cliente y el conteo total de productos.</returns>
        Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForCustomerAsync(
            SearchParamsDTO searchParams
        );

        /// <summary>
        /// Retorna un producto específico por su ID.
        /// </summary>
        /// <param name="id">El ID del producto a buscar.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el producto encontrado o null si no se encuentra.</returns>
        Task<Product?> GetByIdAsync(int id);

        /// <summary>
        /// Crea un nuevo producto en el repositorio.
        /// </summary>
        /// <param name="product">El producto a crear.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el id del producto creado.</returns>
        Task<int> CreateAsync(Product product);

        /// <summary>
        /// Crea o obtiene una categoría por su nombre.
        /// </summary>
        /// <param name="categoryName">El nombre de la categoría.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con la categoría creada o encontrada.</returns>
        Task<Category?> CreateOrGetCategoryAsync(string categoryName);

        /// <summary>
        /// Crea o obtiene una marca por su nombre.
        /// </summary>
        /// <param name="brandName">El nombre de la marca.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con la marca creada o encontrada.</returns>
        Task<Brand?> CreateOrGetBrandAsync(string brandName);

        /// <summary>
        /// Cambia el estado activo de un producto por su ID.
        /// </summary>
        /// <param name="id">El ID del producto cuyo estado se cambiará.</param>
        Task ToggleActiveAsync(int id);

        /// <summary>
        /// Obtiene el stock real de un producto por su ID.
        /// </summary>
        /// <param name="productId">El ID del producto cuyo stock se obtendrá.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el stock real del producto.</returns>
        Task<int> GetRealStockAsync(int productId);

        /// <summary>
        /// Actualiza el stock de un producto por su ID.
        /// </summary>
        /// <param name="productId">El ID del producto cuyo stock se actualizará.</param>
        /// <param name="stock">El nuevo stock del producto.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task UpdateStockAsync(int productId, int stock);

        /// <summary>
        /// Retorna un producto específico por su ID desde el punto de vista de un admin.
        /// </summary>
        /// <param name="id">El ID del producto a buscar.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el producto encontrado o null si no se encuentra.</returns>
        Task<Product?> GetByIdForAdminAsync(int id);

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        /// <param name="product">El producto con los datos actualizados.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task UpdateAsync(Product product);

        /// <summary>
        /// Elimina un producto por su ID.
        /// </summary>
        /// <param name="id">El ID del producto a eliminar.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task DeleteAsync(int id);

        /// <summary>
        /// Verifica si existe un producto con el ID especificado.
        /// </summary>
        /// <param name="id">El ID del producto a verificar.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con true si existe, false en caso contrario.</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Retorna una lista de productos para el cliente con los parámetros de búsqueda especificados.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar los productos.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con una lista de productos para el cliente y el conteo total de productos.</returns>
        Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForCostumerAsync(
            SearchParamsDTO searchParams
        );
    }
}
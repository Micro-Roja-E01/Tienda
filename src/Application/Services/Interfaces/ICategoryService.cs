using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.DTO.CategoryDTO;

namespace Tienda.src.Application.Services.Interfaces
{
    /// <summary>
    /// Define las operaciones disponibles para la gestión de categorías de productos.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Obtiene una lista paginada de categorías con filtros opcionales.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y paginación.</param>
        /// <returns>Un objeto <see cref="PagedCategoriesDTO"/> con los resultados.</returns>
        Task<PagedCategoriesDTO> GetAllAsync(SearchParamsDTO searchParams);

        /// <summary>
        /// Obtiene el detalle de una categoría específica por su identificador.
        /// </summary>
        /// <param name="id">Identificador único de la categoría.</param>
        /// <returns>Un objeto <see cref="CategoryDetailDTO"/> o null si no se encuentra.</returns>
        Task<CategoryDetailDTO?> GetByIdAsync(int id);

        /// <summary>
        /// Crea una nueva categoría en el sistema.
        /// </summary>
        /// <param name="dto">Datos de la categoría a crear.</param>
        /// <returns>El detalle de la categoría creada.</returns>
        Task<CategoryDetailDTO> CreateAsync(CategoryCreateDTO dto);

        /// <summary>
        /// Actualiza una categoría existente.
        /// </summary>
        /// <param name="id">Identificador de la categoría.</param>
        /// <param name="dto">Datos actualizados.</param>
        /// <returns>La categoría actualizada o null si no existe.</returns>
        Task<CategoryDetailDTO?> UpdateAsync(int id, CategoryUpdateDTO dto);

        /// <summary>
        /// Elimina una categoría del sistema.
        /// </summary>
        /// <param name="id">Identificador único de la categoría.</param>
        Task DeleteAsync(int id);
    }
}
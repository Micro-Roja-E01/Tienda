
using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.DTO.BrandDTO;

namespace Tienda.src.Application.Services.Interfaces
{
    /// <summary>
    /// Define las operaciones disponibles para gestionar marcas dentro del sistema.
    /// </summary>
    public interface IBrandService
    {
        /// <summary>
        /// Obtiene una lista paginada de marcas aplicando filtros y parámetros de búsqueda.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda y paginación.</param>
        /// <returns>Una lista paginada de marcas.</returns>
        Task<PagedBrandsDTO> GetAllAsync(SearchParamsDTO searchParams);

        /// <summary>
        /// Obtiene el detalle de una marca específica por su identificador.
        /// </summary>
        /// <param name="id">Identificador único de la marca.</param>
        /// <returns>Un objeto <see cref="BrandDetailDTO"/> si la marca existe; de lo contrario, null.</returns>
        Task<BrandDetailDTO?> GetByIdAsync(int id);

        /// <summary>
        /// Crea una nueva marca a partir de los datos proporcionados.
        /// </summary>
        /// <param name="dto">Datos de la marca a crear.</param>
        /// <returns>El detalle de la marca creada.</returns>
        Task<BrandDetailDTO> CreateAsync(BrandCreateDTO dto);

        /// <summary>
        /// Actualiza los datos de una marca existente.
        /// </summary>
        /// <param name="id">Identificador de la marca a actualizar.</param>
        /// <param name="dto">Datos actualizados de la marca.</param>
        /// <returns>La marca actualizada o null si no existe.</returns>
        Task<BrandDetailDTO?> UpdateAsync(int id, BrandUpdateDTO dto);

        /// <summary>
        /// Elimina una marca existente del sistema.
        /// </summary>
        /// <param name="id">Identificador de la marca a eliminar.</param>
        Task DeleteAsync(int id);
    }
}
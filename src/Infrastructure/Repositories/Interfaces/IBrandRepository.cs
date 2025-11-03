using Tienda.src.Application.Domain.Models;

namespace Tienda.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Define las operaciones de acceso a datos para la entidad marca.
    /// </summary>
    public interface IBrandRepository
    {
        /// <summary>
        /// Obtiene una marca por su identificador.
        /// </summary>
        /// <param name="id">ID de la marca.</param>
        /// <returns>La marca encontrada o <c>null</c>.</returns>
        Task<Brand?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene una marca por su nombre, ignorando mayúsculas y minúsculas.
        /// </summary>
        /// <param name="name">Nombre de la marca.</param>
        /// <returns>La marca encontrada o <c>null</c>.</returns>
        Task<Brand?> GetByNameAsync(string name);

        /// <summary>
        /// Agrega una marca al contexto.
        /// </summary>
        /// <param name="brand">Marca a agregar.</param>
        Task AddAsync(Brand brand);

        /// <summary>
        /// Persiste los cambios pendientes en la base de datos.
        /// </summary>
        Task SaveChangesAsync();
    }
}
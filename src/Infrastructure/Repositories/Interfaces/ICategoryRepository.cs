using Tienda.src.Application.Domain.Models;

namespace Tienda.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Define las operaciones de acceso a datos para la entidad categoría.
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// Indica si existe una categoría con el nombre dado.
        /// Permite excluir un ID concreto para escenarios de actualización.
        /// </summary>
        /// <param name="name">Nombre a validar.</param>
        /// <param name="excludeId">ID de la categoría que se está editando (opcional).</param>
        /// <returns><c>true</c> si ya existe.</returns>
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);

        /// <summary>
        /// Indica si existe una categoría con el slug dado.
        /// </summary>
        /// <param name="slug">Slug a validar.</param>
        /// <param name="excludeId">ID a excluir (opcional).</param>
        /// <returns><c>true</c> si ya existe.</returns>
        Task<bool> ExistsBySlugAsync(string slug, int? excludeId = null);

        /// <summary>
        /// Obtiene una categoría por su identificador.
        /// </summary>
        /// <param name="id">ID de la categoría.</param>
        /// <returns>La categoría encontrada o <c>null</c>.</returns>
        Task<Category?> GetByIdAsync(int id);

        /// <summary>
        /// Agrega una nueva categoría al contexto.
        /// </summary>
        /// <param name="category">Categoría a agregar.</param>
        Task AddAsync(Category category);

        /// <summary>
        /// Persiste los cambios pendientes en el contexto.
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Devuelve una consulta <see cref="IQueryable{Category}"/> para componer filtros desde servicios.
        /// </summary>
        /// <returns>Consulta sobre categorías.</returns>
        IQueryable<Category> Query();
    }
}

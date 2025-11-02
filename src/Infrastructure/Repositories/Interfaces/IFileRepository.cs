using Tienda.src.Application.Domain.Models;

namespace tienda.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de archivos.
    /// Define métodos para manejar operaciones relacionadas con archivos de imagen.
    /// </summary>
    public interface IFileRepository
    {
        /// <summary>
        /// Crea un archivo de imagen en la base de datos.
        /// </summary>
        /// <param name="file">El archivo de imagen a crear.</param>
        /// <returns>True si el archivo se creó correctamente, de lo contrario false y null en caso de que la imagen ya existe.</returns>
        Task<bool?> CreateAsync(Image file);

        /// <summary>
        /// Elimina un archivo de imagen de la base de datos.
        /// </summary>
        /// <param name="publicId">El identificador público del archivo a eliminar.</param>
        /// <returns>True si el archivo se eliminó correctamente, de lo contrario false y null si la imagen no existe.</returns>
        Task<bool?> DeleteAsync(string publicId);

        /// <summary>
        /// Obtiene todas las imágenes asociadas a un producto.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IEnumerable<Image>> GetByProductIdAsync(int id);

        /// <summary>
        /// Obtiene una imagen por su ID.
        /// </summary>
        /// <param name="id">ID de la imagen</param>
        /// <returns>La imagen si existe, null si no existe</returns>
        Task<Image?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todas las imágenes asociadas a un producto, incluyendo las eliminadas (para restauración).
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Lista de todas las imágenes del producto</returns>
        Task<IEnumerable<Image>> GetAllByProductIdAsync(int productId);

        /// <summary>
        /// Actualiza una imagen existente.
        /// </summary>
        /// <param name="image">La imagen con los datos actualizados</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task UpdateAsync(Image image);

        /// <summary>
        /// Marca una imagen como eliminada (soft delete).
        /// </summary>
        /// <param name="id">ID de la imagen a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task SoftDeleteAsync(int id);

        /// <summary>
        /// Restaura una imagen eliminada.
        /// </summary>
        /// <param name="id">ID de la imagen a restaurar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task RestoreAsync(int id);
    }
}
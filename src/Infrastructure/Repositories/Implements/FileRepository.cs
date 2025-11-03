using Microsoft.EntityFrameworkCore;
using Serilog;
using tienda.src.Infrastructure.Repositories.Interfaces;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Infrastructure.Data;

namespace tienda.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Implementación del repositorio para manejar operaciones de archivos de imagen.
    /// </summary>
    public class FileRepository : IFileRepository
    {
        private readonly DataContext _context;

        public FileRepository(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea un archivo de imagen en la base de datos.
        /// </summary>
        /// <param name="file">El archivo de imagen a crear.</param>
        /// <returns>True si el archivo se creó correctamente, false si falló, y null si ya existe.</returns>
        public async Task<bool?> CreateAsync(Image file)
        {
            try
            {
                // Verificar si ya existe una imagen con el mismo PublicId
                var existingImage = await _context.Images
                    .FirstOrDefaultAsync(i => i.PublicId == file.PublicId);

                if (existingImage != null)
                {
                    Log.Warning($"La imagen con PublicId {file.PublicId} ya existe en la base de datos");
                    return null;
                }

                _context.Images.Add(file);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    Log.Information($"Imagen creada exitosamente en la base de datos con PublicId: {file.PublicId}");
                    return true;
                }

                Log.Error($"Error al guardar la imagen en la base de datos con PublicId: {file.PublicId}");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error al crear la imagen en la base de datos con PublicId: {file.PublicId}");
                return false;
            }
        }

        /// <summary>
        /// Elimina un archivo de imagen de la base de datos.
        /// </summary>
        /// <param name="publicId">El identificador público del archivo a eliminar.</param>
        /// <returns>True si el archivo se eliminó correctamente, false si falló, y null si no existe.</returns>
        public async Task<bool?> DeleteAsync(string publicId)
        {
            try
            {
                var image = await _context.Images
                    .FirstOrDefaultAsync(i => i.PublicId == publicId);

                if (image == null)
                {
                    Log.Warning($"La imagen con PublicId {publicId} no existe en la base de datos");
                    return null;
                }

                _context.Images.Remove(image);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    Log.Information($"Imagen eliminada exitosamente de la base de datos con PublicId: {publicId}");
                    return true;
                }

                Log.Error($"Error al eliminar la imagen de la base de datos con PublicId: {publicId}");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error al eliminar la imagen de la base de datos con PublicId: {publicId}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene todas las imágenes asociadas a un producto.
        /// Excluye imágenes marcadas como eliminadas.
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Image>> GetByProductIdAsync(int productId)
        {
            return await _context.Images
                .Where(i => i.ProductId == productId && !i.IsDeleted)  // Solo imágenes activas
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene todas las imágenes asociadas a un producto, incluyendo las eliminadas.
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Lista de todas las imágenes del producto</returns>
        public async Task<IEnumerable<Image>> GetAllByProductIdAsync(int productId)
        {
            return await _context.Images
                .Where(i => i.ProductId == productId)  // Todas las imágenes (incluyendo eliminadas)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una imagen por su ID.
        /// </summary>
        /// <param name="id">ID de la imagen</param>
        /// <returns>La imagen si existe, null si no existe</returns>
        public async Task<Image?> GetByIdAsync(int id)
        {
            return await _context.Images
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        /// <summary>
        /// Actualiza una imagen existente.
        /// </summary>
        /// <param name="image">La imagen con los datos actualizados</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task UpdateAsync(Image image)
        {
            try
            {
                _context.Images.Update(image);
                await _context.SaveChangesAsync();
                Log.Information("Imagen {ImageId} actualizada exitosamente", image.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al actualizar la imagen {ImageId}", image.Id);
                throw;
            }
        }

        /// <summary>
        /// Marca una imagen como eliminada (soft delete).
        /// La imagen NO se elimina de Cloudinary para permitir restauración.
        /// </summary>
        /// <param name="id">ID de la imagen a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task SoftDeleteAsync(int id)
        {
            try
            {
                await _context.Images
                    .Where(i => i.Id == id)
                    .ExecuteUpdateAsync(i => i
                        .SetProperty(x => x.IsDeleted, true)
                        .SetProperty(x => x.DeletedAt, DateTime.UtcNow));

                Log.Information("Imagen {ImageId} marcada como eliminada (soft delete)", id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al marcar imagen {ImageId} como eliminada", id);
                throw;
            }
        }

        /// <summary>
        /// Restaura una imagen eliminada.
        /// </summary>
        /// <param name="id">ID de la imagen a restaurar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task RestoreAsync(int id)
        {
            try
            {
                await _context.Images
                    .Where(i => i.Id == id)
                    .ExecuteUpdateAsync(i => i
                        .SetProperty(x => x.IsDeleted, false)
                        .SetProperty(x => x.DeletedAt, (DateTime?)null));

                Log.Information("Imagen {ImageId} restaurada exitosamente", id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al restaurar imagen {ImageId}", id);
                throw;
            }
        }
    }
}
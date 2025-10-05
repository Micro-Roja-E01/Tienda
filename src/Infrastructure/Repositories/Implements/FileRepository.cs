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
    }
}
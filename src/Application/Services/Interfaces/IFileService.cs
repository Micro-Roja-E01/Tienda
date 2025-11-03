namespace tienda.src.Application.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Sube un archivo a cloudinary y lo asocia a un producto.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task<bool> UploadAsync(IFormFile file, int productId);

        /// <summary>
        /// Elimina un archivo de cloudinary.
        /// </summary>
        /// <param name="publicId"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(string publicId);
    }
}
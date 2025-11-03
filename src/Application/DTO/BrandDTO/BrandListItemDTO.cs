
namespace Tienda.src.Application.DTO.BrandDTO
{
    /// <summary>
    /// DTO utilizado para listar marcas en vistas resumidas.
    /// </summary>
    public class BrandListItemDTO
    {
        /// <summary>
        /// Identificador único de la marca.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la marca.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Slug normalizado de la marca.
        /// </summary>
        public required string Slug { get; set; }

        /// <summary>
        /// Número de productos asociados a la marca.
        /// </summary>
        public int ProductCount { get; set; }

        /// <summary>
        /// Fecha de creación de la marca.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
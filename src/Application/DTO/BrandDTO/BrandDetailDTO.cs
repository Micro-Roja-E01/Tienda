
namespace Tienda.src.Application.DTO.BrandDTO
{
    /// <summary>
    /// DTO que representa el detalle completo de una marca.
    /// </summary>
    public class BrandDetailDTO
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
        /// Slug normalizado único de la marca.
        /// </summary>
        public required string Slug { get; set; }

        /// <summary>
        /// Descripción opcional de la marca.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Cantidad de productos asociados a la marca.
        /// </summary>
        public int ProductCount { get; set; }

        /// <summary>
        /// Fecha de creación de la marca.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
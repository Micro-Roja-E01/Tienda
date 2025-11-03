namespace Tienda.src.Application.DTO.CategoryDTO
{
    /// <summary>
    /// DTO que representa el detalle completo de una categoría.
    /// </summary>
    public class CategoryDetailDTO
    {
        /// <summary>
        /// Identificador único de la categoría.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la categoría.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Slug normalizado único.
        /// </summary>
        public required string Slug { get; set; }

        /// <summary>
        /// Descripción opcional.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Número de productos asociados a la categoría.
        /// </summary>
        public int ProductCount { get; set; }

        /// <summary>
        /// Fecha de creación de la categoría.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
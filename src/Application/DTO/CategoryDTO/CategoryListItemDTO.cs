namespace Tienda.src.Application.DTO.CategoryDTO
{
    /// <summary>
    /// DTO resumido utilizado para listar categorías.
    /// </summary>
    public class CategoryListItemDTO
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
        /// Slug normalizado de la categoría.
        /// </summary>
        public required string Slug { get; set; }

        /// <summary>
        /// Número de productos asociados.
        /// </summary>
        public int ProductCount { get; set; }

        /// <summary>
        /// Fecha de creación de la categoría.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}

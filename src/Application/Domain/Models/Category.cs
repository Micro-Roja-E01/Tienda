namespace Tienda.src.Application.Domain.Models
{
    public class Category
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
        /// Slug normalizado único (para búsquedas, URLs, etc.)
        /// </summary>
        public required string Slug { get; set; }

        /// <summary>
        /// Descripción opcional de la categoría.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Eliminación lógica.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Fecha de creación de la categoría.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

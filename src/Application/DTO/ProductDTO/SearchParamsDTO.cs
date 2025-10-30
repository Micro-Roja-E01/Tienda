
using System.ComponentModel.DataAnnotations;

namespace tienda.src.Application.DTO.ProductDTO
{
    public class SearchParamsDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "El número de página debe ser un valor entero positivo.")]
        public int? PageNumber { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "El tamaño de página debe ser un valor entero positivo.")]
        public int? PageSize { get; set; }

        [MinLength(2, ErrorMessage = "El término de búsqueda debe tener al menos 2 caracteres.")]
        [MaxLength(40, ErrorMessage = "El término de búsqueda no puede tener más de 40 caracteres.")]
        public string? SearchTerm { get; set; }

        // --- NUEVO ---

        // Filtro por categoría exacta 
        [MaxLength(50, ErrorMessage = "El nombre de la categoría no puede tener más de 50 caracteres.")]
        public string? Category { get; set; }

        // Filtro por marca exacta
        [MaxLength(50, ErrorMessage = "El nombre de la marca no puede tener más de 50 caracteres.")]
        public string? Brand { get; set; }

        // Rango de precio en CLP
        [Range(0, int.MaxValue, ErrorMessage = "El precio mínimo debe ser un número positivo.")]
        public int? MinPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El precio máximo debe ser un número positivo.")]
        public int? MaxPrice { get; set; }

        // Ordenamiento seguro
        // sortBy admite SOLO: price | createdAt | title
        public string? SortBy { get; set; }

        // sortDir admite SOLO: asc | desc
        public string? SortDir { get; set; }
    }

}

using System;
using System.Collections.Generic;

namespace tienda.src.Application.DTO.ProductDTO.AdminDTO
{
    /// <summary>
    /// DTO completo de detalle de producto para administradores.
    /// Incluye toda la información de auditoría y estado de eliminación.
    /// </summary>
    public class ProductDetailForAdminDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public int Discount { get; set; }
        public int FinalPrice { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; } = string.Empty; // "Nuevo" o "Usado"
        public bool IsAvailable { get; set; }
        public bool IsDeleted { get; set; }

        // Auditoría completa
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Relaciones
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;

        // Imágenes con información completa
        public List<ImageDetailDTO> Images { get; set; } = new List<ImageDetailDTO>();

        // Indicador de stock
        public string StockIndicator { get; set; } = string.Empty; // "Sin stock", "Últimas unidades", ""
    }

    /// <summary>
    /// DTO detallado de imagen para administradores
    /// </summary>
    public class ImageDetailDTO
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
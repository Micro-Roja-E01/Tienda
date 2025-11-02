using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.Application.Domain.Models
{
    public enum Status
    {
        Nuevo,
        Usado,
    }

    public class Product
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public required int Price { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public int Discount { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public required int Stock { get; set; }

        // Nuevo o Usado
        public required Status Status { get; set; }
        public bool IsAvailable { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;
        public ICollection<Image> Images { get; set; } = new List<Image>();
        public bool IsDeleted { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Precio final después de aplicar el descuento.
        /// Calculado en el servidor para evitar manipulación cliente.
        /// </summary>
        [NotMapped]
        public int FinalPrice
        {
            get
            {
                if (Discount <= 0) return Price;

                // Calcular descuento y redondear hacia arriba
                var discountAmount = (int)Math.Ceiling(Price * (Discount / 100.0));

                // Asegurar que nunca sea negativo
                return Math.Max(0, Price - discountAmount);
            }
        }
    }
}
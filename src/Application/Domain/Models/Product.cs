using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public required int Price { get; set; }
        public int Discount { get; set; }
        public required int Stock { get; set; }

        // Nuevo o Usado
        public required Status Status { get; set; }
        public bool IsAvailable { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;
        public ICollection<Image> Images { get; set; } = new List<Image>();
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
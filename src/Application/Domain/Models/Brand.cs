using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.Application.Domain.Models
{
    public class Brand
    {
        public int Id { get; set; }

        // nombre de la marca (único, case-insensitive)
        public required string Name { get; set; }

        // slug normalizado único
        public required string Slug { get; set; }

        // opcional
        public string? Description { get; set; }

        // soft delete
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
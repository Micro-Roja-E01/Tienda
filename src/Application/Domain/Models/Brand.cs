using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.Application.Domain.Models
{
    /// <summary>
    /// Representa una marca de productos en el sistema.
    /// </summary>
    public class Brand
    {
        /// <summary>
        /// Identificador único de la marca.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la marca (único, sin distinción de mayúsculas o minúsculas).
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Slug normalizado único utilizado para búsquedas o URLs.
        /// </summary>
        public required string Slug { get; set; }

        /// <summary>
        /// Descripción opcional de la marca.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Indica si la marca ha sido eliminada lógicamente (soft delete).
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Fecha en la que se creó el registro de la marca.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
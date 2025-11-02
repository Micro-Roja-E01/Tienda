using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.Application.Domain.Models
{
    /// <summary>
    /// Indica el estado físico del producto (nuevo o usado).
    /// </summary>
    public enum Status
    {
        Nuevo,
        Usado,
    }

    /// <summary>
    /// Representa un producto dentro del catálogo de la tienda.
    /// Contiene información de precios, stock, categoría y marca.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Identificador único del producto.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Título o nombre comercial del producto.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Descripción del producto.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Precio base del producto.
        /// </summary>
        public required int Price { get; set; }

        /// <summary>
        /// Porcentaje de descuento aplicado al producto.
        /// </summary>
        public int Discount { get; set; }

        /// <summary>
        /// Cantidad disponible en inventario.
        /// </summary>
        public required int Stock { get; set; }

        /// <summary>
        /// Estado físico del producto (nuevo o usado).
        /// </summary>
        public required Status Status { get; set; }

        /// <summary>
        /// Indica si el producto está disponible para la venta.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Identificador de la categoría a la que pertenece el producto.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Referencia a la categoría asociada.
        /// </summary>
        public Category Category { get; set; } = null!;

        /// <summary>
        /// Identificador de la marca del producto.
        /// </summary>
        public int BrandId { get; set; }

        /// <summary>
        /// Referencia a la marca asociada.
        /// </summary>
        public Brand Brand { get; set; } = null!;

        /// <summary>
        /// Colección de imágenes asociadas al producto.
        /// </summary>
        public ICollection<Image> Images { get; set; } = new List<Image>();

        /// <summary>
        /// Fecha de última actualización del producto.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de creación del producto.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
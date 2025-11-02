using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        /// Representa el precio del producto.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public required int Price { get; set; }
        /// <summary>
        /// Representa el porcentaje de descuento aplicado al producto.
        /// </summary>  
        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public int Discount { get; set; } = 0;
        /// <summary>
        /// Representa la cantidad de unidades del producto disponibles en stock.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
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
        /// Indica si el producto fue eliminado o no.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Fecha de última actualización del producto.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de creación del producto.
        /// </summary>
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
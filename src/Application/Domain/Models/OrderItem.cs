using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.Application.Domain.Models
{
    /// <summary>
    /// Representa un ítem individual dentro de una orden de compra.
    /// Contiene información del producto al momento de la compra.
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Identificador único del ítem de orden.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identificador de la orden a la que pertenece el ítem.
        /// </summary>
        public required int OrderId { get; set; }

        /// <summary>
        /// Referencia a la orden asociada.
        /// </summary>
        public Order Order { get; set; } = null!;

        /// <summary>
        /// Cantidad de unidades del producto en la orden.
        /// </summary>
        public required int Quantity { get; set; }

        /// <summary>
        /// Título del producto al momento de la compra.
        /// </summary>
        public required string TitleAtMoment { get; set; }

        /// <summary>
        /// Precio unitario del producto al momento de la compra.
        /// </summary>
        public required int PriceAtMoment { get; set; }

        /// <summary>
        /// Descripción del producto al momento de la compra.
        /// </summary>
        public required string DescriptionAtMoment { get; set; }

        /// <summary>
        /// URL de la imagen principal del producto al momento de la compra.
        /// </summary>
        public required string ImageUrlAtMoment { get; set; }
        public required int DiscountAtMoment { get; set; }
    }
}
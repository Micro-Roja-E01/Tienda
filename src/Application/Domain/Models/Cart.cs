using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.Application.Domain.Models
{
    public class Cart
    {
        /// <summary>
        /// Identificador único del carrito de compras.
        /// </summary>
        /// <value></value>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Total del carrito de compras con descuento aplicado.
        /// </summary>
        /// <value></value>
        public int Total { get; set; }

        /// <summary>
        /// Subtotal del carrito de compras sin descuentos aplicados.
        /// </summary>
        /// <value></value>
        public int SubTotal { get; set; }

        /// <summary>
        /// Usuario invitado que posee el carrito de compras.
        /// </summary>
        /// <value></value>
        public required string BuyerId { get; set; } = null!;

        /// <summary>
        /// Identificador del usuario registrado que posee el carrito de compras.
        /// </summary>
        /// <value></value>
        public int? UserId { get; set; }

        /// <summary>
        /// Lista de artículos en el carrito de compras.
        /// </summary>
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        /// <summary>
        /// Fecha de creación del carrito de compras.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de actualización del carrito de compras.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Cantidad total de artículos únicos en el carrito.
        /// </summary>
        public int TotalUniqueItemsCount { get; set; }

        /// <summary>
        /// Monto total ahorrado por descuentos aplicados.
        /// </summary>
        public int TotalSavedAmount { get; set; }
    }
}
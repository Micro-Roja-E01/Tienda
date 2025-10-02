using Bogus.DataSets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.Application.Domain.Models
{
    public class Order
    {
        /// <summary>
        /// Identificador único del pedido.
        /// </summary>
        /// <value></value>
        [Key]
        public required int Id { get; set; }

        /// <summary>
        /// Código del pedido.
        /// </summary>
        /// <value></value>
        public required string Code { get; set; }

        /// <summary>
        /// Total del pedido.
        /// </summary>
        /// <value></value>
        public required int Total { get; set; }

        /// <summary>
        /// Subtotal del pedido.
        /// </summary>
        /// <value></value>
        public required int SubTotal { get; set; }

        /// <summary>
        /// Id del usuario que realizó el pedido.
        /// </summary>
        /// <value></value>
        public required int UserId { get; set; }

        /// <summary>
        /// Usuario que realizó el pedido.
        /// </summary>
        /// <value></value>
        public User User { get; set; } = null!;

        /// <summary>
        /// Fecha de creación del pedido.
        /// </summary>
        /// <value></value>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha de última actualización del pedido.
        /// </summary>
        /// <value></value>
        public required DateTime UpdatedAt { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.Application.Domain.Models
{
    public class CartItem
    {
        /// <summary>
        /// Identificador único del artículo en el carrito.
        /// </summary>
        /// <value></value>
        public int Id { get; set; }
        /// <summary>
        /// Identificador del carrito al que pertenece el artículo.
        /// </summary>
        /// <value></value>
        public int CartId { get; set; }
        /// <summary>
        /// El carrito al que pertenece el artículo.
        /// </summary>
        /// <value></value>
        public Cart Cart { get; set; } = null!;
        /// <summary>
        /// Identificador del producto que representa el artículo en el carrito.
        /// </summary>
        /// <value></value>
        public int ProductId { get; set; }
        /// <summary>
        /// El producto que representa el artículo en el carrito.
        /// </summary>
        /// <value></value>
        public Product Product { get; set; } = null!;
        /// <summary>
        /// Cantidad del producto en el carrito.
        /// </summary>
        /// <value></value>
        public int Quantity { get; set; }
    }
}
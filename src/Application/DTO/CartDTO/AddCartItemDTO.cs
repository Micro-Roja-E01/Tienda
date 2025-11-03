using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.CartDTO
{
    /// <summary>
    /// DTO utilizado para agregar un item al carrito.
    /// </summary>
    public class AddCartItemDTO
    {
        /// <summary>
        /// ID del producto a agregar al carrito.
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = "El ID del producto es requerido.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser un número positivo.")]
        public required int ProductId { get; set; }
        /// <summary>
        /// Cantidad del producto a agregar al carrito.
        /// </summary>
        /// <value></value>

        [Required(ErrorMessage = "La cantidad es requerida.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser un número positivo.")]
        public required int Quantity { get; set; }
    }
}
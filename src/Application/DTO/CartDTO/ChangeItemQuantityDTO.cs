using System.ComponentModel.DataAnnotations;

namespace Tienda.src.Application.DTO.CartDTO
{
    /// <summary>
    /// DTO utilizado para cambiar la cantidad de un item en el carrito.
    /// </summary>
    public class ChangeItemQuantityDTO
    {
        /// <summary>
        /// ID del producto cuyo cantidad se desea cambiar.
        /// </summary>
        /// <value></value>
        [Required(ErrorMessage = "El ID del producto es requerido.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser un número positivo.")]
        public required int ProductId { get; set; }
        /// <summary>
        /// Nueva cantidad del producto.
        /// </summary>
        /// <value></value>

        [Required(ErrorMessage = "La cantidad es requerida.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser un número positivo.")]
        public required int Quantity { get; set; }
    }

}
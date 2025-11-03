namespace Tienda.src.Application.DTO.OrderDTO
{
    /// <summary>
    /// DTO que representa un item individual de producto dentro de una orden de compra.
    /// Almacena la información del producto tal como estaba al momento de la compra (snapshot histórico).
    /// </summary>
    public class OrderItemDTO
    {
        /// <summary>
        /// Título del producto al momento de la compra.
        /// Se guarda como snapshot para mantener histórico incluso si el producto cambia después.
        /// </summary>
        public required string ProductTitle { get; set; }

        /// <summary>
        /// Descripción del producto al momento de la compra.
        /// Se preserva para referencia histórica independiente de cambios futuros en el producto.
        /// </summary>
        public required string ProductDescription { get; set; }

        /// <summary>
        /// URL de la imagen principal del producto al momento de la compra.
        /// Se guarda como referencia histórica del estado visual del producto en ese momento.
        /// </summary>
        public required string MainImageURL { get; set; }

        /// <summary>
        /// Precio del producto al momento de la compra (puede incluir descuentos aplicados).
        /// Formato: string con valor monetario formateado.
        /// Se preserva como histórico ya que los precios pueden cambiar con el tiempo.
        /// </summary>
        public required string PriceAtMoment { get; set; }

        /// <summary>
        /// Cantidad de unidades de este producto compradas en la orden.
        /// </summary>
        public required int Quantity { get; set; }
    }
}
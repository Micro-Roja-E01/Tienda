namespace Tienda.src.Application.DTO.OrderDTO
{
    /// <summary>
    /// DTO que representa los detalles completos de una orden de compra.
    /// Incluye información de la orden, totales y todos los items comprados.
    /// </summary>
    public class OrderDetailDTO
    {
        /// <summary>
        /// Código único de identificación de la orden.
        /// Formato: ORD-YYMMDDHHMMSS-XXX (ejemplo: ORD-251102143025-842)
        /// </summary>
        public required string Code { get; set; }

        /// <summary>
        /// Monto total de la orden con descuentos aplicados.
        /// Formato: string con valor monetario formateado.
        /// </summary>
        public required string Total { get; set; }

        /// <summary>
        /// Monto subtotal de la orden sin descuentos aplicados.
        /// Formato: string con valor monetario formateado.
        /// </summary>
        public required string SubTotal { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó la compra.
        /// Representa el momento exacto en que se creó la orden.
        /// </summary>
        public required DateTime PurchasedAt { get; set; }

        /// <summary>
        /// Lista de items (productos) incluidos en la orden.
        /// Cada item contiene información histórica del producto al momento de la compra.
        /// </summary>
        public required List<OrderItemDTO> Items { get; set; }
    }
}
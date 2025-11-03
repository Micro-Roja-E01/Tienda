namespace Tienda.src.Application.DTO.CartDTO
{
    /// <summary>
    /// DTO que representa el carrito de compras completo con todos sus items y totales calculados.
    /// Soporta tanto carritos de usuarios autenticados como anónimos.
    /// </summary>
    public class CartDTO
    {
        /// <summary>
        /// Identificador único del comprador.
        /// Para usuarios anónimos es un GUID generado en cookie.
        /// Para usuarios autenticados puede ser el ID del usuario o un GUID temporal.
        /// </summary>
        public required string BuyerId { get; set; }

        /// <summary>
        /// ID del usuario autenticado asociado al carrito.
        /// Es null para carritos de usuarios anónimos.
        /// </summary>
        public required int? UserId { get; set; }

        /// <summary>
        /// Lista de items (productos) contenidos en el carrito.
        /// Cada item incluye información del producto, cantidad, precio y subtotales.
        /// </summary>
        public required List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();

        /// <summary>
        /// Precio subtotal del carrito (suma de precios sin descuentos aplicados).
        /// Formato: string con valor monetario formateado.
        /// </summary>
        public required string SubTotalPrice { get; set; }

        /// <summary>
        /// Precio total del carrito (suma de precios con descuentos aplicados).
        /// Formato: string con valor monetario formateado.
        /// </summary>
        public required string TotalPrice { get; set; }

        /// <summary>
        /// Cantidad total de tipos de productos únicos en el carrito.
        /// Por ejemplo, si hay 3 unidades del producto A y 2 del producto B, este valor es 2.
        /// </summary>
        public required int TotalUniqueItemsCount { get; set; }

        /// <summary>
        /// Monto total ahorrado por descuentos aplicados en el carrito.
        /// Calculado como la diferencia entre SubTotal y Total.
        /// </summary>
        public required int TotalSavedAmount { get; set; }
    }
}
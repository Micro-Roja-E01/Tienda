namespace Tienda.src.Application.DTO.CartDTO
{
    /// <summary>
    /// DTO que representa un item individual (producto) dentro del carrito de compras.
    /// Incluye información del producto, cantidad, precios y descuentos aplicados.
    /// </summary>
    public class CartItemDTO
    {
        /// <summary>
        /// Identificador único del producto en el carrito.
        /// </summary>
        public required int ProductId { get; set; }

        /// <summary>
        /// Título o nombre del producto.
        /// </summary>
        public required string ProductTitle { get; set; }

        /// <summary>
        /// URL de la imagen principal del producto.
        /// Puede ser una URL de Cloudinary u otra fuente de almacenamiento.
        /// </summary>
        public required string ProductImageUrl { get; set; }

        /// <summary>
        /// Precio unitario del producto sin descuentos aplicados.
        /// Valor en la moneda base de la aplicación.
        /// </summary>
        public required int Price { get; set; }

        /// <summary>
        /// Cantidad de unidades de este producto en el carrito.
        /// Debe ser mayor a 0. Si es 0 o negativo, el item debería eliminarse del carrito.
        /// </summary>
        public required int Quantity { get; set; }

        /// <summary>
        /// Porcentaje de descuento aplicado al producto (0-100).
        /// Por ejemplo, 15 representa un 15% de descuento.
        /// </summary>
        public required int Discount { get; set; }

        /// <summary>
        /// Precio subtotal del item (Price × Quantity) sin descuentos aplicados.
        /// Formato: string con valor monetario formateado.
        /// </summary>
        public required string SubTotalPrice { get; set; }

        /// <summary>
        /// Precio total del item con descuentos aplicados.
        /// Se calcula como: (Price × Quantity) - (Price × Quantity × Discount / 100).
        /// Formato: string con valor monetario formateado.
        /// </summary>
        public required string TotalPrice { get; set; }
    }

}
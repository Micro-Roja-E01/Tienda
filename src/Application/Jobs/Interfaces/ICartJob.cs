namespace Tienda.src.Application.Jobs.Interfaces
{
    /// <summary>
    /// Interfaz para los trabajos relacionados con el carrito de compras.
    /// </summary>
    public interface ICartJob
    {
        /// <summary>
        /// Envía recordatorios por correo a usuarios con carritos inactivos.
        /// Un carrito se considera inactivo si no ha tenido modificaciones en el número de días configurado.
        /// </summary>
        Task SendCartRemindersAsync();
    }
}
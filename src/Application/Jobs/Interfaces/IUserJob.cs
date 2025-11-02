namespace Tienda.src.Application.Jobs.Interfaces
{
    /// <summary>
    /// Interfaz para los trabajos relacionados con el usuario.
    /// </summary>
    public interface IUserJob
    {
        /// <summary>
        /// Elimina los usuarios que no han confirmado su cuenta dentro del plazo definido.
        /// </summary>
        Task DeleteUnconfirmedAsync();
    }
}
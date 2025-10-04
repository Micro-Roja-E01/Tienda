namespace Tienda.src.Application.Jobs.Interfaces
{
    /// <summary>
    /// Interfaz para los trabajos relacionados con el usuario
    /// </summary>
    public interface IUserJob
    {
        Task DeleteUnconfirmedAsync();
    }
}
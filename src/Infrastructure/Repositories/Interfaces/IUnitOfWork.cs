namespace Tienda.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz para el patrón Unit of Work que gestiona transacciones.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Inicia una transacción de base de datos.
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Confirma todos los cambios realizados en la transacción actual.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Revierte todos los cambios realizados en la transacción actual.
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// Guarda todos los cambios pendientes en el contexto.
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
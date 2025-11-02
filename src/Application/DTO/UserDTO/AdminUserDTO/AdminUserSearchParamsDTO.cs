
namespace Tienda.src.Application.DTO.AdminUserDTO
{
    /// <summary>
    /// DTO que define los parámetros de búsqueda, filtrado y orden para listar usuarios desde el panel de administración.
    /// </summary>
    public class AdminUserSearchParamsDTO
    {
        /// <summary>
        /// Número de página solicitada (1 por defecto).
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Cantidad de registros por página (10 por defecto).
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Filtro por rol. Valores esperados: "Admin" o "Cliente".
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Filtro por estado del usuario. Valores esperados: "active" o "blocked".
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Filtro por correo electrónico (búsqueda parcial).
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Fecha mínima de creación del usuario.
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// Fecha máxima de creación del usuario.
        /// </summary>
        public DateTime? CreatedTo { get; set; }

        /// <summary>
        /// Campo por el que se desea ordenar.
        /// Valores esperados: createdAt | lastLogin | email.
        /// </summary>
        public string? OrderBy { get; set; }

        /// <summary>
        /// Dirección del ordenamiento.
        /// Valores esperados: asc | desc.
        /// </summary>
        public string? OrderDir { get; set; }
    }
}
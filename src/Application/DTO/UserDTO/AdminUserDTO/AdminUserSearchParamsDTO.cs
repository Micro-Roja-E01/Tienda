
namespace Tienda.src.Application.DTO.AdminUserDTO
{
    public class AdminUserSearchParamsDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // filtros
        public string? Role { get; set; }          // "Admin" | "Cliente"
        public string? Status { get; set; }        // "active" | "blocked"
        public string? Email { get; set; }         // búsqueda parcial
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        // orden
        public string? OrderBy { get; set; }       // createdAt | lastLogin | email
        public string? OrderDir { get; set; }      // asc | desc
    }
}

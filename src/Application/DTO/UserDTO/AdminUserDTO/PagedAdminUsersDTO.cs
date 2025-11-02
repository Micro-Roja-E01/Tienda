
namespace Tienda.src.Application.DTO.AdminUserDTO
{
    public class PagedAdminUsersDTO
    {
        public required List<AdminUserListItemDTO> Users { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}

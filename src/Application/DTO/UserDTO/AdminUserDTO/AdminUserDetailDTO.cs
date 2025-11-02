
namespace Tienda.src.Application.DTO.AdminUserDTO
{
    public class AdminUserDetailDTO
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Rut { get; set; }
        public required string Role { get; set; }
        public required string Status { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}

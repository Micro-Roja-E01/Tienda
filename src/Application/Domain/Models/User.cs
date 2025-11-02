using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.Application.Domain.Models
{
    public enum Gender
    {
        Masculino,
        Femenino,
        Otro,
    }

    // para el flujo 9
    public enum UserStatus
    {
        Active,
        Blocked
    }

    public class User : IdentityUser<int>
    {
        public required string Rut { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required Gender Gender { get; set; }
        public required DateTime BirthDate { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // campos para flujo 9
        public UserStatus Status { get; set; } = UserStatus.Active;
        public DateTime? LastLoginAt { get; set; }

        public ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();
    }
}
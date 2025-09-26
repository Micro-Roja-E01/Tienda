using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.Domain.Models;

namespace Tienda.src.Application.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, string roleName, bool rememberMe);
    }
}
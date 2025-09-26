using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.DTO.AuthDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.Application.Services.Implements
{
    public class UserService : IUserService
    {
        public async Task<string> LoginAsync(LoginDTO loginDTO, HttpContext httpContext)
        {
            var ipAdress = httpContext.Connection.RemoteIpAddress?.ToString();
            var user = await _userRepository.GetByEmailAsync(loginDTO.Email);
        }
    }
}
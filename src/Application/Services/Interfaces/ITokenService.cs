using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.Domain.Models;


namespace Tienda.src.Application.Services.Interfaces
{
    /// <summary>
    /// Define los métodos para la generación y manejo de tokens JWT.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Genera un token JWT basado en la información del usuario y su rol.
        /// </summary>
        /// <param name="user">Entidad de usuario autenticado.</param>
        /// <param name="roleName">Nombre del rol asociado al usuario.</param>
        /// <param name="rememberMe">Indica si el token tendrá una duración extendida.</param>
        /// <returns>Token JWT válido.</returns>
        string GenerateToken(User user, string roleName, bool rememberMe);
    }
}
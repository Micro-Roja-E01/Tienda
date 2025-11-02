using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.Application.Services.Implements
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _jwtSecret;

        public TokenService(IConfiguration configuration)
        {
            _configuration =
                configuration
                ?? throw new ArgumentNullException(
                    nameof(configuration),
                    "La configuracio no puede ser nula"
                );
            _jwtSecret =
                _configuration["JWTSecret"]
                ?? throw new InvalidOperationException(
                    "La clave secreta jwt no esta configurada en el appsettings."
                );
        }

        /// <summary>
        /// Genera un token JWT usando los datos del usuario y su rol.
        /// La duración del token depende del parámetro <paramref name="rememberMe"/>.
        /// </summary>
        /// <param name="user">Usuario autenticado.</param>
        /// <param name="roleName">Nombre del rol asignado al usuario.</param>
        /// <param name="rememberMe">
        /// Si es <c>true</c>, el token expira en 24 horas; de lo contrario, en 1 hora.
        /// </param>
        /// <returns>Token JWT serializado listo para ser devuelto al cliente.</returns>
        /// <exception cref="InvalidOperationException">Si no se puede generar el token.</exception>
        public string GenerateToken(User user, string roleName, bool rememberMe)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(ClaimTypes.Role, roleName),
                };
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSecret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddHours(rememberMe ? 24 : 1),
                    signingCredentials: creds
                );
                Log.Information(
                    "Token JWT generado exitosamente para el usuario {Email}",
                    user.Email
                );
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al generar el token JWT para el usuario {UserID}", user.Id);
                throw new InvalidOperationException("No se pudo generar el token JWT.", ex);
            }
        }
    }
}
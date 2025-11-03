using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.Application.Domain.Models
{
    /// <summary>
    /// Representa un rol dentro del sistema de autenticación.
    /// Hereda de IdentityRole con clave primaria entera.
    /// </summary>
    public class Role : IdentityRole<int> { }
}
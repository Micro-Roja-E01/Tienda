using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.API.Controllers
{
    /// <summary>
    /// Controlador base para las operaciones relacionadas con usuarios.
    [Route("api/[controller]")] // Define la ruta base para el controlador, sustituyendo [controller] por el nombre del controlador.
    public class UserController : BaseController { }
}
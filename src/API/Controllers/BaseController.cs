using Microsoft.AspNetCore.Mvc;

namespace Tienda.src.API.Controllers
{
    /// <summary>
    /// Controlador base para la API.
    /// Define la ruta raíz "api/[controller]" y hereda de ControllerBase.
    /// Otros controladores pueden heredar de este para mantener consistencia.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController : ControllerBase { }
}
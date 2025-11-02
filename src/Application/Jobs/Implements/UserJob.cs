using Hangfire;
using Serilog;
using Tienda.src.Application.Jobs.Interfaces;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.Application.Jobs
{
    /// <summary>
    /// Trabajo programado relacionado con operaciones de usuario.
    /// Actualmente se encarga de eliminar usuarios que no han confirmado su correo.
    /// </summary>
    public class UserJob : IUserJob
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Crea una nueva instancia del job de usuario.
        /// </summary>
        /// <param name="userService">Servicio de usuarios utilizado para ejecutar las operaciones.</param>
        /// <param name="_configuration">Configuración de la aplicación (no utilizada actualmente, se inyecta por compatibilidad).</param>
        public UserJob(IUserService userService, IConfiguration _configuration)
        {
            _userService = userService;
        }

        /// <summary>
        /// Elimina de forma periódica los usuarios que aún no han confirmado su cuenta.
        /// El atributo <see cref="AutomaticRetryAttribute"/> controla los reintentos en caso de error.
        /// </summary>
        [AutomaticRetry(Attempts = 10, DelaysInSeconds = new int[] { 60, 120, 300, 600, 900 })]
        public async Task DeleteUnconfirmedAsync()
        {
            Log.Information("Eliminando usuarios no confirmados");
            await _userService.DeleteUnconfirmedAsync();
        }
    }
}

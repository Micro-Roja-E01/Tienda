using Serilog;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.API.Middlewares
{
    /// <summary>
    /// Middleware para manejar operaciones con el carrito de compras con cookies http-only.
    /// Se asegura de que cada cliente tenga un BuyerId persistido en una cookie segura.
    /// </summary>
    public class CartMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly int _cookieExpirationDays;

        /// <summary>
        /// Crea una nueva instancia del middleware de carrito.
        /// Lee desde la configuración la cantidad de días que debe durar la cookie.
        /// </summary>
        /// <param name="next">Delegado del siguiente middleware en el pipeline.</param>
        /// <param name="configuration">Configuración de la aplicación (appsettings).</param>
        /// <exception cref="InvalidOperationException">
        /// Se lanza si no está configurado el valor "CookieExpirationDays".
        /// </exception>
        public CartMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
            _cookieExpirationDays =
                _configuration.GetValue<int?>("CookieExpirationDays")
                ?? throw new InvalidOperationException(
                    "La expiración en días de la cookie no está configurada."
                );
        }

        /// <summary>
        /// Verifica si el request trae la cookie "BuyerId".
        /// Si no existe, genera un nuevo identificador y lo agrega como cookie http-only y segura.
        /// Además, almacena el BuyerId en <see cref="HttpContext.Items"/> para que lo usen los siguientes componentes.
        /// </summary>
        /// <param name="context">Contexto HTTP actual.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            var buyerId = context.Request.Cookies["BuyerId"];

            if (string.IsNullOrEmpty(buyerId))
            {
                Log.Information("No se encontró la cookie de comprador, creando una nueva.");
                buyerId = Guid.CreateVersion7().ToString();

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax, // permite el envío de cookies en solicitudes de origen cruzado
                    Expires = DateTimeOffset.UtcNow.AddDays(_cookieExpirationDays), // extraemos del appsettings la expiración
                    Path = "/", // las cookies serán accesibles desde cualquier ruta
                };
                context.Response.Cookies.Append("BuyerId", buyerId, cookieOptions);
                Log.Information("Se creó una nueva cookie de comprador: {BuyerId}", buyerId);
            }

            // almacenamos el buyerId en el contexto para ser usado en todo el pipeline
            context.Items["BuyerId"] = buyerId;

            await _next(context);
        }
    }
}

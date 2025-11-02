using Serilog;
using System.Net;
using System.Runtime.InteropServices.Marshalling;
using System.Security;
using System.Text.Json;
using Tienda.src.Application.DTO.BaseResponse;

namespace Tienda.src.API.Middlewares
{
    /// <summary>
    /// Middleware global de manejo de excepciones.
    /// Captura cualquier excepción no controlada en el pipeline,
    /// la registra y devuelve una respuesta JSON uniforme con un código HTTP apropiado.
    /// </summary>
    public class ExceptionHandlingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        /// <summary>
        /// Ejecuta el siguiente middleware y captura las excepciones que se produzcan.
        /// Si ocurre una excepción, se mapea a un código HTTP y se devuelve un cuerpo JSON con el detalle.
        /// </summary>
        /// <param name="context">Contexto HTTP actual.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = Guid.NewGuid().ToString();
                context.Response.Headers["trace-id"] = traceId;

                var (statusCode, title) = MapExceptionToStatus(ex);

                ErrorDetail error = new ErrorDetail(title, ex.Message);

                Log.Error(ex, "Excepción no controlada. Trace ID: {TraceId}", traceId);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;

                var json = JsonSerializer.Serialize(
                    error,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                await context.Response.WriteAsync(json);
            }
        }

        /// <summary>
        /// Mapea una excepción conocida a un código de estado HTTP y un título legible.
        /// Si la excepción no es reconocida, devuelve 500 (Error interno del servidor).
        /// </summary>
        /// <param name="ex">Excepción capturada.</param>
        /// <returns>Tupla con el código de estado y el título del error.</returns>
        private static (int, string) MapExceptionToStatus(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException _ => (
                    StatusCodes.Status401Unauthorized,
                    "No autorizado"
                ),
                ArgumentNullException _ => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
                KeyNotFoundException _ => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                InvalidOperationException _ => (
                    StatusCodes.Status409Conflict,
                    "Conflicto de operación"
                ),
                FormatException _ => (StatusCodes.Status400BadRequest, "Formato inválido"),
                SecurityException _ => (StatusCodes.Status403Forbidden, "Acceso prohibido"),
                ArgumentOutOfRangeException _ => (
                    StatusCodes.Status400BadRequest,
                    "Argumento fuera de rango"
                ),
                ArgumentException _ => (StatusCodes.Status400BadRequest, "Argumento inválido"),
                TimeoutException _ => (
                    StatusCodes.Status429TooManyRequests,
                    "Demasiadas solicitudes"
                ),
                JsonException _ => (StatusCodes.Status400BadRequest, "JSON inválido"),
                // parece un código de prueba; lo dejo igual para no cambiar tu lógica
                NullReferenceException => (StatusCodes.Status101SwitchingProtocols, "Test"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor"),
            };
        }
    }
}
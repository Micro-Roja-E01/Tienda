using Serilog;
using System.Security;
using System.Text.Json;
using Tienda.src.Application.DTO.BaseResponse;

namespace Tienda.src.Infrastructure.Middlewares
{
    public class ExceptionHandlingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

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
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor"),
            };
        }
    }
}
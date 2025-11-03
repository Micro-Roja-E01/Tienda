using Hangfire;
using Serilog;
using Tienda.src.Application.Jobs.Interfaces;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.Application.Jobs.Implements
{
    /// <summary>
    /// Trabajo programado relacionado con operaciones de carrito de compras.
    /// Se encarga de enviar recordatorios a usuarios con carritos inactivos.
    /// </summary>
    public class CartJob : ICartJob
    {
        private readonly ICartService _cartService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Crea una nueva instancia del job de carrito.
        /// </summary>
        /// <param name="cartService">Servicio de carrito utilizado para obtener carritos inactivos.</param>
        /// <param name="emailService">Servicio de email utilizado para enviar recordatorios.</param>
        /// <param name="configuration">Configuración de la aplicación.</param>
        public CartJob(
            ICartService cartService,
            IEmailService emailService,
            IConfiguration configuration
        )
        {
            _cartService = cartService;
            _emailService = emailService;
            _configuration = configuration;
        }

        /// <summary>
        /// Envía recordatorios por correo a usuarios con carritos inactivos.
        /// El atributo <see cref="AutomaticRetryAttribute"/> controla los reintentos en caso de error.
        /// </summary>
        [AutomaticRetry(Attempts = 10, DelaysInSeconds = new int[] { 60, 120, 300, 600, 900 })]
        public async Task SendCartRemindersAsync()
        {
            Log.Information("Iniciando envío de recordatorios de carrito");

            var inactiveDays =
                _configuration.GetValue<int?>("Jobs:InactiveDaysForCartReminder")
                ?? throw new InvalidOperationException(
                    "La configuración 'Jobs:InactiveDaysForCartReminder' no está definida."
                );

            Log.Information(
                "Buscando carritos inactivos de más de {InactiveDays} días",
                inactiveDays
            );

            var inactiveCarts = await _cartService.GetInactiveCartsAsync(inactiveDays);

            if (inactiveCarts.Count == 0)
            {
                Log.Information("No se encontraron carritos inactivos para enviar recordatorios");
                return;
            }

            Log.Information(
                "Se encontraron {Count} carritos inactivos. Enviando recordatorios...",
                inactiveCarts.Count
            );

            int successCount = 0;
            int errorCount = 0;

            foreach (var cart in inactiveCarts)
            {
                try
                {
                    Log.Information(
                        "Enviando recordatorio a usuario {UserId} ({Email}), último modificado: {LastModified}",
                        cart.UserId,
                        cart.Email,
                        cart.LastModified
                    );

                    await _emailService.SendCartReminderEmailAsync(cart.Email, cart.UserName);
                    successCount++;

                    Log.Information(
                        "Recordatorio enviado exitosamente a {Email}",
                        cart.Email
                    );
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Log.Error(
                        ex,
                        "Error al enviar recordatorio de carrito a usuario {UserId} ({Email})",
                        cart.UserId,
                        cart.Email
                    );
                }
            }

            Log.Information(
                "Proceso de recordatorios de carrito completado. Exitosos: {SuccessCount}, Errores: {ErrorCount}",
                successCount,
                errorCount
            );
        }
    }
}
using Mapster;

namespace Tienda.src.Application.Mappers
{
    /// <summary>
    /// Clase para extensiones de mapeo.
    /// Contiene configuraciones globales de mapeo.
    /// </summary>
    public class MapperExtensions
    {
        /// <summary>
        /// Registra todas las configuraciones de Mapster que usan los distintos mappers de la aplicación.
        /// Debe llamarse en el arranque (por ejemplo, en Program.cs).
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolver los mappers registrados.</param>
        public static void ConfigureMapster(IServiceProvider serviceProvider)
        {
            var productMapper = serviceProvider.GetService<ProductMapper>();
            productMapper?.ConfigureAllMappings();

            var userMapper = serviceProvider.GetService<UserMapper>();
            userMapper?.ConfigureAllMappings();

            var categoryMapper = serviceProvider.GetService<CategoryMapper>();
            categoryMapper?.ConfigureAllMappings();

            var brandMapper = serviceProvider.GetService<BrandMapper>();
            brandMapper?.ConfigureAllMappings();

            // var cartMapper = serviceProvider.GetService<CartMapper>();
            // cartMapper?.ConfigureAllMappings();

            // var orderMapper = serviceProvider.GetService<OrderMapper>();
            // orderMapper?.ConfigureAllMappings();

            // Configuración global de Mapster para ignorar valores nulos
            TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
        }
    }
}

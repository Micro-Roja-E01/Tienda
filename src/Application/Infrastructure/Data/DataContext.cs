using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tienda.src.Application.Domain.Models;

namespace Tienda.src.Infrastructure.Data
{
    /// <summary>
    /// Contexto de datos para la aplicación, hereda de IdentityDbContext para manejar la identidad de usuarios.
    /// </summary>
    public class DataContext : IdentityDbContext<User, Role, int>
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options) { }

        /// <summary>
        /// Conjunto de entidades de productos.
        /// </summary>
        public DbSet<Product> Products { get; set; } = null!;

        /// <summary>
        /// Conjunto de entidades de imágenes asociadas a productos.
        /// </summary>
        public DbSet<Image> Images { get; set; } = null!;

        /// <summary>
        /// Conjunto de entidades de categorías de productos.
        /// </summary>
        public DbSet<Category> Categories { get; set; } = null!;

        /// <summary>
        /// Conjunto de entidades de marcas.
        /// </summary>
        public DbSet<Brand> Brands { get; set; } = null!;

        /// <summary>
        /// Conjunto de entidades de órdenes de compra.
        /// </summary>
        public DbSet<Order> Orders { get; set; } = null!;

        /// <summary>
        /// Conjunto de entidades de ítems de órdenes.
        /// </summary>
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        /// <summary>
        /// Conjunto de entidades de carritos de compra.
        /// </summary>
        public DbSet<Cart> Carts { get; set; } = null!;

        /// <summary>
        /// Conjunto de entidades de ítems dentro de un carrito.
        /// </summary>
        public DbSet<CartItem> CartItems { get; set; } = null!;

        /// <summary>
        /// Conjunto de entidades de códigos de verificación de usuario.
        /// </summary>
        public DbSet<VerificationCode> VerificationCodes { get; set; } = null!;
    }
}

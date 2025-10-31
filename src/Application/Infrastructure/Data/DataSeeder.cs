using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tienda.src.Application.Domain.Models;

namespace Tienda.src.Infrastructure.Data
{
    public class DataSeeder
    {
        /// <summary>
        /// Método para inicializar la base de datos con datos de prueba.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para obtener el contexto de datos y otros servicios.</param>
        /// <returns>Tarea asíncrona que representa la operación de inicialización.</returns>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                await context.Database.EnsureCreatedAsync();
                await context.Database.MigrateAsync();

                // Creación de roles
                if (!context.Roles.Any())
                {
                    var roles = new List<Role>
                    {
                        new Role { Name = "Admin", NormalizedName = "ADMIN" },
                        new Role { Name = "Cliente", NormalizedName = "CLIENTE" },
                    };
                    foreach (var role in roles)
                    {
                        var result = roleManager.CreateAsync(role).GetAwaiter().GetResult();
                        if (!result.Succeeded)
                        {
                            Log.Error(
                                "Error creando rol {RoleName}: {Errors}",
                                role.Name,
                                string.Join(", ", result.Errors.Select(e => e.Description))
                            );
                            throw new InvalidOperationException(
                                $"No se pudo crear el rol {role.Name}."
                            );
                        }
                    }
                    Log.Information("Roles creados con éxito.");
                }

                // Creación de categorías
                if (!context.Categories.Any())
                {
                    var categories = new List<Category>
                    {
                        new Category { Name = "Electronics", Slug = "electronics", Description = "Electrónica y tecnología" },
                        new Category { Name = "Clothing", Slug = "clothing", Description = "Ropa y accesorios" },
                        new Category { Name = "Home Appliances", Slug = "home-appliances", Description = "Línea blanca y hogar" },
                        new Category { Name = "Books", Slug = "books", Description = "Libros y lectura" },
                        new Category { Name = "Sports", Slug = "sports", Description = "Artículos deportivos" },
                    };
                    await context.Categories.AddRangeAsync(categories);
                    await context.SaveChangesAsync();
                    Log.Information("Categorías creadas con éxito.");
                }
                else
                {
                    // 👇 AQUÍ: actualizar las que quedaron vacías
                    var categoriesWithoutSlug = context.Categories
                        .Where(c => string.IsNullOrEmpty(c.Slug))
                        .ToList();

                    foreach (var c in categoriesWithoutSlug)
                    {
                        c.Slug = GenerateSlug(c.Name);
                    }

                    if (categoriesWithoutSlug.Count > 0)
                    {
                        await context.SaveChangesAsync();
                        Log.Information("Categorías existentes actualizadas con slug.");
                    }
                }
                // Creación de marcas
                if (!await context.Brands.AnyAsync())
                {
                    var brands = new List<Brand>
                    {
                        new Brand { Name = "Sony" },
                        new Brand { Name = "Apple" },
                        new Brand { Name = "HP" },
                    };
                    await context.Brands.AddRangeAsync(brands);
                    await context.SaveChangesAsync();
                    Log.Information("Marcas creadas con éxito.");
                }

                // Creación de usuarios
                if (!await context.Users.AnyAsync())
                {
                    Role customerRole =
                        await context.Roles.FirstOrDefaultAsync(r => r.Name == "Cliente")
                        ?? throw new InvalidOperationException(
                            "El rol de cliente no está configurado."
                        );
                    Role adminRole =
                        await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin")
                        ?? throw new InvalidOperationException(
                            "El rol de administrador no está configurado."
                        );

                    // Creación de usuario administrador
                    User adminUser = new User
                    {
                        FirstName =
                            configuration["User:AdminUser:FirstName"]
                            ?? throw new InvalidOperationException(
                                "El nombre del usuario administrador no está configurado."
                            ),
                        LastName =
                            configuration["User:AdminUser:LastName"]
                            ?? throw new InvalidOperationException(
                                "El apellido del usuario administrador no está configurado."
                            ),
                        Email =
                            configuration["User:AdminUser:Email"]
                            ?? throw new InvalidOperationException(
                                "El email del usuario administrador no está configurado."
                            ),
                        EmailConfirmed = true,
                        Gender = Gender.Masculino,
                        Rut =
                            configuration["User:AdminUser:Rut"]
                            ?? throw new InvalidOperationException(
                                "El RUT del usuario administrador no está configurado."
                            ),
                        BirthDate = DateTime.Parse(
                            configuration["User:AdminUser:BirthDate"]
                                ?? throw new InvalidOperationException(
                                    "La fecha de nacimiento del usuario administrador no está configurada."
                                )
                        ),
                        PhoneNumber =
                            configuration["User:AdminUser:PhoneNumber"]
                            ?? throw new InvalidOperationException(
                                "El número de teléfono del usuario administrador no está configurado."
                            ),
                    };
                    adminUser.UserName = adminUser.Email;
                    var adminPassword =
                        configuration["User:AdminUser:Password"]
                        ?? throw new InvalidOperationException(
                            "La contraseña del usuario administrador no está configurada."
                        );
                    var adminResult = await userManager.CreateAsync(adminUser, adminPassword);
                    if (adminResult.Succeeded)
                    {
                        var roleResult = await userManager.AddToRoleAsync(
                            adminUser,
                            adminRole.Name!
                        );
                        if (!roleResult.Succeeded)
                        {
                            Log.Error(
                                "Error asignando rol de administrador: {Errors}",
                                string.Join(", ", roleResult.Errors.Select(e => e.Description))
                            );
                            throw new InvalidOperationException(
                                "No se pudo asignar el rol de administrador al usuario."
                            );
                        }
                        Log.Information("Usuario administrador creado con éxito.");
                    }
                    else
                    {
                        Log.Error(
                            "Error creando usuario administrador: {Errors}",
                            string.Join(", ", adminResult.Errors.Select(e => e.Description))
                        );
                        throw new InvalidOperationException(
                            "No se pudo crear el usuario administrador."
                        );
                    }
                    // Creación de usuarios aleatorios
                    var randomPassword =
                        configuration["User:RandomUserPassword"]
                        ?? throw new InvalidOperationException(
                            "La contraseña de los usuarios aleatorios no está configurada."
                        );

                    var userFaker = new Faker<User>()
                        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                        .RuleFor(u => u.LastName, f => f.Name.LastName())
                        .RuleFor(u => u.Email, f => f.Internet.Email())
                        .RuleFor(u => u.EmailConfirmed, f => true)
                        .RuleFor(u => u.Gender, f => f.PickRandom<Gender>())
                        .RuleFor(u => u.Rut, f => RandomRut())
                        .RuleFor(u => u.BirthDate, f => f.Date.Past(30, DateTime.Now.AddYears(-18)))
                        .RuleFor(u => u.PhoneNumber, f => RandomPhoneNumber())
                        .RuleFor(u => u.UserName, (f, u) => u.Email);
                    var users = userFaker.Generate(99);
                    foreach (var user in users)
                    {
                        var result = await userManager.CreateAsync(user, randomPassword);

                        if (result.Succeeded)
                        {
                            var roleResult = await userManager.AddToRoleAsync(
                                user,
                                customerRole.Name!
                            );
                            if (!roleResult.Succeeded)
                            {
                                Log.Error(
                                    "Error asignando rol a {Email}: {Errors}",
                                    user.Email,
                                    string.Join(", ", roleResult.Errors.Select(e => e.Description))
                                );
                                throw new InvalidOperationException(
                                    $"No se pudo asignar el rol de cliente al usuario {user.Email}."
                                );
                            }
                        }
                        else
                        {
                            Log.Error(
                                "Error creando usuario {Email}: {Errors}",
                                user.Email,
                                string.Join(", ", result.Errors.Select(e => e.Description))
                            );
                        }
                    }
                    Log.Information("Usuarios creados con éxito.");
                }

                // Creación de productos
                if (!await context.Products.AnyAsync())
                {
                    var categoryIds = await context.Categories.Select(c => c.Id).ToListAsync();
                    var brandIds = await context.Brands.Select(b => b.Id).ToListAsync();

                    if (categoryIds.Any() && brandIds.Any())
                    {
                        var productFaker = new Faker<Product>()
                            .RuleFor(p => p.Title, f => f.Commerce.ProductName())
                            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                            .RuleFor(p => p.Price, f => f.Random.Int(1000, 100000))
                            .RuleFor(p => p.Stock, f => f.Random.Int(1, 100))
                            .RuleFor(p => p.CategoryId, f => f.PickRandom(categoryIds))
                            .RuleFor(p => p.BrandId, f => f.PickRandom(brandIds))
                            .RuleFor(p => p.Status, f => f.PickRandom<Status>());

                        var products = productFaker.Generate(50);
                        await context.Products.AddRangeAsync(products);
                        await context.SaveChangesAsync();
                        Log.Information("Productos creados con éxito.");
                    }

                    // Creación de imágenes
                    if (!await context.Images.AnyAsync())
                    {
                        var productIds = await context.Products.Select(p => p.Id).ToListAsync();
                        var imageFaker = new Faker<Image>()
                            .RuleFor(i => i.ImageUrl, f => f.Image.PicsumUrl())
                            .RuleFor(i => i.PublicId, f => f.Random.Guid().ToString())
                            .RuleFor(i => i.ProductId, f => f.PickRandom(productIds));

                        var images = imageFaker.Generate(20);
                        await context.Images.AddRangeAsync(images);
                        await context.SaveChangesAsync();
                        Log.Information("Imágenes creadas con éxito.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al inicializar la base de datos: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Método para generar un RUT chileno aleatorio.
        /// </summary>
        /// <returns>Un RUT en formato "XXXXXXXX-X".</returns>
        private static string RandomRut()
        {
            var faker = new Faker();
            var rut = faker.Random.Int(1000000, 99999999).ToString();
            var dv = faker.Random.Int(0, 9).ToString();
            return $"{rut}-{dv}";
        }

        /// <summary>
        /// Método para generar un número de teléfono chileno aleatorio.
        /// </summary>
        /// <returns>Un número de teléfono en formato "+569 XXXXXXXX".</returns>
        private static string RandomPhoneNumber()
        {
            var faker = new Faker();
            string firstPartNumber = faker.Random.Int(1000, 9999).ToString();
            string secondPartNumber = faker.Random.Int(1000, 9999).ToString();
            return $"+569 {firstPartNumber}{secondPartNumber}";
        }
        private static string GenerateSlug(string text)
        {
            text = text.Trim().ToLower();
            text = text
                .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");
            text = string.Join("-", text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return text;
        }
    }
}
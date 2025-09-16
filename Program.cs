using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Cryptography;
using Tienda.src.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

# region Logging Configuration
builder.Host.UseSerilog(
    (context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services)
);
# endregion

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

# region Database configuration
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetSection("ConnectionStrings:SqliteDatabase").Value)
);
# endregion

var app = builder.Build();

var _logger = app.Services.GetRequiredService<ILogger<Program>>();
_logger.LogInformation("Configuring Database");
_logger.LogInformation("Application Starting Up");

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.Run();
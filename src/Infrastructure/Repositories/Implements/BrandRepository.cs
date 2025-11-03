using Microsoft.EntityFrameworkCore;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Infrastructure.Data;
using Tienda.src.Infrastructure.Repositories.Interfaces;

namespace Tienda.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Repositorio para la entidad marca.
    /// </summary>
    public class BrandRepository : IBrandRepository
    {
        private readonly DataContext _context;

        public BrandRepository(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene una marca por ID que no esté eliminada lógicamente.
        /// </summary>
        /// <param name="id">ID de la marca.</param>
        /// <returns>La marca o <c>null</c>.</returns>
        public async Task<Brand?> GetByIdAsync(int id)
        {
            return await _context.Brands.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        }

        /// <summary>
        /// Obtiene una marca por nombre, ignorando mayúsculas/minúsculas y que no esté eliminada.
        /// </summary>
        /// <param name="name">Nombre de la marca.</param>
        /// <returns>La marca o <c>null</c>.</returns>
        public async Task<Brand?> GetByNameAsync(string name)
        {
            name = name.Trim();
            return await _context.Brands
                .FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower() && !b.IsDeleted);
        }

        /// <summary>
        /// Agrega una nueva marca al contexto.
        /// </summary>
        /// <param name="brand">Marca a agregar.</param>
        public async Task AddAsync(Brand brand)
        {
            await _context.Brands.AddAsync(brand);
        }

        /// <summary>
        /// Guarda los cambios pendientes en la base de datos.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
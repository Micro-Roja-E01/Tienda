using Mapster;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.BrandDTO;

namespace Tienda.src.Application.Mappers
{
    /// <summary>
    /// Configura los mapeos entre entidades de dominio de marca y sus DTOs.
    /// </summary>
    public class BrandMapper
    {
        /// <summary>
        /// Registra todas las configuraciones de mapeo relacionadas con Brand.
        /// Debe llamarse en el arranque de la aplicación.
        /// </summary>
        public void ConfigureAllMappings()
        {
            ConfigureBrandMappings();
        }

        /// <summary>
        /// Configura los mapeos específicos para la entidad Brand:
        /// dominio → list/detail y DTO → dominio.
        /// </summary>
        private void ConfigureBrandMappings()
        {
            // Domain -> list
            TypeAdapterConfig<Brand, BrandListItemDTO>.NewConfig()
                .Map(dest => dest.ProductCount, _ => 0);

            // Domain -> detail
            TypeAdapterConfig<Brand, BrandDetailDTO>.NewConfig()
                .Map(dest => dest.ProductCount, _ => 0);

            // CreateDTO -> Domain
            TypeAdapterConfig<BrandCreateDTO, Brand>.NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.IsDeleted)
                .Map(dest => dest.Slug, src => GenerateSlug(src.Name))
                .Map(dest => dest.CreatedAt, _ => DateTime.UtcNow);

            // UpdateDTO -> Domain
            TypeAdapterConfig<BrandUpdateDTO, Brand>.NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.IsDeleted)
                .Map(dest => dest.Slug, src => GenerateSlug(src.Name));
        }

        /// <summary>
        /// Genera un slug normalizado a partir del nombre de la marca.
        /// Reemplaza acentos, pasa a minúsculas y separa por guiones.
        /// </summary>
        /// <param name="text">Texto original.</param>
        /// <returns>Slug normalizado.</returns>
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
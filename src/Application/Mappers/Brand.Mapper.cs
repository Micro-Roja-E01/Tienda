// src/Application/Mappers/BrandMapper.cs
using Mapster;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.BrandDTO;

namespace Tienda.src.Application.Mappers
{
    public class BrandMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigureBrandMappings();
        }

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

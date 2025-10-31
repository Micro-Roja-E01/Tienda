using Mapster;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.CategoryDTO;

namespace Tienda.src.Application.Mappers
{
    public class CategoryMapper
    {
        public CategoryMapper() { }

        public void ConfigureAllMappings()
        {
            ConfigureCategoryMappings();
        }

        public void ConfigureCategoryMappings()
        {
            // Domain -> list item
            TypeAdapterConfig<Category, CategoryListItemDTO>.NewConfig()
                .Map(dest => dest.ProductCount, _ => 0); 

            // Domain -> detail
            TypeAdapterConfig<Category, CategoryDetailDTO>.NewConfig()
                .Map(dest => dest.ProductCount, _ => 0);

            // CreateDTO -> Domain
            TypeAdapterConfig<CategoryCreateDTO, Category>.NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.IsDeleted)
                .Map(dest => dest.Slug, src => GenerateSlug(src.Name))
                .Map(dest => dest.CreatedAt, _ => DateTime.UtcNow);

            // UpdateDTO -> Domain
            TypeAdapterConfig<CategoryUpdateDTO, Category>.NewConfig()
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

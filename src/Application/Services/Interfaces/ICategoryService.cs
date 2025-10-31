using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.DTO.CategoryDTO;

namespace Tienda.src.Application.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<PagedCategoriesDTO> GetAllAsync(SearchParamsDTO searchParams);
        Task<CategoryDetailDTO?> GetByIdAsync(int id);
        Task<CategoryDetailDTO> CreateAsync(CategoryCreateDTO dto);
        Task<CategoryDetailDTO?> UpdateAsync(int id, CategoryUpdateDTO dto);
        Task DeleteAsync(int id);
    }
}
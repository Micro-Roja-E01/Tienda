
using tienda.src.Application.DTO.ProductDTO;
using Tienda.src.Application.DTO.BrandDTO;

namespace Tienda.src.Application.Services.Interfaces
{
    public interface IBrandService
    {
        Task<PagedBrandsDTO> GetAllAsync(SearchParamsDTO searchParams);
        Task<BrandDetailDTO?> GetByIdAsync(int id);
        Task<BrandDetailDTO> CreateAsync(BrandCreateDTO dto);
        Task<BrandDetailDTO?> UpdateAsync(int id, BrandUpdateDTO dto);
        Task DeleteAsync(int id);
    }
}

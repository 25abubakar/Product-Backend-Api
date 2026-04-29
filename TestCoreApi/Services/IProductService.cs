using TestCoreApi.Models;
using TestCoreApi.Models.DTOs;

namespace TestCoreApi.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(CreateProductDto dto);
        Task<Product?> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

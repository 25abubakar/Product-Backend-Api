using TestCoreApi.Models;
using TestCoreApi.Models.DTOs;
using TestCoreApi.Repositories;

namespace TestCoreApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Product>> GetAllAsync() =>
            _repository.GetAllAsync();

        public Task<IEnumerable<Product>> GetByCategoryAsync(string category) =>
            _repository.GetByCategoryAsync(category);

        public Task<Product?> GetByIdAsync(int id) =>
            _repository.GetByIdAsync(id);

        public async Task<Product> CreateAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name        = dto.Name,
                Description = dto.Description,
                Price       = dto.Price,
                Stock       = dto.Stock,
                Category    = dto.Category
            };
            return await _repository.CreateAsync(product);
        }

        public async Task<Product?> UpdateAsync(int id, UpdateProductDto dto)
        {
            var updated = new Product
            {
                Name        = dto.Name,
                Description = dto.Description,
                Price       = dto.Price,
                Stock       = dto.Stock,
                Category    = dto.Category
            };
            return await _repository.UpdateAsync(id, updated);
        }

        public Task<bool> DeleteAsync(int id) =>
            _repository.DeleteAsync(id);
    }
}

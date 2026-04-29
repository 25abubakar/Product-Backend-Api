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

        public IEnumerable<Product> GetAll() => _repository.GetAll();

        public Product? GetById(int id) => _repository.GetById(id);

        public Product Create(CreateProductDto dto)
        {
            var product = new Product
            {
                Name        = dto.Name,
                Description = dto.Description,
                Price       = dto.Price,
                Stock       = dto.Stock
            };
            return _repository.Create(product);
        }

        public Product? Update(int id, UpdateProductDto dto)
        {
            var updated = new Product
            {
                Name        = dto.Name,
                Description = dto.Description,
                Price       = dto.Price,
                Stock       = dto.Stock
            };
            return _repository.Update(id, updated);
        }

        public bool Delete(int id) => _repository.Delete(id);
    }
}

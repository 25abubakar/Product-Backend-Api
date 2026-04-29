using Microsoft.EntityFrameworkCore;
using TestCoreApi.Data;
using TestCoreApi.Models;

namespace TestCoreApi.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .OrderBy(p => p.Id)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            return await _context.Products
                .Where(p => p.Category != null && p.Category.ToLower() == category.ToLower())
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            product.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateAsync(int id, Product updated)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing is null) return null;

            existing.Name        = updated.Name;
            existing.Description = updated.Description;
            existing.Price       = updated.Price;
            existing.Stock       = updated.Stock;
            existing.Category    = updated.Category;
            existing.UpdatedAt   = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }
    }
}

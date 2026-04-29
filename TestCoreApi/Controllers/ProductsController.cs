using Microsoft.AspNetCore.Mvc;
using TestCoreApi.DTOs;
using TestCoreApi.Models;
using TestCoreApi.Repositories;

namespace TestCoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repo;

        public ProductsController(IProductRepository repo)
        {
            _repo = repo;
        }

        // GET api/products
        /// <summary>Get all products, optionally filtered by category.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll([FromQuery] string? category)
        {
            var products = string.IsNullOrWhiteSpace(category)
                ? await _repo.GetAllAsync()
                : await _repo.GetByCategoryAsync(category);

            return Ok(products.Select(MapToDto));
        }

        // GET api/products/5
        /// <summary>Get a single product by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product is null)
                return NotFound(new { message = $"Product with id {id} not found." });

            return Ok(MapToDto(product));
        }

        // POST api/products
        /// <summary>Create a new product.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Product
            {
                Name        = dto.Name,
                Description = dto.Description,
                Price       = dto.Price,
                Stock       = dto.Stock,
                Category    = dto.Category
            };

            var created = await _repo.CreateAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }

        // PUT api/products/5
        /// <summary>Fully update an existing product.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Product
            {
                Name        = dto.Name,
                Description = dto.Description,
                Price       = dto.Price,
                Stock       = dto.Stock,
                Category    = dto.Category
            };

            var updated = await _repo.UpdateAsync(id, product);
            if (updated is null)
                return NotFound(new { message = $"Product with id {id} not found." });

            return Ok(MapToDto(updated));
        }

        // DELETE api/products/5
        /// <summary>Delete a product by ID.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Product with id {id} not found." });

            return NoContent();
        }

        // ── helpers ──────────────────────────────────────────────────────────
        private static ProductDto MapToDto(Product p) => new()
        {
            Id          = p.Id,
            Name        = p.Name,
            Description = p.Description,
            Price       = p.Price,
            Stock       = p.Stock,
            Category    = p.Category,
            CreatedAt   = p.CreatedAt,
            UpdatedAt   = p.UpdatedAt
        };
    }
}

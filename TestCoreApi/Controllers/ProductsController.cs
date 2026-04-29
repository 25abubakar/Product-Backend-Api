using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestCoreApi.Models;
using TestCoreApi.Models.DTOs;
using TestCoreApi.Services;

namespace TestCoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    //[Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll([FromQuery] string? category)
        {
            var products = string.IsNullOrWhiteSpace(category)
                ? await _service.GetAllAsync()
                : await _service.GetByCategoryAsync(category);

            return Ok(products.Select(MapToDto));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product is null)
                return NotFound(new { message = $"Product with id {id} not found." });

            return Ok(MapToDto(product));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated is null)
                return NotFound(new { message = $"Product with id {id} not found." });

            return Ok(MapToDto(updated));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Product with id {id} not found." });

            return NoContent();
        }

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

using System.ComponentModel.DataAnnotations;

namespace TestCoreApi.Models.DTOs
{
    /// <summary>Returned to the client — read-only snapshot of a product.</summary>
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>Payload for POST /api/products.</summary>
    public class CreateProductDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 1_000_000.00, ErrorMessage = "Price must be between 0.01 and 1,000,000.")]
        public decimal Price { get; set; }

        [Range(0, 100_000, ErrorMessage = "Stock must be between 0 and 100,000.")]
        public int Stock { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }
    }

    public class UpdateProductDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 1_000_000.00, ErrorMessage = "Price must be between 0.01 and 1,000,000.")]
        public decimal Price { get; set; }

        [Range(0, 100_000, ErrorMessage = "Stock must be between 0 and 100,000.")]
        public int Stock { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }
    }
}

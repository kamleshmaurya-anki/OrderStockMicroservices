using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class CreateProductRequest
{
    [Required, StringLength(150, MinimumLength = 1)]
    public string ProductName { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Price must be >= 0")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "StockQty must be >= 0")]
    public int StockQty { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs;

public class CreateOrderRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
}

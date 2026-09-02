namespace OrderService.Exceptions;

public class InsufficientStockException : AppExceptionBase
{
    public InsufficientStockException(Guid productId, int requested, int available)
        : base($"Cannot place order: requested quantity ({requested}) exceeds available stock ({available}) for product '{productId}'.",
               StatusCodes.Status409Conflict)
    {
    }
}

namespace ProductService.Exceptions;

public class InsufficientStockException : AppExceptionBase
{
    public int AvailableStock { get; }

    public InsufficientStockException(Guid productId, int requested, int available)
        : base($"Insufficient stock for product '{productId}'. Requested: {requested}, Available: {available}.", StatusCodes.Status409Conflict)
    {
        AvailableStock = available;
    }
}

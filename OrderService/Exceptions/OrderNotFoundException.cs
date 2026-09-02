namespace OrderService.Exceptions;

public class OrderNotFoundException : AppExceptionBase
{
    public OrderNotFoundException(Guid orderId)
        : base($"Order with id '{orderId}' was not found.", StatusCodes.Status404NotFound)
    {
    }
}

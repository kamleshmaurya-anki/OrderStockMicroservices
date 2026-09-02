namespace OrderService.Exceptions;

public abstract class AppExceptionBase : Exception
{
    public int StatusCode { get; }

    protected AppExceptionBase(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}

namespace ProductService.Exceptions;

// Marker base for exceptions that should be treated as client (4xx) errors
// by the global exception handler, as opposed to unexpected server errors.
public abstract class AppExceptionBase : Exception
{
    public int StatusCode { get; }

    protected AppExceptionBase(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}

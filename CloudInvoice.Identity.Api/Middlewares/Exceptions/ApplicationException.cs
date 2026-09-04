namespace CloudInvoice.Identity.Api.Middlewares.Exceptions;

/// <summary>
/// Exceção base customizada para a aplicação
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; set; }

    public AppException(string message, int statusCode = 500) : base(message)
    {
        StatusCode = statusCode;
    }
}

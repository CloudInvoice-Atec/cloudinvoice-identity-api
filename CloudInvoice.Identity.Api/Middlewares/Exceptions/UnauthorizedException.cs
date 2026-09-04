namespace CloudInvoice.Identity.Api.Middlewares.Exceptions;

/// <summary>
/// Exceção lançada quando há erro de autenticação
/// </summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Autenticação necessária.")
        : base(message, 401) { }
}

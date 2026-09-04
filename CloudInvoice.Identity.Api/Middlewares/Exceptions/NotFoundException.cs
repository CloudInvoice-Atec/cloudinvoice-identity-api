namespace CloudInvoice.Identity.Api.Middlewares.Exceptions;

/// <summary>
/// Exceção lançada quando um recurso não é encontrado
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}

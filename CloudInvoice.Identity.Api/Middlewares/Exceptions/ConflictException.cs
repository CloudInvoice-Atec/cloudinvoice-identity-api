namespace CloudInvoice.Identity.Api.Middlewares.Exceptions;

/// <summary>
/// Exceção lançada quando há conflito nos dados (ex.: recurso duplicado)
/// </summary>
public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}

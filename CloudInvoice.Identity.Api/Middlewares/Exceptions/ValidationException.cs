namespace CloudInvoice.Identity.Api.Middlewares.Exceptions;

/// <summary>
/// Exceção lançada quando há erro de validação
/// </summary>
public class ValidationException : AppException
{
    public Dictionary<string, string[]> Errors { get; set; }

    public ValidationException(string message, Dictionary<string, string[]> errors = null) : base(message, 400)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }
}

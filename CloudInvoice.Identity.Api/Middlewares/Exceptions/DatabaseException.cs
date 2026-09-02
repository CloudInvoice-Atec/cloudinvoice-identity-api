namespace CloudInvoice.Identity.Api.Middlewares.Exceptions;

/// <summary>
/// Exceção lançada quando há erro de operação na base de dados
/// </summary>
public class DatabaseException : AppException
{
    public DatabaseException(string message, Exception innerException = null)
        : base(message, 500)
    {
        if (innerException != null)
        {
            Data["InnerException"] = innerException.Message;
        }
    }
}

namespace CloudInvoice.Identity.Api.Middlewares.Exceptions;

/// <summary>
/// Exceção lançada quando o utilizador não tem permissão
/// </summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Acesso negado. Você não tem permissão para realizar esta ação.")
        : base(message, 403) { }
}

namespace CloudInvoice.Identity.Application.Interfaces;
{
    public interface IEmailService
    {
        Task EnviarEmailAsync(string emailDestino, string assunto, string mensagemFormatoHtml);
    }
}

using System.Net;
using System.Net.Mail;
using CloudInvoice.Identity.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CloudInvoice.Identity.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string emailDestino, string assunto, string mensagemHtml)
        {
            var emailOrigem = _config["EmailSettings:Mail"];
            var passwordApp = _config["EmailSettings:Password"];
            var servidorSmtp = _config["EmailSettings:Host"];
            var porta = int.Parse(_config["EmailSettings:Port"] ?? "587");
            var displayName = _config["EmailSettings:DisplayName"] ?? "CloudInvoice";

            var carta = new MailMessage();
            carta.From = new MailAddress(emailOrigem, displayName);
            carta.To.Add(emailDestino);
            carta.Subject = assunto;
            carta.Body = mensagemHtml;
            carta.IsBodyHtml = true;

            using (var smtp = new SmtpClient(servidorSmtp, porta))
            {
                smtp.Credentials = new NetworkCredential(emailOrigem, passwordApp);
                smtp.EnableSsl = true;

                try
                {
                    await smtp.SendMailAsync(carta);
                    Console.WriteLine("[SUCESSO] Email enviado com sucesso para: " + emailDestino);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERRO SMTP DO GMAIL]: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine("[DETALHE INTERNO]: " + ex.InnerException.Message);
                    }
                }
            }
        }
    }
}
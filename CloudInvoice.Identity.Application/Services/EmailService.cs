using System.Net;
using System.Net.Mail;

namespace CloudInvoice.Identity.Application.Interfaces;
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarEmailAsync(string emailDestino, string assunto, string mensagemHtml)
        {
            var emailOrigem = _config["ConfiguracoesEmail:EmailOrigem"];
            var passwordApp = _config["ConfiguracoesEmail:PasswordApp"];
            var servidorSmtp = _config["ConfiguracoesEmail:ServidorSmtp"];
            var porta = int.Parse(_config["ConfiguracoesEmail:Porta"]);

            var carta = new MailMessage();
            carta.From = new MailAddress(emailOrigem, "TranspoTech");
            carta.To.Add(emailDestino);
            carta.Subject = assunto;
            carta.Body = mensagemHtml;
            carta.IsBodyHtml = true; 

            using (var smtp = new SmtpClient(servidorSmtp, porta))
            {
                smtp.Credentials = new NetworkCredential(emailOrigem, passwordApp);
                smtp.EnableSsl = true; 

                await smtp.SendMailAsync(carta);
            }
        }
    }
}

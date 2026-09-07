using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using CloudInvoice.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CloudInvoice.Identity.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private const string LogoContentId = "cloudinvoice-logo";
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _environment;

        public EmailService(IConfiguration config, IWebHostEnvironment environment)
        {
            _config = config;
            _environment = environment;
        }

        public async Task SendEmailAsync(string emailDestino, string assunto, string mensagemHtml)
        {
            var emailOrigem = _config["EmailSettings:Mail"];
            var passwordApp = _config["EmailSettings:Password"];
            var servidorSmtp = _config["EmailSettings:Host"];
            var porta = int.Parse(_config["EmailSettings:Port"] ?? "587");
            var displayName = _config["EmailSettings:DisplayName"] ?? "CloudInvoice";

            using var carta = new MailMessage();
            carta.From = new MailAddress(emailOrigem!, displayName);
            carta.To.Add(emailDestino);
            carta.Subject = assunto;
            carta.IsBodyHtml = true;

            var plainView = AlternateView.CreateAlternateViewFromString(mensagemHtml, Encoding.UTF8, MediaTypeNames.Text.Html);
            var logoPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "assets", "images", "logo-horizontal.png");

            if (File.Exists(logoPath))
            {
                var logoResource = new LinkedResource(logoPath, "image/png")
                {
                    ContentId = LogoContentId,
                    TransferEncoding = TransferEncoding.Base64
                };

                logoResource.ContentType.Name = "logo-horizontal.png";
                plainView.LinkedResources.Add(logoResource);
            }

            carta.AlternateViews.Add(plainView);
            carta.Body = mensagemHtml;

            using var smtp = new SmtpClient(servidorSmtp, porta)
            {
                Credentials = new NetworkCredential(emailOrigem, passwordApp),
                EnableSsl = true
            };

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
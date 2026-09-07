namespace CloudInvoice.Identity.Application.Email;

public static class EmailTemplates
{
    private const string BrandName = "CloudInvoice";
    private const string BrandTagline = "Gestão Financeira Inteligente e Segura";
    private const string LogoContentId = "cloudinvoice-logo";
    private const string BackgroundColor = "#f8f9fa";
    private const string ContainerBackground = "#ffffff";
    private const string AccentColor = "#727cf5";
    private const string TitleColor = "#313a46";
    private const string TextColor = "#6c757d";
    private const string MutedColor = "#adb5bd";
    private const string BorderColor = "#e9ecef";

    public static string GetWelcomeEmail(string nome, string linkParaEmail)
    {
        return BuildEmail(
            headerHtml: $"<img src='cid:{LogoContentId}' alt='{BrandName}' class='logo' />",
            title: "Ativação de Conta",
            greeting: $"Bem-vindo à equipa, {nome}!",
            introduction: "O seu perfil de acesso à plataforma <b>CloudInvoice</b> foi criado com sucesso pelo Administrador do sistema.",
            message: "Para garantir a segurança da sua conta, por favor clique no botão abaixo para definir a sua palavra-passe pessoal e ativar o seu acesso:",
            actionText: "Definir a Minha Palavra-passe",
            actionUrl: linkParaEmail,
            actionHelp: "Se o botão não funcionar, copie e cole este link no seu navegador:",
            actionLinkText: linkParaEmail,
            footer: $"&copy; {DateTime.Now.Year} CloudInvoice - Gestão Financeira Inteligente e Segura.<br>Este é um email automático, por favor não responda."
        );
    }

    public static string GetPasswordResetEmail(string nome, string linkParaReset)
    {
        return BuildEmail(
            headerHtml: $"<img src='cid:{LogoContentId}' alt='{BrandName}' class='logo' />",
            title: "Recuperação de Palavra-passe",
            greeting: $"Olá, <b>{nome}</b>!",
            introduction: "Recebemos um pedido para redefinir a palavra-passe associada à tua conta na plataforma <b>CloudInvoice</b>.",
            message: "Se fizeste este pedido, clica no botão abaixo para definir uma nova palavra-passe:",
            actionText: "Redefinir Palavra-passe",
            actionUrl: linkParaReset,
            actionHelp: "Se o botão não funcionar, copia e cole este link no teu navegador:",
            actionLinkText: linkParaReset,
            footer: $"&copy; {DateTime.Now.Year} CloudInvoice. Todos os direitos reservados.<br>Este é um email automático, por favor não responda.",
            disclaimer: "Se não solicitaste esta alteração, podes ignorar este e-mail com segurança. A tua palavra-passe atual manter-se-á inalterada."
        );
    }

    private static string BuildEmail(
        string headerHtml,
        string title,
        string greeting,
        string introduction,
        string message,
        string actionText,
        string actionUrl,
        string actionHelp,
        string actionLinkText,
        string footer,
        string? disclaimer = null)
    {
        var disclaimerHtml = string.IsNullOrWhiteSpace(disclaimer)
            ? string.Empty
            : $"<p class='disclaimer'>{disclaimer}</p>";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: {BackgroundColor};
            margin: 0;
            padding: 0;
        }}

        .email-container {{
            max-width: 600px;
            margin: 40px auto;
            background-color: {ContainerBackground};
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 15px rgba(0,0,0,0.05);
            border: 1px solid {BorderColor};
        }}

        .header {{
            background-color: {ContainerBackground};
            padding: 32px 40px;
            text-align: center;
            border-bottom: 1px solid #f1f3f5;
        }}

        .logo {{
            max-height: 50px;
            width: auto;
            display: inline-block;
        }}

        .brand-title {{
            font-size: 26px;
            font-weight: 800;
            color: {AccentColor};
            text-decoration: none;
            letter-spacing: -0.5px;
            margin: 0;
        }}

        .brand-subtitle {{
            font-size: 12px;
            color: {TextColor};
            margin-top: 4px;
            text-transform: uppercase;
            letter-spacing: 1px;
        }}

        .content {{
            padding: 40px;
            color: #333333;
            line-height: 1.6;
        }}

        .content h2 {{
            color: {TitleColor};
            margin-top: 0;
            font-size: 24px;
        }}

        .content p {{
            font-size: 16px;
            color: {TextColor};
        }}

        .btn-container {{
            text-align: center;
            margin: 35px 0;
        }}

        .btn {{
            display: inline-block;
            background-color: {AccentColor};
            color: #ffffff !important;
            text-decoration: none;
            padding: 14px 30px;
            font-size: 16px;
            font-weight: bold;
            border-radius: 8px;
            box-shadow: 0 2px 6px rgba(114, 124, 245, 0.4);
        }}

        .helper-link {{
            font-size: 14px;
            color: {TextColor};
        }}

        .helper-link a {{
            color: {AccentColor};
            word-break: break-all;
        }}

        .disclaimer {{
            font-size: 13px;
            color: {MutedColor};
            margin-top: 30px;
        }}

        .footer {{
            background-color: {BackgroundColor};
            padding: 20px 40px;
            text-align: center;
            font-size: 13px;
            color: {MutedColor};
            border-top: 1px solid {BorderColor};
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            {headerHtml}
        </div>

        <div class='content'>
            <h2>{title}</h2>
            <p>{greeting}</p>
            <p>{introduction}</p>
            <p>{message}</p>

            <div class='btn-container'>
                <a href='{actionUrl}' class='btn'>{actionText}</a>
            </div>

            <p class='helper-link'>
                {actionHelp}<br>
                <a href='{actionLinkText}'>{actionLinkText}</a>
            </p>

            {disclaimerHtml}
        </div>

        <div class='footer'>
            <p>{footer}</p>
        </div>
    </div>
</body>
</html>";
    }
}

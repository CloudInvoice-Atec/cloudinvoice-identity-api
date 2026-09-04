namespace CloudInvoice.Identity.Infrastructure.Services;

public static class EmailTemplates
{
    public static string GetWelcomeEmail(string nome, string linkParaEmail, string logoUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f8f9fa;
            margin: 0;
            padding: 0;
        }}

        .email-container {{
            max-width: 600px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 15px rgba(0,0,0,0.05);
            border: 1px solid #e9ecef;
        }}

        .header {{
            background-color: #ffffff;
            padding: 30px 40px;
            text-align: center;
            border-bottom: 1px solid #f1f3f5;
        }}

        .header img {{
            max-height: 50px;
            width: auto;
            display: inline-block;
        }}

        .content {{
            padding: 40px;
            color: #333333;
            line-height: 1.6;
        }}

        .content h2 {{
            color: #313a46;
            margin-top: 0;
            font-size: 24px;
        }}

        .content p {{
            font-size: 16px;
            color: #6c757d;
        }}

        .btn-container {{
            text-align: center;
            margin: 35px 0;
        }}

        .btn {{
            display: inline-block;
            background-color: #727cf5;
            color: #ffffff !important;
            text-decoration: none;
            padding: 14px 30px;
            font-size: 16px;
            font-weight: bold;
            border-radius: 8px;
            box-shadow: 0 2px 6px rgba(114, 124, 245, 0.4);
        }}

        .footer {{
            background-color: #f8f9fa;
            padding: 20px 40px;
            text-align: center;
            font-size: 13px;
            color: #adb5bd;
            border-top: 1px solid #e9ecef;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <!-- Logo horizontal completo com a nuvem e subtítulo -->
            <img src='{logoUrl}' alt='CloudInvoice' />
        </div>

        <div class='content'>
            <h2>Bem-vindo à equipa, {nome}!</h2>
            <p>O seu perfil de acesso à plataforma <b>CloudInvoice</b> foi criado com sucesso pelo Administrador do sistema.</p>
            <p>Para garantir a segurança da sua conta, por favor clique no botão abaixo para definir a sua palavra-passe pessoal e ativar o seu acesso:</p>

            <div class='btn-container'>
                <a href='{linkParaEmail}' class='btn'>Definir a Minha Palavra-passe</a>
            </div>

            <p style='font-size: 14px; color: #6c757d;'>
                Se o botão não funcionar, copie e cole este link no seu navegador:<br>
                <a href='{linkParaEmail}' style='color: #727cf5; word-break: break-all;'>{linkParaEmail}</a>
            </p>
        </div>

        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} CloudInvoice - Gestão Financeira Inteligente e Segura.<br>Este é um email automático, por favor não responda.</p>
        </div>
    </div>
</body>
</html>";
    }
}
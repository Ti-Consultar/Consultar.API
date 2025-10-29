using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace _4_InfraData._3_Utils.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string emailAddress, string newPassword)
        {
            try
            {
                var email = _configuration["EmailSettings:Email"];
                var password = _configuration["EmailSettings:Password"];

                var smtpClient = CreateSmtpClient(email, password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(email),
                    Subject = "🔐 Redefinição de Senha - MRP",
                    IsBodyHtml = true,
                    Body = BuildPasswordResetEmailHtml(newPassword)
                };

                mailMessage.To.Add(emailAddress);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail de redefinição de senha: {ex.Message}");
                throw;
            }
        }

        public async Task SendWelcomeAsync(string emailAddress, string company, string name)
        {
            try
            {
                var email = _configuration["EmailSettings:Email"];
                var password = _configuration["EmailSettings:Password"];

                var smtpClient = CreateSmtpClient(email, password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(email),
                    Subject = "🎉 Bem-vindo ao MRP!",
                    IsBodyHtml = true,
                    Body = BuildWelcomeEmailHtml(company, name)
                };

                mailMessage.To.Add(emailAddress);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail de boas-vindas: {ex.Message}");
                throw;
            }
        }

        public async Task SendWelcomeSubCompanyAsync(string emailAddress, string company, string subcompany, string name)
        {
            try
            {
                var email = _configuration["EmailSettings:Email"];
                var password = _configuration["EmailSettings:Password"];

                var smtpClient = CreateSmtpClient(email, password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(email),
                    Subject = "🎉 Bem-vindo ao MRP!",
                    IsBodyHtml = true,
                    Body = BuildWelcomeSubCompanyEmailHtml(company, name, subcompany)
                };

                mailMessage.To.Add(emailAddress);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail de boas-vindas: {ex.Message}");
                throw;
            }
        }


        public async Task SendUserWelcomeAsync(string emailAddress, string name, string password)
        {
            try
            {
                var email = _configuration["EmailSettings:Email"];
                var passwordEmail = _configuration["EmailSettings:Password"];

                var smtpClient = CreateSmtpClient(email, passwordEmail);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(email),
                    Subject = "👋 Bem-vindo ao MRP!",
                    IsBodyHtml = true,
                    Body = BuildUserWelcomeEmailHtml(name, emailAddress, password)
                };

                mailMessage.To.Add(emailAddress);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail de boas-vindas ao usuário: {ex.Message}");
                throw;
            }
        }
        private string BuildUserWelcomeEmailHtml(string name, string email, string password)
        {
            // 🔗 URL do sistema em produção
            var systemUrl = "https://dev.consultarmrp.com.br/login";

            return $@"
            <html>
            <head>
                {GetEmailStyles()}
            </head>
            <body>
                <div class='container'>
                    <div class='card'>
                        <div class='title'>Bem-vindo ao MRP, {name}!</div>
                        <div class='subtitle'>Sua conta foi criada com sucesso 🎉</div>
                        <p>Agora você já pode acessar o sistema e explorar todos os recursos disponíveis.</p>

                        <p>Use as credenciais abaixo para fazer login:</p>
                        <div class='password-box'>
                            <div><strong>E-mail:</strong> {email}</div>
                            <div><strong>Senha:</strong> {password}</div>
                        </div>

                        <p style='text-align:center; margin-top:25px;'>
                            <a href='{systemUrl}' 
                               style='background-color:#007BFF;
                                      color:white;
                                      padding:12px 25px;
                                      border-radius:8px;
                                      text-decoration:none;
                                      font-weight:bold;
                                      font-size:16px;
                                      display:inline-block;
                                      box-shadow:0 4px 8px rgba(0, 123, 255, 0.3);'>
                                👉 Acessar o Sistema
                            </a>
                        </p>

                        <p style='text-align:center; margin-top:20px; font-size:14px; color:#555;'>
                            Recomendamos que altere sua senha após o primeiro acesso por segurança.
                        </p>

                        <div class='footer'>
                            {DateTime.Now.Year} MRP © - Todos os direitos reservados.
                        </div>
                    </div>
                </div>
            </body>
            </html>";
        }




        private SmtpClient CreateSmtpClient(string email, string password)
        {
            return new SmtpClient("smtp.outlook.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true
            };
        }

        private string GetEmailStyles() => @"
            <style>
                body {
                    background-color: #f4f9fc;
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    margin: 0;
                    padding: 0;
                }
                .container {
                    max-width: 600px;
                    margin: 50px auto;
                    background-color: #ffffff;
                    border-radius: 12px;
                    box-shadow: 0 8px 16px rgba(0, 123, 255, 0.2);
                    padding: 30px;
                    color: #333;
                }
                .header {
                    text-align: center;
                    padding-bottom: 20px;
                }
                .header h1 {
                    color: #007BFF;
                    font-size: 26px;
                }
                .content p {
                    font-size: 16px;
                    line-height: 1.6;
                }
                .password-box {
                    margin: 20px 0;
                    padding: 15px;
                    background-color: #e6f0ff;
                    border-left: 6px solid #007BFF;
                    font-size: 18px;
                    font-weight: bold;
                    color: #007BFF;
                    text-align: center;
                    border-radius: 6px;
                }
                .footer {
                    margin-top: 30px;
                    text-align: center;
                    font-size: 12px;
                    color: #999;
                }
                .title {
                    font-size: 24px;
                    font-weight: bold;
                    color: #007BFF;
                    margin-bottom: 10px;
                }
                .subtitle {
                    font-size: 16px;
                    margin-bottom: 20px;
                }
                .card {
                    background-color: #fff;
                    padding: 30px;
                    border-radius: 10px;
                    box-shadow: 0 4px 8px rgba(0,0,0,0.1);
                    color: #333;
                }
            </style>";

        private string BuildPasswordResetEmailHtml(string password) => $@"
            <html>
            <head>
                {GetEmailStyles()}
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Redefinição de Senha</h1>
                    </div>
                    <div class='content'>
                        <p>Olá,</p>
                        <p>Recebemos uma solicitação para redefinir a sua senha. Aqui está a sua nova senha de acesso ao sistema MRP:</p>
                        <div class='password-box'>{password}</div>
                        <p>Recomendamos que você altere esta senha após fazer login, para manter sua conta segura.</p>
                        <p>Se você não solicitou esta alteração, entre em contato com o suporte imediatamente.</p>
                        <p>Abraços,<br>Equipe MRP</p>
                    </div>
                    <div class='footer'>
                      {DateTime.Now.Year} MRP © - Todos os direitos reservados.
                    </div>
                </div>
            </body>
            </html>";

        private string BuildWelcomeEmailHtml(string company, string name) => $@"
            <html>
            <head>
                {GetEmailStyles()}
            </head>
            <body>
                <div class='container'>
                    <div class='card'>
                        <div class='title'>Bem-vindo ao MRP!</div>
                        <div class='subtitle'>Parabéns {name} por cadastrar sua nova empresa <strong>{company}</strong> no nosso sistema.</div>
                        <p>Estamos felizes em ter você com a gente. Explore todos os recursos do MRP e otimize a gestão do seu negócio!</p>
                        <p>Se precisar de ajuda, nossa equipe está à disposição.</p>
                        <div class='footer'> {DateTime.Now.Year} MRP © - Todos os direitos reservados.</div>
                    </div>
                </div>
            </body>
            </html>";

        private string BuildWelcomeSubCompanyEmailHtml(string company, string name, string subcompany) => $@"
            <html>
            <head>
                {GetEmailStyles()}
            </head>
            <body>
                <div class='container'>
                    <div class='card'>
                        <div class='title'>Bem-vindo ao MRP!</div>
                        <div class='subtitle'>
                            Olá {name}, parabéns por expandir seus negócios! 🎉<br />
                            A nova filial <strong>{subcompany}</strong> da empresa <strong>{company}</strong> foi cadastrada com sucesso em nossa plataforma.
                        </div>
                        <p>
                            É um prazer fazer parte do crescimento da sua empresa. Aproveite todos os recursos que o MRP oferece para tornar sua gestão ainda mais eficiente.
                        </p>
                        <p>
                            Caso precise de suporte ou tenha dúvidas, nossa equipe está sempre pronta para ajudar.
                        </p>
                        <div class='footer'>
                            {DateTime.Now.Year} MRP © - Todos os direitos reservados.
                        </div>
                    </div>
                </div>
            </body>
            </html>";

    }
}

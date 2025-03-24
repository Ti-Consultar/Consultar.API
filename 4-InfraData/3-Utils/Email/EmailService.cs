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

                var smtpClient = new SmtpClient("smtp.outlook.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(email, password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(email),
                    Subject = "Redefinição de Senha",
                    IsBodyHtml = true,
                    Body = $@"
                                <html>
                                <body>
                                    <p>Olá,</p>
                                    <p>Recebemos uma solicitação para redefinir a sua senha. Aqui está a sua nova senha:</p>
                                    <p><strong>Nova senha:</strong> {newPassword}</p>
                                    <p>Lembre-se de atualizar sua senha ao fazer login para garantir a segurança da sua conta.</p>
                                    <p>Obrigado,</p>
                                    <p>Consultar Consultoria</p>
                                </body>
                                </html>"
                                
                };

                mailMessage.To.Add(emailAddress);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Aqui você pode tratar a exceção de acordo com os requisitos do seu aplicativo.
                // Por exemplo, registrar a exceção ou notificar o administrador do sistema.
                Console.WriteLine($"Erro ao enviar e-mail de redefinição de senha: {ex.Message}");
                throw; // Lança a exceção para que o chamador possa lidar com ela conforme necessário.
            }
        }
    }
}

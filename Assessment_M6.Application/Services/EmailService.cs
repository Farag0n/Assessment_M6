using System.Net;
using System.Net.Mail;
using Assessment_M6.Application.DTOs;
using Assessment_M6.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Assessment_M6.Application.Services;

public class EmailService : IEmailService
{
    private readonly EmailDtos.EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailDtos.EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailDtos.EmailMessage emailMessage)
    {
        try
        {
            var message = new MimeMessage();//se crea un objeto que representa el mensaje
            //establece el remitente
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            //se agrega un destinatario
            message.To.Add(new MailboxAddress(emailMessage.ToName, emailMessage.ToEmail));
            
            message.Subject = emailMessage.Subject;

            var bodyBuilder = new BodyBuilder();
            if (emailMessage.IsHtml)
            {
                bodyBuilder.HtmlBody = emailMessage.Body;
            }
            else
            {
                bodyBuilder.TextBody = emailMessage.Body;
            }
            
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            
            await client.ConnectAsync(
                _emailSettings.SmtpServer, 
                _emailSettings.SmtpPort, 
                _emailSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            
            await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email enviado exitosamente a {Email}", emailMessage.ToEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email a {Email}: {Message}", emailMessage.ToEmail, ex.Message);
            throw new ApplicationException($"Error al enviar email: {ex.Message}", ex);
        }
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string toName, string username)
    {
        var subject = "¡Bienvenido a Assessment_M6!";
        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .footer {{ margin-top: 20px; padding: 10px; text-align: center; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>¡Bienvenido a Assessment_M6!</h1>
                    </div>
                    <div class='content'>
                        <p>Hola <strong>{toName}</strong>,</p>
                        <p>Tu registro ha sido exitoso con el nombre de usuario: <strong>{username}</strong></p>
                        <p>Ya puedes iniciar sesión en nuestra plataforma y comenzar a gestionar empleados y departamentos.</p>
                        <p>Si no realizaste este registro, por favor ignora este correo.</p>
                        <br>
                        <p>Saludos,<br>El equipo de Assessment_M6</p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} Assessment_M6. Todos los derechos reservados.</p>
                    </div>
                </div>
            </body>
            </html>";

        var emailMessage = new EmailDtos.EmailMessage
        {
            ToEmail = toEmail,
            ToName = toName,
            Subject = subject,
            Body = body,
            IsHtml = true
        };

        await SendEmailAsync(emailMessage);
    }
}
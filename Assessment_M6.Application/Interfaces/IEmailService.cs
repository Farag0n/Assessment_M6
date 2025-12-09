using Assessment_M6.Application.DTOs;

namespace Assessment_M6.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(EmailDtos.EmailMessage emailMessage);
    Task SendWelcomeEmailAsync(string toEmail, string toName, string username);
}
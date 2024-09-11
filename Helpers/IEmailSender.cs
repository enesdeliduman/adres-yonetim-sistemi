using Microsoft.AspNetCore.Mvc;

namespace AYS.Helpers
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
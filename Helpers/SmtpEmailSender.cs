using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace AYS.Helpers
{

    public class SmtpEmailSender : IEmailSender
    {
        private string? _host;
        private int _port;
        private bool _ssl;
        private string? _username;
        private string? _password;

        public SmtpEmailSender(string? host, int port, bool ssl, string? username, string? password)
        {
            _host = host;
            _port = port;
            _ssl = ssl;
            _username = username;
            _password = password;
        }
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_username, _password),
                EnableSsl = _ssl,

            };
            return client.SendMailAsync(new MailMessage(_username, email, subject, message
                + "<br>"
                + "Teşekkür ederiz"
            )
            { IsBodyHtml = true });
        }
    }
}
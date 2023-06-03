using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace OwListy.Services
{
    public static class EmailService
    {
        public static async Task SendEmail(string To, string Subject, string Body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("OwListy", Settings.EmailUsername));
            message.To.Add(new MailboxAddress("", To));
            message.Subject = Subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = Body;
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(Settings.EmailHost, Settings.EmailPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(Settings.EmailUsername, Settings.EmailPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
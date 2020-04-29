using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ADSBackend.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713

    public class EmailSender : IEmailSender
    {
        private IConfiguration Configuration { get; set; }

        public EmailSender(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
            SmtpClient client = new SmtpClient("smtp.mail.yahoo.com")
            {
                UseDefaultCredentials = false,
                Port = 465,
                EnableSsl = true,
                Credentials = new NetworkCredential("fblamanager@yahoo.com", config["EmailPassword"])
            };

            MailMessage mailMessage = new MailMessage
            {
                IsBodyHtml = true,
                From = new MailAddress("fblamanager@yahoo.com", "ADS Backend"),
                Body = message,
                Subject = subject,
            };
            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }
}
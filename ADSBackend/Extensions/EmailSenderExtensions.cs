using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ADSBackend.Services
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Confirm your email for FBLA Manager", $"Welcome to FBLA Manager! Please confirm your account through this link: <a href='{HtmlEncoder.Default.Encode(link)}'>Confirmation</a>");
        }
    }
}

using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Services
{
    public class FakeEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Geliştirme ortamı için e-posta gönderimi yapılmaz.
            return Task.CompletedTask;
        }
    }
} 
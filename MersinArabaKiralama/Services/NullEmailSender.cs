using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MersinArabaKiralama.Services
{
    /// <summary>
    /// Basit "no-op" e-posta gönderici. Gerçek e-posta altyapısı kurulu değilse
    /// Identity UI kayıt ve şifre sıfırlama işlemlerinin hata vermemesini sağlar.
    /// </summary>
    public class NullEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Burada gerçek SMTP ya da başka e-posta servisine entegrasyon yapabilirsin.
            // Şimdilik hiçbir şey yapma.
            return Task.CompletedTask;
        }
    }
}

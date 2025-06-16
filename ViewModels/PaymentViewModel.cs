using System.ComponentModel.DataAnnotations;
using MersinAracKiralama.Models;

namespace MersinAracKiralama.ViewModels
{
    public class PaymentViewModel
    {
        public Car Car { get; set; } = null!;

        [Required, Display(Name = "Kart Üzerindeki İsim")]
        public string CardName { get; set; } = string.Empty;

        [Required, Display(Name = "Kart Numarası"), CreditCard]
        public string CardNumber { get; set; } = string.Empty;

        [Required, Display(Name = "Son Kullanma (AA/YY)")]
        public string Expiration { get; set; } = string.Empty;

        [Required, Display(Name = "CVV")]
        public string CVV { get; set; } = string.Empty;
    }
}

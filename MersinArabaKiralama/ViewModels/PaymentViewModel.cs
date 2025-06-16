using MersinArabaKiralama.Models;
using System.ComponentModel.DataAnnotations;

namespace MersinArabaKiralama.ViewModels
{
    public class PaymentViewModel
    {
        public Car? Car { get; set; }
        
        [Display(Name = "Kiralama Günü")]
        public int RentalDays { get; set; } = 1;

        [Display(Name = "Toplam Fiyat")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Kart üzerindeki isim gereklidir.")]
        [Display(Name = "Kart Üzerindeki İsim")]
        public string? CardName { get; set; }

        [Required(ErrorMessage = "Kart numarası gereklidir.")]
        [Display(Name = "Kart Numarası")]
        [CreditCard(ErrorMessage = "Geçerli bir kart numarası giriniz.")]
        public string? CardNumber { get; set; }

        [Required(ErrorMessage = "Son kullanma tarihi gereklidir.")]
        [Display(Name = "Son Kullanma Tarihi (AA/YY)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Geçerli bir son kullanma tarihi giriniz (AA/YY).")]
        public string? Expiration { get; set; }

        [Required(ErrorMessage = "CVV gereklidir.")]
        [Display(Name = "CVV")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Geçerli bir CVV giriniz (3 veya 4 haneli).")]
        public string? CVV { get; set; }
    }
}
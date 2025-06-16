using MersinArabaKiralama.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MersinArabaKiralama.ViewModels
{
    public class PaymentViewModel
    {
        public int? CarId { get; set; }
        public Car? Car { get; set; }
        
        [Display(Name = "Kiralama Başlangıç Tarihi")]
        [Required(ErrorMessage = "Kiralama başlangıç tarihi gereklidir.")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }
        
        [Display(Name = "Kiralama Bitiş Tarihi")]
        [Required(ErrorMessage = "Kiralama bitiş tarihi gereklidir.")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Kiralama Günü")]
        public int RentalDays { get; set; } = 1;

        [Display(Name = "Günlük Fiyat")]
        [DataType(DataType.Currency)]
        public decimal DailyPrice { get; set; }

        [Display(Name = "Toplam Fiyat")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Para Birimi")]
        public string Currency { get; set; } = "TRY";

        [Required(ErrorMessage = "Kart üzerindeki isim gereklidir.")]
        [Display(Name = "Kart Üzerindeki İsim")]
        [StringLength(100, ErrorMessage = "İsim en fazla 100 karakter olabilir.")]
        public string CardName { get; set; }

        [Required(ErrorMessage = "Kart numarası gereklidir.")]
        [Display(Name = "Kart Numarası")]
        [CreditCard(ErrorMessage = "Geçerli bir kart numarası giriniz.")]
        [RegularExpression(@"^[0-9]{13,19}$", ErrorMessage = "Geçerli bir kart numarası giriniz (13-19 hane).")]
        [JsonIgnore] // Hassas bilgi olduğu için loglarda görünmesin
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Son kullanma ayı gereklidir.")]
        [Display(Name = "Son Kullanma Ayı")]
        [Range(1, 12, ErrorMessage = "Geçerli bir ay giriniz (1-12).")]
        public int? ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Son kullanma yılı gereklidir.")]
        [Display(Name = "Son Kullanma Yılı")]
        [Range(2023, 2040, ErrorMessage = "Geçerli bir yıl giriniz.")]
        public int? ExpiryYear { get; set; }

        [JsonIgnore]
        public DateTime ExpiryDate => 
            (ExpiryMonth.HasValue && ExpiryYear.HasValue) 
                ? new DateTime(2000 + ExpiryYear.Value, ExpiryMonth.Value, 1).AddMonths(1).AddDays(-1)
                : DateTime.MinValue;

        [Required(ErrorMessage = "CVV gereklidir.")]
        [Display(Name = "CVV")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Geçerli bir CVV giriniz (3 veya 4 haneli).")]
        [JsonIgnore] // Hassas bilgi olduğu için loglarda görünmesin
        public string CVV { get; set; }

        [Display(Name = "Taksit Sayısı")]
        [Range(1, 12, ErrorMessage = "Taksit sayısı 1-12 arasında olmalıdır.")]
        public int Installment { get; set; } = 1;

        [Display(Name = "Taksitli Toplam Tutar")]
        [DataType(DataType.Currency)]
        public decimal TotalWithInstallment => TotalPrice * (1 + (Installment - 1) * 0.02m); // %2 vade farkı

        [Display(Name = "Aylık Taksit Tutarı")]
        [DataType(DataType.Currency)]
        public decimal MonthlyInstallment => TotalWithInstallment / Installment;
    }
}
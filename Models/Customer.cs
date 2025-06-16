using System.ComponentModel.DataAnnotations;

namespace MersinAracKiralama.Models
{
    // Müşteri modelini temsil eder
    public class Customer
    {
        [Key]
        public int Id { get; set; } // Müşteri kimliği
        [Required]
        public string Name { get; set; } // Ad Soyad
        [Required, EmailAddress]
        public string Email { get; set; } // E-posta
    }
} 
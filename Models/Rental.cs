using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MersinAracKiralama.Models
{
    // Kiralama modelini temsil eder
    public class Rental
    {
        [Key]
        public int Id { get; set; } // Kiralama kimliği
        [Required]
        public int CarId { get; set; } // Araç
        [ForeignKey("CarId")]
        public Car Car { get; set; }
        [Required]
        public int CustomerId { get; set; } // Müşteri
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
        [Required]
        public DateTime StartDate { get; set; } // Başlangıç tarihi
        [Required]
        public DateTime EndDate { get; set; } // Bitiş tarihi
        [Range(0, 100000)]
        public decimal TotalPrice { get; set; } // Toplam fiyat
    }
} 
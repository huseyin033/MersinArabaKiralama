using System.ComponentModel.DataAnnotations;

namespace MersinAracKiralama.Models
{
    // Araç modelini temsil eder
    public class Car
    {
        [Key]
        public int Id { get; set; } // Araç kimliği
        [Required]
        public string Brand { get; set; } // Marka
        [Required]
        public string Model { get; set; } // Model
        [Range(2000, 2100)]
        public int Year { get; set; } // Yıl
        [Range(0, 10000)]
        public decimal DailyPrice { get; set; } // Günlük fiyat
        public bool IsAvailable { get; set; } // Müsaitlik
    }
} 
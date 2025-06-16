using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MersinAracKiralama.Models
{
    // Rezervasyon modelini temsil eder
    public class Reservation
    {
        [Key]
        public int Id { get; set; } // Rezervasyon kimliği

        [Required]
        public int CarId { get; set; } // Kiralanan araç
        [ForeignKey("CarId")]
        public Car Car { get; set; }

        [Required]
        public string UserId { get; set; } // Rezervasyonu yapan kullanıcı
        // Identity ile ilişki kurulacak

        [Required]
        public DateTime StartDate { get; set; } // Başlangıç tarihi

        [Required]
        public DateTime EndDate { get; set; } // Bitiş tarihi

        [Range(0, 100000)]
        public decimal TotalPrice { get; set; } // Toplam ücret

        [Required, StringLength(100)]
        public string PickupLocation { get; set; } // Teslim alma noktası

        [Required, StringLength(100)]
        public string DropoffLocation { get; set; } // Teslim bırakma noktası
    }
} 
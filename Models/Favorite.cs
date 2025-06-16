using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MersinAracKiralama.Models
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public Car Car { get; set; } = null!;
        [Required]
        public string UserId { get; set; } = string.Empty; // Identity user Id
    }
}

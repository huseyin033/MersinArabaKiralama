using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MersinArabaKiralama.Models
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public Car? Car { get; set; }
    }
}
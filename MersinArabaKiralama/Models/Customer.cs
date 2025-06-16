namespace MersinArabaKiralama.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserId { get; set; } = null!;
    }
}
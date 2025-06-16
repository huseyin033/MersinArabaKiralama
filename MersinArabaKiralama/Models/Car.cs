namespace MersinArabaKiralama.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public decimal DailyPrice { get; set; }
        public bool IsAvailable { get; set; }
        public string Description { get; set; } = null!;
        public int SeatCount { get; set; }
        public string Transmission { get; set; } = null!; // Manuel/Otomatik
        public string FuelType { get; set; } = null!; // Benzin/Dizel/Elektrik
        public string ImageUrl { get; set; } = null!;
    }
}
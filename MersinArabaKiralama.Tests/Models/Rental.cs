using System;

namespace MersinArabaKiralama.Tests.Models
{
    public class Rental
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public RentalStatus Status { get; set; }
        public string CancellationReason { get; set; }
        public DateTime? ReturnDate { get; set; }
    }

    public enum RentalStatus
    {
        Pending,
        Confirmed,
        InProgress,
        Completed,
        Cancelled
    }
}

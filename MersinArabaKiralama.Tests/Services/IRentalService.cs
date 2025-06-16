using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MersinArabaKiralama.Tests.Models;

namespace MersinArabaKiralama.Tests.Services
{
    public interface IRentalService
    {
        Task<Rental> GetRentalByIdAsync(int id);
        Task<IEnumerable<Rental>> GetRentalsByCustomerIdAsync(int customerId);
        Task<IEnumerable<Rental>> GetRentalsByCarIdAsync(int carId);
        Task<IEnumerable<Rental>> GetRentalsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Rental> AddRentalAsync(Rental rental);
        Task<bool> UpdateRentalAsync(Rental rental);
        Task<bool> CancelRentalAsync(int rentalId, string reason);
        Task<bool> CompleteRentalAsync(int rentalId);
    }
}

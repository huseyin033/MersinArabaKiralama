using System;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Tests.Services
{
    public interface ICarService
    {
        Task<bool> IsCarAvailableAsync(int carId, DateTime startDate, DateTime endDate, int? excludedRentalId = null);
        Task<bool> UpdateCarStatusAsync(int carId, bool isAvailable);
    }
}

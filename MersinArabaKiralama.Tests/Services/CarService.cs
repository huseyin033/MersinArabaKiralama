using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MersinArabaKiralama.Tests.Models;

namespace MersinArabaKiralama.Tests.Services
{
    public class CarService : ICarService
    {
        private readonly ILogger<CarService> _logger;

        public CarService(ILogger<CarService> logger)
        {
            _logger = logger;
        }

        public Task<bool> IsCarAvailableAsync(int carId, DateTime startDate, DateTime endDate, int? excludedRentalId = null)
        {
            _logger.LogInformation($"Checking availability for car {carId} from {startDate} to {endDate}");
            
            // Basit bir uygulama: Her zaman true döndür (gerçek uygulamada veritabanı kontrolü yapılır)
            return Task.FromResult(true);
        }

        public Task<bool> UpdateCarStatusAsync(int carId, bool isAvailable)
        {
            _logger.LogInformation($"Updating car {carId} availability to {isAvailable}");
            
            // Basit bir uygulama: Her zaman başarılı olarak kabul et
            return Task.FromResult(true);
        }
    }
}

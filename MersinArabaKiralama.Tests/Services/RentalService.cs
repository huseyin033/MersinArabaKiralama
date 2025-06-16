using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MersinArabaKiralama.Tests.Models;

namespace MersinArabaKiralama.Tests.Services
{
    public class RentalService : IRentalService
    {
        private readonly ILogger<RentalService> _logger;
        private readonly ICarService _carService;
        private readonly ApplicationDbContext _context;

        public RentalService(
            ApplicationDbContext context, 
            ILogger<RentalService> logger, 
            ICarService carService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _carService = carService ?? throw new ArgumentNullException(nameof(carService));
        }

        public async Task<Rental> GetRentalByIdAsync(int id)
        {
            _logger.LogInformation($"Getting rental with ID: {id}");
            return await _context.Rentals.FindAsync(id);
        }

        public async Task<IEnumerable<Rental>> GetRentalsByCustomerIdAsync(int customerId)
        {
            _logger.LogInformation($"Getting rentals for customer ID: {customerId}");
            return await _context.Rentals
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rental>> GetRentalsByCarIdAsync(int carId)
        {
            _logger.LogInformation($"Getting rentals for car ID: {carId}");
            return await _context.Rentals
                .Where(r => r.CarId == carId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rental>> GetRentalsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation($"Getting rentals between {startDate} and {endDate}");
            return await _context.Rentals
                .Where(r => r.StartDate <= endDate && r.EndDate >= startDate)
                .OrderBy(r => r.StartDate)
                .ToListAsync();
        }

        public async Task<Rental> AddRentalAsync(Rental rental)
        {
            if (rental == null)
                throw new ArgumentNullException(nameof(rental));

            _logger.LogInformation($"Adding new rental for car {rental.CarId} and customer {rental.CustomerId}");

            // Arabanın müsait olup olmadığını kontrol et
            var isAvailable = await _carService.IsCarAvailableAsync(
                rental.CarId, rental.StartDate, rental.EndDate);

            if (!isAvailable)
            {
                _logger.LogWarning($"Car {rental.CarId} is not available for the selected dates");
                throw new InvalidOperationException("The selected car is not available for the selected dates.");
            }

            // Toplam ücreti hesapla (gerçek uygulamada daha karmaşık hesaplamalar yapılabilir)
            var days = (rental.EndDate - rental.StartDate).Days;
            var car = await _context.Cars.FindAsync(rental.CarId);
            rental.TotalPrice = days * car.DailyPrice;

            // Kiralama durumunu ayarla
            rental.Status = RentalStatus.Confirmed;

            // Kiralama kaydını ekle
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            // Arabanın durumunu güncelle
            await _carService.UpdateCarStatusAsync(rental.CarId, false);

            _logger.LogInformation($"Rental {rental.Id} created successfully");
            return rental;
        }

        public async Task<bool> UpdateRentalAsync(Rental rental)
        {
            if (rental == null)
                throw new ArgumentNullException(nameof(rental));

            _logger.LogInformation($"Updating rental {rental.Id}");

            var existingRental = await _context.Rentals.FindAsync(rental.Id);
            if (existingRental == null)
            {
                _logger.LogWarning($"Rental {rental.Id} not found");
                return false;
            }

            // Güncelleme işlemleri
            _context.Entry(existingRental).CurrentValues.SetValues(rental);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Rental {rental.Id} updated successfully");
            return true;
        }

        public async Task<bool> CancelRentalAsync(int rentalId, string reason)
        {
            _logger.LogInformation($"Cancelling rental {rentalId}");

            var rental = await _context.Rentals.FindAsync(rentalId);
            if (rental == null)
            {
                _logger.LogWarning($"Rental {rentalId} not found");
                return false;
            }

            // Sadece onaylı veya bekleyen kiralamalar iptal edilebilir
            if (rental.Status != RentalStatus.Confirmed && rental.Status != RentalStatus.Pending)
            {
                _logger.LogWarning($"Rental {rentalId} cannot be cancelled because its status is {rental.Status}");
                return false;
            }

            // Kiralama durumunu iptal olarak güncelle
            rental.Status = RentalStatus.Cancelled;
            rental.CancellationReason = reason;

            // Arabanın durumunu müsait olarak güncelle
            await _carService.UpdateCarStatusAsync(rental.CarId, true);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Rental {rentalId} cancelled successfully");
            return true;
        }

        public async Task<bool> CompleteRentalAsync(int rentalId)
        {
            _logger.LogInformation($"Completing rental {rentalId}");

            var rental = await _context.Rentals.FindAsync(rentalId);
            if (rental == null)
            {
                _logger.LogWarning($"Rental {rentalId} not found");
                return false;
            }

            // Sadece onaylı kiralamalar tamamlanabilir
            if (rental.Status != RentalStatus.Confirmed)
            {
                _logger.LogWarning($"Rental {rentalId} cannot be completed because its status is {rental.Status}");
                return false;
            }

            // Kiralama durumunu tamamlandı olarak güncelle
            rental.Status = RentalStatus.Completed;
            rental.ReturnDate = DateTime.Now;

            // Arabanın durumunu müsait olarak güncelle
            await _carService.UpdateCarStatusAsync(rental.CarId, true);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Rental {rentalId} completed successfully");
            return true;
        }
    }
}

using MersinArabaKiralama.Data;
using MersinArabaKiralama.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Services
{
    public class RentalService : IRentalService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RentalService> _logger;
        private readonly ICarService _carService;

        public RentalService(
            ApplicationDbContext context, 
            ILogger<RentalService> logger,
            ICarService carService)
        {
            _context = context;
            _logger = logger;
            _carService = carService;
        }

        public async Task<IEnumerable<Rental>> GetAllRentalsAsync(QueryParameters parameters)
        {
            try
            {
                _logger.LogInformation("Tüm kiralama kayıtları getiriliyor");
                
                var query = _context.Rentals
                    .Include(r => r.Car)
                    .Include(r => r.Customer)
                    .AsQueryable();

                // Arama terimi varsa uygula
                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                {
                    var searchTerm = parameters.SearchTerm.ToLower();
                    query = query.Where(r => 
                        r.Customer.FirstName.ToLower().Contains(searchTerm) ||
                        r.Customer.LastName.ToLower().Contains(searchTerm) ||
                        r.Customer.Email.ToLower().Contains(searchTerm) ||
                        r.Car.Brand.ToLower().Contains(searchTerm) ||
                        r.Car.Model.ToLower().Contains(searchTerm));
                }

                // Sıralama
                if (!string.IsNullOrEmpty(parameters.OrderBy))
                {
                    query = parameters.OrderBy.ToLower() switch
                    {
                        "startdate" => parameters.Descending ? 
                            query.OrderByDescending(r => r.StartDate) : 
                            query.OrderBy(r => r.StartDate),
                        "enddate" => parameters.Descending ? 
                            query.OrderByDescending(r => r.EndDate) : 
                            query.OrderBy(r => r.EndDate),
                        "totalprice" => parameters.Descending ? 
                            query.OrderByDescending(r => r.TotalPrice) : 
                            query.OrderBy(r => r.TotalPrice),
                        _ => query.OrderBy(r => r.Id)
                    };
                }
                else
                {
                    query = query.OrderByDescending(r => r.StartDate);
                }

                // Sayfalama
                return await query
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kiralama kayıtları getirilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<Rental?> GetRentalByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"{id} ID'li kiralama kaydı getiriliyor");
                return await _context.Rentals
                    .Include(r => r.Car)
                    .Include(r => r.Customer)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li kiralama kaydı getirilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<IEnumerable<Rental>> GetUserRentalsAsync(string userId)
        {
            try
            {
                _logger.LogInformation($"{userId} ID'li kullanıcının kiralama kayıtları getiriliyor");
                return await _context.Rentals
                    .Include(r => r.Car)
                    .Include(r => r.Customer)
                    .Where(r => r.Customer.UserId == userId)
                    .OrderByDescending(r => r.StartDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{userId} ID'li kullanıcının kiralama kayıtları getirilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<IEnumerable<Rental>> GetCustomerRentalsAsync(int customerId)
        {
            try
            {
                _logger.LogInformation($"{customerId} ID'li müşterinin kiralama kayıtları getiriliyor");
                return await _context.Rentals
                    .Include(r => r.Car)
                    .Where(r => r.CustomerId == customerId)
                    .OrderByDescending(r => r.StartDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{customerId} ID'li müşterinin kiralama kayıtları getirilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<IEnumerable<Rental>> GetCarRentalsAsync(int carId)
        {
            try
            {
                _logger.LogInformation($"{carId} ID'li aracın kiralama kayıtları getiriliyor");
                return await _context.Rentals
                    .Include(r => r.Customer)
                    .Where(r => r.CarId == carId)
                    .OrderByDescending(r => r.StartDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{carId} ID'li aracın kiralama kayıtları getirilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<IEnumerable<Rental>> GetRentalsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation($"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} tarihleri arasındaki kiralama kayıtları getiriliyor");
                return await _context.Rentals
                    .Include(r => r.Car)
                    .Include(r => r.Customer)
                    .Where(r => 
                        (r.StartDate >= startDate && r.StartDate <= endDate) ||
                        (r.EndDate >= startDate && r.EndDate <= endDate) ||
                        (r.StartDate <= startDate && r.EndDate >= endDate))
                    .OrderBy(r => r.StartDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} tarihleri arasındaki kiralama kayıtları getirilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<bool> IsCarAvailableForRentAsync(int carId, DateTime startDate, DateTime endDate, int? excludeRentalId = null)
        {
            try
            {
                // Tarih kontrolü
                if (startDate >= endDate)
                {
                    _logger.LogWarning("Başlangıç tarihi bitiş tarihinden önce olmalıdır");
                    return false;
                }

                // Arabanın aktif kiralama durumunu kontrol et
                var hasActiveRentals = await _context.Rentals
                    .Where(r => r.CarId == carId && 
                              r.Id != excludeRentalId &&
                              r.Status != RentalStatus.Cancelled &&
                              r.Status != RentalStatus.Completed &&
                              ((r.StartDate >= startDate && r.StartDate < endDate) ||
                               (r.EndDate > startDate && r.EndDate <= endDate) ||
                               (r.StartDate <= startDate && r.EndDate >= endDate)))
                    .AnyAsync();

                return !hasActiveRentals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{carId} ID'li aracın {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} tarihleri arasındaki müsaitlik durumu kontrol edilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<Rental> AddRentalAsync(Rental rental)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validasyonlar
                if (rental.StartDate >= rental.EndDate)
                    throw new InvalidOperationException("Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");

                // Aracın müsait olup olmadığını kontrol et
                var isCarAvailable = await IsCarAvailableForRentAsync(
                    rental.CarId, 
                    rental.StartDate, 
                    rental.EndDate);

                if (!isCarAvailable)
                    throw new InvalidOperationException("Seçilen tarihler arasında araç müsait değil.");

                // Toplam ücreti hesapla
                var car = await _context.Cars.FindAsync(rental.CarId);
                if (car == null)
                    throw new InvalidOperationException("Araç bulunamadı.");

                var rentalDays = (rental.EndDate - rental.StartDate).Days;
                rental.TotalPrice = car.DailyPrice * rentalDays;

                // Varsayılan değerleri ayarla
                rental.RentalDate = DateTime.UtcNow;
                rental.Status = RentalStatus.Pending;

                // Kiralama kaydını ekle
                await _context.Rentals.AddAsync(rental);
                await _context.SaveChangesAsync();

                // Arabanın durumunu güncelle
                await _carService.UpdateCarStatusAsync(rental.CarId, false);

                await transaction.CommitAsync();
                _logger.LogInformation($"{rental.Id} ID'li kiralama kaydı başarıyla eklendi");
                
                return rental;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Kiralama kaydı eklenirken bir hata oluştu");
                throw;
            }
        }

        public async Task<bool> UpdateRentalAsync(Rental rental)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingRental = await _context.Rentals.FindAsync(rental.Id);
                if (existingRental == null)
                    return false;

                // Sadece belirli durumlarda güncellemeye izin ver
                if (existingRental.Status != RentalStatus.Pending)
                    throw new InvalidOperationException("Sadece bekleyen kiralama kayıtları güncellenebilir.");

                // Eğer tarih değiştiyse müsaitlik kontrolü yap
                if (existingRental.StartDate != rental.StartDate || 
                    existingRental.EndDate != rental.EndDate ||
                    existingRental.CarId != rental.CarId)
                {
                    var isCarAvailable = await IsCarAvailableForRentAsync(
                        rental.CarId, 
                        rental.StartDate, 
                        rental.EndDate,
                        rental.Id);

                    if (!isCarAvailable)
                        throw new InvalidOperationException("Seçilen tarihler arasında araç müsait değil.");

                    // Eğer araç değiştiyse eski aracın durumunu güncelle
                    if (existingRental.CarId != rental.CarId)
                    {
                        await _carService.UpdateCarStatusAsync(existingRental.CarId, true);
                        await _carService.UpdateCarStatusAsync(rental.CarId, false);
                    }
                }

                // Toplam ücreti güncelle
                if (existingRental.StartDate != rental.StartDate || 
                    existingRental.EndDate != rental.EndDate ||
                    existingRental.CarId != rental.CarId)
                {
                    var car = await _context.Cars.FindAsync(rental.CarId);
                    if (car == null)
                        throw new InvalidOperationException("Araç bulunamadı.");

                    var rentalDays = (rental.EndDate - rental.StartDate).Days;
                    rental.TotalPrice = car.DailyPrice * rentalDays;
                }

                // Değişiklikleri uygula
                _context.Entry(existingRental).CurrentValues.SetValues(rental);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                _logger.LogInformation($"{rental.Id} ID'li kiralama kaydı başarıyla güncellendi");
                
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"{rental.Id} ID'li kiralama kaydı güncellenirken bir hata oluştu");
                throw;
            }
        }

        public async Task<bool> CancelRentalAsync(int id, string cancellationReason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var rental = await _context.Rentals.FindAsync(id);
                if (rental == null)
                    return false;

                // Sadece belirli durumlarda iptale izin ver
                if (rental.Status == RentalStatus.Cancelled)
                    return true; // Zaten iptal edilmiş

                if (rental.Status == RentalStatus.Completed || rental.Status == RentalStatus.Returned)
                    throw new InvalidOperationException("Tamamlanmış veya iade edilmiş kiralama kayıtları iptal edilemez.");

                // Kiralama durumunu güncelle
                rental.Status = RentalStatus.Cancelled;
                rental.CancellationReason = cancellationReason;
                rental.CancellationDate = DateTime.UtcNow;

                // Arabanın durumunu güncelle
                await _carService.UpdateCarStatusAsync(rental.CarId, true);


                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation($"{id} ID'li kiralama kaydı başarıyla iptal edildi");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"{id} ID'li kiralama kaydı iptal edilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<bool> CompleteRentalAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var rental = await _context.Rentals.FindAsync(id);
                if (rental == null)
                    return false;

                // Sadece bekleyen kiralama işlemleri tamamlanabilir
                if (rental.Status != RentalStatus.Pending)
                    throw new InvalidOperationException("Sadece bekleyen kiralama işlemleri tamamlanabilir.");

                // Kiralama durumunu güncelle
                rental.Status = RentalStatus.Completed;
                rental.PickupDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation($"{id} ID'li kiralama kaydı başarıyla tamamlandı olarak işaretlendi");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"{id} ID'li kiralama kaydı tamamlanırken bir hata oluştu");
                throw;
            }
        }

        public async Task<bool> FinishRentalAsync(int id, string returnNotes, decimal? additionalCharges)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var rental = await _context.Rentals
                    .Include(r => r.Car)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (rental == null)
                    return false;

                // Sadece tamamlanmış kiralama işlemleri sonlandırılabilir
                if (rental.Status != RentalStatus.Completed)
                    throw new InvalidOperationException("Sadece tamamlanmış kiralama işlemleri sonlandırılabilir.");

                // Kiralama durumunu güncelle
                rental.Status = RentalStatus.Returned;
                rental.ReturnDate = DateTime.UtcNow;
                rental.ReturnNotes = returnNotes;
                
                // Ek ücretleri uygula
                if (additionalCharges.HasValue && additionalCharges.Value > 0)
                {
                    rental.AdditionalCharges = additionalCharges.Value;
                    rental.TotalPrice += additionalCharges.Value;
                }

                // Arabanın durumunu güncelle
                await _carService.UpdateCarStatusAsync(rental.CarId, true);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation($"{id} ID'li kiralama kaydı başarıyla sonlandırıldı");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"{id} ID'li kiralama kaydı sonlandırılırken bir hata oluştu");
                throw;
            }
        }

        public async Task<bool> DeleteRentalAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var rental = await _context.Rentals.FindAsync(id);
                if (rental == null)
                    return false;

                // Sadece iptal edilmiş veya tamamlanmış kiralama kayıtları silinebilir
                if (rental.Status != RentalStatus.Cancelled && 
                    rental.Status != RentalStatus.Returned)
                {
                    throw new InvalidOperationException("Sadece iptal edilmiş veya tamamlanmış kiralama kayıtları silinebilir.");
                }

                _context.Rentals.Remove(rental);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                _logger.LogInformation($"{id} ID'li kiralama kaydı başarıyla silindi");
                
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"{id} ID'li kiralama kaydı silinirken bir hata oluştu");
                throw;
            }
        }
    }
}
using MersinArabaKiralama.Data;
using MersinArabaKiralama.Exceptions;
using MersinArabaKiralama.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Services
{
    public class CarService : ICarService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CarService> _logger;

        public CarService(ApplicationDbContext context, ILogger<CarService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Car>> GetAllCarsAsync(QueryParameters parameters)
        {
            _logger.LogInformation("Tüm araçlar getiriliyor...");
            try
            {
                var (query, _) = await GetCarsQuery(parameters);
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Araçlar getirilirken bir hata oluştu");
                throw new ApiException("Araçlar getirilirken bir hata oluştu", 500, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<Car> Cars, int TotalCount)> GetAllCarsWithCountAsync(QueryParameters parameters)
        {
            try
            {
                _logger.LogDebug("Araçlar sayfalanmış olarak getiriliyor. Sayfa: {PageNumber}, Sayfa Boyutu: {PageSize}", 
                    parameters.PageNumber, parameters.PageSize);
                    
                var (query, countQuery) = await GetCarsQuery(parameters);
                var cars = await query.ToListAsync();
                var totalCount = await countQuery.CountAsync();
                
                _logger.LogDebug("Toplam {Count} araç bulundu, {PageSize} adet getirildi", 
                    totalCount, cars.Count);
                    
                return (cars, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Araçlar getirilirken bir hata oluştu");
                throw new ApiException("Araçlar getirilirken bir hata oluştu", 500, ex);
            }
        }

        private async Task<(IQueryable<Car> Query, IQueryable<Car> CountQuery)> GetCarsQuery(QueryParameters parameters, bool includeCount = false)
        {
            var query = _context.Cars.AsQueryable();

            // Arama terimi varsa uygula
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                query = query.Where(c => 
                    c.Brand.ToLower().Contains(searchTerm) || 
                    c.Model.ToLower().Contains(searchTerm) ||
                    c.LicensePlate.ToLower().Contains(searchTerm) ||
                    c.Color.ToLower().Contains(searchTerm));
            }

            // Filtreleri uygula
            if (parameters.Filters != null && parameters.Filters.Count > 0)
            {
                if (parameters.Filters.TryGetValue("brand", out var brand) && !string.IsNullOrEmpty(brand))
                {
                    query = query.Where(c => c.Brand.ToLower() == brand.ToLower());
                }

                if (parameters.Filters.TryGetValue("model", out var model) && !string.IsNullOrEmpty(model))
                {
                    query = query.Where(c => c.Model.ToLower().Contains(model.ToLower()));
                }

                if (parameters.Filters.TryGetValue("minYear", out var minYearStr) && int.TryParse(minYearStr, out var minYear))
                {
                    query = query.Where(c => c.Year >= minYear);
                }

                if (parameters.Filters.TryGetValue("maxYear", out var maxYearStr) && int.TryParse(maxYearStr, out var maxYear))
                {
                    query = query.Where(c => c.Year <= maxYear);
                }

                if (parameters.Filters.TryGetValue("minPrice", out var minPriceStr) && decimal.TryParse(minPriceStr, out var minPrice))
                {
                    query = query.Where(c => c.DailyPrice >= minPrice);
                }

                if (parameters.Filters.TryGetValue("maxPrice", out var maxPriceStr) && decimal.TryParse(maxPriceStr, out var maxPrice))
                {
                    query = query.Where(c => c.DailyPrice <= maxPrice);
                }


                if (parameters.Filters.TryGetValue("isAvailable", out var isAvailableStr) && bool.TryParse(isAvailableStr, out var isAvailable))
                {
                    query = query.Where(c => c.IsAvailable == isAvailable);
                }
            }

            // Toplam kayıt sayısını al
            IQueryable<Car> countQuery = null;
            if (includeCount)
            {
                countQuery = query;
            }


            // Sıralama
            if (!string.IsNullOrEmpty(parameters.OrderBy))
            {
                query = parameters.OrderBy.ToLower() switch
                {
                    "brand" => parameters.SortDirection == SortDirection.Descending ? 
                        query.OrderByDescending(c => c.Brand) : 
                        query.OrderBy(c => c.Brand),
                    "model" => parameters.SortDirection == SortDirection.Descending ? 
                        query.OrderByDescending(c => c.Model) : 
                        query.OrderBy(c => c.Model),
                    "price" => parameters.SortDirection == SortDirection.Descending ? 
                        query.OrderByDescending(c => c.DailyPrice) : 
                        query.OrderBy(c => c.DailyPrice),
                    "year" => parameters.SortDirection == SortDirection.Descending ? 
                        query.OrderByDescending(c => c.Year) : 
                        query.OrderBy(c => c.Year),
                    _ => parameters.SortDirection == SortDirection.Descending ?
                        query.OrderByDescending(c => c.Id) :
                        query.OrderBy(c => c.Id)
                };
            }
            else
            {
                query = parameters.SortDirection == SortDirection.Descending ?
                    query.OrderByDescending(c => c.Id) :
                    query.OrderBy(c => c.Id);
            }

            // Sayfalama uygula
            if (parameters.PageSize > 0)
            {
                query = query
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize);
            }

            return (query.AsNoTracking(), countQuery);
        }

        /// <inheritdoc/>
        public async Task<Car?> GetCarByIdAsync(int id)
        {
            try
            {
                _logger.LogDebug("{CarId} ID'li araç getiriliyor", id);
                
                var car = await _context.Cars
                    .Include(c => c.Brand)
                    .FirstOrDefaultAsync(c => c.Id == id);
                    
                if (car == null)
                {
                    _logger.LogWarning("{CarId} ID'li araç bulunamadı", id);
                }
                
                return car;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{CarId} ID'li araç getirilirken bir hata oluştu", id);
                throw new ApiException($"{id} ID'li araç getirilirken bir hata oluştu", 500, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Car?> GetCarByLicensePlateAsync(string licensePlate)
        {
            try
            {
                _logger.LogDebug("{LicensePlate} plakalı araç getiriliyor", licensePlate);
                
                var car = await _context.Cars
                    .FirstOrDefaultAsync(c => c.LicensePlate == licensePlate);
                    
                if (car == null)
                {
                    _logger.LogWarning("{LicensePlate} plakalı araç bulunamadı", licensePlate);
                }
                
                return car;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{LicensePlate} plakalı araç getirilirken bir hata oluştu", licensePlate);
                throw new ApiException($"{licensePlate} plakalı araç getirilirken bir hata oluştu", 500, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Car> AddCarAsync(Car car)
        {
            try
            {
                _logger.LogInformation("Yeni araç ekleniyor: {LicensePlate}", car.LicensePlate);
                
                // Aynı plakada araç var mı kontrol et
                var existingCar = await _context.Cars
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.LicensePlate == car.LicensePlate);
                    
                if (existingCar != null)
                {
                    _logger.Warning("{LicensePlate} plakalı bir araç zaten mevcut", car.LicensePlate);
                    throw new ApiException($"{car.LicensePlate} plakalı bir araç zaten mevcut", 400);
                }
                
                _context.Cars.Add(car);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Yeni araç başarıyla eklendi. Araç ID: {CarId}", car.Id);
                
                return car;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Araç eklenirken veritabanı hatası oluştu");
                throw new ApiException("Araç eklenirken bir hata oluştu. Lütfen bilgileri kontrol edip tekrar deneyiniz.", 500, ex);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Araç eklenirken beklenmeyen bir hata oluştu");
                throw new ApiException("Araç eklenirken beklenmeyen bir hata oluştu", 500, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Car> UpdateCarAsync(Car car)
        {
            try
            {
                _logger.LogInformation("{CarId} ID'li araç güncelleniyor", car.Id);
                
                // Aracın varlığını kontrol et
                var existingCar = await _context.Cars
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == car.Id);
                    
                if (existingCar == null)
                {
                    _logger.Warning("{CarId} ID'li araç bulunamadı", car.Id);
                    throw new ApiException($"{car.Id} ID'li araç bulunamadı", 404);
                }
                
                // Plaka değişmiş mi kontrol et
                if (existingCar.LicensePlate != car.LicensePlate)
                {
                    // Yeni plaka başka bir araçta kullanılıyor mu?
                    var plateInUse = await _context.Cars
                        .AnyAsync(c => c.LicensePlate == car.LicensePlate && c.Id != car.Id);
                        
                    if (plateInUse)
                    {
                        _logger.Warning("{LicensePlate} plakası başka bir araçta kullanılıyor", car.LicensePlate);
                        throw new ApiException($"{car.LicensePlate} plakası başka bir araçta kullanılıyor", 400);
                    }
                }
                
                _context.Entry(car).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("{CarId} ID'li araç başarıyla güncellendi", car.Id);
                
                return car;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "{CarId} ID'li araç güncellenirken çakışma oluştu", car.Id);
                throw new ApiException("Araç güncellenirken bir çakışma oluştu. Lütfen sayfayı yenileyip tekrar deneyiniz.", 409, ex);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "{CarId} ID'li araç güncellenirken bir hata oluştu", car.Id);
                throw new ApiException("Araç güncellenirken bir hata oluştu", 500, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCarAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                _logger.LogInformation("{CarId} ID'li araç siliniyor", id);
                
                // Aracı ve ilişkili kiralama kayıtlarını yükle
                var car = await _context.Cars
                    .Include(c => c.Rentals)
                    .FirstOrDefaultAsync(c => c.Id == id);
                    
                if (car == null)
                {
                    _logger.LogWarning("{CarId} ID'li araç bulunamadı", id);
                    throw new ApiException($"{id} ID'li araç bulunamadı", 404);
                }
                
                // İlişkili kiralama kayıtlarını kontrol et
                if (car.Rentals != null && car.Rentals.Any())
                {
                    _logger.LogWarning("{CarId} ID'li aracın {RentalCount} adet kiralama kaydı bulunuyor", 
                        id, car.Rentals.Count);
                        
                    // Aktif veya yaklaşan kiralama var mı kontrol et
                    var activeRentals = car.Rentals
                        .Where(r => r.Status == RentalStatus.Active || r.Status == RentalStatus.Upcoming)
                        .ToList();
                        
                    if (activeRentals.Any())
                    {
                        var rentalIds = string.Join(", ", activeRentals.Select(r => r.Id));
                        _logger.LogWarning("{CarId} ID'li aracın aktif/planlı kiralama kayıtları var. Kiralama ID'leri: {RentalIds}", 
                            id, rentalIds);
                            
                        throw new ApiException(
                            "Bu aracın aktif veya planlanmış kiralama işlemleri bulunmaktadır. " +
                            "Önce bu işlemleri iptal etmelisiniz.", 400);
                    }
                    
                    // Eski kiralama kayıtlarını sil
                    _context.Rentals.RemoveRange(car.Rentals);
                    _logger.LogInformation("{RentalCount} adet eski kiralama kaydı silindi", car.Rentals.Count);
                }
                
                // Aracı sil
                _context.Cars.Remove(car);
                
                // Değişiklikleri kaydet
                int affectedRows = await _context.SaveChangesAsync();
                
                if (affectedRows > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("{CarId} ID'li araç ve ilişkili kayıtlar başarıyla silindi. Etkilenen satır sayısı: {AffectedRows}", 
                        id, affectedRows);
                    return true;
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("{CarId} ID'li araç silinirken hiçbir kayıt etkilenmedi", id);
                    throw new ApiException("Araç silinirken bir hata oluştu. Hiçbir kayıt etkilenmedi.", 500);
                }
            }
            catch (DbUpdateException dbEx) when (dbEx.InnerException?.Message?.Contains("FOREIGN KEY") == true)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "{CarId} ID'li araç için yabancı anahtar kısıtlaması ihlali", id);
                    
                throw new ApiException(
                    "Bu aracı silemezsiniz çünkü başka kayıtlarla ilişkilendirilmiştir. " +
                    "Lütfen önce ilişkili kayıtları temizleyiniz.", 400, dbEx);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "{CarId} ID'li araç silinirken bir hata oluştu", id);
                throw new ApiException("Araç silinirken bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.", 500, ex);
            }
        }

        public async Task<IEnumerable<Car>> GetCarsByBrandAsync(string brand)
        {
            try
            {
                return await _context.Cars
                    .Where(c => c.Brand.ToLower() == brand.ToLower())
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{brand} markalı araçlar getirilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<IEnumerable<Car>> GetCarsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            try
            {
                _logger.LogInformation("Fiyat aralığına göre araçlar getiriliyor: {MinPrice} - {MaxPrice}", minPrice, maxPrice);
                return await _context.Cars
                    .Where(c => c.DailyPrice >= minPrice && c.DailyPrice <= maxPrice)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fiyat aralığına göre araçlar getirilirken bir hata oluştu: {MinPrice} - {MaxPrice}", minPrice, maxPrice);
                throw new ApiException("Araçlar getirilirken bir hata oluştu", 500, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SetCarAvailabilityAsync(int id, bool isAvailable)
        {
            try
            {
                _logger.LogInformation("{Id} ID'li aracın müsaitlik durumu güncelleniyor. Yeni durum: {IsAvailable}", id, isAvailable);
                
                var car = await _context.Cars.FindAsync(id);
                if (car == null)
                {
                    _logger.LogWarning("{Id} ID'li araç bulunamadı", id);
                    return false;
                }

                car.IsAvailable = isAvailable;
                _context.Cars.Update(car);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("{Id} ID'li aracın müsaitlik durumu başarıyla güncellendi. Yeni durum: {IsAvailable}", id, isAvailable);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Id} ID'li aracın müsaitlik durumu güncellenirken bir hata oluştu", id);
                throw new ApiException("Aracın müsaitlik durumu güncellenirken bir hata oluştu", 500, ex);
            }
        }
    }
}
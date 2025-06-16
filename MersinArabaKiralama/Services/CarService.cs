using MersinArabaKiralama.Data;
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

        public async Task<IEnumerable<Car>> GetAllCarsAsync(QueryParameters parameters)
        {
            try
            {
                var query = _context.Cars.AsQueryable();

                // Arama terimi varsa uygula
                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                {
                    var searchTerm = parameters.SearchTerm.ToLower();
                    query = query.Where(c => 
                        c.Brand.ToLower().Contains(searchTerm) || 
                        c.Model.ToLower().Contains(searchTerm));
                }

                // Sıralama
                if (!string.IsNullOrEmpty(parameters.OrderBy))
                {
                    query = parameters.OrderBy.ToLower() switch
                    {
                        "brand" => parameters.Descending ? 
                            query.OrderByDescending(c => c.Brand) : 
                            query.OrderBy(c => c.Brand),
                        "price" => parameters.Descending ? 
                            query.OrderByDescending(c => c.DailyPrice) : 
                            query.OrderBy(c => c.DailyPrice),
                        "year" => parameters.Descending ? 
                            query.OrderByDescending(c => c.Year) : 
                            query.OrderBy(c => c.Year),
                        _ => query.OrderBy(c => c.Id)
                    };
                }
                else
                {
                    query = query.OrderBy(c => c.Id);
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
                _logger.LogError(ex, "Araçlar getirilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<Car?> GetCarByIdAsync(int id)
        {
            try
            {
                return await _context.Cars
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li araç getirilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<Car> AddCarAsync(Car car)
        {
            try
            {
                await _context.Cars.AddAsync(car);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"{car.Brand} {car.Model} aracı başarıyla eklendi");
                return car;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Araç eklenirken bir hata oluştu");
                throw;
            }
        }

        public async Task<bool> UpdateCarAsync(Car car)
        {
            try
            {
                var existingCar = await _context.Cars.FindAsync(car.Id);
                if (existingCar == null)
                    return false;

                _context.Entry(existingCar).CurrentValues.SetValues(car);
                var result = await _context.SaveChangesAsync() > 0;
                
                if (result)
                    _logger.LogInformation($"{car.Id} ID'li araç başarıyla güncellendi");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{car.Id} ID'li araç güncellenirken bir hata oluştu");
                throw;
            }
        }

        public async Task<bool> DeleteCarAsync(int id)
        {
            try
            {
                var car = await _context.Cars.FindAsync(id);
                if (car == null)
                    return false;

                _context.Cars.Remove(car);
                var result = await _context.SaveChangesAsync() > 0;
                
                if (result)
                    _logger.LogInformation($"{id} ID'li araç başarıyla silindi");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li araç silinirken bir hata oluştu");
                throw;
            }
        }

        public async Task<bool> SetCarAvailabilityAsync(int id, bool isAvailable)
        {
            try
            {
                var car = await _context.Cars.FindAsync(id);
                if (car == null)
                    return false;

                car.IsAvailable = isAvailable;
                var result = await _context.SaveChangesAsync() > 0;
                
                if (result)
                    _logger.LogInformation($"{id} ID'li aracın müsaitlik durumu {isAvailable} olarak güncellendi");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li aracın müsaitlik durumu güncellenirken bir hata oluştu");
                throw;
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
                return await _context.Cars
                    .Where(c => c.DailyPrice >= minPrice && c.DailyPrice <= maxPrice)
                    .OrderBy(c => c.DailyPrice)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{minPrice}-{maxPrice} fiyat aralığındaki araçlar getirilirken bir hata oluştu");
                throw;
            }
        }
    }
}
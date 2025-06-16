using MersinAracKiralama.Data;
using MersinAracKiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace MersinAracKiralama.Services
{
    // Araç işlemleri için servis sınıfı
    public class CarService : ICarService
    {
        private readonly ApplicationDbContext _context;
        public CarService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Car>> GetAllAsync()
        {
            return await _context.Cars.ToListAsync();
        }

        public async Task<Car?> GetByIdAsync(int id)
        {
            return await _context.Cars.FindAsync(id);
        }

        public async Task<Car> AddAsync(Car car)
        {
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();
            return car;
        }

        public async Task<bool> UpdateAsync(Car car)
        {
            _context.Cars.Update(car);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return false;
            _context.Cars.Remove(car);
            return await _context.SaveChangesAsync() > 0;
        }
    }
} 
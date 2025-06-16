using MersinAracKiralama.Data;
using MersinAracKiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace MersinAracKiralama.Services
{
    // Kiralama işlemleri için servis sınıfı
    public class RentalService : IRentalService
    {
        private readonly ApplicationDbContext _context;
        public RentalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Rental>> GetAllAsync()
        {
            return await _context.Rentals.Include(r => r.Car).Include(r => r.Customer).ToListAsync();
        }

        public async Task<Rental?> GetByIdAsync(int id)
        {
            return await _context.Rentals.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Rental> AddAsync(Rental rental)
        {
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();
            return rental;
        }

        public async Task<bool> UpdateAsync(Rental rental)
        {
            _context.Rentals.Update(rental);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return false;
            _context.Rentals.Remove(rental);
            return await _context.SaveChangesAsync() > 0;
        }
    }
} 
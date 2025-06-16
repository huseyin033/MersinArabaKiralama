using MersinAracKiralama.Models;

namespace MersinAracKiralama.Services
{
    // Kiralama işlemleri için servis arabirimi
    public interface IRentalService
    {
        Task<List<Rental>> GetAllAsync();
        Task<Rental?> GetByIdAsync(int id);
        Task<Rental> AddAsync(Rental rental);
        Task<bool> UpdateAsync(Rental rental);
        Task<bool> DeleteAsync(int id);
    }
} 
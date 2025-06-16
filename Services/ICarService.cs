using MersinAracKiralama.Models;

namespace MersinAracKiralama.Services
{
    // Araç işlemleri için servis arabirimi
    public interface ICarService
    {
        Task<List<Car>> GetAllAsync();
        Task<Car?> GetByIdAsync(int id);
        Task<Car> AddAsync(Car car);
        Task<bool> UpdateAsync(Car car);
        Task<bool> DeleteAsync(int id);
    }
} 
using MersinAracKiralama.Models;

namespace MersinAracKiralama.Services
{
    // Müşteri işlemleri için servis arabirimi
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer> AddAsync(Customer customer);
        Task<bool> UpdateAsync(Customer customer);
        Task<bool> DeleteAsync(int id);
    }
} 
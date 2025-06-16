using MersinAracKiralama.Data;
using MersinAracKiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace MersinAracKiralama.Services
{
    // Müşteri işlemleri için servis sınıfı
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<Customer> AddAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;
            _context.Customers.Remove(customer);
            return await _context.SaveChangesAsync() > 0;
        }
    }
} 
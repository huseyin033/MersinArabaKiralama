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
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ApplicationDbContext context, ILogger<CustomerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync(QueryParameters parameters)
        {
            try
            {
                var query = _context.Customers.AsQueryable();

                // Arama terimi varsa uygula
                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                {
                    var searchTerm = parameters.SearchTerm.ToLower();
                    query = query.Where(c => 
                        c.FirstName.ToLower().Contains(searchTerm) || 
                        c.LastName.ToLower().Contains(searchTerm) ||
                        c.Email.ToLower().Contains(searchTerm));
                }

                // Sıralama
                if (!string.IsNullOrEmpty(parameters.OrderBy))
                {
                    query = parameters.OrderBy.ToLower() switch
                    {
                        "name" => parameters.Descending ? 
                            query.OrderByDescending(c => c.LastName).ThenByDescending(c => c.FirstName) : 
                            query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName),
                        "email" => parameters.Descending ? 
                            query.OrderByDescending(c => c.Email) : 
                            query.OrderBy(c => c.Email),
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
                _logger.LogError(ex, "Müşteriler getirilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            try
            {
                return await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li müşteri getirilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            try
            {
                return await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{email} e-posta adresli müşteri getirilirken bir hata oluştu");
                throw;
            }
        }

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            try
            {
                // E-posta adresinin benzersiz olduğunu kontrol et
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == customer.Email);

                if (existingCustomer != null)
                {
                    throw new InvalidOperationException("Bu e-posta adresi zaten kullanılıyor.");
                }

                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"{customer.FirstName} {customer.LastName} adlı müşteri başarıyla eklendi");
                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri eklenirken bir hata oluştu");
                throw;
            }
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                var existingCustomer = await _context.Customers.FindAsync(customer.Id);
                if (existingCustomer == null)
                    return false;

                // E-posta adresinin başka bir müşteri tarafından kullanılıp kullanılmadığını kontrol et
                var emailInUse = await _context.Customers
                    .AnyAsync(c => c.Id != customer.Id && c.Email == customer.Email);

                if (emailInUse)
                {
                    throw new InvalidOperationException("Bu e-posta adresi başka bir müşteri tarafından kullanılıyor.");
                }

                _context.Entry(existingCustomer).CurrentValues.SetValues(customer);
                var result = await _context.SaveChangesAsync() > 0;
                
                if (result)
                    _logger.LogInformation($"{customer.Id} ID'li müşteri başarıyla güncellendi");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{customer.Id} ID'li müşteri güncellenirken bir hata oluştu");
                throw;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                    return false;

                // Müşterinin aktif kiralama işlemi var mı kontrol et
                var hasActiveRentals = await _context.Rentals
                    .AnyAsync(r => r.CustomerId == id && r.ReturnDate == null);

                if (hasActiveRentals)
                {
                    throw new InvalidOperationException("Bu müşterinin aktif kiralama işlemi olduğu için silinemez.");
                }

                _context.Customers.Remove(customer);
                var result = await _context.SaveChangesAsync() > 0;
                
                if (result)
                    _logger.LogInformation($"{id} ID'li müşteri başarıyla silindi");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li müşteri silinirken bir hata oluştu");
                throw;
            }
        }

        public async Task<IEnumerable<Rental>> GetCustomerRentalsAsync(int customerId)
        {
            try
            {
                return await _context.Rentals
                    .Include(r => r.Car)
                    .Where(r => r.CustomerId == customerId)
                    .OrderByDescending(r => r.RentalDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{customerId} ID'li müşterinin kiralama geçmişi getirilirken bir hata oluştu");
                throw;
            }
        }
    }
}
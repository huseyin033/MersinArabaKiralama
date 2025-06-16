using MersinArabaKiralama.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Services
{
    public interface ICustomerService
    {
        /// <summary>
        /// Tüm müşterileri getirir (Sayfalama bilgisi olmadan)
        /// </summary>
        Task<IEnumerable<Customer>> GetAllCustomersAsync(QueryParameters parameters);

        /// <summary>
        /// Tüm müşterileri sayfalama bilgileriyle birlikte getirir
        /// </summary>
        /// <returns>(Müşteri listesi, Toplam kayıt sayısı)</returns>
        Task<(IEnumerable<Customer> Customers, int TotalCount)> GetAllCustomersWithCountAsync(QueryParameters parameters);

        /// <summary>
        /// ID'ye göre müşteri getirir
        /// </summary>
        Task<Customer?> GetCustomerByIdAsync(int id);

        /// <summary>
        /// E-posta adresine göre müşteri getirir
        /// </summary>
        Task<Customer?> GetCustomerByEmailAsync(string email);

        /// <summary>
        /// Yeni bir müşteri ekler
        /// </summary>
        Task<Customer> AddCustomerAsync(Customer customer);

        /// <summary>
        /// Mevcut bir müşteriyi günceller
        /// </summary>
        Task<bool> UpdateCustomerAsync(Customer customer);

        /// <summary>
        /// Bir müşteriyi siler
        /// </summary>
        Task<bool> DeleteCustomerAsync(int id);

        /// <summary>
        /// Müşterinin kiralama geçmişini getirir
        /// </summary>
        Task<IEnumerable<Rental>> GetCustomerRentalsAsync(int customerId);
    }
}
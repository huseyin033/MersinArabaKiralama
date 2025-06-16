using MersinArabaKiralama.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Services
{
    public interface IRentalService
    {
        /// <summary>
        /// Tüm kiralama kayıtlarını getirir (Sayfalama bilgisi olmadan)
        /// </summary>
        Task<IEnumerable<Rental>> GetAllRentalsAsync(QueryParameters parameters);

        /// <summary>
        /// Tüm kiralama kayıtlarını sayfalama bilgileriyle birlikte getirir
        /// </summary>
        /// <returns>(Kiralama listesi, Toplam kayıt sayısı)</returns>
        Task<(IEnumerable<Rental> Rentals, int TotalCount)> GetAllRentalsWithCountAsync(QueryParameters parameters);

        /// <summary>
        /// ID'ye göre kiralama kaydı getirir
        /// </summary>
        Task<Rental?> GetRentalByIdAsync(int id);

        /// <summary>
        /// Kullanıcıya ait kiralama kayıtlarını getirir
        /// </summary>
        Task<IEnumerable<Rental>> GetUserRentalsAsync(string userId);

        /// <summary>
        /// Müşteriye ait kiralama kayıtlarını getirir
        /// </summary>
        Task<IEnumerable<Rental>> GetCustomerRentalsAsync(int customerId);

        /// <summary>
        /// Araca ait kiralama kayıtlarını getirir
        /// </summary>
        Task<IEnumerable<Rental>> GetCarRentalsAsync(int carId);

        /// <summary>
        /// Belirli bir tarih aralığındaki kiralama kayıtlarını getirir
        /// </summary>
        Task<IEnumerable<Rental>> GetRentalsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Yeni bir kiralama kaydı ekler
        /// </summary>
        Task<Rental> AddRentalAsync(Rental rental);

        /// <summary>
        /// Kiralama kaydını günceller
        /// </summary>
        Task<bool> UpdateRentalAsync(Rental rental);

        /// <summary>
        /// Kiralama kaydını iptal eder
        /// </summary>
        Task<bool> CancelRentalAsync(int id, string cancellationReason);

        /// <summary>
        /// Kiralama kaydını tamamlar (aracın teslim alındığı anlamına gelir)
        /// </summary>
        Task<bool> CompleteRentalAsync(int id);

        /// <summary>
        /// Kiralama kaydını sonlandırır (aracın teslim edildiği anlamına gelir)
        /// </summary>
        Task<bool> FinishRentalAsync(int id, string returnNotes, decimal? additionalCharges);

        /// <summary>
        /// Kiralama kaydını siler
        /// </summary>
        Task<bool> DeleteRentalAsync(int id);

        /// <summary>
        /// Belirli bir tarih aralığında aracın müsait olup olmadığını kontrol eder
        /// </summary>
        Task<bool> IsCarAvailableForRentAsync(int carId, DateTime startDate, DateTime endDate, int? excludeRentalId = null);
    }
}
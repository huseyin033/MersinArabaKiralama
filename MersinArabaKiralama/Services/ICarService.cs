using MersinArabaKiralama.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Services
{
    public interface ICarService
    {
        /// <summary>
        /// Tüm araçları getirir
        /// </summary>
        Task<IEnumerable<Car>> GetAllCarsAsync(QueryParameters parameters);

        /// <summary>
        /// ID'ye göre araç getirir
        /// </summary>
        Task<Car?> GetCarByIdAsync(int id);

        /// <summary>
        /// Yeni bir araç ekler
        /// </summary>
        Task<Car> AddCarAsync(Car car);

        /// <summary>
        /// Mevcut bir aracı günceller
        /// </summary>
        Task<bool> UpdateCarAsync(Car car);

        /// <summary>
        /// Bir aracı siler
        /// </summary>
        Task<bool> DeleteCarAsync(int id);


        /// <summary>
        /// Aracın müsaitlik durumunu günceller
        /// </summary>
        Task<bool> SetCarAvailabilityAsync(int id, bool isAvailable);

        /// <summary>
        /// Markaya göre araçları getirir
        /// </summary>
        Task<IEnumerable<Car>> GetCarsByBrandAsync(string brand);

        /// <summary>
        /// Fiyat aralığına göre araçları getirir
        /// </summary>
        Task<IEnumerable<Car>> GetCarsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    }
}
using MersinArabaKiralama.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Services
{
    public interface ICarService
    {
        /// <summary>
        /// Tüm araçları getirir (Sayfalama bilgisi olmadan)
        /// </summary>
        Task<IEnumerable<Car>> GetAllCarsAsync(QueryParameters parameters);

        /// <summary>
        /// Tüm araçları sayfalama bilgileriyle birlikte getirir
        /// </summary>
        /// <returns>(Araç listesi, Toplam kayıt sayısı)</returns>
        Task<(IEnumerable<Car> Cars, int TotalCount)> GetAllCarsWithCountAsync(QueryParameters parameters);

        /// <summary>
        /// ID'ye göre araç getirir
        /// </summary>
        Task<Car?> GetCarByIdAsync(int id);

        /// <summary>
        /// Plaka numarasına göre araç getirir
        /// </summary>
        /// <param name="licensePlate">Araç plaka numarası</param>
        /// <returns>Eşleşen araç veya null</returns>
        Task<Car?> GetCarByLicensePlateAsync(string licensePlate);

        /// <summary>
        /// Yeni bir araç ekler
        /// </summary>
        Task<Car> AddCarAsync(Car car);

        /// <summary>
        /// Mevcut bir aracı günceller
        /// </summary>
        /// <param name="car">Güncellenecek araç bilgileri</param>
        /// <returns>Güncellenmiş araç bilgileri</returns>
        /// <exception cref="ApiException">Araç bulunamazsa veya güncelleme sırasında hata oluşursa fırlatılır</exception>
        Task<Car> UpdateCarAsync(Car car);

        /// <summary>
        /// Bir aracı siler
        /// </summary>
        /// <param name="id">Silinecek aracın ID'si</param>
        /// <returns>İşlem başarılıysa true, aksi halde false döner</returns>
        /// <exception cref="ApiException">Araç bulunamazsa veya silme işlemi sırasında hata oluşursa fırlatılır</exception>
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
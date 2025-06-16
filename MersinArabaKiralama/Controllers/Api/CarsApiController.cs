using Microsoft.AspNetCore.Mvc;
using MersinArabaKiralama.Data;
using MersinArabaKiralama.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using MersinArabaKiralama.Services;
using MersinArabaKiralama.Exceptions;
using System.Linq.Expressions;
using System.Net;

namespace MersinArabaKiralama.Controllers.Api
{
    /// <summary>
/// Araç işlemleri API'si
/// </summary>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
public class CarsApiController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly ILogger<CarsApiController> _logger;
        private readonly ApplicationDbContext _context;

        public CarsApiController(ICarService carService, ILogger<CarsApiController> logger, ApplicationDbContext context)
        {
            _carService = carService ?? throw new ArgumentNullException(nameof(carService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Tüm araçları getirir
        /// </summary>
        /// <param name="parameters">Sayfalama, sıralama ve filtreleme parametreleri</param>
        /// <response code="200">Başarılı işlem</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Car>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCars([FromQuery] QueryParameters parameters)
        {
            _logger.LogInformation("Araç listesi isteniyor. Sayfa: {PageNumber}, Sayfa Boyutu: {PageSize}", 
                parameters.PageNumber, parameters.PageSize);

            try
            {
                var (cars, totalCount) = await _carService.GetAllCarsWithCountAsync(parameters);

                var pagination = new PaginationMetadata
                {
                    TotalCount = totalCount,
                    PageSize = parameters.PageSize,
                    CurrentPage = parameters.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize)
                };

                var response = ApiResponse<IEnumerable<Car>>.Success(
                    cars,
                    "Araçlar başarıyla getirildi"
                );
                response.Pagination = pagination;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Araçlar getirilirken bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error(
                        "Araçlar getirilirken bir hata oluştu",
                        HttpStatusCode.InternalServerError,
                        new[] { ex.Message }
                    )
                );
            }
        }

        /// <summary>
        /// Tüm araçları sayfalı olarak getirir
        /// </summary>
        /// <param name="parameters">Sayfalama ve filtreleme parametreleri</param>
        /// <returns>Araç listesi ve sayfalama bilgileri</returns>
        /// <response code="200">Başarılı işlem</response>
        /// <response code="400">Geçersiz istek parametreleri</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Car>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> GetCarsV2([FromQuery] QueryParameters parameters)
        {
            _logger.LogInformation("Araç listesi isteniyor. Sayfa: {PageNumber}, Sayfa Boyutu: {PageSize}", 
                parameters.PageNumber, parameters.PageSize);

            try
            {
                var (cars, totalCount) = await _carService.GetAllCarsWithCountAsync(parameters);

                var pagination = new PaginationMetadata
                {
                    TotalCount = totalCount,
                    PageSize = parameters.PageSize,
                    CurrentPage = parameters.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize)
                };

                var response = ApiResponse<IEnumerable<Car>>.Success(
                    cars,
                    "Araçlar başarıyla getirildi"
                );
                response.Pagination = pagination;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Araçlar getirilirken bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error(
                        "Araçlar getirilirken bir hata oluştu",
                        HttpStatusCode.InternalServerError,
                        new[] { ex.Message }
                    )
                );
            }
        }

        /// <summary>
        /// ID'ye göre araç getirir
        /// </summary>
        /// <param name="id">Araç ID'si</param>
        /// <response code="200">Başarılı işlem</response>
        /// <response code="404">Araç bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Car>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCar(int id)
        {
            try
            {
                _logger.LogInformation("{CarId} ID'li araç getiriliyor", id);
                var car = await _carService.GetCarByIdAsync(id);

                if (car == null)
                {
                    _logger.LogWarning("{CarId} ID'li araç bulunamadı", id);
                    return NotFound(ApiResponse<object>.Error(
                        $"ID'si {id} olan araç bulunamadı.",
                        HttpStatusCode.NotFound
                    ));
                }

                return Ok(ApiResponse<Car>.Success(car, "Araç başarıyla getirildi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li araç getirilirken bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error(
                        "Araç getirilirken bir hata oluştu",
                        HttpStatusCode.InternalServerError,
                        new[] { ex.Message }
                    )
                );
            }
        }

        /// <summary>
        /// Yeni bir araç ekler
        /// </summary>
        /// <param name="car">Eklenecek araç bilgileri</param>
        /// <response code="201">Araç başarıyla eklendi</response>
        /// <response code="400">Geçersiz veri</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="403">Erişim reddedildi</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<Car>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PostCar([FromBody] Car car)
        {
            try
            {
                _logger.LogInformation("Yeni araç ekleniyor: {Brand} {Model}", car.Brand, car.Model);
                
                // Model doğrulama
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .Where(e => !string.IsNullOrEmpty(e))
                        .ToList();

                    _logger.LogWarning("Geçersiz model durumu: {Errors}", string.Join(", ", errors));
                    return BadRequest(ApiResponse<object>.Error(
                        "Geçersiz veri", 
                        HttpStatusCode.BadRequest,
                        errors
                    ));
                }


                try
                {
                    // Varsayılan değerleri ayarla
                    car.CreatedAt = DateTime.UtcNow;
                    car.UpdatedAt = DateTime.UtcNow;
                    car.IsAvailable = true;

                    var createdCar = await _carService.AddCarAsync(car);
                    _logger.LogInformation($"{createdCar.Id} ID'li yeni araç eklendi: {createdCar.Brand} {createdCar.Model}");

                    var response = ApiResponse<Car>.Success(
                        createdCar,
                        HttpStatusCode.Created,
                        $"{createdCar.Brand} {createdCar.Model} aracı başarıyla eklendi"
                    );

                    return CreatedAtAction(
                        nameof(GetCar),
                        new { id = createdCar.Id, version = "1.0" },
                        response
                    );
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("plakalı bir araç zaten mevcut"))
                {
                    _logger.LogWarning(ex, "Aynı plakaya sahip bir araç zaten mevcut: {LicensePlate}", car.LicensePlate);
                    return BadRequest(ApiResponse<object>.Error(
                        $"{car.LicensePlate} plakalı bir araç zaten mevcut",
                        HttpStatusCode.BadRequest
                    ));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Araç eklenirken bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error(
                        "Araç eklenirken bir hata oluştu",
                        HttpStatusCode.InternalServerError,
                        new[] { ex.Message }
                    )
                );
            }
        }

        /// <summary>
        /// Mevcut bir aracı günceller
        /// </summary>
        /// <param name="id">Güncellenecek aracın ID'si</param>
        /// <param name="carDto">Güncellenmiş araç bilgileri</param>
        /// <returns>Güncellenmiş araç bilgileri</returns>
        /// <response code="200">Araç başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek (ID'ler uyuşmuyor veya model doğrulama hatası)</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="403">İzin reddedildi</response>
        /// <response code="404">Araç bulunamadı</response>
        /// <response code="409">Çakışma (eşzamanlı güncelleme çakışması)</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<Car>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] Car carDto)
        {
            try
            {
                _logger.LogInformation("{CarId} ID'li araç güncelleniyor", id);
                
                // Model doğrulaması
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .Where(e => !string.IsNullOrEmpty(e))
                        .ToList();
                        
                    _logger.LogWarning("Model doğrulama hataları: {Errors}", string.Join(", ", errors));
                    return BadRequest(ApiResponse<object>.Error(
                        "Geçersiz veri",
                        HttpStatusCode.BadRequest,
                        errors
                    ));
                }
                
                // URL'deki ID ile gövdedeki ID'nin uyumluluğunu kontrol et
                if (id != carDto.Id)
                {
                    _logger.LogWarning("URL'deki ID ({UrlId}) ile gövdedeki ID ({BodyId}) uyuşmuyor", 
                        id, carDto.Id);
                        
                    return BadRequest(ApiResponse<object>.Error(
                        "URL'deki ID ile gövdedeki ID uyuşmuyor.",
                        HttpStatusCode.BadRequest
                    ));
                }
                
                // Mevcut aracı getir
                var existingCar = await _carService.GetCarByIdAsync(id);
                if (existingCar == null)
                {
                    _logger.LogWarning("{CarId} ID'li araç bulunamadı", id);
                    return NotFound(ApiResponse<object>.Error(
                        $"ID'si {id} olan araç bulunamadı.",
                        HttpStatusCode.NotFound
                    ));
                }
                
                // Plaka değiştiyse kontrol et
                if (existingCar.LicensePlate != carDto.LicensePlate)
                {
                    var carWithSamePlate = await _carService.GetCarByLicensePlateAsync(carDto.LicensePlate);
                    if (carWithSamePlate != null && carWithSamePlate.Id != id)
                    {
                        _logger.LogWarning("{LicensePlate} plakası başka bir araçta kullanılıyor", carDto.LicensePlate);
                        return BadRequest(ApiResponse<object>.Error(
                            $"{carDto.LicensePlate} plakalı başka bir araç zaten mevcut",
                            HttpStatusCode.BadRequest
                        ));
                    }
                }
                
                // Güncelleme zamanını ayarla
                carDto.UpdatedAt = DateTime.UtcNow;
                
                // Aracı güncelle
                var updatedCar = await _carService.UpdateCarAsync(carDto);
                
                _logger.LogInformation("{CarId} ID'li araç başarıyla güncellendi", id);
                
                return Ok(ApiResponse<Car>.Success(
                    updatedCar,
                    $"ID'si {id} olan araç başarıyla güncellendi"
                ));
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                _logger.LogWarning(ex, "{CarId} ID'li araç bulunamadı", id);
                return NotFound(ApiResponse<object>.Error(
                    ex.Message,
                    HttpStatusCode.NotFound
                ));
            }
            catch (ApiException ex) when (ex.StatusCode == 409)
            {
                _logger.LogWarning(ex, "{CarId} ID'li araç güncellenirken çakışma oluştu", id);
                return Conflict(ApiResponse<object>.Error(
                    ex.Message,
                    HttpStatusCode.Conflict,
                    ex.Errors
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{CarId} ID'li araç güncellenirken bir hata oluştu", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error(
                        "Araç güncellenirken bir hata oluştu",
                        HttpStatusCode.InternalServerError,
                        new[] { ex.Message }
                    )
                );
            }
        }

        /// <summary>
        /// Bir aracı siler
        /// </summary>
        /// <param name="id">Silinecek aracın ID'si</param>
        /// <response code="204">Araç başarıyla silindi</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="403">İzin reddedildi</response>
        /// <response code="404">Araç bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <summary>
        /// Bir aracı siler
        /// </summary>
        /// <param name="id">Silinecek aracın ID'si</param>
        /// <returns>İşlem sonucu</returns>
        /// <response code="200">Araç başarıyla silindi</response>
        /// <response code="400">Araç silinemiyor (aktif kiralama var veya geçersiz istek)</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="403">İzin reddedildi</response>
        /// <response code="404">Araç bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCar(int id)
        {
            try
            {
                _logger.LogInformation("Araç silme işlemi başlatıldı. Araç ID: {CarId}", id);
                _logger.LogInformation("{CarId} ID'li araç siliniyor", id);
                
                var car = await _carService.GetCarByIdAsync(id);
                if (car == null)
                {
                    _logger.LogWarning("{CarId} ID'li araç bulunamadı", id);
                    return NotFound(ApiResponse<object>.Error(
                        $"ID'si {id} olan araç bulunamadı.",
                        HttpStatusCode.NotFound
                    ));
                }

                // Aktif kiralama kontrolü
                var hasActiveRentals = await _context.Rentals
                    .AnyAsync(r => r.CarId == id && 
                                (r.Status == RentalStatus.Active || 
                                 r.Status == RentalStatus.Upcoming));

                if (hasActiveRentals)
                {
                    _logger.LogWarning("{CarId} ID'li araç için aktif veya yaklaşan kiralama bulunuyor, silinemez", id);
                    return BadRequest(ApiResponse<object>.Error(
                        "Bu aracın aktif veya yaklaşan kiralama işlemi bulunmaktadır. Önce kiralama işlemlerini tamamlayınız.",
                        HttpStatusCode.BadRequest
                    ));
                }

                await _carService.DeleteCarAsync(id);
                _logger.LogInformation("{CarId} ID'li araç başarıyla silindi", id);
                
                return Ok(ApiResponse<object>.Success(
                    null,
                    $"ID'si {id} olan araç başarıyla silindi"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li araç silinirken bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error(
                        "Araç silinirken bir hata oluştu",
                        HttpStatusCode.InternalServerError,
                        new[] { ex.Message }
                    )
                );
            }
        }

        /// <summary>
        /// Aracın müsaitlik durumunu günceller
        /// </summary>
        /// <param name="id">Araç ID'si</param>
        /// <param name="isAvailable">Aracın yeni müsaitlik durumu</param>
        /// <response code="204">Müsaitlik durumu başarıyla güncellendi</response>
        [HttpPut("{id}/availability")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCarAvailability(int id, [FromBody] bool isAvailable)
        {
            try
            {
                    var hasActiveRentals = await _context.Rentals
                        .AnyAsync(r => r.CarId == id && 
                                    (r.Status == RentalStatus.Active || 
                                     r.Status == RentalStatus.Pending ||
                                     r.Status == RentalStatus.Approved));
                    
                    if (hasActiveRentals)
                    {
                        _logger.LogWarning($"{id} ID'li aracın aktif kiralamaları olduğu için müsait yapılamaz");
                        return BadRequest(ApiResponse<object>.Error(
                            "Bu aracın aktif kiralamaları bulunuyor. Önce kiralama işlemlerini tamamlayın.",
                            HttpStatusCode.BadRequest
                        ));
                    }

                var result = await _carService.SetCarAvailabilityAsync(id, isAvailable);
                if (!result)
                {
                    _logger.LogError($"{id} ID'li aracın müsaitlik durumu güncellenirken bir hata oluştu");
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ApiResponse<object>.Error(
                            "Müsaitlik durumu güncellenirken bir hata oluştu",
                            HttpStatusCode.InternalServerError
                        )
                    );
                }

                _logger.LogInformation($"{id} ID'li aracın müsaitlik durumu başarıyla güncellendi. Yeni durum: {isAvailable}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li aracın müsaitlik durumu güncellenirken bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error(
                        "Müsaitlik durumu güncellenirken bir hata oluştu",
                        HttpStatusCode.InternalServerError,
                        new[] { ex.Message }
                    )
                );
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using MersinArabaKiralama.Data;
using MersinArabaKiralama.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using MersinArabaKiralama.Services;
using System.Linq.Expressions;

namespace MersinArabaKiralama.Controllers.Api
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class CarsApiController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly ILogger<CarsApiController> _logger;

        public CarsApiController(ICarService carService, ILogger<CarsApiController> logger)
        {
            _carService = carService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm araçları getirir
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Car>>>> GetCars([FromQuery] QueryParameters parameters)
        {
            try
            {
                _logger.LogInformation("Tüm araçlar getiriliyor");
                var cars = await _carService.GetAllCarsAsync(parameters);
                return Ok(ApiResponse<IEnumerable<Car>>.SuccessResult(cars));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Araçlar getirilirken hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
            }
        }

        /// <summary>
        /// ID'ye göre araç getirir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Car>>> GetCar(int id)
        {
            try
            {
                _logger.LogInformation($"{id} ID'li araç getiriliyor");
                var car = await _carService.GetCarByIdAsync(id);

                if (car == null)
                {
                    _logger.LogWarning($"{id} ID'li araç bulunamadı");
                    return NotFound(ApiResponse<object>.ErrorResult("Araç bulunamadı"));
                }

                return Ok(ApiResponse<Car>.SuccessResult(car));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li araç getirilirken hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
            }
        }

        /// <summary>
        /// Yeni bir araç ekler
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<Car>>> PostCar(Car car)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    _logger.LogWarning("Geçersiz model durumu: {Errors}", string.Join(", ", errors));
                    return BadRequest(ApiResponse<object>.ErrorResult("Geçersiz veri", errors));
                }

                _logger.LogInformation("Yeni araç ekleniyor: {Brand} {Model}", car.Brand, car.Model);
                var createdCar = await _carService.AddCarAsync(car);

                return CreatedAtAction(nameof(GetCar),
                    new { id = createdCar.Id, version = "1.0" },
                    ApiResponse<Car>.SuccessResult(createdCar, "Araç başarıyla eklendi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Araç eklenirken hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Araç eklenirken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Mevcut bir aracı günceller
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCar(int id, Car car)
        {
            try
            {
                if (id != car.Id)
                {
                    _logger.LogWarning("İstekteki ID ile araç ID'si uyuşmuyor");
                    return BadRequest(ApiResponse<object>.ErrorResult("ID'ler uyuşmuyor"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    _logger.LogWarning("Geçersiz model durumu: {Errors}", string.Join(", ", errors));
                    return BadRequest(ApiResponse<object>.ErrorResult("Geçersiz veri", errors));
                }

                _logger.LogInformation($"{id} ID'li araç güncelleniyor");
                var result = await _carService.UpdateCarAsync(car);

                if (!result)
                {
                    _logger.LogWarning($"{id} ID'li araç bulunamadı");
                    return NotFound(ApiResponse<object>.ErrorResult("Araç bulunamadı"));
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li araç güncellenirken hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Araç güncellenirken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Bir aracı siler
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            try
            {
                _logger.LogInformation($"{id} ID'li araç siliniyor");
                var result = await _carService.DeleteCarAsync(id);

                if (!result)
                {
                    _logger.LogWarning($"{id} ID'li araç bulunamadı");
                    return NotFound(ApiResponse<object>.ErrorResult("Araç bulunamadı"));
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li araç silinirken hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Araç silinirken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Aracın müsaitlik durumunu günceller
        /// </summary>
        [HttpPatch("{id}/availability")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> SetAvailability(int id, [FromBody] bool isAvailable)
        {
            try
            {
                _logger.LogInformation($"{id} ID'li aracın müsaitlik durumu güncelleniyor: {isAvailable}");
                var result = await _carService.SetCarAvailabilityAsync(id, isAvailable);

                if (!result)
                {
                    _logger.LogWarning($"{id} ID'li araç bulunamadı");
                    return NotFound(ApiResponse<object>.ErrorResult("Araç bulunamadı"));
                }

                return Ok(ApiResponse<object>.SuccessResult("Müsaitlik durumu güncellendi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li aracın müsaitlik durumu güncellenirken hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Müsaitlik durumu güncellenirken bir hata oluştu"));
            }
        }
    }
}
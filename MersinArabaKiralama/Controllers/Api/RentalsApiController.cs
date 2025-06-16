using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MersinArabaKiralama.Models;
using MersinArabaKiralama.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Controllers.Api
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class RentalsApiController : ControllerBase
    {
        private readonly IRentalService _rentalService;
        private readonly ICarService _carService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<RentalsApiController> _logger;

        public RentalsApiController(
            IRentalService rentalService,
            ICarService carService,
            ICustomerService customerService,
            ILogger<RentalsApiController> logger)
        {
            _rentalService = rentalService;
            _carService = carService;
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm kiralama kayıtlarını getirir
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Rental>>>> GetRentals([FromQuery] QueryParameters parameters)
        {
            try
            {
                _logger.LogInformation("Tüm kiralama kayıtları getiriliyor");
                var rentals = await _rentalService.GetAllRentalsAsync(parameters);
                return Ok(ApiResponse<IEnumerable<Rental>>.SuccessResult(rentals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kiralama kayıtları getirilirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
            }
        }

        /// <summary>
        /// ID'ye göre kiralama kaydı getirir
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<Rental>>> GetRental(int id)
        {
            try
            {
                _logger.LogInformation($"{id} ID'li kiralama kaydı getiriliyor");
                var rental = await _rentalService.GetRentalByIdAsync(id);

                if (rental == null)
                {
                    _logger.LogWarning($"{id} ID'li kiralama kaydı bulunamadı");
                    return NotFound(ApiResponse<object>.ErrorResult("Kiralama kaydı bulunamadı"));
                }

                // Kullanıcı kontrolü (Admin değilse sadece kendi kayıtlarını görebilir)
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!User.IsInRole("Admin") && rental.Customer?.UserId != userId)
                {
                    return Forbid();
                }

                return Ok(ApiResponse<Rental>.SuccessResult(rental));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li kiralama kaydı getirilirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
            }
        }

        /// <summary>
        /// Kullanıcının kiralama geçmişini getirir
        /// </summary>
        [HttpGet("my-rentals")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<IEnumerable<Rental>> >> GetMyRentals()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Kullanıcı bilgisi alınamadı"));
                }

                var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                if (customer == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Müşteri bilgisi bulunamadı"));
                }

                _logger.LogInformation($"{customer.Id} ID'li müşterinin kiralama geçmişi getiriliyor");
                var rentals = await _rentalService.GetCustomerRentalsAsync(customer.Id);
                return Ok(ApiResponse<IEnumerable<Rental>>.SuccessResult(rentals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı kiralama geçmişi getirilirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
            }
        }

        public class CreateRentalRequest
        {
            [Required(ErrorMessage = "Araç ID'si gereklidir")]
            public int CarId { get; set; }

            [Required(ErrorMessage = "Başlangıç tarihi gereklidir")]
            public DateTime StartDate { get; set; }

            [Required(ErrorMessage = "Bitiş tarihi gereklidir")]
            public DateTime EndDate { get; set; }

            [StringLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir")]
            public string? Notes { get; set; }
        }

        /// <summary>
        /// Yeni bir kiralama kaydı oluşturur
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<Rental>>> CreateRental([FromBody] CreateRentalRequest request)
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

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Kullanıcı bilgisi alınamadı"));
                }

                var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                if (customer == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Müşteri bilgisi bulunamadı"));
                }

                // Aracın müsait olup olmadığını kontrol et
                var isCarAvailable = await _rentalService.IsCarAvailableForRentAsync(
                    request.CarId, 
                    request.StartDate, 
                    request.EndDate);

                if (!isCarAvailable)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Seçilen tarihlerde araç müsait değil"));
                }

                // Kiralama kaydını oluştur
                var rental = new Rental
                {
                    CarId = request.CarId,
                    CustomerId = customer.Id,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Notes = request.Notes,
                    Status = RentalStatus.Pending,
                    RentalDate = DateTime.UtcNow
                };

                var createdRental = await _rentalService.AddRentalAsync(rental);
                
                _logger.LogInformation($"{createdRental.Id} ID'li yeni kiralama kaydı oluşturuldu");
                
                return CreatedAtAction(
                    nameof(GetRental), 
                    new { id = createdRental.Id, version = "1.0" },
                    ApiResponse<Rental>.SuccessResult(createdRental, "Kiralama kaydı başarıyla oluşturuldu"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Kiralama kaydı oluşturulurken geçersiz işlem: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kiralama kaydı oluşturulurken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Kiralama kaydı oluşturulurken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Kiralama kaydını iptal eder
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelRental(int id, [FromBody] CancelRentalRequest request)
        {
            try
            {
                var rental = await _rentalService.GetRentalByIdAsync(id);
                if (rental == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Kiralama kaydı bulunamadı"));
                }

                // Yetki kontrolü (Admin veya kendi kaydı)
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!User.IsInRole("Admin") && (rental.Customer?.UserId != userId || rental.Status != RentalStatus.Pending))
                {
                    return Forbid();
                }

                var result = await _rentalService.CancelRentalAsync(id, request.CancellationReason);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Kiralama kaydı iptal edilemedi"));
                }

                _logger.LogInformation($"{id} ID'li kiralama kaydı iptal edildi");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Kiralama kaydı iptal edilirken geçersiz işlem: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li kiralama kaydı iptal edilirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Kiralama kaydı iptal edilirken bir hata oluştu"));
            }
        }

        public class CancelRentalRequest
        {
            [Required(ErrorMessage = "İptal nedeni gereklidir")]
            [StringLength(500, ErrorMessage = "İptal nedeni en fazla 500 karakter olabilir")]
            public string CancellationReason { get; set; } = string.Empty;
        }

        /// <summary>
        /// Kiralama kaydını tamamlar (Araç teslim alındı)
        /// </summary>
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CompleteRental(int id)
        {
            try
            {
                var result = await _rentalService.CompleteRentalAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Kiralama kaydı bulunamadı veya işlem yapılamaz durumda"));
                }

                _logger.LogInformation($"{id} ID'li kiralama kaydı tamamlandı olarak işaretlendi");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Kiralama kaydı tamamlanırken geçersiz işlem: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li kiralama kaydı tamamlanırken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Kiralama kaydı tamamlanırken bir hata oluştu"));
            }
        }

        public class FinishRentalRequest
        {
            [StringLength(1000, ErrorMessage = "Not en fazla 1000 karakter olabilir")]
            public string? ReturnNotes { get; set; }
            
            [Range(0, double.MaxValue, ErrorMessage = "Ek ücret negatif olamaz")]
            public decimal? AdditionalCharges { get; set; }
        }

        /// <summary>
        /// Kiralama kaydını sonlandırır (Araç teslim edildi)
        /// </summary>
        [HttpPost("{id}/finish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FinishRental(int id, [FromBody] FinishRentalRequest request)
        {
            try
            {
                var result = await _rentalService.FinishRentalAsync(
                    id, 
                    request.ReturnNotes ?? string.Empty, 
                    request.AdditionalCharges);

                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Kiralama kaydı bulunamadı veya işlem yapılamaz durumda"));
                }

                _logger.LogInformation($"{id} ID'li kiralama kaydı sonlandırıldı");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Kiralama kaydı sonlandırılırken geçersiz işlem: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li kiralama kaydı sonlandırılırken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Kiralama kaydı sonlandırılırken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Belirli bir tarih aralığındaki kiralama kayıtlarını getirir
        /// </summary>
        [HttpGet("by-date-range")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Rental>> >> GetRentalsByDateRange(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                _logger.LogInformation($"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} tarihleri arasındaki kiralama kayıtları getiriliyor");
                var rentals = await _rentalService.GetRentalsByDateRangeAsync(startDate, endDate);
                return Ok(ApiResponse<IEnumerable<Rental>>.SuccessResult(rentals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} tarihleri arasındaki kiralama kayıtları getirilirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
            }
        }

        /// <summary>
        /// Belirli bir araca ait kiralama kayıtlarını getirir
        /// </summary>
        [HttpGet("by-car/{carId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Rental>> >> GetRentalsByCar(int carId)
        {
            try
            {
                _logger.LogInformation($"{carId} ID'li araca ait kiralama kayıtları getiriliyor");
                var rentals = await _rentalService.GetCarRentalsAsync(carId);
                return Ok(ApiResponse<IEnumerable<Rental>>.SuccessResult(rentals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{carId} ID'li araca ait kiralama kayıtları getirilirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
            }
        }

        /// <summary>
        /// Belirli bir müşteriye ait kiralama kayıtlarını getirir
        /// </summary>
        [HttpGet("by-customer/{customerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Rental>> >> GetRentalsByCustomer(int customerId)
        {
            try
            {
                _logger.LogInformation($"{customerId} ID'li müşteriye ait kiralama kayıtları getiriliyor");
                var rentals = await _rentalService.GetCustomerRentalsAsync(customerId);
                return Ok(ApiResponse<IEnumerable<Rental>>.SuccessResult(rentals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{customerId} ID'li müşteriye ait kiralama kayıtları getirilirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
            }
        }

        /// <summary>
        /// Araç müsaitlik kontrolü yapar
        /// </summary>
        [HttpGet("check-availability")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<bool>>> CheckAvailability(
            [FromQuery] int carId, 
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                var isAvailable = await _rentalService.IsCarAvailableForRentAsync(carId, startDate, endDate);
                return Ok(ApiResponse<bool>.SuccessResult(isAvailable, isAvailable 
                    ? "Araç seçilen tarihlerde müsait" 
                    : "Araç seçilen tarihlerde müsait değil"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{carId} ID'li araç için müsaitlik kontrolü yapılırken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
            }
        }

        public class FakePaymentRequest
        {
            [Required(ErrorMessage = "Kart numarası gereklidir")]
            [CreditCard(ErrorMessage = "Geçersiz kart numarası")]
            public string CardNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Son kullanma tarihi gereklidir")]
            [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Geçersiz son kullanma tarihi (MM/YY)")]
            public string Expiry { get; set; } = string.Empty;

            [Required(ErrorMessage = "CVV gereklidir")]
            [StringLength(4, MinimumLength = 3, ErrorMessage = "Geçersiz CVV")]
            public string Cvv { get; set; } = string.Empty;

            [Required(ErrorMessage = "Kart sahibi adı gereklidir")]
            public string CardHolderName { get; set; } = string.Empty;
        }

        /// <summary>
        /// Sahte ödeme işlemi gerçekleştirir (Demo amaçlıdır)
        /// </summary>
        [HttpPost("fake-payment")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> ProcessFakePayment(
            [FromQuery] int rentalId,
            [FromBody] FakePaymentRequest request)
        {
            try
            {
                // Kiralama kaydını kontrol et
                var rental = await _rentalService.GetRentalByIdAsync(rentalId);
                if (rental == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Kiralama kaydı bulunamadı"));
                }

                // Kullanıcı kontrolü (Admin veya kendi kaydı)
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!User.IsInRole("Admin") && rental.Customer?.UserId != userId)
                {
                    return Forbid();
                }

                // Burada gerçek bir ödeme işlemi yapılmaz, sadece demo amaçlıdır
                _logger.LogInformation($"{rentalId} ID'li kiralama için ödeme alındı");

                // Kiralama durumunu güncelle
                rental.PaymentStatus = PaymentStatus.Completed;
                rental.PaymentDate = DateTime.UtcNow;
                await _rentalService.UpdateRentalAsync(rental);

                return Ok(ApiResponse<object>.SuccessResult(
                    null, 
                    "Ödeme başarıyla alındı ve kiralama onaylandı"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{rentalId} ID'li kiralama için ödeme işlenirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Ödeme işlenirken bir hata oluştu"));
            }
        }
    }
}
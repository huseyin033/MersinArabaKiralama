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
        /// <param name="parameters">Sayfalama ve sıralama parametreleri</param>
        /// <response code="200">Başarılı işlem</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Rental>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRentals([FromQuery] QueryParameters parameters)
        {
            try
            {
                _logger.LogInformation("Tüm kiralama kayıtları getiriliyor");
                
                var (rentals, totalCount) = await _rentalService.GetAllRentalsWithCountAsync(parameters);
                
                // Sayfalama bilgilerini oluştur
                var pagination = new PaginationMetadata
                {
                    TotalCount = totalCount,
                    PageSize = parameters.PageSize,
                    CurrentPage = parameters.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize)
                };

                var response = ApiResponse<IEnumerable<Rental>>.Success(
                    rentals,
                    pagination: pagination,
                    message: $"Toplam {totalCount} kiralama kaydı listelendi"
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kiralama kayıtları getirilirken bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error("Kiralama kayıtları getirilirken bir hata oluştu", HttpStatusCode.InternalServerError, new[] { ex.Message })
                );
            }
        }

        /// <summary>
        /// ID'ye göre kiralama kaydı getirir
        /// </summary>
        /// <param name="id">Kiralama kaydı ID'si</param>
        /// <response code="200">Başarılı işlem</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="403">Erişim reddedildi</response>
        /// <response code="404">Kiralama kaydı bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<Rental>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRental(int id)
        {
            try
            {
                _logger.LogInformation($"{id} ID'li kiralama kaydı getiriliyor");
                var rental = await _rentalService.GetRentalByIdAsync(id);

                if (rental == null)
                {
                    _logger.LogWarning($"{id} ID'li kiralama kaydı bulunamadı");
                    return NotFound(ApiResponse<object>.Error(
                        $"{id} ID'li kiralama kaydı bulunamadı", 
                        HttpStatusCode.NotFound
                    ));
                }

                // Kullanıcı kontrolü (Admin değilse sadece kendi kayıtlarını görebilir)
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!User.IsInRole("Admin") && rental.Customer?.UserId != userId)
                {
                    _logger.LogWarning($"{userId} ID'li kullanıcı yetkisiz kiralama kaydı erişimi denedi: {id}");
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        ApiResponse<object>.Error(
                            "Bu kaynağa erişim izniniz yok",
                            HttpStatusCode.Forbidden
                        )
                    );
                }

                var response = ApiResponse<Rental>.Success(
                    rental,
                    $"{id} ID'li kiralama kaydı başarıyla getirildi"
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li kiralama kaydı getirilirken bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error(
                        "Kiralama kaydı getirilirken bir hata oluştu",
                        HttpStatusCode.InternalServerError,
                        new[] { ex.Message }
                    )
                );
            }
        }

        /// <summary>
        /// Kullanıcının kiralama geçmişini getirir
        /// </summary>
        /// <response code="200">Başarılı işlem</response>
        /// <response code="400">Geçersiz istek</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="404">Müşteri bilgisi bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet("my-rentals")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Rental>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyRentals([FromQuery] QueryParameters parameters)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.Error(
                        "Kullanıcı bilgisi alınamadı",
                        HttpStatusCode.Unauthorized
                    ));
                }

                var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                if (customer == null)
                {
                    _logger.LogWarning($"{userId} ID'li kullanıcı için müşteri bilgisi bulunamadı");
                    return NotFound(ApiResponse<object>.Error(
                        "Müşteri bilgisi bulunamadı",
                        HttpStatusCode.NotFound
                    ));
                }

                _logger.LogInformation($"{customer.Id} ID'li müşterinin kiralama geçmişi getiriliyor");
                
                // Müşteriye özel filtre ekle
                parameters.Filters["CustomerId"] = customer.Id.ToString();
                
                var (rentals, totalCount) = await _rentalService.GetAllRentalsWithCountAsync(parameters);
                
                // Sayfalama bilgilerini oluştur
                var pagination = new PaginationMetadata
                {
                    TotalCount = totalCount,
                    PageSize = parameters.PageSize,
                    CurrentPage = parameters.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize)
                };

                var response = ApiResponse<IEnumerable<Rental>>.Success(
                    rentals,
                    pagination: pagination,
                    message: $"Toplam {totalCount} kiralama kaydınız listeleniyor"
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı kiralama geçmişi getirilirken bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error(
                        "Kiralama geçmişi getirilirken bir hata oluştu",
                        HttpStatusCode.InternalServerError,
                        new[] { ex.Message }
                    )
                );
            }
        }

        public class CreateRentalRequest
        {
            /// <summary>
            /// Kiralanacak aracın ID'si
            /// </summary>
            /// <example>1</example>
            [Required(ErrorMessage = "Araç ID'si gereklidir")]
            [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir araç ID'si giriniz")]
            public int CarId { get; set; }

            /// <summary>
            /// Kira başlangıç tarihi
            /// </summary>
            /// <example>2025-01-01T10:00:00Z</example>
            [Required(ErrorMessage = "Başlangıç tarihi gereklidir")]
            [DataType(DataType.DateTime)]
            public DateTime StartDate { get; set; }

            /// <summary>
            /// Kira bitiş tarihi
            /// </summary>
            /// <example>2025-01-05T10:00:00Z</example>
            [Required(ErrorMessage = "Bitiş tarihi gereklidir")]
            [DataType(DataType.DateTime)]
            public DateTime EndDate { get; set; }

            /// <summary>
            /// Kiralama ile ilgili notlar
            /// </summary>
            /// <example>Ekstra koltuk istiyorum</example>
            [StringLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir")]
            public string? Notes { get; set; }
        }

        /// <summary>
        /// Yeni bir kiralama kaydı oluşturur
        /// </summary>
        /// <param name="request">Kiralama bilgileri</param>
        /// <response code="201">Kiralama başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="404">Müşteri veya araç bulunamadı</response>
        /// <response code="409">Araç seçilen tarihlerde müsait değil</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<Rental>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateRental([FromBody] CreateRentalRequest request)
        {
            try
            {
                _logger.LogInformation("Yeni kiralama kaydı oluşturuluyor: {CarId}, {StartDate} - {EndDate}", 
                    request.CarId, request.StartDate, request.EndDate);

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

                // Tarih kontrolü
                if (request.StartDate < DateTime.UtcNow.Date)
                {
                    return BadRequest(ApiResponse<object>.Error(
                        "Başlangıç tarihi geçmiş bir tarih olamaz",
                        HttpStatusCode.BadRequest
                    ));
                }

                if (request.EndDate <= request.StartDate)
                {
                    return BadRequest(ApiResponse<object>.Error(
                        "Bitiş tarihi, başlangıç tarihinden sonra olmalıdır",
                        HttpStatusCode.BadRequest
                    ));
                }

                // Kullanıcı kontrolü
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.Error(
                        "Kullanıcı bilgisi alınamadı",
                        HttpStatusCode.Unauthorized
                    ));
                }

                // Müşteri kontrolü
                var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                if (customer == null)
                {
                    _logger.LogWarning($"{userId} ID'li kullanıcı için müşteri bilgisi bulunamadı");
                    return NotFound(ApiResponse<object>.Error(
                        "Müşteri bilgisi bulunamadı. Lütfen profil bilgilerinizi tamamlayın.",
                        HttpStatusCode.NotFound
                    ));
                }

                // Araç kontrolü
                var car = await _carService.GetCarByIdAsync(request.CarId);
                if (car == null)
                {
                    _logger.LogWarning($"{request.CarId} ID'li araç bulunamadı");
                    return NotFound(ApiResponse<object>.Error(
                        "Araç bulunamadı",
                        HttpStatusCode.NotFound
                    ));
                }


                // Aracın müsait olup olmadığını kontrol et
                var isCarAvailable = await _rentalService.IsCarAvailableForRentAsync(
                    request.CarId, 
                    request.StartDate, 
                    request.EndDate);

                if (!isCarAvailable)
                {
                    _logger.LogWarning($"{request.CarId} ID'li araç seçilen tarihlerde müsait değil: {request.StartDate} - {request.EndDate}");
                    return StatusCode(
                        StatusCodes.Status409Conflict,
                        ApiResponse<object>.Error(
                            "Seçilen tarihlerde araç müsait değil",
                            HttpStatusCode.Conflict
                        )
                    );
                }

                // Toplam ücreti hesapla
                var rentalDays = (request.EndDate - request.StartDate).Days + 1;
                var totalPrice = car.DailyPrice * rentalDays;

                // Kiralama kaydını oluştur
                var rental = new Rental
                {
                    CarId = request.CarId,
                    CustomerId = customer.Id,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    TotalPrice = totalPrice,
                    Notes = request.Notes,
                    Status = RentalStatus.Pending,
                    RentalDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdRental = await _rentalService.AddRentalAsync(rental);
                
                _logger.LogInformation($"{createdRental.Id} ID'li yeni kiralama kaydı oluşturuldu. Müşteri: {customer.Id}, Araç: {car.Id}");
                
                var response = ApiResponse<Rental>.Success(
                    createdRental,
                    HttpStatusCode.Created,
                    "Kiralama kaydı başarıyla oluşturuldu"
                );

                return CreatedAtAction(
                    nameof(GetRental), 
                    new { id = createdRental.Id, version = "1.0" },
                    response
                );
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
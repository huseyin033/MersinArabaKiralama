using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MersinArabaKiralama.Models;
using MersinArabaKiralama.Services;
using MersinArabaKiralama.ViewModels;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Controllers.Api
{
    /// <summary>
    /// Ödeme işlemlerini yöneten controller
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ICarService _carService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            ICarService carService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _carService = carService ?? throw new ArgumentNullException(nameof(carService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Yeni bir ödeme işlemi başlatır
        /// </summary>
        /// <param name="model">Ödeme bilgileri</param>
        /// <returns>Ödeme sonucu</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PaymentResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentViewModel model)
        {
            try
            {
                _logger.LogInformation("Yeni ödeme işlemi başlatılıyor. Araç ID: {CarId}", model.CarId);

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

                // Araç kontrolü
                var car = await _carService.GetCarByIdAsync(model.CarId ?? 0);
                if (car == null)
                {
                    _logger.LogWarning("Araç bulunamadı. ID: {CarId}", model.CarId);
                    return NotFound(ApiResponse<object>.Error(
                        "Araç bulunamadı",
                        HttpStatusCode.NotFound
                    ));
                }

                // Tarih kontrolü
                if (model.StartDate >= model.EndDate)
                {
                    _logger.LogWarning("Geçersiz tarih aralığı. Başlangıç: {StartDate}, Bitiş: {EndDate}", 
                        model.StartDate, model.EndDate);
                    return BadRequest(ApiResponse<object>.Error(
                        "Bitiş tarihi başlangıç tarihinden sonra olmalıdır",
                        HttpStatusCode.BadRequest
                    ));
                }

                // Kiralama gün sayısını hesapla
                model.RentalDays = (int)(model.EndDate.Value - model.StartDate.Value).TotalDays;
                if (model.RentalDays < 1)
                {
                    _logger.LogWarning("Geçersiz kiralama süresi: {Days} gün", model.RentalDays);
                    return BadRequest(ApiResponse<object>.Error(
                        "Minimum 1 günlük kiralama yapabilirsiniz",
                        HttpStatusCode.BadRequest
                    ));
                }

                // Toplam fiyatı hesapla
                model.DailyPrice = car.DailyPrice;
                model.TotalPrice = model.DailyPrice * model.RentalDays;

                // Ödeme işlemini gerçekleştir
                var paymentResult = await _paymentService.ProcessPaymentAsync(model);

                if (!paymentResult.Success)
                {
                    _logger.LogWarning("Ödeme işlemi başarısız. Hata: {Message}", paymentResult.Message);
                    return BadRequest(ApiResponse<object>.Error(
                        paymentResult.Message,
                        HttpStatusCode.BadRequest
                    ));
                }

                _logger.LogInformation("Ödeme işlemi başarılı. İşlem ID: {TransactionId}", paymentResult.TransactionId);
                
                return Ok(ApiResponse<PaymentResult>.Success(
                    paymentResult,
                    $"Ödeme işlemi başarıyla tamamlandı. İşlem No: {paymentResult.TransactionId}"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme işlemi sırasında bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error(
                        "Ödeme işlemi sırasında bir hata oluştu",
                        HttpStatusCode.InternalServerError,
                        new[] { ex.Message }
                    )
                );
            }
        }

        /// <summary>
        /// Ödeme işlemini iptal eder
        /// </summary>
        /// <param name="paymentId">İptal edilecek ödeme ID'si</param>
        /// <returns>İşlem sonucu</returns>
        [HttpPost("{paymentId}/cancel")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelPayment(string paymentId)
        {
            try
            {
                _logger.LogInformation("Ödeme iptal işlemi başlatılıyor. İşlem ID: {PaymentId}", paymentId);

                if (string.IsNullOrWhiteSpace(paymentId))
                {
                    _logger.LogWarning("Geçersiz ödeme ID'si");
                    return BadRequest(ApiResponse<object>.Error(
                        "Geçersiz ödeme ID'si",
                        HttpStatusCode.BadRequest
                    ));
                }

                var result = await _paymentService.CancelPaymentAsync(paymentId);
                
                if (!result)
                {
                    _logger.LogWarning("Ödeme iptal işlemi başarısız. İşlem ID: {PaymentId}", paymentId);
                    return BadRequest(ApiResponse<object>.Error(
                        "Ödeme iptal edilemedi",
                        HttpStatusCode.BadRequest
                    ));
                }

                _logger.LogInformation("Ödeme başarıyla iptal edildi. İşlem ID: {PaymentId}", paymentId);
                return Ok(ApiResponse<bool>.Success(
                    true,
                    "Ödeme başarıyla iptal edildi"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme iptal işlemi sırasında bir hata oluştu. İşlem ID: {PaymentId}", paymentId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error(
                        "Ödeme iptal işlemi sırasında bir hata oluştu",
                        HttpStatusCode.InternalServerError,
                        new[] { ex.Message }
                    )
                );
            }
        }

        /// <summary>
        /// Ödeme durumunu sorgular
        /// </summary>
        /// <param name="paymentId">Sorgulanacak ödeme ID'si</param>
        /// <returns>Ödeme durumu</returns>
        [HttpGet("{paymentId}/status")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PaymentStatus>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaymentStatus(string paymentId)
        {
            try
            {
                _logger.LogInformation("Ödeme durumu sorgulanıyor. İşlem ID: {PaymentId}", paymentId);

                if (string.IsNullOrWhiteSpace(paymentId))
                {
                    _logger.LogWarning("Geçersiz ödeme ID'si");
                    return BadRequest(ApiResponse<object>.Error(
                        "Geçersiz ödeme ID'si",
                        HttpStatusCode.BadRequest
                    ));
                }

                var status = await _paymentService.CheckPaymentStatusAsync(paymentId);
                
                _logger.LogInformation("Ödeme durumu alındı. İşlem ID: {PaymentId}, Durum: {Status}", 
                    paymentId, status);
                
                return Ok(ApiResponse<PaymentStatus>.Success(
                    status,
                    $"Ödeme durumu başarıyla getirildi: {status}"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme durumu sorgulanırken bir hata oluştu. İşlem ID: {PaymentId}", paymentId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error(
                        "Ödeme durumu sorgulanırken bir hata oluştu",
                        HttpStatusCode.InternalServerError,
                        new[] { ex.Message }
                    )
                );
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MersinArabaKiralama.Models;
using MersinArabaKiralama.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Controllers.Api
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class CustomersApiController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersApiController> _logger;

        public CustomersApiController(
            ICustomerService customerService,
            ILogger<CustomersApiController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm müşterileri getirir
        /// </summary>
        /// <summary>
        /// Tüm müşterileri getirir
        /// </summary>
        /// <param name="parameters">Sayfalama ve sıralama parametreleri</param>
        /// <response code="200">Başarılı işlem</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Customer>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomers([FromQuery] QueryParameters parameters)
        {
            try
            {
                _logger.LogInformation("Tüm müşteriler getiriliyor");
                
                var (customers, totalCount) = await _customerService.GetAllCustomersWithCountAsync(parameters);
                
                // Sayfalama bilgilerini oluştur
                var pagination = new PaginationMetadata
                {
                    TotalCount = totalCount,
                    PageSize = parameters.PageSize,
                    CurrentPage = parameters.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize)
                };

                var response = ApiResponse<IEnumerable<Customer>>.Success(
                    customers,
                    pagination: pagination,
                    message: $"Toplam {totalCount} müşteri listelendi"
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteriler getirilirken bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    ApiResponse<object>.Error("Müşteriler getirilirken bir hata oluştu", HttpStatusCode.InternalServerError, new[] { ex.Message })
                );
            }
        }

        /// <summary>
        /// ID'ye göre müşteri getirir
        /// </summary>
        /// <summary>
        /// ID'ye göre müşteri getirir
        /// </summary>
        /// <param name="id">Müşteri ID'si</param>
        /// <response code="200">Müşteri bulundu</response>
        /// <response code="404">Müşteri bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<Customer>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomer(int id)
        {
            try
            {
                _logger.LogInformation($"{id} ID'li müşteri getiriliyor");
                var customer = await _customerService.GetCustomerByIdAsync(id);

                if (customer == null)
                {
                    _logger.LogWarning($"{id} ID'li müşteri bulunamadı");
                    return NotFound(ApiResponse<object>.NotFound($"{id} ID'li müşteri bulunamadı"));
                }

                return Ok(ApiResponse<Customer>.Success(customer, $"{customer.FullName} adlı müşteri getirildi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li müşteri getirilirken bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error("Müşteri getirilirken bir hata oluştu", HttpStatusCode.InternalServerError, new[] { ex.Message })
                );
            }
        }

        /// <summary>
        /// E-posta adresine göre müşteri getirir
        /// </summary>
        /// <summary>
        /// E-posta adresine göre müşteri getirir
        /// </summary>
        /// <param name="email">Müşteri e-posta adresi</param>
        /// <response code="200">Müşteri bulundu</response>
        /// <response code="400">Geçersiz e-posta formatı</response>
        /// <response code="404">Müşteri bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet("by-email/{email}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<Customer>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerByEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Geçerli bir e-posta adresi giriniz"));
                }

                _logger.LogInformation($"{email} e-posta adresli müşteri getiriliyor");
                var customer = await _customerService.GetCustomerByEmailAsync(email);

                if (customer == null)
                {
                    _logger.LogWarning($"{email} e-posta adresli müşteri bulunamadı");
                    return NotFound(ApiResponse<object>.NotFound($"{email} e-posta adresli müşteri bulunamadı"));
                }

                return Ok(ApiResponse<Customer>.Success(customer, $"{customer.Email} adresine sahip müşteri getirildi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{email} e-posta adresli müşteri getirilirken bir hata oluştu");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Error("Müşteri getirilirken bir hata oluştu", HttpStatusCode.InternalServerError, new[] { ex.Message })
                );
            }
        }

        /// <summary>
        /// Yeni bir müşteri ekler
        /// </summary>
        [HttpPost]
        [AllowAnonymous] // Kayıt işlemi için authentication gerekmez
        public async Task<ActionResult<ApiResponse<Customer>>> PostCustomer(Customer customer)
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

                _logger.LogInformation($"{customer.FirstName} {customer.LastName} adlı yeni müşteri ekleniyor");
                var createdCustomer = await _customerService.AddCustomerAsync(customer);

                return CreatedAtAction(
                    nameof(GetCustomer),
                    new { id = createdCustomer.Id, version = "1.0" },
                    ApiResponse<Customer>.SuccessResult(createdCustomer, "Müşteri başarıyla eklendi"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Müşteri eklenirken geçersiz işlem: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri eklenirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Müşteri eklenirken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Mevcut bir müşteriyi günceller
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            try
            {
                if (id != customer.Id)
                {
                    _logger.LogWarning("İstekteki ID ile müşteri ID'si uyuşmuyor");
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

                _logger.LogInformation($"{id} ID'li müşteri güncelleniyor");
                var result = await _customerService.UpdateCustomerAsync(customer);

                if (!result)
                {
                    _logger.LogWarning($"{id} ID'li müşteri bulunamadı");
                    return NotFound(ApiResponse<object>.ErrorResult("Müşteri bulunamadı"));
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Müşteri güncellenirken geçersiz işlem: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li müşteri güncellenirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Müşteri güncellenirken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Bir müşteriyi siler
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                _logger.LogInformation($"{id} ID'li müşteri siliniyor");
                var result = await _customerService.DeleteCustomerAsync(id);

                if (!result)
                {
                    _logger.LogWarning($"{id} ID'li müşteri bulunamadı");
                    return NotFound(ApiResponse<object>.ErrorResult("Müşteri bulunamadı"));
                }


                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Müşteri silinirken geçersiz işlem: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li müşteri silinirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Müşteri silinirken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Müşterinin kiralama geçmişini getirir
        /// </summary>
        [HttpGet("{id}/rentals")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Rental>>>> GetCustomerRentals(int id)
        {
            try
            {
                _logger.LogInformation($"{id} ID'li müşterinin kiralama geçmişi getiriliyor");
                var rentals = await _customerService.GetCustomerRentalsAsync(id);
                return Ok(ApiResponse<IEnumerable<Rental>>.SuccessResult(rentals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li müşterinin kiralama geçmişi getirilirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
            }
        }
    }
}
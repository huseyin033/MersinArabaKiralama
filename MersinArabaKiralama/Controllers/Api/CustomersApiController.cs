using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MersinArabaKiralama.Models;
using MersinArabaKiralama.Services;
using System;
using System.Collections.Generic;
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
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Customer>>>> GetCustomers([FromQuery] QueryParameters parameters)
        {
            try
            {
                _logger.LogInformation("Tüm müşteriler getiriliyor");
                var customers = await _customerService.GetAllCustomersAsync(parameters);
                return Ok(ApiResponse<IEnumerable<Customer>>.SuccessResult(customers));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteriler getirilirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
            }
        }

        /// <summary>
        /// ID'ye göre müşteri getirir
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<Customer>>> GetCustomer(int id)
        {
            try
            {
                _logger.LogInformation($"{id} ID'li müşteri getiriliyor");
                var customer = await _customerService.GetCustomerByIdAsync(id);

                if (customer == null)
                {
                    _logger.LogWarning($"{id} ID'li müşteri bulunamadı");
                    return NotFound(ApiResponse<object>.ErrorResult("Müşteri bulunamadı"));
                }

                return Ok(ApiResponse<Customer>.SuccessResult(customer));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{id} ID'li müşteri getirilirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
            }
        }

        /// <summary>
        /// E-posta adresine göre müşteri getirir
        /// </summary>
        [HttpGet("by-email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<Customer>>> GetCustomerByEmail(string email)
        {
            try
            {
                _logger.LogInformation($"{email} e-posta adresli müşteri getiriliyor");
                var customer = await _customerService.GetCustomerByEmailAsync(email);

                if (customer == null)
                {
                    _logger.LogWarning($"{email} e-posta adresli müşteri bulunamadı");
                    return NotFound(ApiResponse<object>.ErrorResult("Müşteri bulunamadı"));
                }

                return Ok(ApiResponse<Customer>.SuccessResult(customer));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{email} e-posta adresli müşteri getirilirken bir hata oluştu");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Bir hata oluştu"));
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
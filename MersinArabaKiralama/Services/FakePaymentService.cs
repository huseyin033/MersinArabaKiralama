using Microsoft.Extensions.Logging;
using MersinArabaKiralama.Models;
using MersinArabaKiralama.ViewModels;
using System;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Services
{
    public class FakePaymentService : IPaymentService
    {
        private readonly ILogger<FakePaymentService> _logger;
        private static readonly Random _random = new Random();

        public FakePaymentService(ILogger<FakePaymentService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentViewModel payment)
        {
            _logger.LogInformation($"Ödeme işlemi başlatılıyor. Tutar: {payment.Amount} {payment.Currency}");
            
            // Simüle edilmiş gecikme (50-300ms arası rastgele)
            await Task.Delay(_random.Next(50, 300));

            try
            {
                // Basit validasyonlar
                if (payment == null)
                    throw new ArgumentNullException(nameof(payment));

                if (payment.Amount <= 0)
                    throw new ArgumentException("Geçersiz ödeme tutarı", nameof(payment.Amount));

                // Kredi kartı numarası validasyonu (basit Luhn algoritması)
                if (!IsValidCreditCardNumber(payment.CardNumber))
                {
                    _logger.LogWarning("Geçersiz kredi kartı numarası");
                    return new PaymentResult
                    {
                        Success = false,
                        Message = "Geçersiz kredi kartı numarası",
                        TransactionDate = DateTime.UtcNow
                    };
                }

                // Kartın son kullanma tarihi kontrolü
                if (payment.ExpiryDate < DateTime.Now)
                {
                    _logger.LogWarning("Kartın son kullanma tarihi geçmiş");
                    return new PaymentResult
                    {
                        Success = false,
                        Message = "Kartın son kullanma tarihi geçmiş",
                        TransactionDate = DateTime.UtcNow
                    };
                }

                // CVV kontrolü
                if (string.IsNullOrWhiteSpace(payment.CVV) || payment.CVV.Length < 3 || payment.CVV.Length > 4)
                {
                    _logger.LogWarning("Geçersiz CVV numarası");
                    return new PaymentResult
                    {
                        Success = false,
                        Message = "Geçersiz CVV numarası",
                        TransactionDate = DateTime.UtcNow
                    };
                }

                // Rastgele başarısız ödeme şansı (%5)
                if (_random.Next(1, 101) <= 5)
                {
                    _logger.LogWarning("Ödeme işlemi başarısız oldu (simüle edilmiş hata)");
                    return new PaymentResult
                    {
                        Success = false,
                        Message = "Ödeme işlemi başarısız oldu. Lütfen kart bilgilerinizi kontrol edip tekrar deneyin.",
                        TransactionDate = DateTime.UtcNow
                    };
                }

                // Başarılı ödeme
                var transactionId = GenerateTransactionId();
                _logger.LogInformation($"Ödeme başarılı. İşlem ID: {transactionId}");
                
                return new PaymentResult
                {
                    Success = true,
                    TransactionId = transactionId,
                    Message = "Ödeme başarıyla alındı",
                    TransactionDate = DateTime.UtcNow,
                    Amount = payment.Amount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme işlemi sırasında bir hata oluştu");
                throw new ApplicationException("Ödeme işlemi sırasında bir hata oluştu", ex);
            }
        }

        public async Task<bool> CancelPaymentAsync(string paymentId)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                throw new ArgumentNullException(nameof(paymentId));

            _logger.LogInformation($"İptal işlemi başlatılıyor. İşlem ID: {paymentId}");
            
            // Simüle edilmiş gecikme (100-500ms arası rastgele)
            await Task.Delay(_random.Next(100, 500));

            // Rastgele başarısız iptal şansı (%10)
            if (_random.Next(1, 101) <= 10)
            {
                _logger.LogWarning($"İptal işlemi başarısız oldu. İşlem ID: {paymentId}");
                return false;
            }

            _logger.LogInformation($"İptal işlemi başarılı. İşlem ID: {paymentId}");
            return true;
        }

        public async Task<PaymentStatus> CheckPaymentStatusAsync(string paymentId)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                throw new ArgumentNullException(nameof(paymentId));

            _logger.LogInformation($"Ödeme durumu sorgulanıyor. İşlem ID: {paymentId}");
            
            // Simüle edilmiş gecikme (50-200ms arası rastgele)
            await Task.Delay(_random.Next(50, 200));

            // Rastgele bir ödeme durumu döndür (çoğunlukla Completed)
            var statuses = new[]
            {
                PaymentStatus.Pending,
                PaymentStatus.Completed,
                PaymentStatus.Completed,
                PaymentStatus.Completed,
                PaymentStatus.Failed,
                PaymentStatus.Refunded,
                PaymentStatus.Cancelled
            };

            var status = statuses[_random.Next(statuses.Length)];
            _logger.LogInformation($"Ödeme durumu: {status}. İşlem ID: {paymentId}");
            
            return status;
        }

        private bool IsValidCreditCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            // Sadece rakamları al
            var cleanNumber = new string(cardNumber.Where(char.IsDigit).ToArray());
            
            // Luhn Algoritması
            int sum = 0;
            bool isSecond = false;
            
            for (int i = cleanNumber.Length - 1; i >= 0; i--)
            {
                int digit = cleanNumber[i] - '0';

                if (isSecond)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit = (digit / 10) + (digit % 10);
                    }
                }

                sum += digit;
                isSecond = !isSecond;
            }

            return (sum % 10 == 0);
        }

        private string GenerateTransactionId()
        {
            // Örnek: PAY-20230616-ABCD1234
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
                
            return $"PAY-{date}-{random}";
        }
    }
}

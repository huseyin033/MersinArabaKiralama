using MersinArabaKiralama.Models;
using MersinArabaKiralama.ViewModels;

namespace MersinArabaKiralama.Services
{
    public interface IPaymentService
    {
        /// <summary>
        /// Sahte ödeme işlemi gerçekleştirir
        /// </summary>
        /// <param name="payment">Ödeme bilgileri</param>
        /// <returns>Ödeme sonucunu döndürür</returns>
        Task<PaymentResult> ProcessPaymentAsync(PaymentViewModel payment);

        /// <summary>
        /// Ödeme iptal işlemi gerçekleştirir
        /// </summary>
        /// <param name="paymentId">İptal edilecek ödeme ID'si</param>
        /// <returns>İşlem sonucu</returns>
        Task<bool> CancelPaymentAsync(string paymentId);

        /// <summary>
        /// Ödeme durumunu kontrol eder
        /// </summary>
        /// <param name="paymentId">Sorgulanacak ödeme ID'si</param>
        /// <returns>Ödeme durumu</returns>
        Task<PaymentStatus> CheckPaymentStatusAsync(string paymentId);
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded,
        Cancelled
    }
}

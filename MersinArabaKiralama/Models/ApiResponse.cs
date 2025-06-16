using System.Net;

namespace MersinArabaKiralama.Models
{
    /// <summary>
    /// Sayfalama bilgilerini içeren sınıf
    /// </summary>
    public class PaginationMetadata
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    /// <summary>
    /// API yanıtları için temel sınıf
    /// </summary>
    /// <typeparam name="T">Dönecek veri tipi</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// İşlemin başarılı olup olmadığını belirtir
        /// </summary>
        public bool Success { get; set; } // Bu, 'Success' özelliğinin TEK tanımı olmalı

        /// <summary>
        /// HTTP durum kodu
        /// </summary>
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        /// <summary>
        /// İşlem hakkında bilgilendirici mesaj
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// İşlem sonucu dönen veri
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Sayfalama bilgileri
        /// </summary>
        public PaginationMetadata? Pagination { get; set; }

        /// <summary>
        /// Hata mesajları listesi
        /// </summary>
        public IEnumerable<string>? Errors { get; set; }

        /// <summary>
        /// Yanıtın oluşturulma zamanı (UTC)
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string? Path { get; set; }

        // Başarılı yanıtlar
        public static ApiResponse<T> Success(T data, string? message = null, PaginationMetadata? pagination = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Data = data,
                Message = message ?? "İşlem başarılı",
                Pagination = pagination
            };
        }

        public static ApiResponse<T> Created(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = HttpStatusCode.Created,
                Data = data,
                Message = message ?? "Kayıt başarıyla oluşturuldu"
            };
        }

        public static ApiResponse<T> NoContent(string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = HttpStatusCode.NoContent,
                Message = message ?? "İçerik bulunamadı"
            };
        }

        // Hata yanıtları
        public static ApiResponse<T> BadRequest(string message, IEnumerable<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false, // Tüm hata metotlarında 'Success' kullanılıyor
                StatusCode = HttpStatusCode.BadRequest,
                Message = message,
                Errors = errors
            };
        }

        public static ApiResponse<T> Unauthorized(string message = "Yetkiniz yok")
        {
            return new ApiResponse<T>
            {
                Success = false, // Tüm hata metotlarında 'Success' kullanılıyor
                StatusCode = HttpStatusCode.Unauthorized,
                Message = message
            };
        }

        public static ApiResponse<T> Forbidden(string message = "Erişim engellendi")
        {
            return new ApiResponse<T>
            {
                Success = false, // Tüm hata metotlarında 'Success' kullanılıyor
                StatusCode = HttpStatusCode.Forbidden,
                Message = message
            };
        }

        public static ApiResponse<T> NotFound(string message = "Kaynak bulunamadı")
        {
            return new ApiResponse<T>
            {
                Success = false, // Tüm hata metotlarında 'Success' kullanılıyor
                StatusCode = HttpStatusCode.NotFound,
                Message = message
            };
        }

        /// <summary>
        /// Genel hata yanıtı oluşturur
        /// </summary>
        /// <param name="message">Hata mesajı</param>
        /// <param name="statusCode">HTTP durum kodu</param>
        /// <param name="errors">Hata detayları listesi</param>
        /// <returns>ApiResponse&lt;T&gt; nesnesi</returns>
        public static ApiResponse<T> Error(string message, HttpStatusCode statusCode, IEnumerable<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success1 = false, // Tüm hata metotlarında 'Success' kullanılıyor
                StatusCode = statusCode,
                Message = message,
                Errors = errors ?? Array.Empty<string>()
            };
        }
    }
}



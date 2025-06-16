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
        public bool Success { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string? Message { get; set; }
        public T? Data { get; set; }
        public PaginationMetadata? Pagination { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string? Path { get; set; }

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

        public static ApiResponse<T> BadRequest(string message, IEnumerable<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Message = message,
                Errors = errors
            };
        }

        public static ApiResponse<T> Unauthorized(string message = "Yetkiniz yok")
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = HttpStatusCode.Unauthorized,
                Message = message
            };
        }

        public static ApiResponse<T> Forbidden(string message = "Erişim engellendi")
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = HttpStatusCode.Forbidden,
                Message = message
            };
        }

        public static ApiResponse<T> NotFound(string message = "Kaynak bulunamadı")
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = message
            };
        }

        public static ApiResponse<T> Error(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, IEnumerable<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Errors = errors
            };
        }
    }
}

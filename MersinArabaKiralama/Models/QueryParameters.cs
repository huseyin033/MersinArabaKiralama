using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MersinArabaKiralama.Models
{
    /// <summary>
    /// Sıralama yönünü belirten enum
    /// </summary>
    public enum SortDirection
    {
        [Description("asc")]
        Ascending,
        [Description("desc")]
        Descending
    }

    /// <summary>
    /// API isteklerinde kullanılacak sorgu parametreleri
    /// </summary>
    public class QueryParameters
    {
        private const int DefaultMaxPageSize = 100;
        private const int DefaultPageSize = 10;
        private int _pageSize = DefaultPageSize;
        private int _maxPageSize = DefaultMaxPageSize;
        private string? _orderBy;

        /// <summary>
        /// Sayfa numarası (1'den başlar)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası 1'den büyük olmalıdır")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Sayfa başına kayıt sayısı
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Sayfa boyutu 1'den büyük olmalıdır")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > _maxPageSize ? _maxPageSize : (value < 1 ? 1 : value);
        }

        /// <summary>
        /// Maksimum sayfa boyutu
        /// </summary>
        [JsonIgnore]
        public int MaxPageSize
        {
            get => _maxPageSize;
            set
            {
                _maxPageSize = value;
                if (_pageSize > _maxPageSize)
                    _pageSize = _maxPageSize;
            }
        }

        /// <summary>
        /// Arama terimi
        /// </summary>
        [StringLength(100, ErrorMessage = "Arama terimi en fazla 100 karakter olabilir")]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Sıralanacak alan adı
        /// </summary>
        public string? OrderBy
        {
            get => _orderBy;
            set => _orderBy = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        /// <summary>
        /// Sıralama yönü (asc/desc)
        /// </summary>
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

        /// <summary>
        /// Sıralama ifadesini döndürür ("fieldName asc/desc" formatında)
        /// </summary>
        [JsonIgnore]
        public string? OrderByString => string.IsNullOrEmpty(OrderBy) 
            ? null 
            : $"{OrderBy} {(SortDirection == SortDirection.Ascending ? "asc" : "desc")}";

        /// <summary>
        /// Atlanacak kayıt sayısı (sayfalama için)
        /// </summary>
        [JsonIgnore]
        public int Skip => (PageNumber - 1) * PageSize;

        /// <summary>
        /// Filtreleme için ekstra parametreler (key-value çiftleri)
        /// </summary>
        public Dictionary<string, string> Filters { get; set; } = new();

        /// <summary>
        /// Belirtilen filtre değerini döndürür
        /// </summary>
        public string? GetFilterValue(string key)
        {
            return Filters.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Belirtilen filtre değerini ayarlar
        /// </summary>
        public void SetFilterValue(string key, string value)
        {
            if (Filters.ContainsKey(key))
                Filters[key] = value;
            else
                Filters.Add(key, value);
        }
    }
}

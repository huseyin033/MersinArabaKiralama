/**
 * Tarih formatlama yardımcısı
 * @param {Date|string} date - Formatlanacak tarih
 * @param {string} format - İstenen çıktı formatı (default: 'DD.MM.YYYY')
 * @returns {string} Formatlanmış tarih string'i
 */
export const formatDate = (date, format = 'DD.MM.YYYY') => {
  if (!date) return '';
  
  const d = new Date(date);
  if (isNaN(d.getTime())) return '';
  
  const day = d.getDate().toString().padStart(2, '0');
  const month = (d.getMonth() + 1).toString().padStart(2, '0');
  const year = d.getFullYear();
  const hours = d.getHours().toString().padStart(2, '0');
  const minutes = d.getMinutes().toString().padStart(2, '0');
  
  return format
    .replace('DD', day)
    .replace('MM', month)
    .replace('YYYY', year)
    .replace('HH', hours)
    .replace('mm', minutes);
};

/**
 * Sayısal değeri para birimi formatında gösterir
 * @param {number} value - Formatlanacak sayı
 * @param {string} currency - Para birimi (default: '₺')
 * @param {number} decimals - Ondalık basamak sayısı (default: 2)
 * @returns {string} Formatlanmış para birimi
 */
export const formatCurrency = (value, currency = '₺', decimals = 2) => {
  if (value === null || value === undefined || isNaN(value)) return `${currency}0,00`;
  
  const fixedValue = Number(value).toFixed(decimals);
  const parts = fixedValue.toString().split('.');
  parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  
  return `${currency}${parts.join(',')}`;
};

/**
 * String'deki türkçe karakterleri ingilizce karşılıklarına çevirir
 * @param {string} text - Dönüştürülecek metin
 * @returns {string} Dönüştürülmüş metin
 */
export const toEnglishChars = (text) => {
  if (!text) return '';
  
  const trChars = 'ğüşıöçĞÜŞİÖÇ';
  const enChars = 'gusiocGUSIOC';
  
  return text.split('').map(char => {
    const index = trChars.indexOf(char);
    return index !== -1 ? enChars[index] : char;
  }).join('');
};

/**
 * Bir string'i URL uyumlu hale getirir
 * @param {string} text - Dönüştürülecek metin
 * @returns {string} URL uyumlu metin
 */
export const toUrlSlug = (text) => {
  if (!text) return '';
  
  return toEnglishChars(text)
    .toLowerCase()
    .replace(/[^\w\s-]/g, '') // Özel karakterleri kaldır
    .replace(/\s+/g, '-') // Boşlukları tire ile değiştir
    .replace(/--+/g, '-') // Ardışık tireleri tek tire yap
    .trim() // Baştaki ve sondaki boşlukları kaldır
    .replace(/^-+|-+$/g, ''); // Baştaki ve sondaki tireleri kaldır
};

/**
 * İki tarih arasındaki gün farkını hesaplar
 * @param {Date|string} startDate - Başlangıç tarihi
 * @param {Date|string} endDate - Bitiş tarihi
 * @returns {number} İki tarih arasındaki gün sayısı
 */
export const getDaysBetweenDates = (startDate, endDate) => {
  const start = new Date(startDate);
  const end = new Date(endDate);
  
  // Tarihleri sıfırlama (saat, dakika, saniye, milisaniye)
  start.setHours(0, 0, 0, 0);
  end.setHours(0, 0, 0, 0);
  
  const timeDiff = end - start;
  return Math.ceil(timeDiff / (1000 * 60 * 60 * 24)) + 1; // +1 gün dahil etmek için
};

/**
 * Verilen boyuta göre dosya boyutunu uygun formatta gösterir
 * @param {number} bytes - Dosya boyutu (byte)
 * @param {number} decimals - Ondalık basamak sayısı
 * @returns {string} Formatlanmış dosya boyutu
 */
export const formatFileSize = (bytes, decimals = 2) => {
  if (bytes === 0) return '0 Bytes';
  
  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
  
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
};

/**
 * Verilen metni belirtilen uzunlukta kısaltır
 * @param {string} text - Kısaltılacak metin
 * @param {number} maxLength - Maksimum uzunluk
 * @param {string} suffix - Son ek (default: '...')
 * @returns {string} Kısaltılmış metin
 */
export const truncateText = (text, maxLength, suffix = '...') => {
  if (!text || text.length <= maxLength) return text || '';
  return text.substring(0, maxLength) + suffix;
};

/**
 * Telefon numarasını formatlar
 * @param {string} phone - Formatlanacak telefon numarası
 * @returns {string} Formatlanmış telefon numarası
 */
export const formatPhoneNumber = (phone) => {
  if (!phone) return '';
  
  // Sadece sayıları al
  const cleaned = ('' + phone).replace(/\D/g, '');
  
  // 10 haneli numara ise (5XX XXX XX XX)
  if (cleaned.length === 10) {
    return cleaned.replace(/(\d{3})(\d{3})(\d{2})(\d{2})/, '($1) $2 $3 $4');
  }
  
  // 11 haneli numara ise (05XX XXX XX XX)
  if (cleaned.length === 11) {
    return cleaned.replace(/(\d{4})(\d{3})(\d{2})(\d{2})/, '($1) $2 $3 $4');
  }
  
  // Diğer durumlarda orijinal numarayı döndür
  return phone;
};

/**
 * E-posta adresinin geçerli olup olmadığını kontrol eder
 * @param {string} email - Kontrol edilecek e-posta adresi
 * @returns {boolean} Geçerliyse true, değilse false
 */
export const isValidEmail = (email) => {
  if (!email) return false;
  
  const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return re.test(String(email).toLowerCase());
};

/**
 * Şifrenin güvenlik kurallarına uygun olup olmadığını kontrol eder
 * @param {string} password - Kontrol edilecek şifre
 * @returns {Object} Geçerlilik durumu ve hata mesajı
 */
export const validatePassword = (password) => {
  if (!password) {
    return {
      isValid: false,
      message: 'Şifre gereklidir.'
    };
  }
  
  if (password.length < 8) {
    return {
      isValid: false,
      message: 'Şifre en az 8 karakter uzunluğunda olmalıdır.'
    };
  }
  
  if (!/[A-Z]/.test(password)) {
    return {
      isValid: false,
      message: 'Şifre en az bir büyük harf içermelidir.'
    };
  }
  
  if (!/[a-z]/.test(password)) {
    return {
      isValid: false,
      message: 'Şifre en az bir küçük harf içermelidir.'
    };
  }
  
  if (!/[0-9]/.test(password)) {
    return {
      isValid: false,
      message: 'Şifre en az bir rakam içermelidir.'
    };
  }
  
  // if (!/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
  //   return {
  //     isValid: false,
  //     message: 'Şifre en az bir özel karakter içermelidir.'
  //   };
  // }
  
  return {
    isValid: true,
    message: 'Şifre geçerli.'
  };
};

/**
 * TC Kimlik Numarası doğrulama
 * @param {string} tcNo - Doğrulanacak TC Kimlik Numarası
 * @returns {boolean} Geçerliyse true, değilse false
 */
export const validateTCKN = (tcNo) => {
  if (!tcNo) return false;
  
  // 11 haneli mi ve sadece rakamlardan mı oluşuyor?
  if (tcNo.length !== 11 || !/^\d+$/.test(tcNo)) {
    return false;
  }
  
  // İlk hane 0 olamaz
  if (tcNo[0] === '0') {
    return false;
  }
  
  // 1-3-5-7-9. hanelerin toplamının 7 katından, 2-4-6-8. hanelerin toplamı çıkarılıp
  // 10'a bölümünden kalan 10. haneyi vermeli
  const digits = tcNo.split('').map(Number);
  const tenthDigit = (digits[0] + digits[2] + digits[4] + digits[6] + digits[8]) * 7 - 
                    (digits[1] + digits[3] + digits[5] + digits[7]);
  
  if (tenthDigit % 10 !== digits[9]) {
    return false;
  }
  
  // İlk 10 hanenin toplamının 10'a bölümünden kalan 11. haneyi vermeli
  const sumFirstTen = digits.slice(0, 10).reduce((a, b) => a + b, 0);
  if (sumFirstTen % 10 !== digits[10]) {
    return false;
  }
  
  return true;
};

/**
 * Vergi Kimlik Numarası doğrulama
 * @param {string} vkn - Doğrulanacak Vergi Kimlik Numarası
 * @returns {boolean} Geçerliyse true, değilse false
 */
export const validateVKN = (vkn) => {
  if (!vkn) return false;
  
  // 10 haneli mi ve sadece rakamlardan mı oluşuyor?
  if (vkn.length !== 10 || !/^\d+$/.test(vkn)) {
    return false;
  }
  
  const digits = vkn.split('').map(Number);
  
  // İlk hane 0 olamaz
  if (digits[0] === 0) {
    return false;
  }
  
  // VKN doğrulama algoritması
  const weights = [3, 1, 7, 2, 4, 1, 2, 3, 4, 5];
  let sum = 0;
  
  for (let i = 0; i < 9; i++) {
    sum += (digits[i] + weights[i]) % 10 * Math.pow(2, 9 - i) % 9;
    
    if (digits[i] !== 0 && sum === 0) {
      sum = 9;
    }
    
    sum = sum % 10;
  }
  
  const checksum = (10 - sum) % 10;
  
  return checksum === digits[9];
};

/**
 * Nesneyi derinlemesine kopyalar
 * @param {Object} obj - Kopyalanacak nesne
 * @returns {Object} Derin kopyalanmış yeni nesne
 */
export const deepClone = (obj) => {
  if (obj === null || typeof obj !== 'object') {
    return obj;
  }
  
  if (obj instanceof Date) {
    return new Date(obj);
  }
  
  if (Array.isArray(obj)) {
    return obj.map(item => deepClone(item));
  }
  
  const cloned = {};
  
  for (const key in obj) {
    if (obj.hasOwnProperty(key)) {
      cloned[key] = deepClone(obj[key]);
    }
  }
  
  return cloned;
};

/**
 * İki nesnenin eşit olup olmadığını derinlemesine karşılaştırır
 * @param {Object} obj1 - İlk nesne
 * @param {Object} obj2 - İkinci nesne
 * @returns {boolean} Eşitse true, değilse false
 */
export const deepEqual = (obj1, obj2) => {
  // İlk olarak basit karşılaştırma
  if (obj1 === obj2) return true;
  
  // Null veya undefined kontrolü
  if (obj1 == null || obj2 == null) return false;
  
  // Tip kontrolü
  if (typeof obj1 !== typeof obj2) return false;
  
  // Tarih karşılaştırması
  if (obj1 instanceof Date && obj2 instanceof Date) {
    return obj1.getTime() === obj2.getTime();
  }
  
  // Dizi karşılaştırması
  if (Array.isArray(obj1) && Array.isArray(obj2)) {
    if (obj1.length !== obj2.length) return false;
    
    for (let i = 0; i < obj1.length; i++) {
      if (!deepEqual(obj1[i], obj2[i])) return false;
    }
    
    return true;
  }
  
  // Nesne karşılaştırması
  if (typeof obj1 === 'object' && typeof obj2 === 'object') {
    const keys1 = Object.keys(obj1);
    const keys2 = Object.keys(obj2);
    
    if (keys1.length !== keys2.length) return false;
    
    for (const key of keys1) {
      if (!keys2.includes(key)) return false;
      if (!deepEqual(obj1[key], obj2[key])) return false;
    }
    
    return true;
  }
  
  // Diğer tipler için basit karşılaştırma
  return obj1 === obj2;
};

/**
 * Verilen milisaniye kadar bekler
 * @param {number} ms - Beklenecek süre (milisaniye)
 * @returns {Promise} Belirtilen süre sonunda çözülen Promise
 */
export const sleep = (ms) => {
  return new Promise(resolve => setTimeout(resolve, ms));
};

/**
 * Verilen değeri belirtilen aralıkta sınırlandırır
 * @param {number} value - Sınırlandırılacak değer
 * @param {number} min - Minimum değer
 * @param {number} max - Maksimum değer
 * @returns {number} Sınırlandırılmış değer
 */
export const clamp = (value, min, max) => {
  return Math.min(Math.max(value, min), max);
};

/**
 * Rastgele bir renk kodu üretir
 * @param {number} opacity - Opaklık değeri (0-1 arası)
 * @returns {string} Rastgele renk kodu (rgba formatında)
 */
export const getRandomColor = (opacity = 1) => {
  const r = Math.floor(Math.random() * 256);
  const g = Math.floor(Math.random() * 256);
  const b = Math.floor(Math.random() * 256);
  
  return `rgba(${r}, ${g}, ${b}, ${opacity})`;
};

/**
 * İki koordinat arasındaki mesafeyi kilometre cinsinden hesaplar (Haversine formülü)
 * @param {number} lat1 - Başlangıç enlemi
 * @param {number} lon1 - Başlangıç boylamı
 * @param {number} lat2 - Bitiş enlemi
 * @param {number} lon2 - Bitiş boylamı
 * @returns {number} İki nokta arasındaki mesafe (km)
 */
export const getDistanceFromLatLonInKm = (lat1, lon1, lat2, lon2) => {
  const R = 6371; // Dünya'nın yarıçapı (km)
  const dLat = deg2rad(lat2 - lat1);
  const dLon = deg2rad(lon2 - lon1);
  const a = 
    Math.sin(dLat/2) * Math.sin(dLat/2) +
    Math.cos(deg2rad(lat1)) * Math.cos(deg2rad(lat2)) * 
    Math.sin(dLon/2) * Math.sin(dLon/2);
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
  const distance = R * c; // Mesafe (km)
  
  return distance;
};

/**
 * Dereceyi radyana çevirir
 * @param {number} deg - Derece cinsinden açı
 * @returns {number} Radyan cinsinden açı
 */
const deg2rad = (deg) => {
  return deg * (Math.PI/180);
};

export default {
  formatDate,
  formatCurrency,
  toEnglishChars,
  toUrlSlug,
  getDaysBetweenDates,
  formatFileSize,
  truncateText,
  formatPhoneNumber,
  isValidEmail,
  validatePassword,
  validateTCKN,
  validateVKN,
  deepClone,
  deepEqual,
  sleep,
  clamp,
  getRandomColor,
  getDistanceFromLatLonInKm
};

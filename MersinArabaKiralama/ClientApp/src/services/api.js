import axios from 'axios';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

// Axios instance oluşturma
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, // 10 saniye zaman aşımı
});

// Request interceptor - her istek öncesi çalışır
api.interceptors.request.use(
  (config) => {
    // Token varsa header'a ekle
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor - her yanıt sonrası çalışır
api.interceptors.response.use(
  (response) => {
    return response.data;
  },
  (error) => {
    // Hata yönetimi
    if (error.response) {
      // Sunucudan hata yanıtı alındı
      const { status, data } = error.response;
      
      // 401 Unauthorized - Token süresi dolmuş veya geçersiz
      if (status === 401) {
        // Kullanıcıyı login sayfasına yönlendir
        localStorage.removeItem('token');
        window.location.href = '/login';
      }
      
      // 403 Forbidden - Yetkisiz erişim
      if (status === 403) {
        // Yetkisiz erişim hatası göster
        console.error('Bu işlem için yetkiniz yok.');
      }
      
      // 404 Not Found
      if (status === 404) {
        console.error('İstenen kaynak bulunamadı.');
      }
      
      // 500 Internal Server Error
      if (status >= 500) {
        console.error('Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.');
      }
      
      return Promise.reject({
        message: data.message || 'Bir hata oluştu',
        status,
        data: data.data || null,
      });
    } else if (error.request) {
      // İstek yapıldı ancak yanıt alınamadı
      console.error('Sunucuya bağlanılamadı. Lütfen internet bağlantınızı kontrol edin.');
      return Promise.reject({
        message: 'Sunucuya bağlanılamadı. Lütfen internet bağlantınızı kontrol edin.',
        status: 0,
      });
    } else {
      // İstek gönderilirken bir hata oluştu
      console.error('İstek gönderilirken bir hata oluştu:', error.message);
      return Promise.reject({
        message: 'İstek gönderilirken bir hata oluştu',
        status: -1,
      });
    }
  }
);

export default api;

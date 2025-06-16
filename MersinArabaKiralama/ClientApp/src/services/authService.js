import api from './api';

const authService = {
  // Giriş yap
  async login(email, password) {
    try {
      const response = await api.post('/auth/login', { email, password });
      
      if (response.data && response.data.token) {
        // Token'ı localStorage'a kaydet
        localStorage.setItem('token', response.data.token);
        
        // Kullanıcı bilgilerini döndür
        return {
          success: true,
          user: response.data.user
        };
      }
      
      return {
        success: false,
        message: 'Giriş başarısız. Lütfen bilgilerinizi kontrol edin.'
      };
    } catch (error) {
      console.error('Giriş hatası:', error);
      return {
        success: false,
        message: error.message || 'Giriş sırasında bir hata oluştu. Lütfen tekrar deneyin.'
      };
    }
  },
  
  // Kayıt ol
  async register(userData) {
    try {
      const response = await api.post('/auth/register', userData);
      
      if (response.data && response.data.token) {
        // Token'ı localStorage'a kaydet
        localStorage.setItem('token', response.data.token);
        
        return {
          success: true,
          user: response.data.user
        };
      }
      
      return {
        success: false,
        message: 'Kayıt sırasında bir hata oluştu.'
      };
    } catch (error) {
      console.error('Kayıt hatası:', error);
      
      // API'den gelen hata mesajını işle
      let errorMessage = 'Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.';
      
      if (error.response && error.response.data && error.response.data.message) {
        errorMessage = error.response.data.message;
      }
      
      return {
        success: false,
        message: errorMessage
      };
    }
  },
  
  // Çıkış yap
  logout() {
    // Token'ı kaldır
    localStorage.removeItem('token');
    
    // Sayfayı yenile
    window.location.href = '/login';
  },
  
  // Kullanıcı bilgilerini getir
  async getCurrentUser() {
    try {
      const response = await api.get('/auth/me');
      return response.data;
    } catch (error) {
      console.error('Kullanıcı bilgileri alınamadı:', error);
      return null;
    }
  },
  
  // Token kontrolü
  isAuthenticated() {
    return !!localStorage.getItem('token');
  },
  
  // Token'ı getir
  getToken() {
    return localStorage.getItem('token');
  },
  
  // Şifre sıfırlama isteği gönder
  async forgotPassword(email) {
    try {
      await api.post('/auth/forgot-password', { email });
      return { success: true };
    } catch (error) {
      console.error('Şifre sıfırlama isteği gönderilemedi:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Şifre sıfırlama isteği gönderilirken bir hata oluştu.'
      };
    }
  },
  
  // Şifre sıfırla
  async resetPassword(token, newPassword) {
    try {
      await api.post('/auth/reset-password', { token, newPassword });
      return { success: true };
    } catch (error) {
      console.error('Şifre sıfırlama başarısız oldu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Şifre sıfırlama işlemi başarısız oldu.'
      };
    }
  },
  
  // E-posta doğrulama
  async verifyEmail(token) {
    try {
      await api.get(`/auth/verify-email?token=${token}`);
      return { success: true };
    } catch (error) {
      console.error('E-posta doğrulanamadı:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'E-posta doğrulanırken bir hata oluştu.'
      };
    }
  },
  
  // Profil güncelleme
  async updateProfile(userData) {
    try {
      const response = await api.put('/auth/profile', userData);
      return {
        success: true,
        user: response.data.user
      };
    } catch (error) {
      console.error('Profil güncellenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Profil güncellenirken bir hata oluştu.'
      };
    }
  },
  
  // Şifre değiştirme
  async changePassword(currentPassword, newPassword) {
    try {
      await api.put('/auth/change-password', { currentPassword, newPassword });
      return { success: true };
    } catch (error) {
      console.error('Şifre değiştirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Şifre değiştirilirken bir hata oluştu.'
      };
    }
  }
};

export default authService;

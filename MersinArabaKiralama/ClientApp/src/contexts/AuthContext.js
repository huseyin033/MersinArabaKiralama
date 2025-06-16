import React, { createContext, useState, useEffect, useContext } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import authService from '../services/authService';

// Context oluştur
const AuthContext = createContext(null);

// Özel hook
const useAuth = () => {
  return useContext(AuthContext);
};

// Auth Provider bileşeni
const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const navigate = useNavigate();
  const location = useLocation();

  // Sayfa yenilendiğinde kullanıcıyı kontrol et
  useEffect(() => {
    const checkAuth = async () => {
      try {
        const token = localStorage.getItem('token');
        
        if (token) {
          const userData = await authService.getCurrentUser();
          if (userData) {
            setUser(userData);
          } else {
            // Geçersiz token
            localStorage.removeItem('token');
          }
        }
      } catch (err) {
        console.error('Kimlik doğrulama kontrolü sırasında hata:', err);
        localStorage.removeItem('token');
      } finally {
        setLoading(false);
      }
    };

    checkAuth();
  }, []);

  // Giriş işlemi
  const login = async (email, password) => {
    try {
      setError(null);
      const result = await authService.login(email, password);
      
      if (result.success) {
        setUser(result.user);
        
        // Yönlendirilecek sayfayı belirle
        const from = location.state?.from?.pathname || '/';
        navigate(from, { replace: true });
        
        return { success: true };
      } else {
        setError(result.message || 'Giriş başarısız. Lütfen tekrar deneyin.');
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error('Giriş sırasında hata:', err);
      const errorMessage = err.response?.data?.message || 'Giriş sırasında bir hata oluştu.';
      setError(errorMessage);
      return { success: false, message: errorMessage };
    }
  };

  // Kayıt işlemi
  const register = async (userData) => {
    try {
      setError(null);
      const result = await authService.register(userData);
      
      if (result.success) {
        setUser(result.user);
        navigate('/email-verification', { state: { email: userData.email } });
        return { success: true };
      } else {
        setError(result.message || 'Kayıt başarısız. Lütfen tekrar deneyin.');
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error('Kayıt sırasında hata:', err);
      const errorMessage = err.response?.data?.message || 'Kayıt sırasında bir hata oluştu.';
      setError(errorMessage);
      return { success: false, message: errorMessage };
    }
  };

  // Çıkış işlemi
  const logout = () => {
    authService.logout();
    setUser(null);
    navigate('/login');
  };

  // Şifremi unuttum
  const forgotPassword = async (email) => {
    try {
      setError(null);
      const result = await authService.forgotPassword(email);
      
      if (result.success) {
        return { success: true };
      } else {
        setError(result.message || 'Şifre sıfırlama isteği gönderilemedi.');
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error('Şifre sıfırlama isteği gönderilirken hata:', err);
      const errorMessage = err.response?.data?.message || 'Şifre sıfırlama isteği gönderilirken bir hata oluştu.';
      setError(errorMessage);
      return { success: false, message: errorMessage };
    }
  };

  // Şifre sıfırlama
  const resetPassword = async (token, newPassword) => {
    try {
      setError(null);
      const result = await authService.resetPassword(token, newPassword);
      
      if (result.success) {
        return { success: true };
      } else {
        setError(result.message || 'Şifre sıfırlama başarısız oldu.');
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error('Şifre sıfırlama sırasında hata:', err);
      const errorMessage = err.response?.data?.message || 'Şifre sıfırlama sırasında bir hata oluştu.';
      setError(errorMessage);
      return { success: false, message: errorMessage };
    }
  };

  // E-posta doğrulama
  const verifyEmail = async (token) => {
    try {
      setError(null);
      const result = await authService.verifyEmail(token);
      
      if (result.success) {
        // Eğer kullanıcı giriş yapmışsa, kullanıcı bilgilerini güncelle
        if (user) {
          setUser({ ...user, emailVerified: true });
        }
        return { success: true };
      } else {
        setError(result.message || 'E-posta doğrulama başarısız oldu.');
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error('E-posta doğrulama sırasında hata:', err);
      const errorMessage = err.response?.data?.message || 'E-posta doğrulama sırasında bir hata oluştu.';
      setError(errorMessage);
      return { success: false, message: errorMessage };
    }
  };

  // Kullanıcı bilgilerini güncelle
  const updateProfile = async (userData) => {
    try {
      setError(null);
      const result = await authService.updateProfile(userData);
      
      if (result.success) {
        setUser(result.user);
        return { success: true };
      } else {
        setError(result.message || 'Profil güncellenirken bir hata oluştu.');
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error('Profil güncelleme sırasında hata:', err);
      const errorMessage = err.response?.data?.message || 'Profil güncellenirken bir hata oluştu.';
      setError(errorMessage);
      return { success: false, message: errorMessage };
    }
  };

  // Şifre değiştir
  const changePassword = async (currentPassword, newPassword) => {
    try {
      setError(null);
      const result = await authService.changePassword(currentPassword, newPassword);
      
      if (result.success) {
        return { success: true };
      } else {
        setError(result.message || 'Şifre değiştirilirken bir hata oluştu.');
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error('Şifre değiştirme sırasında hata:', err);
      const errorMessage = err.response?.data?.message || 'Şifre değiştirilirken bir hata oluştu.';
      setError(errorMessage);
      return { success: false, message: errorMessage };
    }
  };

  // Context değeri
  const value = {
    user,
    loading,
    error,
    isAuthenticated: !!user,
    login,
    register,
    logout,
    forgotPassword,
    resetPassword,
    verifyEmail,
    updateProfile,
    changePassword,
    setError
  };

  return (
    <AuthContext.Provider value={value}>
      {!loading && children}
    </AuthContext.Provider>
  );
};

export { AuthProvider, useAuth };

// Kullanım örneği:
// 1. Uygulamanın en üst seviyesinde:
//    <AuthProvider>
//      <App />
//    </AuthProvider>
//
// 2. Bileşen içinde:
//    const { user, isAuthenticated, login, logout } = useAuth();

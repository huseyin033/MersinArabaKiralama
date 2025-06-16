import api from './api';

const paymentService = {
  // Ödeme başlat
  async initiatePayment(paymentData) {
    try {
      const response = await api.post('/payments/initiate', paymentData);
      return {
        success: true,
        data: response.data,
        message: 'Ödeme başlatıldı.'
      };
    } catch (error) {
      console.error('Ödeme başlatılırken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Ödeme başlatılırken bir hata oluştu.'
      };
    }
  },

  // Ödeme onayı
  async confirmPayment(paymentId, paymentMethod) {
    try {
      const response = await api.post(`/payments/${paymentId}/confirm`, { paymentMethod });
      return {
        success: true,
        data: response.data,
        message: 'Ödeme başarıyla tamamlandı.'
      };
    } catch (error) {
      console.error('Ödeme onaylanırken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Ödeme onaylanırken bir hata oluştu.'
      };
    }
  },

  // Ödeme iptali
  async cancelPayment(paymentId) {
    try {
      await api.delete(`/payments/${paymentId}`);
      return {
        success: true,
        message: 'Ödeme başarıyla iptal edildi.'
      };
    } catch (error) {
      console.error('Ödeme iptal edilirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Ödeme iptal edilirken bir hata oluştu.'
      };
    }
  },

  // Ödeme geçmişini getir
  async getPaymentHistory(userId, filters = {}) {
    try {
      const response = await api.get(`/users/${userId}/payments`, { params: filters });
      return {
        success: true,
        data: response.data,
        pagination: response.pagination || {}
      };
    } catch (error) {
      console.error('Ödeme geçmişi getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Ödeme geçmişi getirilirken bir hata oluştu.'
      };
    }
  },

  // Ödeme detayını getir
  async getPaymentDetails(paymentId) {
    try {
      const response = await api.get(`/payments/${paymentId}`);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Ödeme detayı getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Ödeme detayı getirilirken bir hata oluştu.'
      };
    }
  },

  // İade işlemi başlat
  async initiateRefund(paymentId, refundData) {
    try {
      const response = await api.post(`/payments/${paymentId}/refund`, refundData);
      return {
        success: true,
        data: response.data,
        message: 'İade işlemi başlatıldı.'
      };
    } catch (error) {
      console.error('İade işlemi başlatılırken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'İade işlemi başlatılırken bir hata oluştu.'
      };
    }
  },

  // Kayıtlı ödeme yöntemlerini getir
  async getSavedPaymentMethods(userId) {
    try {
      const response = await api.get(`/users/${userId}/payment-methods`);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Kayıtlı ödeme yöntemleri getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Kayıtlı ödeme yöntemleri getirilirken bir hata oluştu.'
      };
    }
  },

  // Ödeme yöntemi ekle
  async addPaymentMethod(userId, paymentMethodData) {
    try {
      const response = await api.post(`/users/${userId}/payment-methods`, paymentMethodData);
      return {
        success: true,
        data: response.data,
        message: 'Ödeme yöntemi başarıyla eklendi.'
      };
    } catch (error) {
      console.error('Ödeme yöntemi eklenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Ödeme yöntemi eklenirken bir hata oluştu.'
      };
    }
  },

  // Ödeme yöntemini kaldır
  async removePaymentMethod(userId, paymentMethodId) {
    try {
      await api.delete(`/users/${userId}/payment-methods/${paymentMethodId}`);
      return {
        success: true,
        message: 'Ödeme yöntemi başarıyla kaldırıldı.'
      };
    } catch (error) {
      console.error('Ödeme yöntemi kaldırılırken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Ödeme yöntemi kaldırılırken bir hata oluştu.'
      };
    }
  },

  // Varsayılan ödeme yöntemini güncelle
  async setDefaultPaymentMethod(userId, paymentMethodId) {
    try {
      await api.patch(`/users/${userId}/payment-methods/${paymentMethodId}/set-default`);
      return {
        success: true,
        message: 'Varsayılan ödeme yöntemi başarıyla güncellendi.'
      };
    } catch (error) {
      console.error('Varsayılan ödeme yöntemi güncellenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Varsayılan ödeme yöntemi güncellenirken bir hata oluştu.'
      };
    }
  },

  // Kupon kodu doğrula
  async validateCouponCode(code) {
    try {
      const response = await api.get('/coupons/validate', { params: { code } });
      return {
        success: true,
        data: response.data,
        message: 'Kupon kodu geçerli.'
      };
    } catch (error) {
      console.error('Kupon kodu doğrulanırken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Kupon kodu doğrulanırken bir hata oluştu.'
      };
    }
  },

  // Ödeme yöntemlerini getir
  async getAvailablePaymentMethods() {
    try {
      const response = await api.get('/payments/methods');
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Ödeme yöntemleri getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Ödeme yöntemleri getirilirken bir hata oluştu.'
      };
    }
  },

  // Ödeme durumunu sorgula
  async checkPaymentStatus(paymentId) {
    try {
      const response = await api.get(`/payments/${paymentId}/status`);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Ödeme durumu sorgulanırken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Ödeme durumu sorgulanırken bir hata oluştu.'
      };
    }
  }
};

export default paymentService;

import api from './api';

const userService = {
  // Kullanıcı bilgilerini güncelle
  async updateProfile(userId, userData) {
    try {
      const response = await api.put(`/users/${userId}`, userData);
      return {
        success: true,
        user: response.data,
        message: 'Profil bilgileriniz başarıyla güncellendi.'
      };
    } catch (error) {
      console.error('Profil güncellenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Profil güncellenirken bir hata oluştu.'
      };
    }
  },

  // Kullanıcı şifresini değiştir
  async changePassword(userId, currentPassword, newPassword) {
    try {
      await api.put(`/users/${userId}/change-password`, { currentPassword, newPassword });
      return {
        success: true,
        message: 'Şifreniz başarıyla değiştirildi.'
      };
    } catch (error) {
      console.error('Şifre değiştirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Şifre değiştirilirken bir hata oluştu.'
      };
    }
  },

  // Kullanıcı adreslerini getir
  async getAddresses(userId) {
    try {
      const response = await api.get(`/users/${userId}/addresses`);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Adresler getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Adresler getirilirken bir hata oluştu.'
      };
    }
  },

  // Yeni adres ekle
  async addAddress(userId, addressData) {
    try {
      const response = await api.post(`/users/${userId}/addresses`, addressData);
      return {
        success: true,
        data: response.data,
        message: 'Adres başarıyla eklendi.'
      };
    } catch (error) {
      console.error('Adres eklenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Adres eklenirken bir hata oluştu.'
      };
    }
  },

  // Adres güncelle
  async updateAddress(userId, addressId, addressData) {
    try {
      const response = await api.put(`/users/${userId}/addresses/${addressId}`, addressData);
      return {
        success: true,
        data: response.data,
        message: 'Adres başarıyla güncellendi.'
      };
    } catch (error) {
      console.error('Adres güncellenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Adres güncellenirken bir hata oluştu.'
      };
    }
  },

  // Adres sil
  async deleteAddress(userId, addressId) {
    try {
      await api.delete(`/users/${userId}/addresses/${addressId}`);
      return {
        success: true,
        message: 'Adres başarıyla silindi.'
      };
    } catch (error) {
      console.error('Adres silinirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Adres silinirken bir hata oluştu.'
      };
    }
  },

  // Varsayılan adresi ayarla
  async setDefaultAddress(userId, addressId) {
    try {
      await api.patch(`/users/${userId}/addresses/${addressId}/set-default`);
      return {
        success: true,
        message: 'Varsayılan adres başarıyla güncellendi.'
      };
    } catch (error) {
      console.error('Varsayılan adres ayarlanırken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Varsayılan adres ayarlanırken bir hata oluştu.'
      };
    }
  },

  // Kullanıcı belgelerini getir
  async getDocuments(userId) {
    try {
      const response = await api.get(`/users/${userId}/documents`);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Belgeler getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Belgeler getirilirken bir hata oluştu.'
      };
    }
  },

  // Belge yükle
  async uploadDocument(userId, documentData) {
    try {
      const formData = new FormData();
      formData.append('document', documentData.file);
      formData.append('type', documentData.type);
      
      if (documentData.expiryDate) {
        formData.append('expiryDate', documentData.expiryDate);
      }

      const response = await api.post(`/users/${userId}/documents`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      return {
        success: true,
        data: response.data,
        message: 'Belge başarıyla yüklendi.'
      };
    } catch (error) {
      console.error('Belge yüklenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Belge yüklenirken bir hata oluştu.'
      };
    }
  },

  // Belge sil
  async deleteDocument(userId, documentId) {
    try {
      await api.delete(`/users/${userId}/documents/${documentId}`);
      return {
        success: true,
        message: 'Belge başarıyla silindi.'
      };
    } catch (error) {
      console.error('Belge silinirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Belge silinirken bir hata oluştu.'
      };
    }
  },

  // Kullanıcı bildirimlerini getir
  async getNotifications(userId, filters = {}) {
    try {
      const response = await api.get(`/users/${userId}/notifications`, { params: filters });
      return {
        success: true,
        data: response.data,
        pagination: response.pagination || {}
      };
    } catch (error) {
      console.error('Bildirimler getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Bildirimler getirilirken bir hata oluştu.'
      };
    }
  },

  // Bildirimi okundu olarak işaretle
  async markNotificationAsRead(userId, notificationId) {
    try {
      await api.patch(`/users/${userId}/notifications/${notificationId}/read`);
      return {
        success: true,
        message: 'Bildirim okundu olarak işaretlendi.'
      };
    } catch (error) {
      console.error('Bildirim güncellenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Bildirim güncellenirken bir hata oluştu.'
      };
    }
  },

  // Tüm bildirimleri okundu olarak işaretle
  async markAllNotificationsAsRead(userId) {
    try {
      await api.patch(`/users/${userId}/notifications/mark-all-read`);
      return {
        success: true,
        message: 'Tüm bildirimler okundu olarak işaretlendi.'
      };
    } catch (error) {
      console.error('Bildirimler güncellenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Bildirimler güncellenirken bir hata oluştu.'
      };
    }
  },

  // Kullanıcı tercihlerini getir
  async getPreferences(userId) {
    try {
      const response = await api.get(`/users/${userId}/preferences`);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Kullanıcı tercihleri getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Kullanıcı tercihleri getirilirken bir hata oluştu.'
      };
    }
  },

  // Kullanıcı tercihlerini güncelle
  async updatePreferences(userId, preferences) {
    try {
      const response = await api.put(`/users/${userId}/preferences`, { preferences });
      return {
        success: true,
        data: response.data,
        message: 'Tercihleriniz başarıyla güncellendi.'
      };
    } catch (error) {
      console.error('Kullanıcı tercihleri güncellenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Kullanıcı tercihleri güncellenirken bir hata oluştu.'
      };
    }
  },

  // Kullanıcı hesabını sil
  async deleteAccount(userId, password) {
    try {
      await api.delete(`/users/${userId}`, { data: { password } });
      return {
        success: true,
        message: 'Hesabınız başarıyla silindi.'
      };
    } catch (error) {
      console.error('Hesap silinirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Hesap silinirken bir hata oluştu.'
      };
    }
  },

  // Kullanıcı etkinlik geçmişini getir
  async getActivityLogs(userId, filters = {}) {
    try {
      const response = await api.get(`/users/${userId}/activity-logs`, { params: filters });
      return {
        success: true,
        data: response.data,
        pagination: response.pagination || {}
      };
    } catch (error) {
      console.error('Etkinlik geçmişi getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Etkinlik geçmişi getirilirken bir hata oluştu.'
      };
    }
  },

  // Kullanıcı istatistiklerini getir
  async getUserStatistics(userId) {
    try {
      const response = await api.get(`/users/${userId}/statistics`);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Kullanıcı istatistikleri getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Kullanıcı istatistikleri getirilirken bir hata oluştu.'
      };
    }
  }
};

export default userService;

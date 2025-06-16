import api from './api';

const searchService = {
  // Araç arama
  async searchCars(filters = {}) {
    try {
      const response = await api.get('/search/cars', { params: filters });
      return {
        success: true,
        data: response.data,
        pagination: response.pagination || {}
      };
    } catch (error) {
      console.error('Araç aranırken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Araç aranırken bir hata oluştu.'
      };
    }
  },

  // Popüler aramaları getir
  async getPopularSearches(limit = 5) {
    try {
      const response = await api.get('/search/popular', { params: { limit } });
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Popüler aramalar getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Popüler aramalar getirilirken bir hata oluştu.'
      };
    }
  },

  // Son aramaları getir
  async getRecentSearches(userId, limit = 5) {
    try {
      const response = await api.get(`/users/${userId}/recent-searches`, { params: { limit } });
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Son aramalar getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Son aramalar getirilirken bir hata oluştu.'
      };
    }
  },

  // Arama önerileri getir
  async getSearchSuggestions(query) {
    try {
      const response = await api.get('/search/suggestions', { params: { q: query } });
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Arama önerileri getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Arama önerileri getirilirken bir hata oluştu.'
      };
    }
  },

  // Konuma göre araçları getir
  async searchCarsByLocation(location, radius = 10, filters = {}) {
    try {
      const response = await api.get('/search/location', { 
        params: { 
          lat: location.lat, 
          lng: location.lng, 
          radius,
          ...filters 
        } 
      });
      return {
        success: true,
        data: response.data,
        pagination: response.pagination || {}
      };
    } catch (error) {
      console.error('Konuma göre araçlar getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Konuma göre araçlar getirilirken bir hata oluştu.'
      };
    }
  },

  // Filtre seçeneklerini getir
  async getFilterOptions() {
    try {
      const response = await api.get('/search/filter-options');
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Filtre seçenekleri getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Filtre seçenekleri getirilirken bir hata oluştu.'
      };
    }
  },

  // Arama kaydet
  async saveSearch(userId, searchData) {
    try {
      const response = await api.post(`/users/${userId}/saved-searches`, searchData);
      return {
        success: true,
        data: response.data,
        message: 'Arama başarıyla kaydedildi.'
      };
    } catch (error) {
      console.error('Arama kaydedilirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Arama kaydedilirken bir hata oluştu.'
      };
    }
  },

  // Kaydedilmiş aramaları getir
  async getSavedSearches(userId, filters = {}) {
    try {
      const response = await api.get(`/users/${userId}/saved-searches`, { params: filters });
      return {
        success: true,
        data: response.data,
        pagination: response.pagination || {}
      };
    } catch (error) {
      console.error('Kaydedilmiş aramalar getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Kaydedilmiş aramalar getirilirken bir hata oluştu.'
      };
    }
  },

  // Kaydedilmiş aramayı sil
  async deleteSavedSearch(userId, searchId) {
    try {
      await api.delete(`/users/${userId}/saved-searches/${searchId}`);
      return {
        success: true,
        message: 'Kaydedilmiş arama başarıyla silindi.'
      };
    } catch (error) {
      console.error('Kaydedilmiş arama silinirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Kaydedilmiş arama silinirken bir hata oluştu.'
      };
    }
  },

  // Arama uyarısı oluştur
  async createSearchAlert(userId, alertData) {
    try {
      const response = await api.post(`/users/${userId}/search-alerts`, alertData);
      return {
        success: true,
        data: response.data,
        message: 'Arama uyarısı başarıyla oluşturuldu.'
      };
    } catch (error) {
      console.error('Arama uyarısı oluşturulurken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Arama uyarısı oluşturulurken bir hata oluştu.'
      };
    }
  },

  // Arama uyarılarını getir
  async getSearchAlerts(userId, filters = {}) {
    try {
      const response = await api.get(`/users/${userId}/search-alerts`, { params: filters });
      return {
        success: true,
        data: response.data,
        pagination: response.pagination || {}
      };
    } catch (error) {
      console.error('Arama uyarıları getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Arama uyarıları getirilirken bir hata oluştu.'
      };
    }
  },

  // Arama uyarısını güncelle
  async updateSearchAlert(userId, alertId, alertData) {
    try {
      const response = await api.put(`/users/${userId}/search-alerts/${alertId}`, alertData);
      return {
        success: true,
        data: response.data,
        message: 'Arama uyarısı başarıyla güncellendi.'
      };
    } catch (error) {
      console.error('Arama uyarısı güncellenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Arama uyarısı güncellenirken bir hata oluştu.'
      };
    }
  },

  // Arama uyarısını sil
  async deleteSearchAlert(userId, alertId) {
    try {
      await api.delete(`/users/${userId}/search-alerts/${alertId}`);
      return {
        success: true,
        message: 'Arama uyarısı başarıyla silindi.'
      };
    } catch (error) {
      console.error('Arama uyarısı silinirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Arama uyarısı silinirken bir hata oluştu.'
      };
    }
  },

  // Arama uyarısını etkinleştir/devre dışı bırak
  async toggleSearchAlert(userId, alertId, isActive) {
    try {
      const response = await api.patch(`/users/${userId}/search-alerts/${alertId}/toggle`, { isActive });
      return {
        success: true,
        data: response.data,
        message: `Arama uyarısı başarıyla ${isActive ? 'etkinleştirildi' : 'devre dışı bırakıldı'}.`
      };
    } catch (error) {
      console.error('Arama uyarısı güncellenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Arama uyarısı güncellenirken bir hata oluştu.'
      };
    }
  }
};

export default searchService;

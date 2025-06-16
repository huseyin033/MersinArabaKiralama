import api from './api';

const rentalService = {
  // Tüm araçları getir
  async getAllCars(filters = {}) {
    try {
      const response = await api.get('/cars', { params: filters });
      return {
        success: true,
        data: response.data,
        pagination: response.pagination || {}
      };
    } catch (error) {
      console.error('Araçlar getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Araçlar getirilirken bir hata oluştu.'
      };
    }
  },

  // ID'ye göre araç detayını getir
  async getCarById(id) {
    try {
      const response = await api.get(`/cars/${id}`);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Araç detayı getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Araç detayı getirilirken bir hata oluştu.'
      };
    }
  },

  // Araç kirala
  async rentCar(rentalData) {
    try {
      const response = await api.post('/rentals', rentalData);
      return {
        success: true,
        data: response.data,
        message: 'Araç başarıyla kiralandı.'
      };
    } catch (error) {
      console.error('Araç kiralanırken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Araç kiralanırken bir hata oluştu.'
      };
    }
  },

  // Kullanıcının kiralama geçmişini getir
  async getRentalHistory(userId, filters = {}) {
    try {
      const response = await api.get(`/users/${userId}/rentals`, { params: filters });
      return {
        success: true,
        data: response.data,
        pagination: response.pagination || {}
      };
    } catch (error) {
      console.error('Kiralama geçmişi getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Kiralama geçmişi getirilirken bir hata oluştu.'
      };
    }
  },

  // Kiralama detayını getir
  async getRentalDetails(rentalId) {
    try {
      const response = await api.get(`/rentals/${rentalId}`);
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Kiralama detayı getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Kiralama detayı getirilirken bir hata oluştu.'
      };
    }
  },

  // Kiralama iptali
  async cancelRental(rentalId) {
    try {
      await api.delete(`/rentals/${rentalId}`);
      return {
        success: true,
        message: 'Kiralama başarıyla iptal edildi.'
      };
    } catch (error) {
      console.error('Kiralama iptal edilirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Kiralama iptal edilirken bir hata oluştu.'
      };
    }
  },

  // Kiralama süresini uzat
  async extendRental(rentalId, newEndDate) {
    try {
      const response = await api.patch(`/rentals/${rentalId}/extend`, { newEndDate });
      return {
        success: true,
        data: response.data,
        message: 'Kiralama süresi başarıyla uzatıldı.'
      };
    } catch (error) {
      console.error('Kiralama süresi uzatılırken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Kiralama süresi uzatılırken bir hata oluştu.'
      };
    }
  },

  // Araç kullanılabilirlik kontrolü
  async checkCarAvailability(carId, startDate, endDate) {
    try {
      const response = await api.get(`/cars/${carId}/availability`, {
        params: { startDate, endDate }
      });
      return {
        success: true,
        isAvailable: response.data.isAvailable,
        message: response.data.message
      };
    } catch (error) {
      console.error('Araç kullanılabilirliği kontrol edilirken hata oluştu:', error);
      return {
        success: false,
        isAvailable: false,
        message: error.response?.data?.message || 'Araç kullanılabilirliği kontrol edilirken bir hata oluştu.'
      };
    }
  },

  // Araç arama
  async searchCars(searchParams) {
    try {
      const response = await api.get('/cars/search', { params: searchParams });
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

  // Popüler araçları getir
  async getPopularCars(limit = 5) {
    try {
      const response = await api.get('/cars/popular', { params: { limit } });
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Popüler araçlar getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Popüler araçlar getirilirken bir hata oluştu.'
      };
    }
  },

  // Araç değerlendirmesi ekle
  async addCarReview(carId, reviewData) {
    try {
      const response = await api.post(`/cars/${carId}/reviews`, reviewData);
      return {
        success: true,
        data: response.data,
        message: 'Değerlendirme başarıyla eklendi.'
      };
    } catch (error) {
      console.error('Değerlendirme eklenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Değerlendirme eklenirken bir hata oluştu.'
      };
    }
  },

  // Araç resimlerini yükle
  async uploadCarImages(carId, images) {
    try {
      const formData = new FormData();
      images.forEach((image) => {
        formData.append('images', image);
      });

      const response = await api.post(`/cars/${carId}/images`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      return {
        success: true,
        data: response.data,
        message: 'Resimler başarıyla yüklendi.'
      };
    } catch (error) {
      console.error('Resim yüklenirken hata oluştu:', error);
      return {
        success: false,
        message: error.response?.data?.message || 'Resim yüklenirken bir hata oluştu.'
      };
    }
  },

  // Araç kategorilerini getir
  async getCarCategories() {
    try {
      const response = await api.get('/cars/categories');
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Araç kategorileri getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Araç kategorileri getirilirken bir hata oluştu.'
      };
    }
  },

  // Araç markalarını getir
  async getCarBrands() {
    try {
      const response = await api.get('/cars/brands');
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      console.error('Araç markaları getirilirken hata oluştu:', error);
      return {
        success: false,
        message: error.message || 'Araç markaları getirilirken bir hata oluştu.'
      };
    }
  }
};

export default rentalService;

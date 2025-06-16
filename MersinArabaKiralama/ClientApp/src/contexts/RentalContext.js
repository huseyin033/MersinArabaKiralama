import React, { createContext, useState, useContext, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import rentalService from '../services/rentalService';
import searchService from '../services/searchService';
import { formatDate } from '../utils/helpers';

// Context oluştur
const RentalContext = createContext(null);

// Özel hook
const useRental = () => {
  return useContext(RentalContext);
};

// Rental Provider bileşeni
const RentalProvider = ({ children }) => {
  const [cars, setCars] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [selectedCar, setSelectedCar] = useState(null);
  const [searchParams, setSearchParams] = useState({
    pickupLocation: '',
    pickupDate: new Date(),
    returnDate: new Date(Date.now() + 24 * 60 * 60 * 1000), // 1 gün sonrası
    carType: '',
    minPrice: 0,
    maxPrice: 10000,
    transmission: '',
    fuelType: '',
    sortBy: 'price',
    sortOrder: 'asc',
    page: 1,
    limit: 10
  });
  
  const [pagination, setPagination] = useState({
    total: 0,
    totalPages: 1,
    currentPage: 1,
    hasNextPage: false,
    hasPrevPage: false
  });
  
  const [filters, setFilters] = useState({
    brands: [],
    carTypes: [],
    fuelTypes: [],
    transmissions: [],
    features: []
  });
  
  const [rentalDetails, setRentalDetails] = useState({
    car: null,
    pickupLocation: '',
    returnLocation: '',
    pickupDate: null,
    returnDate: null,
    insurance: 'standard',
    additionalDriver: false,
    babySeat: false,
    navigation: false,
    totalPrice: 0,
    discount: 0,
    couponCode: ''
  });
  
  const navigate = useNavigate();

  // Araçları getir
  const fetchCars = async (params = {}) => {
    try {
      setLoading(true);
      setError(null);
      
      // Mevcut arama parametreleriyle birleştir
      const queryParams = { ...searchParams, ...params };
      
      // Tarih formatını düzenle
      if (queryParams.pickupDate) {
        queryParams.pickupDate = formatDate(queryParams.pickupDate, 'YYYY-MM-DD');
      }
      
      if (queryParams.returnDate) {
        queryParams.returnDate = formatDate(queryParams.returnDate, 'YYYY-MM-DD');
      }
      
      const result = await searchService.searchCars(queryParams);
      
      if (result.success) {
        setCars(result.data);
        
        // Sayfalama bilgilerini güncelle
        if (result.pagination) {
          setPagination({
            total: result.pagination.total || 0,
            totalPages: result.pagination.totalPages || 1,
            currentPage: result.pagination.currentPage || 1,
            hasNextPage: result.pagination.hasNextPage || false,
            hasPrevPage: result.pagination.hasPrevPage || false
          });
        }
        
        return result.data;
      } else {
        throw new Error(result.message || 'Araçlar getirilirken bir hata oluştu.');
      }
    } catch (err) {
      console.error('Araçlar getirilirken hata:', err);
      setError(err.message || 'Araçlar getirilirken bir hata oluştu.');
      return [];
    } finally {
      setLoading(false);
    }
  };
  
  // Filtre seçeneklerini getir
  const fetchFilterOptions = async () => {
    try {
      const result = await searchService.getFilterOptions();
      
      if (result.success && result.data) {
        setFilters({
          brands: result.data.brands || [],
          carTypes: result.data.carTypes || [],
          fuelTypes: result.data.fuelTypes || [],
          transmissions: result.data.transmissions || [],
          features: result.data.features || []
        });
      }
      
      return result.data || {};
    } catch (err) {
      console.error('Filtre seçenekleri getirilirken hata:', err);
      return {};
    }
  };
  
  // Popüler araçları getir
  const fetchPopularCars = async (limit = 5) => {
    try {
      const result = await rentalService.getPopularCars(limit);
      
      if (result.success) {
        return result.data || [];
      }
      
      return [];
    } catch (err) {
      console.error('Popüler araçlar getirilirken hata:', err);
      return [];
    }
  };
  
  // Araç detayını getir
  const fetchCarDetails = async (carId) => {
    try {
      setLoading(true);
      setError(null);
      
      const result = await rentalService.getCarById(carId);
      
      if (result.success) {
        setSelectedCar(result.data);
        return result.data;
      } else {
        throw new Error(result.message || 'Araç detayları getirilirken bir hata oluştu.');
      }
    } catch (err) {
      console.error('Araç detayları getirilirken hata:', err);
      setError(err.message || 'Araç detayları getirilirken bir hata oluştu.');
      return null;
    } finally {
      setLoading(false);
    }
  };
  
  // Arama parametrelerini güncelle
  const updateSearchParams = (newParams) => {
    setSearchParams(prev => ({
      ...prev,
      ...newParams,
      // Eğer sayfa değiştiyse, sayfa numarasını sıfırla
      ...(newParams.pickupDate || newParams.returnDate || newParams.pickupLocation 
        ? { page: 1 } 
        : {})
    }));
  };
  
  // Sayfa değiştir
  const changePage = (page) => {
    updateSearchParams({ page });
  };
  
  // Sıralama yap
  const sortCars = (sortBy, sortOrder = 'asc') => {
    updateSearchParams({ sortBy, sortOrder, page: 1 });
  };
  
  // Kiralama detaylarını güncelle
  const updateRentalDetails = (details) => {
    setRentalDetails(prev => ({
      ...prev,
      ...details
    }));
  };
  
  // Araç kiralama işlemi
  const rentCar = async (rentalData) => {
    try {
      setLoading(true);
      setError(null);
      
      const result = await rentalService.rentCar(rentalData);
      
      if (result.success) {
        // Başarılı kiralama sonrası yapılacak işlemler
        navigate(`/rental-confirmation/${result.data.rentalId}`);
        return { success: true, data: result.data };
      } else {
        throw new Error(result.message || 'Araç kiralanırken bir hata oluştu.');
      }
    } catch (err) {
      console.error('Kiralama işlemi sırasında hata:', err);
      setError(err.message || 'Araç kiralanırken bir hata oluştu.');
      return { success: false, message: err.message };
    } finally {
      setLoading(false);
    }
  };
  
  // Araç kullanılabilirliğini kontrol et
  const checkCarAvailability = async (carId, startDate, endDate) => {
    try {
      const result = await rentalService.checkCarAvailability(carId, startDate, endDate);
      return result;
    } catch (err) {
      console.error('Kullanılabilirlik kontrolü sırasında hata:', err);
      return {
        success: false,
        isAvailable: false,
        message: err.message || 'Kullanılabilirlik kontrolü sırasında bir hata oluştu.'
      };
    }
  };
  
  // Kupon kodunu doğrula
  const validateCoupon = async (couponCode) => {
    try {
      // Burada paymentService üzerinden kupon doğrulaması yapılabilir
      // Şimdilik örnek bir yanıt dönüyoruz
      return {
        success: true,
        valid: true,
        discount: 10, // %10 indirim
        message: 'Kupon kodu geçerli.'
      };
    } catch (err) {
      console.error('Kupon doğrulanırken hata:', err);
      return {
        success: false,
        valid: false,
        message: err.message || 'Kupon doğrulanırken bir hata oluştu.'
      };
    }
  };
  
  // Context değeri
  const value = {
    cars,
    loading,
    error,
    selectedCar,
    searchParams,
    pagination,
    filters,
    rentalDetails,
    fetchCars,
    fetchFilterOptions,
    fetchPopularCars,
    fetchCarDetails,
    updateSearchParams,
    changePage,
    sortCars,
    updateRentalDetails,
    rentCar,
    checkCarAvailability,
    validateCoupon,
    setError
  };

  return (
    <RentalContext.Provider value={value}>
      {children}
    </RentalContext.Provider>
  );
};

export { RentalProvider, useRental };

// Kullanım örneği:
// 1. Uygulamanın en üst seviyesinde:
//    <RentalProvider>
//      <App />
//    </RentalProvider>
//
// 2. Bileşen içinde:
//    const { 
//      cars, 
//      loading, 
//      fetchCars, 
//      updateSearchParams, 
//      rentCar 
//    } = useRental();

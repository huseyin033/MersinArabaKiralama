import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useRental } from '../contexts/RentalContext';
import { useNotification } from '../contexts/NotificationContext';
import { Box, Typography, Button, Grid, Paper, Divider, Chip, Rating, TextField, Tabs, Tab, Container } from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers';
import { LocalGasStation, DirectionsCar, People, AcUnit, Speed, CheckCircle } from '@mui/icons-material';
import { format } from 'date-fns';
import { tr } from 'date-fns/locale';

const CarDetailPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { getCarById, checkAvailability, createReservation } = useRental();
  const { showNotification } = useNotification();
  
  const [car, setCar] = useState(null);
  const [loading, setLoading] = useState(true);
  const [dates, setDates] = useState({
    startDate: null,
    endDate: null
  });
  const [tabValue, setTabValue] = useState(0);
  const [isAvailable, setIsAvailable] = useState(null);
  const [priceDetails, setPriceDetails] = useState(null);
  
  useEffect(() => {
    const fetchCarDetails = async () => {
      try {
        const carData = await getCarById(id);
        setCar(carData);
      } catch (error) {
        showNotification('Araç detayları yüklenirken bir hata oluştu', 'error');
        navigate('/araclar');
      } finally {
        setLoading(false);
      }
    };
    
    fetchCarDetails();
  }, [id]);
  
  const handleDateChange = (field) => (newValue) => {
    setDates(prev => ({
      ...prev,
      [field]: newValue
    }));
    
    // Tarihler değiştiğinde önceki kontrolü sıfırla
    if (field === 'startDate' || field === 'endDate') {
      setIsAvailable(null);
      setPriceDetails(null);
    }
  };
  
  const handleCheckAvailability = async () => {
    if (!dates.startDate || !dates.endDate) {
      showNotification('Lütfen başlangıç ve bitiş tarihlerini seçin', 'warning');
      return;
    }
    
    try {
      const availability = await checkAvailability(id, {
        startDate: format(dates.startDate, 'yyyy-MM-dd'),
        endDate: format(dates.endDate, 'yyyy-MM-dd')
      });
      
      setIsAvailable(availability.isAvailable);
      setPriceDetails(availability.priceDetails);
      
      if (!availability.isAvailable) {
        showNotification('Seçtiğiniz tarihlerde araç müsait değil', 'warning');
      }
    } catch (error) {
      showNotification('Müsaitlik kontrolü sırasında bir hata oluştu', 'error');
    }
  };
  
  const handleReservation = async () => {
    try {
      const reservation = await createReservation({
        carId: id,
        startDate: format(dates.startDate, 'yyyy-MM-dd'),
        endDate: format(dates.endDate, 'yyyy-MM-dd'),
        priceDetails
      });
      
      navigate(`/odeme/${reservation.id}`);
    } catch (error) {
      showNotification('Rezervasyon oluşturulurken bir hata oluştu', 'error');
    }
  };
  
  if (loading) {
    return <div>Yükleniyor...</div>;
  }
  
  if (!car) {
    return <div>Araç bulunamadı</div>;
  }
  
  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Grid container spacing={4}>
        {/* Araç Resimleri */}
        <Grid item xs={12} md={8}>
          <Paper elevation={3} sx={{ p: 2, mb: 3 }}>
            <img 
              src={car.images?.[0] || '/default-car.jpg'} 
              alt={car.brand + ' ' + car.model}
              style={{ width: '100%', borderRadius: 8, marginBottom: 16 }}
            />
            
            <Tabs 
              value={tabValue} 
              onChange={(e, newValue) => setTabValue(newValue)}
              variant="scrollable"
              scrollButtons="auto"
            >
              <Tab label="Genel Bakış" />
              <Tab label="Özellikler" />
              <Tab label="Kiralama Koşulları" />
            </Tabs>
            
            <Box sx={{ p: 2 }}>
              {tabValue === 0 && (
                <Box>
                  <Typography variant="h5" gutterBottom>{car.brand} {car.model}</Typography>
                  <Typography variant="body1" paragraph>{car.description}</Typography>
                  
                  <Box display="flex" flexWrap="wrap" gap={2} mb={3}>
                    <Chip icon={<LocalGasStation />} label={car.fuelType} />
                    <Chip icon={<DirectionsCar />} label={car.transmission} />
                    <Chip icon={<People />} label={`${car.seats} Kişi`} />
                    {car.hasAirConditioning && <Chip icon={<AcUnit />} label="Klima" />}
                  </Box>
                  
                  <Rating value={car.rating || 0} readOnly precision={0.5} />
                  <Typography variant="caption" display="block" color="text.secondary">
                    {car.reviewCount || 0} değerlendirme
                  </Typography>
                </Box>
              )}
              
              {tabValue === 1 && (
                <Box>
                  <Typography variant="h6" gutterBottom>Teknik Özellikler</Typography>
                  <Grid container spacing={2}>
                    <Grid item xs={6}>
                      <Typography><strong>Marka:</strong> {car.brand}</Typography>
                      <Typography><strong>Model:</strong> {car.model}</Typography>
                      <Typography><strong>Yakıt Türü:</strong> {car.fuelType}</Typography>
                      <Typography><strong>Vites:</strong> {car.transmission}</Typography>
                    </Grid>
                    <Grid item xs={6}>
                      <Typography><strong>Yakıt Tüketimi:</strong> {car.fuelConsumption} lt/100km</Typography>
                      <Typography><strong>Bagaj Hacmi:</strong> {car.luggageCapacity} lt</Typography>
                      <Typography><strong>Koltuk Sayısı:</strong> {car.seats}</Typography>
                      <Typography><strong>Ehliyet Yaşı:</strong> {car.minDrivingLicenseAge}+</Typography>
                    </Grid>
                  </Grid>
                </Box>
              )}
              
              {tabValue === 2 && (
                <Box>
                  <Typography variant="h6" gutterBottom>Kiralama Koşulları</Typography>
                  <Typography paragraph>
                    • Minimum sürücü yaşı: {car.minDriverAge} yaş
                  </Typography>
                  <Typography paragraph>
                    • Minimum sürüş deneyimi: {car.minDrivingExperience} yıl
                  </Typography>
                  <Typography paragraph>
                    • Kasko: {car.insurance ? 'Dahil' : 'Dahil Değil'}
                  </Typography>
                  <Typography paragraph>
                    • KM Sınırı: {car.dailyKmLimit || 'Sınırsız'}
                  </Typography>
                </Box>
              )}
            </Box>
          </Paper>
        </Grid>
        
        {/* Rezervasyon Formu */}
        <Grid item xs={12} md={4}>
          <Paper elevation={3} sx={{ p: 3, position: 'sticky', top: 20 }}>
            <Typography variant="h6" gutterBottom>Hızlı Rezervasyon</Typography>
            <Divider sx={{ my: 2 }} />
            
            <Box mb={3}>
              <Box display="flex" justifyContent="space-between" mb={1}>
                <Typography variant="body2">Günlük Kira</Typography>
                <Typography variant="body2" fontWeight="bold">{car.dailyPrice} ₺</Typography>
              </Box>
              {car.weeklyDiscount > 0 && (
                <Box display="flex" justifyContent="space-between" mb={1}>
                  <Typography variant="body2">Haftalık İndirim ({car.weeklyDiscount}%)</Typography>
                  <Typography variant="body2" color="success.main">-{car.dailyPrice * car.weeklyDiscount / 100} ₺</Typography>
                </Box>
              )}
              {car.monthlyDiscount > 0 && (
                <Box display="flex" justifyContent="space-between" mb={1}>
                  <Typography variant="body2">Aylık İndirim ({car.monthlyDiscount}%)</Typography>
                  <Typography variant="body2" color="success.main">-{car.dailyPrice * car.monthlyDiscount / 100} ₺</Typography>
                </Box>
              )}
            </Box>
            
            <Box mb={3}>
              <DatePicker
                label="Alış Tarihi"
                value={dates.startDate}
                onChange={handleDateChange('startDate')}
                renderInput={(params) => <TextField {...params} fullWidth margin="normal" />}
                minDate={new Date()}
                localeText={{
                  okButtonLabel: 'Tamam',
                  cancelButtonLabel: 'İptal',
                  toolbarTitle: 'Tarih Seçin',
                  todayButtonLabel: 'Bugün',
                }}
              />
              
              <DatePicker
                label="Teslim Tarihi"
                value={dates.endDate}
                onChange={handleDateChange('endDate')}
                renderInput={(params) => <TextField {...params} fullWidth margin="normal" />}
                minDate={dates.startDate || new Date()}
                localeText={{
                  okButtonLabel: 'Tamam',
                  cancelButtonLabel: 'İptal',
                  toolbarTitle: 'Tarih Seçin',
                  todayButtonLabel: 'Bugün',
                }}
              />
              
              <Button 
                fullWidth 
                variant="contained" 
                color="primary" 
                onClick={handleCheckAvailability}
                sx={{ mt: 2 }}
              >
                Müsaitlik Kontrol Et
              </Button>
            </Box>
            
            {isAvailable !== null && (
              <Box>
                {isAvailable ? (
                  <Box>
                    <Box display="flex" justifyContent="space-between" mb={1}>
                      <Typography>Toplam Süre:</Typography>
                      <Typography>{priceDetails?.days || 0} Gün</Typography>
                    </Box>
                    <Box display="flex" justifyContent="space-between" mb={1}>
                      <Typography>Araç Kira Bedeli:</Typography>
                      <Typography>{priceDetails?.subtotal || 0} ₺</Typography>
                    </Box>
                    {priceDetails?.discount > 0 && (
                      <Box display="flex" justifyContent="space-between" mb={1}>
                        <Typography>İndirim:</Typography>
                        <Typography color="success.main">-{priceDetails.discount} ₺</Typography>
                      </Box>
                    )}
                    {priceDetails?.insuranceFee > 0 && (
                      <Box display="flex" justifyContent="space-between" mb={1}>
                        <Typography>Kasko Ücreti:</Typography>
                        <Typography>{priceDetails.insuranceFee} ₺</Typography>
                      </Box>
                    )}
                    <Divider sx={{ my: 2 }} />
                    <Box display="flex" justifyContent="space-between" mb={2}>
                      <Typography variant="h6">Toplam Tutar:</Typography>
                      <Typography variant="h6" color="primary">
                        {priceDetails?.total || 0} ₺
                      </Typography>
                    </Box>
                    
                    <Button 
                      fullWidth 
                      variant="contained" 
                      color="primary" 
                      size="large"
                      onClick={handleReservation}
                      startIcon={<CheckCircle />}
                    >
                      Hemen Kirala
                    </Button>
                  </Box>
                ) : (
                  <Typography color="error" align="center">
                    Seçtiğiniz tarihlerde araç müsait değil.
                  </Typography>
                )}
              </Box>
            )}
            
            <Box mt={2} textAlign="center">
              <Typography variant="caption" color="text.secondary">
                * Fiyatlarımıza KDV dahildir.
              </Typography>
            </Box>
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
};

export default CarDetailPage;

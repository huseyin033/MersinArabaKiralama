import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useRental } from '../contexts/RentalContext';
import { useNotification } from '../contexts/NotificationContext';
import { Elements } from '@stripe/react-stripe-js';
import { loadStripe } from '@stripe/stripe-js';
import {
  Container,
  Paper,
  Typography,
  Stepper,
  Step,
  StepLabel,
  Box,
  Button,
  Divider,
  Grid,
  TextField,
  FormControlLabel,
  Checkbox,
  Alert,
  CircularProgress
} from '@mui/material';
import {
  CreditCard as CreditCardIcon,
  Person as PersonIcon,
  Receipt as ReceiptIcon,
  CheckCircle as CheckCircleIcon,
  Home as HomeIcon,
  ArrowBack as ArrowBackIcon
} from '@mui/icons-material';
import { format } from 'date-fns';
import { tr } from 'date-fns/locale';

// Stripe Elements bileşenleri
import StripePaymentForm from '../components/payment/StripePaymentForm';

const steps = ['Rezervasyon Özeti', 'Ödeme Bilgileri', 'Onay'];

// Stripe public key'i yükleyin
const stripePromise = loadStripe(process.env.REACT_APP_STRIPE_PUBLIC_KEY);

const CheckoutPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { getReservationById, confirmReservation } = useRental();
  const { showNotification } = useNotification();
  
  const [activeStep, setActiveStep] = useState(0);
  const [reservation, setReservation] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [agreedToTerms, setAgreedToTerms] = useState(false);
  const [paymentMethod, setPaymentMethod] = useState('credit_card');
  const [billingAddress, setBillingAddress] = useState({
    fullName: '',
    phone: '',
    address: '',
    city: '',
    district: '',
    postalCode: ''
  });
  const [clientSecret, setClientSecret] = useState('');
  
  useEffect(() => {
    const fetchReservation = async () => {
      try {
        const data = await getReservationById(id);
        setReservation(data);
        
        // Fatura adresi varsa doldur
        if (user) {
          setBillingAddress(prev => ({
            ...prev,
            fullName: user.fullName || '',
            phone: user.phone || '',
            address: user.address || '',
            city: user.city || '',
            district: user.district || '',
            postalCode: user.postalCode || ''
          }));
        }
        
        // Ödeme için client secret al
        // Bu kısımda backend'den client secret alınacak
        // Örnek: const { clientSecret } = await paymentService.createPaymentIntent(data.id);
        // setClientSecret(clientSecret);
        
      } catch (err) {
        setError('Rezervasyon bilgileri yüklenirken bir hata oluştu.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    
    fetchReservation();
  }, [id, user]);
  
  const handleNext = () => {
    if (activeStep === 0 && !validateStep1()) return;
    if (activeStep === 1 && !validateStep2()) return;
    
    setActiveStep((prevStep) => prevStep + 1);
    
    // Son adıma geçildiğinde ödemeyi onayla
    if (activeStep === steps.length - 2) {
      handleConfirmReservation();
    }
  };
  
  const handleBack = () => {
    setActiveStep((prevStep) => prevStep - 1);
  };
  
  const validateStep1 = () => {
    if (!billingAddress.fullName.trim()) {
      showNotification('Lütfen ad soyad bilgisini giriniz.', 'error');
      return false;
    }
    if (!billingAddress.phone.trim() || billingAddress.phone.replace(/\D/g, '').length < 10) {
      showNotification('Lütfen geçerli bir telefon numarası giriniz.', 'error');
      return false;
    }
    return true;
  };
  
  const validateStep2 = () => {
    if (!agreedToTerms) {
      showNotification('Lütfen kiralama koşullarını kabul ediniz.', 'error');
      return false;
    }
    return true;
  };
  
  const handleBillingAddressChange = (e) => {
    const { name, value } = e.target;
    setBillingAddress(prev => ({
      ...prev,
      [name]: value
    }));
  };
  
  const handleConfirmReservation = async () => {
    try {
      // Ödeme işlemi burada yapılacak
      // Stripe Elements ile ödeme işlemi tamamlandıktan sonra bu fonksiyon çağrılacak
      
      // Örnek ödeme onaylama kodu:
      // const result = await confirmReservation(reservation.id, {
      //   paymentMethodId: paymentMethodId,
      //   billingAddress
      // });
      
      // Başarılı olduğunda teşekkür sayfasına yönlendir
      // navigate(`/rezervasyon/tebrikler/${result.reservationNumber}`);
      
      // Şimdilik doğrudan teşekkür sayfasına yönlendiriyoruz
      navigate(`/rezervasyon/tebrikler/ABC123`);
      
    } catch (err) {
      setError('Ödeme işlemi sırasında bir hata oluştu. Lütfen tekrar deneyiniz.');
      console.error(err);
    }
  };
  
  const handlePaymentSuccess = (paymentResult) => {
    console.log('Ödeme başarılı:', paymentResult);
    handleNext();
  };
  
  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }
  
  if (error) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>
        <Button 
          variant="outlined" 
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate(-1)}
        >
          Geri Dön
        </Button>
      </Container>
    );
  }
  
  if (!reservation) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="warning">Rezervasyon bulunamadı.</Alert>
        <Button 
          variant="outlined" 
          startIcon={<HomeIcon />}
          onClick={() => navigate('/')}
          sx={{ mt: 2 }}
        >
          Ana Sayfaya Dön
        </Button>
      </Container>
    );
  }
  
  const { car, startDate, endDate, priceDetails } = reservation;
  
  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stepper activeStep={activeStep} alternativeLabel sx={{ mb: 4 }}>
        {steps.map((label) => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>
      
      <Paper elevation={3} sx={{ p: { xs: 2, md: 4 }, mb: 4 }}>
        {activeStep === 0 && (
          <Box>
            <Typography variant="h5" gutterBottom>Rezervasyon Özeti</Typography>
            <Divider sx={{ my: 2 }} />
            
            <Grid container spacing={4}>
              <Grid item xs={12} md={8}>
                <Box mb={4}>
                  <Typography variant="h6" gutterBottom>Kişisel Bilgiler</Typography>
                  <Grid container spacing={2}>
                    <Grid item xs={12} md={6}>
                      <TextField
                        fullWidth
                        label="Ad Soyad"
                        name="fullName"
                        value={billingAddress.fullName}
                        onChange={handleBillingAddressChange}
                        margin="normal"
                        required
                      />
                    </Grid>
                    <Grid item xs={12} md={6}>
                      <TextField
                        fullWidth
                        label="Telefon Numarası"
                        name="phone"
                        value={billingAddress.phone}
                        onChange={handleBillingAddressChange}
                        margin="normal"
                        required
                        placeholder="5__ ___ __ __"
                      />
                    </Grid>
                    <Grid item xs={12}>
                      <TextField
                        fullWidth
                        label="Adres"
                        name="address"
                        value={billingAddress.address}
                        onChange={handleBillingAddressChange}
                        margin="normal"
                        multiline
                        rows={2}
                      />
                    </Grid>
                    <Grid item xs={12} md={4}>
                      <TextField
                        fullWidth
                        label="İl"
                        name="city"
                        value={billingAddress.city}
                        onChange={handleBillingAddressChange}
                        margin="normal"
                      />
                    </Grid>
                    <Grid item xs={12} md={4}>
                      <TextField
                        fullWidth
                        label="İlçe"
                        name="district"
                        value={billingAddress.district}
                        onChange={handleBillingAddressChange}
                        margin="normal"
                      />
                    </Grid>
                    <Grid item xs={12} md={4}>
                      <TextField
                        fullWidth
                        label="Posta Kodu"
                        name="postalCode"
                        value={billingAddress.postalCode}
                        onChange={handleBillingAddressChange}
                        margin="normal"
                      />
                    </Grid>
                  </Grid>
                </Box>
                
                <Box>
                  <Typography variant="h6" gutterBottom>Kiralama Bilgileri</Typography>
                  <Grid container spacing={2}>
                    <Grid item xs={12} md={6}>
                      <TextField
                        fullWidth
                        label="Alış Tarihi"
                        value={format(new Date(startDate), 'dd MMMM yyyy EEEE', { locale: tr })}
                        margin="normal"
                        disabled
                      />
                    </Grid>
                    <Grid item xs={12} md={6}>
                      <TextField
                        fullWidth
                        label="Teslim Tarihi"
                        value={format(new Date(endDate), 'dd MMMM yyyy EEEE', { locale: tr })}
                        margin="normal"
                        disabled
                      />
                    </Grid>
                    <Grid item xs={12}>
                      <TextField
                        fullWidth
                        label="Toplam Gün"
                        value={`${priceDetails?.days || 0} Gün`}
                        margin="normal"
                        disabled
                      />
                    </Grid>
                  </Grid>
                </Box>
              </Grid>
              
              <Grid item xs={12} md={4}>
                <Paper elevation={2} sx={{ p: 3, position: 'sticky', top: 20 }}>
                  <Typography variant="h6" gutterBottom>Özet</Typography>
                  <Divider sx={{ my: 2 }} />
                  
                  <Box display="flex" justifyContent="space-between" mb={1}>
                    <Typography>Araç:</Typography>
                    <Typography fontWeight="bold">{car.brand} {car.model}</Typography>
                  </Box>
                  
                  <Box display="flex" justifyContent="space-between" mb={1}>
                    <Typography>Kira Süresi:</Typography>
                    <Typography>{priceDetails?.days || 0} Gün</Typography>
                  </Box>
                  
                  <Box display="flex" justifyContent="space-between" mb={1}>
                    <Typography>Günlük Ücret:</Typography>
                    <Typography>{priceDetails?.dailyRate || 0} ₺</Typography>
                  </Box>
                  
                  {priceDetails?.discount > 0 && (
                    <Box display="flex" justifyContent="space-between" mb={1} color="success.main">
                      <Typography>İndirim:</Typography>
                      <Typography>-{priceDetails.discount} ₺</Typography>
                    </Box>
                  )}
                  
                  {priceDetails?.insuranceFee > 0 && (
                    <Box display="flex" justifyContent="space-between" mb={1}>
                      <Typography>Kasko Ücreti:</Typography>
                      <Typography>+{priceDetails.insuranceFee} ₺</Typography>
                    </Box>
                  )}
                  
                  <Divider sx={{ my: 2 }} />
                  
                  <Box display="flex" justifyContent="space-between" mb={2}>
                    <Typography variant="h6">Toplam:</Typography>
                    <Typography variant="h6" color="primary">
                      {priceDetails?.total || 0} ₺
                    </Typography>
                  </Box>
                  
                  <FormControlLabel
                    control={
                      <Checkbox
                        checked={agreedToTerms}
                        onChange={(e) => setAgreedToTerms(e.target.checked)}
                        color="primary"
                      />
                    }
                    label={
                      <Typography variant="body2">
                        <a href="/kullanim-kosullari" target="_blank" rel="noopener noreferrer">Kullanım koşullarını</a> ve 
                        <a href="/gizlilik-politikasi" target="_blank" rel="noopener noreferrer"> gizlilik politikasını</a> okudum, onaylıyorum.
                      </Typography>
                    }
                    sx={{ mt: 1, display: 'block' }}
                  />
                </Paper>
              </Grid>
            </Grid>
          </Box>
        )}
        
        {activeStep === 1 && (
          <Box>
            <Typography variant="h5" gutterBottom>Ödeme İşlemi</Typography>
            <Divider sx={{ my: 2 }} />
            
            <Grid container spacing={4}>
              <Grid item xs={12} md={8}>
                <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
                  <Box display="flex" alignItems="center" mb={3}>
                    <CreditCardIcon color="primary" sx={{ mr: 1 }} />
                    <Typography variant="h6">Kredi Kartı ile Ödeme</Typography>
                  </Box>
                  
                  {clientSecret ? (
                    <Elements 
                      stripe={stripePromise}
                      options={{
                        clientSecret,
                        appearance: {
                          theme: 'stripe',
                          variables: {
                            colorPrimary: '#1976d2',
                            colorBackground: '#ffffff',
                            colorText: '#30313d',
                            colorDanger: '#df1b41',
                            fontFamily: 'Roboto, sans-serif',
                          },
                        },
                      }}
                    >
                      <StripePaymentForm 
                        onSuccess={handlePaymentSuccess} 
                        onError={(err) => setError(err.message)}
                      />
                    </Elements>
                  ) : (
                    <Box textAlign="center" py={4}>
                      <CircularProgress />
                      <Typography variant="body2" color="text.secondary" mt={2}>
                        Ödeme sayfası yükleniyor...
                      </Typography>
                    </Box>
                  )}
                </Paper>
                
                <Paper elevation={2} sx={{ p: 3 }}>
                  <Box display="flex" alignItems="center" mb={2}>
                    <ReceiptIcon color="primary" sx={{ mr: 1 }} />
                    <Typography variant="h6">Fatura Bilgileri</Typography>
                  </Box>
                  
                  <Grid container spacing={2}>
                    <Grid item xs={12}>
                      <TextField
                        fullWidth
                        label="Fatura Adresi"
                        name="address"
                        value={billingAddress.address}
                        onChange={handleBillingAddressChange}
                        margin="normal"
                        multiline
                        rows={2}
                      />
                    </Grid>
                    <Grid item xs={12} md={6}>
                      <TextField
                        fullWidth
                        label="Vergi Dairesi"
                        margin="normal"
                        placeholder="Varsa vergi dairesi giriniz"
                      />
                    </Grid>
                    <Grid item xs={12} md={6}>
                      <TextField
                        fullWidth
                        label="Vergi Numarası / TCKN"
                        margin="normal"
                        placeholder="Vergi veya TC kimlik numaranız"
                      />
                    </Grid>
                  </Grid>
                </Paper>
              </Grid>
              
              <Grid item xs={12} md={4}>
                <Paper elevation={2} sx={{ p: 3, position: 'sticky', top: 20 }}>
                  <Typography variant="h6" gutterBottom>Rezervasyon Özeti</Typography>
                  <Divider sx={{ my: 2 }} />
                  
                  <Box mb={2}>
                    <Typography variant="subtitle2" color="text.secondary">Araç</Typography>
                    <Typography>{car.brand} {car.model} {car.modelYear}</Typography>
                  </Box>
                  
                  <Box mb={2}>
                    <Typography variant="subtitle2" color="text.secondary">Kiralama Tarihleri</Typography>
                    <Typography>
                      {format(new Date(startDate), 'dd MMM yyyy')} - {format(new Date(endDate), 'dd MMM yyyy')}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {priceDetails?.days || 0} Gün
                    </Typography>
                  </Box>
                  
                  <Divider sx={{ my: 2 }} />
                  
                  <Box mb={1}>
                    <Box display="flex" justifyContent="space-between" mb={0.5}>
                      <Typography>Araç Kira Bedeli:</Typography>
                      <Typography>{priceDetails?.subtotal || 0} ₺</Typography>
                    </Box>
                    
                    {priceDetails?.discount > 0 && (
                      <Box display="flex" justifyContent="space-between" mb={0.5} color="success.main">
                        <Typography>İndirim:</Typography>
                        <Typography>-{priceDetails.discount} ₺</Typography>
                      </Box>
                    )}
                    
                    {priceDetails?.insuranceFee > 0 && (
                      <Box display="flex" justifyContent="space-between" mb={0.5}>
                        <Typography>Kasko Ücreti:</Typography>
                        <Typography>+{priceDetails.insuranceFee} ₺</Typography>
                      </Box>
                    )}
                    
                    <Box display="flex" justifyContent="space-between" mt={2}>
                      <Typography variant="subtitle1" fontWeight="bold">Toplam:</Typography>
                      <Typography variant="subtitle1" fontWeight="bold" color="primary">
                        {priceDetails?.total || 0} ₺
                      </Typography>
                    </Box>
                  </Box>
                  
                  <Button
                    fullWidth
                    variant="contained"
                    color="primary"
                    size="large"
                    onClick={handleNext}
                    disabled={!agreedToTerms || !clientSecret}
                    sx={{ mt: 2 }}
                  >
                    Ödemeyi Tamamla
                  </Button>
                  
                  <Typography variant="caption" display="block" textAlign="center" mt={1} color="text.secondary">
                    Güvenli ödeme ile ödemenizi güvenle tamamlayın.
                  </Typography>
                </Paper>
              </Grid>
            </Grid>
          </Box>
        )}
        
        {activeStep === 2 && (
          <Box textAlign="center" py={6}>
            <CheckCircleIcon color="success" sx={{ fontSize: 80, mb: 3 }} />
            <Typography variant="h4" gutterBottom>Ödemeniz Alındı!</Typography>
            <Typography variant="h6" color="text.secondary" paragraph>
              Rezervasyon Numaranız: <strong>ABC123</strong>
            </Typography>
            <Typography variant="body1" color="text.secondary" paragraph>
              Rezervasyon detaylarınızı e-posta adresinize gönderdik. Ayrıca 
              <strong> profil sayfanızdan</strong> da rezervasyonlarınızı takip edebilirsiniz.
            </Typography>
            
            <Box mt={4} display="flex" justifyContent="center" gap={2}>
              <Button 
                variant="contained" 
                color="primary" 
                size="large" 
                onClick={() => navigate('/profil/rezervasyonlarim')}
                startIcon={<PersonIcon />}
              >
                Rezervasyonlarım
              </Button>
              <Button 
                variant="outlined" 
                color="primary" 
                size="large" 
                onClick={() => navigate('/')}
                startIcon={<HomeIcon />}
              >
                Ana Sayfa
              </Button>
            </Box>
          </Box>
        )}
        
        <Box sx={{ display: 'flex', justifyContent: 'space-between', pt: 4 }}>
          <Button
            disabled={activeStep === 0}
            onClick={handleBack}
            sx={{ mr: 1 }}
          >
            Geri
          </Button>
          
          {activeStep < steps.length - 1 && (
            <Button
              variant="contained"
              onClick={handleNext}
              disabled={!agreedToTerms || activeStep === 1} // Ödeme adımında buton devre dışı, Stripe formu kullanılacak
            >
              {activeStep === steps.length - 2 ? 'Ödemeyi Tamamla' : 'Devam Et'}
            </Button>
          )}
        </Box>
      </Paper>
    </Container>
  );
};

export default CheckoutPage;

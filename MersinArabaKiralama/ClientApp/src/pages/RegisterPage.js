import React, { useState } from 'react';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  TextField,
  Button,
  Link,
  Paper,
  InputAdornment,
  IconButton,
  Alert,
  FormControlLabel,
  Checkbox,
  Divider,
  useTheme,
  Grid
} from '@mui/material';
import { 
  Person as PersonIcon, 
  Email as EmailIcon, 
  Lock as LockIcon, 
  Phone as PhoneIcon,
  Visibility, 
  VisibilityOff 
} from '@mui/icons-material';

const RegisterPage = () => {
  const [showPassword, setShowPassword] = useState(false);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    password: '',
    confirmPassword: '',
    acceptTerms: false,
  });
  
  const [errors, setErrors] = useState({});
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const theme = useTheme();
  const navigate = useNavigate();

  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.firstName.trim()) {
      newErrors.firstName = 'Ad alanı zorunludur';
    }
    
    if (!formData.lastName.trim()) {
      newErrors.lastName = 'Soyad alanı zorunludur';
    }
    
    if (!formData.email) {
      newErrors.email = 'E-posta adresi zorunludur';
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.email = 'Geçerli bir e-posta adresi giriniz';
    }
    
    if (!formData.phone) {
      newErrors.phone = 'Telefon numarası zorunludur';
    } else if (!/^[0-9\-\s()]+$/.test(formData.phone)) {
      newErrors.phone = 'Geçerli bir telefon numarası giriniz';
    }
    
    if (!formData.password) {
      newErrors.password = 'Şifre alanı zorunludur';
    } else if (formData.password.length < 6) {
      newErrors.password = 'Şifre en az 6 karakter olmalıdır';
    }
    
    if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Şifreler eşleşmiyor';
    }
    
    if (!formData.acceptTerms) {
      newErrors.acceptTerms = 'Kullanım koşullarını kabul etmelisiniz';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleChange = (e) => {
    const { name, value, checked, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
    
    // Hata mesajını temizle
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    
    if (!validateForm()) {
      return;
    }
    
    setLoading(true);
    
    try {
      // TODO: API entegrasyonu yapılacak
      console.log('Kayıt olunuyor:', formData);
      await new Promise(resolve => setTimeout(resolve, 1000)); // Simüle edilmiş API çağrısı
      
      // Başarılı kayıt sonrası yönlendirme
      navigate('/register/success', { 
        state: { email: formData.email } 
      });
    } catch (err) {
      setError(err.message || 'Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="md" sx={{ py: 8 }}>
      <Paper elevation={3} sx={{ p: 4, borderRadius: 2 }}>
        <Box sx={{ textAlign: 'center', mb: 4 }}>
          <Typography variant="h4" component="h1" sx={{ fontWeight: 'bold', mb: 1 }}>
            Yeni Hesap Oluştur
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Zaten hesabınız var mı?{' '}
            <Link component={RouterLink} to="/login" color="primary">
              Giriş yapın
            </Link>
          </Typography>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}

        <Box component="form" onSubmit={handleSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Adınız"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                margin="normal"
                error={!!errors.firstName}
                helperText={errors.firstName}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <PersonIcon color="action" />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Soyadınız"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                margin="normal"
                error={!!errors.lastName}
                helperText={errors.lastName}
              />
            </Grid>
          </Grid>

          <TextField
            fullWidth
            label="E-posta Adresi"
            name="email"
            type="email"
            value={formData.email}
            onChange={handleChange}
            margin="normal"
            error={!!errors.email}
            helperText={errors.email}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <EmailIcon color="action" />
                </InputAdornment>
              ),
            }}
            sx={{ mb: 2 }}
          />

          <TextField
            fullWidth
            label="Telefon Numarası"
            name="phone"
            value={formData.phone}
            onChange={handleChange}
            margin="normal"
            error={!!errors.phone}
            helperText={errors.phone || 'Örn: 5551234567'}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <PhoneIcon color="action" />
                </InputAdornment>
              ),
            }}
            sx={{ mb: 2 }}
          />

          <TextField
            fullWidth
            label="Şifre"
            name="password"
            type={showPassword ? 'text' : 'password'}
            value={formData.password}
            onChange={handleChange}
            margin="normal"
            error={!!errors.password}
            helperText={errors.password || 'En az 6 karakter olmalıdır'}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <LockIcon color="action" />
                </InputAdornment>
              ),
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    aria-label="toggle password visibility"
                    onClick={() => setShowPassword(!showPassword)}
                    edge="end"
                  >
                    {showPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              ),
            }}
            sx={{ mb: 2 }}
          />

          <TextField
            fullWidth
            label="Şifre Tekrar"
            name="confirmPassword"
            type={showPassword ? 'text' : 'password'}
            value={formData.confirmPassword}
            onChange={handleChange}
            margin="normal"
            error={!!errors.confirmPassword}
            helperText={errors.confirmPassword}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <LockIcon color="action" />
                </InputAdornment>
              ),
            }}
            sx={{ mb: 3 }}
          />

          <FormControlLabel
            control={
              <Checkbox
                checked={formData.acceptTerms}
                onChange={handleChange}
                name="acceptTerms"
                color="primary"
              />
            }
            label={
              <Typography variant="body2" color={errors.acceptTerms ? 'error' : 'textSecondary'}>
                <Link component={RouterLink} to="/terms" color="primary">
                  Kullanım koşullarını
                </Link>{' '}
                ve{' '}
                <Link component={RouterLink} to="/privacy" color="primary">
                  gizlilik politikasını
                </Link>{' '}
                okudum ve kabul ediyorum.
              </Typography>
            }
            sx={{ 
              display: 'flex',
              alignItems: 'flex-start',
              mb: 3,
              '& .MuiFormControlLabel-label': {
                marginTop: '3px'
              }
            }}
          />
          {errors.acceptTerms && (
            <Typography color="error" variant="caption" display="block" gutterBottom>
              {errors.acceptTerms}
            </Typography>
          )}

          <Button
            type="submit"
            fullWidth
            variant="contained"
            size="large"
            disabled={loading}
            sx={{
              py: 1.5,
              fontSize: '1.1rem',
              textTransform: 'none',
              fontWeight: 'bold',
              mb: 3
            }}
          >
            {loading ? 'Hesap Oluşturuluyor...' : 'Hesap Oluştur'}
          </Button>

          <Divider sx={{ my: 3 }}>veya</Divider>

          <Button
            fullWidth
            variant="outlined"
            size="large"
            startIcon={
              <Box
                component="img"
                src="https://www.google.com/favicon.ico"
                alt="Google"
                sx={{ width: 20, height: 20, mr: 1 }}
              />
            }
            sx={{
              py: 1.5,
              fontSize: '1rem',
              textTransform: 'none',
              mb: 2,
              borderColor: theme.palette.divider,
              '&:hover': {
                borderColor: theme.palette.text.primary,
              },
            }}
          >
            Google ile Kayıt Ol
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default RegisterPage;

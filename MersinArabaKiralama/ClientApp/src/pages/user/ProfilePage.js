import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useNotification } from '../../contexts/NotificationContext';
import {
  Container,
  Paper,
  Typography,
  Box,
  Button,
  Avatar,
  TextField,
  Grid,
  Divider,
  Tabs,
  Tab,
  IconButton,
  InputAdornment,
  Alert,
  CircularProgress
} from '@mui/material';
import {
  Person as PersonIcon,
  Edit as EditIcon,
  Check as CheckIcon,
  Close as CloseIcon,
  Visibility,
  VisibilityOff,
  Lock as LockIcon,
  Email as EmailIcon,
  Phone as PhoneIcon,
  Home as HomeIcon,
  CreditCard as CreditCardIcon,
  History as HistoryIcon,
  Logout as LogoutIcon
} from '@mui/icons-material';

const ProfilePage = () => {
  const { user, updateProfile, changePassword, logout } = useAuth();
  const { showNotification } = useNotification();
  const navigate = useNavigate();
  
  const [activeTab, setActiveTab] = useState(0);
  const [editMode, setEditMode] = useState(false);
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    phone: '',
    address: '',
    city: '',
    district: '',
    postalCode: ''
  });
  
  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  
  const [errors, setErrors] = useState({});
  
  useEffect(() => {
    if (user) {
      setFormData({
        fullName: user.fullName || '',
        email: user.email || '',
        phone: user.phone || '',
        address: user.address || '',
        city: user.city || '',
        district: user.district || '',
        postalCode: user.postalCode || ''
      });
    }
  }, [user]);
  
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
    setEditMode(false);
    setErrors({});
  };
  
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };
  
  const handlePasswordChange = (e) => {
    const { name, value } = e.target;
    setPasswordData(prev => ({
      ...prev,
      [name]: value
    }));
  };
  
  const validateProfileForm = () => {
    const newErrors = {};
    
    if (!formData.fullName.trim()) {
      newErrors.fullName = 'Ad soyad zorunludur';
    }
    
    if (!formData.email) {
      newErrors.email = 'E-posta adresi zorunludur';
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.email = 'Geçerli bir e-posta adresi giriniz';
    }
    
    if (formData.phone && !/^[0-9\s()+-]*$/.test(formData.phone)) {
      newErrors.phone = 'Geçerli bir telefon numarası giriniz';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };
  
  const validatePasswordForm = () => {
    const newErrors = {};
    
    if (!passwordData.currentPassword) {
      newErrors.currentPassword = 'Mevcut şifrenizi giriniz';
    }
    
    if (!passwordData.newPassword) {
      newErrors.newPassword = 'Yeni şifre giriniz';
    } else if (passwordData.newPassword.length < 6) {
      newErrors.newPassword = 'Şifre en az 6 karakter olmalıdır';
    }
    
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      newErrors.confirmPassword = 'Şifreler eşleşmiyor';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };
  
  const handleSaveProfile = async () => {
    if (!validateProfileForm()) return;
    
    setLoading(true);
    
    try {
      await updateProfile(formData);
      showNotification('Profil bilgileriniz güncellendi', 'success');
      setEditMode(false);
    } catch (error) {
      console.error('Profil güncellenirken hata oluştu:', error);
      showNotification('Profil güncellenirken bir hata oluştu', 'error');
    } finally {
      setLoading(false);
    }
  };
  
  const handleChangePassword = async () => {
    if (!validatePasswordForm()) return;
    
    setLoading(true);
    
    try {
      await changePassword({
        currentPassword: passwordData.currentPassword,
        newPassword: passwordData.newPassword
      });
      
      showNotification('Şifreniz başarıyla değiştirildi', 'success');
      setPasswordData({
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      });
    } catch (error) {
      console.error('Şifre değiştirilirken hata oluştu:', error);
      showNotification(error.response?.data?.message || 'Şifre değiştirilirken bir hata oluştu', 'error');
    } finally {
      setLoading(false);
    }
  };
  
  const handleLogout = async () => {
    try {
      await logout();
      showNotification('Başarıyla çıkış yapıldı', 'success');
      navigate('/');
    } catch (error) {
      console.error('Çıkış yapılırken hata oluştu:', error);
      showNotification('Çıkış yapılırken bir hata oluştu', 'error');
    }
  };
  
  if (!user) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }
  
  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Grid container spacing={4}>
        {/* Sol Menü */}
        <Grid item xs={12} md={3}>
          <Paper elevation={3} sx={{ p: 3, mb: 3 }}>
            <Box display="flex" flexDirection="column" alignItems="center" mb={3}>
              <Avatar 
                sx={{ 
                  width: 100, 
                  height: 100, 
                  mb: 2,
                  bgcolor: 'primary.main',
                  fontSize: '2.5rem'
                }}
              >
                {user.fullName ? user.fullName.charAt(0).toUpperCase() : 'K'}
              </Avatar>
              <Typography variant="h6" align="center">{user.fullName || 'Kullanıcı'}</Typography>
              <Typography variant="body2" color="text.secondary">{user.email}</Typography>
            </Box>
            
            <Divider sx={{ my: 2 }} />
            
            <Tabs
              orientation="vertical"
              variant="scrollable"
              value={activeTab}
              onChange={handleTabChange}
              sx={{ borderRight: 1, borderColor: 'divider' }}
            >
              <Tab 
                icon={<PersonIcon />} 
                label="Profil Bilgileri" 
                iconPosition="start" 
                sx={{ justifyContent: 'flex-start', minHeight: 48 }}
              />
              <Tab 
                icon={<LockIcon />} 
                label="Şifre Değiştir" 
                iconPosition="start" 
                sx={{ justifyContent: 'flex-start', minHeight: 48 }}
              />
              <Tab 
                icon={<CreditCardIcon />} 
                label="Ödeme Yöntemlerim" 
                iconPosition="start" 
                sx={{ justifyContent: 'flex-start', minHeight: 48 }}
              />
              <Tab 
                icon={<HistoryIcon />} 
                label="Rezervasyon Geçmişi" 
                iconPosition="start" 
                sx={{ justifyContent: 'flex-start', minHeight: 48 }}
              />
              <Divider sx={{ my: 1 }} />
              <Tab 
                icon={<LogoutIcon />} 
                label="Çıkış Yap" 
                iconPosition="start" 
                sx={{ justifyContent: 'flex-start', minHeight: 48, color: 'error.main' }}
                onClick={handleLogout}
              />
            </Tabs>
          </Paper>
        </Grid>
        
        {/* İçerik Alanı */}
        <Grid item xs={12} md={9}>
          <Paper elevation={3} sx={{ p: 4 }}>
            {activeTab === 0 && (
              <Box>
                <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
                  <Typography variant="h5">Profil Bilgileri</Typography>
                  {!editMode ? (
                    <Button 
                      variant="outlined" 
                      startIcon={<EditIcon />}
                      onClick={() => setEditMode(true)}
                    >
                      Düzenle
                    </Button>
                  ) : (
                    <Box>
                      <Button 
                        variant="outlined" 
                        color="error" 
                        startIcon={<CloseIcon />}
                        onClick={() => setEditMode(false)}
                        sx={{ mr: 1 }}
                      >
                        İptal
                      </Button>
                      <Button 
                        variant="contained" 
                        color="primary" 
                        startIcon={<CheckIcon />}
                        onClick={handleSaveProfile}
                        disabled={loading}
                      >
                        {loading ? 'Kaydediliyor...' : 'Kaydet'}
                      </Button>
                    </Box>
                  )}
                </Box>
                
                <Divider sx={{ mb: 3 }} />
                
                <Grid container spacing={3}>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Ad Soyad"
                      name="fullName"
                      value={formData.fullName}
                      onChange={handleInputChange}
                      margin="normal"
                      disabled={!editMode}
                      error={!!errors.fullName}
                      helperText={errors.fullName}
                      InputProps={{
                        startAdornment: (
                          <InputAdornment position="start">
                            <PersonIcon color={errors.fullName ? 'error' : 'action'} />
                          </InputAdornment>
                        ),
                      }}
                    />
                  </Grid>
                  
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="E-posta Adresi"
                      name="email"
                      type="email"
                      value={formData.email}
                      onChange={handleInputChange}
                      margin="normal"
                      disabled={!editMode}
                      error={!!errors.email}
                      helperText={errors.email}
                      InputProps={{
                        startAdornment: (
                          <InputAdornment position="start">
                            <EmailIcon color={errors.email ? 'error' : 'action'} />
                          </InputAdornment>
                        ),
                      }}
                    />
                  </Grid>
                  
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Telefon Numarası"
                      name="phone"
                      value={formData.phone}
                      onChange={handleInputChange}
                      margin="normal"
                      disabled={!editMode}
                      error={!!errors.phone}
                      helperText={errors.phone || 'Örnek: 5xxxxxxxxx'}
                      InputProps={{
                        startAdornment: (
                          <InputAdornment position="start">
                            <PhoneIcon color={errors.phone ? 'error' : 'action'} />
                          </InputAdornment>
                        ),
                      }}
                    />
                  </Grid>
                  
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Adres"
                      name="address"
                      value={formData.address}
                      onChange={handleInputChange}
                      margin="normal"
                      multiline
                      rows={2}
                      disabled={!editMode}
                      InputProps={{
                        startAdornment: (
                          <InputAdornment position="start">
                            <HomeIcon color="action" />
                          </InputAdornment>
                        ),
                      }}
                    />
                  </Grid>
                  
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      label="İl"
                      name="city"
                      value={formData.city}
                      onChange={handleInputChange}
                      margin="normal"
                      disabled={!editMode}
                    />
                  </Grid>
                  
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      label="İlçe"
                      name="district"
                      value={formData.district}
                      onChange={handleInputChange}
                      margin="normal"
                      disabled={!editMode}
                    />
                  </Grid>
                  
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      label="Posta Kodu"
                      name="postalCode"
                      value={formData.postalCode}
                      onChange={handleInputChange}
                      margin="normal"
                      disabled={!editMode}
                    />
                  </Grid>
                </Grid>
              </Box>
            )}
            
            {activeTab === 1 && (
              <Box>
                <Typography variant="h5" gutterBottom>Şifre Değiştir</Typography>
                <Divider sx={{ mb: 3 }} />
                
                <Grid container spacing={3}>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Mevcut Şifre"
                      name="currentPassword"
                      type={showPassword ? 'text' : 'password'}
                      value={passwordData.currentPassword}
                      onChange={handlePasswordChange}
                      margin="normal"
                      error={!!errors.currentPassword}
                      helperText={errors.currentPassword}
                      InputProps={{
                        startAdornment: (
                          <InputAdornment position="start">
                            <LockIcon color={errors.currentPassword ? 'error' : 'action'} />
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
                    />
                  </Grid>
                  
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Yeni Şifre"
                      name="newPassword"
                      type={showNewPassword ? 'text' : 'password'}
                      value={passwordData.newPassword}
                      onChange={handlePasswordChange}
                      margin="normal"
                      error={!!errors.newPassword}
                      helperText={errors.newPassword || 'En az 6 karakter olmalıdır'}
                      InputProps={{
                        startAdornment: (
                          <InputAdornment position="start">
                            <LockIcon color={errors.newPassword ? 'error' : 'action'} />
                          </InputAdornment>
                        ),
                        endAdornment: (
                          <InputAdornment position="end">
                            <IconButton
                              aria-label="toggle password visibility"
                              onClick={() => setShowNewPassword(!showNewPassword)}
                              edge="end"
                            >
                              {showNewPassword ? <VisibilityOff /> : <Visibility />}
                            </IconButton>
                          </InputAdornment>
                        ),
                      }}
                    />
                  </Grid>
                  
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Yeni Şifre (Tekrar)"
                      name="confirmPassword"
                      type={showConfirmPassword ? 'text' : 'password'}
                      value={passwordData.confirmPassword}
                      onChange={handlePasswordChange}
                      margin="normal"
                      error={!!errors.confirmPassword}
                      helperText={errors.confirmPassword}
                      InputProps={{
                        startAdornment: (
                          <InputAdornment position="start">
                            <LockIcon color={errors.confirmPassword ? 'error' : 'action'} />
                          </InputAdornment>
                        ),
                        endAdornment: (
                          <InputAdornment position="end">
                            <IconButton
                              aria-label="toggle password visibility"
                              onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                              edge="end"
                            >
                              {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                            </IconButton>
                          </InputAdornment>
                        ),
                      }}
                    />
                  </Grid>
                  
                  <Grid item xs={12}>
                    <Button
                      variant="contained"
                      color="primary"
                      size="large"
                      onClick={handleChangePassword}
                      disabled={loading}
                      startIcon={loading ? <CircularProgress size={20} color="inherit" /> : <CheckIcon />}
                    >
                      {loading ? 'İşleniyor...' : 'Şifreyi Güncelle'}
                    </Button>
                  </Grid>
                </Grid>
              </Box>
            )}
            
            {activeTab === 2 && (
              <Box>
                <Typography variant="h5" gutterBottom>Ödeme Yöntemlerim</Typography>
                <Divider sx={{ mb: 3 }} />
                
                <Alert severity="info" sx={{ mb: 3 }}>
                  Güvenli ödeme yöntemlerinizi buradan yönetebilirsiniz.
                </Alert>
                
                <Box textAlign="center" py={4}>
                  <CreditCardIcon color="action" sx={{ fontSize: 60, mb: 2 }} />
                  <Typography variant="h6" color="text.secondary" gutterBottom>
                    Kayıtlı ödeme yöntemi bulunamadı
                  </Typography>
                  <Typography variant="body1" color="text.secondary" paragraph>
                    Rezervasyon yaparken ödeme yöntemi ekleyebilirsiniz.
                  </Typography>
                  <Button 
                    variant="outlined" 
                    color="primary"
                    onClick={() => navigate('/araclar')}
                  >
                    Araçları Görüntüle
                  </Button>
                </Box>
              </Box>
            )}
            
            {activeTab === 3 && (
              <Box>
                <Typography variant="h5" gutterBottom>Rezervasyon Geçmişi</Typography>
                <Divider sx={{ mb: 3 }} />
                
                <Box textAlign="center" py={4}>
                  <HistoryIcon color="action" sx={{ fontSize: 60, mb: 2 }} />
                  <Typography variant="h6" color="text.secondary" gutterBottom>
                    Henüz rezervasyonunuz bulunmuyor
                  </Typography>
                  <Typography variant="body1" color="text.secondary" paragraph>
                    Hemen bir araç kiralayın ve ilk rezervasyonunuzu yapın.
                  </Typography>
                  <Button 
                    variant="contained" 
                    color="primary"
                    onClick={() => navigate('/araclar')}
                  >
                    Araçları Görüntüle
                  </Button>
                </Box>
              </Box>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
};

export default ProfilePage;

import React, { useEffect } from 'react';
import { Routes, Route, useNavigate, useLocation } from 'react-router-dom';
import { Box, Container, CssBaseline, CircularProgress } from '@mui/material';
import { ThemeProvider } from '@mui/material/styles';
import theme from './theme';
import AppProvider from './providers/AppProvider';
import { useAuth } from './contexts/AuthContext';
import { useNotification } from './contexts/NotificationContext';

// Layout Bileşenleri
import Navbar from './components/layout/Navbar';
import Footer from './components/layout/Footer';

// Sayfalar
import HomePage from './pages/HomePage';
import CarsPage from './pages/CarsPage';
import CarDetailPage from './pages/CarDetailPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ProfilePage from './pages/ProfilePage';
import MyRentalsPage from './pages/MyRentalsPage';
import CheckoutPage from './pages/CheckoutPage';
import ConfirmationPage from './pages/ConfirmationPage';
import NotFoundPage from './pages/NotFoundPage';

// Korumalı Rota Bileşeni
const ProtectedRoute = ({ children, requiredRoles = [] }) => {
  const { user, isAuthenticated, loading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const { error } = useNotification();

  useEffect(() => {
    if (!loading && !isAuthenticated) {
      // Giriş yapılmamışsa login sayfasına yönlendir
      navigate('/login', { 
        state: { from: location },
        replace: true 
      });
      error('Bu sayfayı görüntülemek için giriş yapmalısınız.');
    } else if (!loading && isAuthenticated && requiredRoles.length > 0) {
      // Rol kontrolü yapılıyorsa ve kullanıcının yetkisi yoksa
      const hasRequiredRole = requiredRoles.some(role => user.roles?.includes(role));
      
      if (!hasRequiredRole) {
        navigate('/', { replace: true });
        error('Bu sayfaya erişim yetkiniz bulunmuyor.');
      }
    }
  }, [isAuthenticated, loading, user, navigate, location, requiredRoles, error]);

  if (loading) {
    return (
      <Box sx={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '100vh' 
      }}>
        <CircularProgress />
      </Box>
    );
  }

  return isAuthenticated ? children : null;
};

// Uygulama Bileşeni
function App() {
  return (
    <AppProvider>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
          <Navbar />
          <Container 
            component="main" 
            maxWidth="xl"
            sx={{ 
              mt: { xs: 2, md: 4 },
              mb: 4, 
              flex: 1,
              px: { xs: 2, sm: 3 },
              transition: 'all 0.3s ease-in-out'
            }}
          >
            <Routes>
              {/* Genel Sayfalar */}
              <Route path="/" element={<HomePage />} />
              <Route path="/cars" element={<CarsPage />} />
              <Route path="/cars/:id" element={<CarDetailPage />} />
              
              {/* Kimlik Doğrulama Sayfaları */}
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              
              {/* Kullanıcı Sayfaları (Korumalı) */}
              <Route 
                path="/profile" 
                element={
                  <ProtectedRoute>
                    <ProfilePage />
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/my-rentals" 
                element={
                  <ProtectedRoute>
                    <MyRentalsPage />
                  </ProtectedRoute>
                } 
              />
              
              {/* Ödeme ve Onay Sayfaları */}
              <Route 
                path="/checkout" 
                element={
                  <ProtectedRoute>
                    <CheckoutPage />
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/confirmation/:rentalId" 
                element={
                  <ProtectedRoute>
                    <ConfirmationPage />
                  </ProtectedRoute>
                } 
              />
              
              {/* 404 Sayfası */}
              <Route path="*" element={<NotFoundPage />} />
            </Routes>
          </Container>
          <Footer />
        </Box>
      </ThemeProvider>
    </AppProvider>
  );
}

export default App;

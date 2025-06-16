import React, { useState, useEffect } from 'react';
import { Link as RouterLink, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useNotification } from '../../contexts/NotificationContext';
import { 
  AppBar, 
  Toolbar, 
  Typography, 
  Button, 
  IconButton, 
  Menu, 
  MenuItem, 
  Avatar, 
  Box,
  Container,
  useScrollTrigger,
  Slide,
  Tooltip
} from '@mui/material';
import {
  Menu as MenuIcon,
  DirectionsCar as CarIcon,
  Person as PersonIcon,
  ExitToApp as LogoutIcon,
  Notifications as NotificationsIcon,
  Search as SearchIcon
} from '@mui/icons-material';

// Scroll ile navbar'ın görünümünü değiştirmek için
function HideOnScroll({ children }) {
  const trigger = useScrollTrigger();
  return (
    <Slide appear={false} direction="down" in={!trigger}>
      {children}
    </Slide>
  );
}

const Navbar = () => {
  const { user, isAuthenticated, logout } = useAuth();
  const { showNotification } = useNotification();
  const navigate = useNavigate();
  const location = useLocation();
  
  const [anchorEl, setAnchorEl] = useState(null);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [scrolled, setScrolled] = useState(false);

  // Kullanıcı menüsünü yönet
  const handleMenuOpen = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  // Çıkış yap
  const handleLogout = async () => {
    try {
      await logout();
      showNotification('Başarıyla çıkış yapıldı', 'success');
      navigate('/');
    } catch (error) {
      console.error('Çıkış yapılırken hata oluştu:', error);
      showNotification('Çıkış yapılırken bir hata oluştu', 'error');
    }
    handleMenuClose();
  };

  // Scroll durumunu takip et
  useEffect(() => {
    const handleScroll = () => {
      const isScrolled = window.scrollY > 10;
      if (isScrolled !== scrolled) {
        setScrolled(isScrolled);
      }
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, [scrolled]);

  // Sayfa değiştiğinde mobil menüyü kapat
  useEffect(() => {
    setMobileMenuOpen(false);
  }, [location]);

  // Kullanıcı adının baş harflerini al
  const getInitials = (name) => {
    if (!name) return 'K';
    return name
      .split(' ')
      .map(part => part[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  };

  return (
    <>
      <HideOnScroll>
        <AppBar 
          position="fixed" 
          elevation={scrolled ? 4 : 0}
          sx={{
            backgroundColor: scrolled ? 'background.paper' : 'transparent',
            backdropFilter: scrolled ? 'blur(10px)' : 'none',
            transition: 'all 0.3s ease-in-out',
            boxShadow: 'none',
            borderBottom: scrolled ? '1px solid' : 'none',
            borderColor: 'divider',
          }}
        >
          <Container maxWidth="xl">
            <Toolbar disableGutters sx={{ minHeight: 70 }}>
              {/* Logo */}
              <Box 
                component={RouterLink} 
                to="/" 
                sx={{ 
                  display: 'flex', 
                  alignItems: 'center', 
                  textDecoration: 'none',
                  mr: 3
                }}
              >
                <CarIcon sx={{ 
                  display: { xs: 'none', md: 'flex' }, 
                  mr: 1, 
                  color: scrolled ? 'primary.main' : 'white',
                  fontSize: 30
                }} />
                <Typography
                  variant="h6"
                  noWrap
                  sx={{
                    fontWeight: 700,
                    color: scrolled ? 'text.primary' : 'white',
                    textDecoration: 'none',
                    display: { xs: 'none', sm: 'block' },
                  }}
                >
                  MERSİN ARAÇ KİRALAMA
                </Typography>
              </Box>

              {/* Masaüstü Navigasyon */}
              <Box sx={{ 
                flexGrow: 1, 
                display: { xs: 'none', md: 'flex' },
                ml: 4,
                gap: 1
              }}>
                <Button
                  component={RouterLink}
                  to="/araclar"
                  sx={{ 
                    my: 2, 
                    color: scrolled ? 'text.primary' : 'white',
                    '&:hover': {
                      backgroundColor: scrolled ? 'action.hover' : 'rgba(255, 255, 255, 0.1)',
                    },
                    px: 2,
                    borderRadius: 1,
                  }}
                >
                  Araçlar
                </Button>
                <Button
                  component={RouterLink}
                  to="/fiyatlar"
                  sx={{ 
                    my: 2, 
                    color: scrolled ? 'text.primary' : 'white',
                    '&:hover': {
                      backgroundColor: scrolled ? 'action.hover' : 'rgba(255, 255, 255, 0.1)',
                    },
                    px: 2,
                    borderRadius: 1,
                  }}
                >
                  Fiyatlar
                </Button>
                <Button
                  component={RouterLink}
                  to="/nasil-calisir"
                  sx={{ 
                    my: 2, 
                    color: scrolled ? 'text.primary' : 'white',
                    '&:hover': {
                      backgroundColor: scrolled ? 'action.hover' : 'rgba(255, 255, 255, 0.1)',
                    },
                    px: 2,
                    borderRadius: 1,
                  }}
                >
                  Nasıl Çalışır?
                </Button>
              </Box>

              {/* Kullanıcı İşlemleri */}
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                {isAuthenticated ? (
                  <>
                    <Tooltip title="Bildirimler">
                      <IconButton 
                        size="large"
                        sx={{ 
                          color: scrolled ? 'text.primary' : 'white',
                        }}
                      >
                        <Badge badgeContent={3} color="error">
                          <NotificationsIcon />
                        </Badge>
                      </IconButton>
                    </Tooltip>
                    
                    <Tooltip title="Hesap ayarları">
                      <IconButton
                        onClick={handleMenuOpen}
                        size="small"
                        sx={{ ml: 1 }}
                      >
                        <Avatar 
                          alt={user?.fullName || 'Kullanıcı'} 
                          sx={{ 
                            width: 36, 
                            height: 36,
                            bgcolor: scrolled ? 'primary.main' : 'white',
                            color: scrolled ? 'white' : 'primary.main',
                            fontWeight: 'bold',
                          }}
                        >
                          {getInitials(user?.fullName || 'K')}
                        </Avatar>
                      </IconButton>
                    </Tooltip>
                    
                    <Menu
                      anchorEl={anchorEl}
                      open={Boolean(anchorEl)}
                      onClose={handleMenuClose}
                      onClick={handleMenuClose}
                      PaperProps={{
                        elevation: 3,
                        sx: {
                          mt: 1.5,
                          minWidth: 200,
                          '& .MuiAvatar-root': {
                            width: 32,
                            height: 32,
                            ml: -0.5,
                            mr: 1,
                          },
                        },
                      }}
                      transformOrigin={{ horizontal: 'right', vertical: 'top' }}
                      anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
                    >
                      <Box sx={{ px: 2, py: 1 }}>
                        <Typography variant="subtitle2" fontWeight={600}>
                          {user?.fullName || 'Kullanıcı'}
                        </Typography>
                        <Typography variant="body2" color="text.secondary" noWrap>
                          {user?.email || ''}
                        </Typography>
                      </Box>
                      <Divider sx={{ my: 1 }} />
                      
                      <MenuItem component={RouterLink} to="/profil">
                        <PersonIcon fontSize="small" sx={{ mr: 1 }} />
                        Profilim
                      </MenuItem>
                      
                      <MenuItem component={RouterLink} to="/rezervasyonlarim">
                        <DashboardIcon fontSize="small" sx={{ mr: 1 }} />
                        Rezervasyonlarım
                      </MenuItem>
                      
                      <Divider />
                      
                      <MenuItem onClick={handleLogout}>
                        <LogoutIcon fontSize="small" sx={{ mr: 1 }} />
                        Çıkış Yap
                      </MenuItem>
                    </Menu>
                  </>
                ) : (
                  <>
                    <Button
                      component={RouterLink}
                      to="/giris"
                      variant={scrolled ? "outlined" : "text"}
                      color={scrolled ? "primary" : "inherit"}
                      sx={{
                        color: scrolled ? 'primary.main' : 'white',
                        borderColor: scrolled ? 'primary.main' : 'white',
                        '&:hover': {
                          backgroundColor: scrolled ? 'action.hover' : 'rgba(255, 255, 255, 0.1)',
                        },
                      }}
                    >
                      Giriş Yap
                    </Button>
                    <Button
                      component={RouterLink}
                      to="/kayit"
                      variant="contained"
                      color="primary"
                      sx={{
                        ml: 1,
                        boxShadow: 'none',
                        '&:hover': {
                          boxShadow: 'none',
                        },
                      }}
                    >
                      Kayıt Ol
                    </Button>
                  </>
                )}
                
                {/* Mobil Menü Butonu */}
                <IconButton
                  size="large"
                  aria-label="menüyü göster"
                  onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
                  sx={{ 
                    color: scrolled ? 'text.primary' : 'white',
                    display: { md: 'none' },
                  }}
                >
                  {mobileMenuOpen ? <MenuIcon /> : <MenuIcon />}
                </IconButton>
              </Box>
            </Toolbar>
          </Container>
          
          {/* Mobil Menü */}
          <Box
            sx={{
              display: { xs: mobileMenuOpen ? 'block' : 'none', md: 'none' },
              pb: 2,
              px: 2,
              backgroundColor: 'background.paper',
              borderTop: '1px solid',
              borderColor: 'divider',
            }}
          >
            <Button
              fullWidth
              component={RouterLink}
              to="/araclar"
              startIcon={<CarIcon />}
              sx={{
                justifyContent: 'flex-start',
                color: 'text.primary',
                mb: 1,
              }}
            >
              Araçlar
            </Button>
            <Button
              fullWidth
              component={RouterLink}
              to="/fiyatlar"
              startIcon={<CarIcon />}
              sx={{
                justifyContent: 'flex-start',
                color: 'text.primary',
                mb: 1,
              }}
            >
              Fiyatlar
            </Button>
            <Button
              fullWidth
              component={RouterLink}
              to="/nasil-calisir"
              startIcon={<CarIcon />}
              sx={{
                justifyContent: 'flex-start',
                color: 'text.primary',
                mb: 1,
              }}
            >
              Nasıl Çalışır?
            </Button>
            
            {isAuthenticated ? (
              <>
                <Divider sx={{ my: 1 }} />
                <Button
                  fullWidth
                  component={RouterLink}
                  to="/profil"
                  startIcon={<PersonIcon />}
                  sx={{
                    justifyContent: 'flex-start',
                    color: 'text.primary',
                    mb: 1,
                  }}
                >
                  Profilim
                </Button>
                <Button
                  fullWidth
                  onClick={handleLogout}
                  startIcon={<LogoutIcon />}
                  sx={{
                    justifyContent: 'flex-start',
                    color: 'error.main',
                  }}
                >
                  Çıkış Yap
                </Button>
              </>
            ) : (
              <>
                <Divider sx={{ my: 1 }} />
                <Button
                  fullWidth
                  component={RouterLink}
                  to="/giris"
                  variant="outlined"
                  sx={{
                    justifyContent: 'center',
                    mb: 1,
                  }}
                >
                  Giriş Yap
                </Button>
                <Button
                  fullWidth
                  component={RouterLink}
                  to="/kayit"
                  variant="contained"
                  sx={{
                    justifyContent: 'center',
                  }}
                >
                  Kayıt Ol
                </Button>
              </>
            )}
          </Box>
        </AppBar>
      </HideOnScroll>
      
      {/* Sayfa içeriğini navbar'ın altına itmek için boşluk ekle */}
      <Toolbar />
    </>
  );
};

export default Navbar;

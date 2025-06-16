import React from 'react';
import { Box, Container, Grid, Typography, Link, Divider, IconButton } from '@mui/material';
import { Facebook, Twitter, Instagram, LinkedIn, YouTube } from '@mui/icons-material';
import { Link as RouterLink } from 'react-router-dom';

const Footer = () => {
  const currentYear = new Date().getFullYear();

  // Footer linkleri
  const companyLinks = [
    { text: 'Hakkımızda', to: '/about' },
    { text: 'Ekibimiz', to: '/team' },
    { text: 'Kariyer', to: '/careers' },
    { text: 'Basın', to: '/press' },
    { text: 'Blog', to: '/blog' },
  ];

  const supportLinks = [
    { text: 'Sıkça Sorulan Sorular', to: '/faq' },
    { text: 'Yardım Merkezi', to: '/help' },
    { text: 'İletişim', to: '/contact' },
    { text: 'Gizlilik Politikası', to: '/privacy' },
    { text: 'Kullanım Koşulları', to: '/terms' },
  ];

  const contactInfo = {
    address: 'Mersin, Türkiye',
    phone: '+90 555 123 45 67',
    email: 'info@mersinarackirala.com',
  };

  const socialLinks = [
    { icon: <Facebook />, url: 'https://facebook.com' },
    { icon: <Twitter />, url: 'https://twitter.com' },
    { icon: <Instagram />, url: 'https://instagram.com' },
    { icon: <LinkedIn />, url: 'https://linkedin.com' },
    { icon: <YouTube />, url: 'https://youtube.com' },
  ];

  return (
    <Box 
      component="footer" 
      sx={{ 
        backgroundColor: 'background.paper',
        borderTop: '1px solid',
        borderColor: 'divider',
        py: 6,
        mt: 'auto'
      }}
    >
      <Container maxWidth="lg">
        <Grid container spacing={4}>
          {/* Şirket Bilgileri */}
          <Grid item xs={12} md={4}>
            <Typography variant="h6" color="primary" gutterBottom>
              Mersin Araç Kiralama
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Mersin'in en güvenilir ve uygun fiyatlı araç kiralama hizmeti ile yanınızdayız.
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
              {socialLinks.map((social, index) => (
                <IconButton 
                  key={index} 
                  component="a" 
                  href={social.url} 
                  target="_blank" 
                  rel="noopener noreferrer"
                  color="primary"
                  sx={{ 
                    border: '1px solid', 
                    borderColor: 'divider',
                    '&:hover': {
                      backgroundColor: 'action.hover',
                    }
                  }}
                >
                  {social.icon}
                </IconButton>
              ))}
            </Box>
          </Grid>

          {/* Hızlı Linkler */}
          <Grid item xs={6} md={2}>
            <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
              Şirket
            </Typography>
            <Box component="nav" sx={{ display: 'flex', flexDirection: 'column' }}>
              {companyLinks.map((link, index) => (
                <Link
                  key={index}
                  component={RouterLink}
                  to={link.to}
                  variant="body2"
                  color="text.secondary"
                  sx={{
                    mb: 1,
                    textDecoration: 'none',
                    '&:hover': {
                      color: 'primary.main',
                      textDecoration: 'underline',
                    },
                  }}
                >
                  {link.text}
                </Link>
              ))}
            </Box>
          </Grid>

          {/* Destek */}
          <Grid item xs={6} md={2}>
            <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
              Destek
            </Typography>
            <Box component="nav" sx={{ display: 'flex', flexDirection: 'column' }}>
              {supportLinks.map((link, index) => (
                <Link
                  key={index}
                  component={RouterLink}
                  to={link.to}
                  variant="body2"
                  color="text.secondary"
                  sx={{
                    mb: 1,
                    textDecoration: 'none',
                    '&:hover': {
                      color: 'primary.main',
                      textDecoration: 'underline',
                    },
                  }}
                >
                  {link.text}
                </Link>
              ))}
            </Box>
          </Grid>

          {/* İletişim Bilgileri */}
          <Grid item xs={12} md={4}>
            <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
              İletişim
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
              <Typography variant="body2" color="text.secondary">
                {contactInfo.address}
              </Typography>
              <Link 
                href={`tel:${contactInfo.phone}`} 
                variant="body2" 
                color="text.secondary"
                sx={{
                  textDecoration: 'none',
                  '&:hover': {
                    color: 'primary.main',
                    textDecoration: 'underline',
                  },
                }}
              >
                {contactInfo.phone}
              </Link>
              <Link 
                href={`mailto:${contactInfo.email}`}
                variant="body2" 
                color="text.secondary"
                sx={{
                  textDecoration: 'none',
                  '&:hover': {
                    color: 'primary.main',
                    textDecoration: 'underline',
                  },
                }}
              >
                {contactInfo.email}
              </Link>
            </Box>
          </Grid>
        </Grid>

        <Divider sx={{ my: 4 }} />

        <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography variant="body2" color="text.secondary" align="center">
            © {currentYear} Mersin Araç Kiralama. Tüm hakları saklıdır.
          </Typography>
          
          <Box sx={{ display: 'flex', gap: 2, mt: { xs: 2, sm: 0 } }}>
            <Link 
              component={RouterLink} 
              to="/privacy" 
              variant="body2" 
              color="text.secondary"
              sx={{
                textDecoration: 'none',
                '&:hover': {
                  color: 'primary.main',
                  textDecoration: 'underline',
                },
              }}
            >
              Gizlilik Politikası
            </Link>
            <Link 
              component={RouterLink} 
              to="/terms" 
              variant="body2" 
              color="text.secondary"
              sx={{
                textDecoration: 'none',
                '&:hover': {
                  color: 'primary.main',
                  textDecoration: 'underline',
                },
              }}
            >
              Kullanım Koşulları
            </Link>
            <Link 
              component={RouterLink} 
              to="/cookies" 
              variant="body2" 
              color="text.secondary"
              sx={{
                textDecoration: 'none',
                '&:hover': {
                  color: 'primary.main',
                  textDecoration: 'underline',
                },
              }}
            >
              Çerez Politikası
            </Link>
          </Box>
        </Box>
      </Container>
    </Box>
  );
};

export default Footer;

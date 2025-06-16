import React from 'react';
import { Box, Button, Container, Typography, useTheme } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import { styled } from '@mui/material/styles';

const HeroSection = styled(Box)(({ theme }) => ({
  background: `linear-gradient(rgba(0, 0, 0, 0.7), rgba(0, 0, 0, 0.7)), 
    url('https://images.unsplash.com/photo-1486262715619-67b85e0b08d3?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1000&q=80')`,
  backgroundSize: 'cover',
  backgroundPosition: 'center',
  color: '#fff',
  padding: theme.spacing(15, 0),
  textAlign: 'center',
  marginTop: -64, // Navbar yüksekliği kadar yukarı çek
}));

const FeatureBox = styled(Box)(({ theme }) => ({
  padding: theme.spacing(4),
  textAlign: 'center',
  borderRadius: theme.shape.borderRadius,
  backgroundColor: theme.palette.background.paper,
  boxShadow: theme.shadows[2],
  height: '100%',
}));

const features = [
  {
    title: 'Geniş Araç Yelpazesi',
    description: 'Ekonomik otomobillerden lüks araçlara kadar geniş araç yelpazesi.',
    icon: '🚗',
  },
  {
    title: 'Uygun Fiyat Garantisi',
    description: 'En uygun fiyat garantisi ve özel kampanyalar.',
    icon: '💰',
  },
  {
    title: '7/24 Müşteri Desteği',
    description: 'Kesintisiz müşteri hizmetleri ve yol yardımı.',
    icon: '📞',
  },
];

const HomePage = () => {
  const theme = useTheme();

  return (
    <Box>
      <HeroSection>
        <Container maxWidth="md">
          <Typography variant="h2" component="h1" gutterBottom sx={{ fontWeight: 'bold' }}>
            Mersin'de En Uygun Araç Kiralama
          </Typography>
          <Typography variant="h5" paragraph sx={{ mb: 4 }}>
            En iyi fiyat garantisiyle hemen aracınızı kiralayın
          </Typography>
          <Button
            variant="contained"
            color="primary"
            size="large"
            component={RouterLink}
            to="/cars"
            sx={{ 
              py: 1.5, 
              px: 4, 
              fontSize: '1.1rem',
              textTransform: 'none',
              fontWeight: 'bold'
            }}
          >
            Hemen Kirala
          </Button>
        </Container>
      </HeroSection>

      <Container maxWidth="lg" sx={{ py: 8 }}>
        <Typography variant="h4" component="h2" align="center" gutterBottom sx={{ fontWeight: 'bold', mb: 6 }}>
          Neden Bizi Tercih Etmelisiniz?
        </Typography>
        
        <Box sx={{ 
          display: 'grid', 
          gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', md: 'repeat(3, 1fr)' },
          gap: 4,
          mt: 4 
        }}>
          {features.map((feature, index) => (
            <FeatureBox key={index}>
              <Typography variant="h3" sx={{ mb: 2 }}>{feature.icon}</Typography>
              <Typography variant="h6" component="h3" gutterBottom sx={{ fontWeight: 'bold' }}>
                {feature.title}
              </Typography>
              <Typography variant="body1" color="text.secondary">
                {feature.description}
              </Typography>
            </FeatureBox>
          ))}
        </Box>
      </Container>

      <Box sx={{ 
        backgroundColor: theme.palette.grey[100],
        py: 8,
        mt: 8
      }}>
        <Container maxWidth="md" sx={{ textAlign: 'center' }}>
          <Typography variant="h4" component="h2" gutterBottom sx={{ fontWeight: 'bold' }}>
            Hemen Üye Olun
          </Typography>
          <Typography variant="body1" color="text.secondary" paragraph sx={{ mb: 4, maxWidth: '700px', mx: 'auto' }}>
            Hemen üye olun, özel fırsatlardan yararlanın ve araç kiralamanın keyfini çıkarın.
          </Typography>
          <Button 
            variant="contained" 
            color="primary" 
            size="large"
            component={RouterLink}
            to="/register"
            sx={{ 
              py: 1.5, 
              px: 4, 
              fontSize: '1.1rem',
              textTransform: 'none',
              fontWeight: 'bold'
            }}
          >
            Ücretsiz Üye Ol
          </Button>
        </Container>
      </Box>
    </Box>
  );
};

export default HomePage;

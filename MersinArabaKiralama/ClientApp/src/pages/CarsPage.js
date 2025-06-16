import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Grid,
  Card,
  CardMedia,
  CardContent,
  Typography,
  Button,
  Box,
  TextField,
  InputAdornment,
  MenuItem,
  FormControl,
  InputLabel,
  Select,
  Slider,
  Chip,
  Pagination,
  useTheme,
  Paper,
  Skeleton
} from '@mui/material';
import {
  Search as SearchIcon,
  DirectionsCar as CarIcon,
  Person as PersonIcon,
  AcUnit as AcIcon,
  DirectionsCarFilled as TransmissionIcon,
  LocalGasStation as FuelIcon,
  Star as StarIcon,
  StarBorder as StarBorderIcon,
  FilterList as FilterIcon
} from '@mui/icons-material';

// Örnek araç verileri (API'den çekilecek)
const sampleCars = [
  {
    id: 1,
    brand: 'BMW',
    model: '320i',
    year: 2022,
    price: 2500,
    image: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1000&q=80',
    rating: 4.8,
    reviews: 124,
    transmission: 'Otomatik',
    fuel: 'Benzin',
    seats: 5,
    ac: true,
    features: ['Bluetooth', 'Geri Görüş Kamerası', 'Park Sensörü']
  },
  // Diğer araçlar...
];

// Filtre seçenekleri
const brands = ['Tüm Markalar', 'BMW', 'Mercedes', 'Audi', 'Volkswagen', 'Renault', 'Fiat'];
const fuelTypes = ['Tüm Yakıtlar', 'Benzin', 'Dizel', 'Hibrit', 'Elektrikli'];
const transmissionTypes = ['Tüm Vitesler', 'Otomatik', 'Manuel'];

const CarsPage = () => {
  const [cars, setCars] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState({
    search: '',
    brand: 'Tüm Markalar',
    fuelType: 'Tüm Yakıtlar',
    transmission: 'Tüm Vitesler',
    priceRange: [0, 5000],
    sortBy: 'popularity'
  });
  const [page, setPage] = useState(1);
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);
  const theme = useTheme();
  const navigate = useNavigate();

  // API'den araçları çek
  useEffect(() => {
    const fetchCars = async () => {
      try {
        // Simüle edilmiş API çağrısı
        await new Promise(resolve => setTimeout(resolve, 1000));
        setCars(sampleCars);
      } catch (error) {
        console.error('Araçlar yüklenirken hata oluştu:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchCars();
  }, []);

  const handleFilterChange = (event) => {
    const { name, value } = event.target;
    setFilters(prev => ({
      ...prev,
      [name]: value
    }));
    setPage(1); // Filtre değiştiğinde sayfayı sıfırla
  };

  const handlePriceChange = (event, newValue) => {
    setFilters(prev => ({
      ...prev,
      priceRange: newValue
    }));
  };

  const handleRentNow = (carId) => {
    navigate(`/cars/${carId}/rent`);
  };

  const handlePageChange = (event, value) => {
    setPage(value);
    // Burada API'den yeni sayfa verilerini çekebilirsiniz
  };

  // Filtrelenmiş ve sıralanmış araçlar
  const filteredCars = cars.filter(car => {
    return (
      (filters.brand === 'Tüm Markalar' || car.brand === filters.brand) &&
      (filters.fuelType === 'Tüm Yakıtlar' || car.fuel === filters.fuelType) &&
      (filters.transmission === 'Tüm Vitesler' || car.transmission === filters.transmission) &&
      (car.price >= filters.priceRange[0] && car.price <= filters.priceRange[1]) &&
      (car.brand.toLowerCase().includes(filters.search.toLowerCase()) ||
       car.model.toLowerCase().includes(filters.search.toLowerCase()))
    );
  });

  // Sayfalama
  const itemsPerPage = 6;
  const pageCount = Math.ceil(filteredCars.length / itemsPerPage);
  const paginatedCars = filteredCars.slice(
    (page - 1) * itemsPerPage,
    page * itemsPerPage
  );

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" sx={{ fontWeight: 'bold', mb: 2 }}>
          Kiralık Araçlar
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Size uygun aracı bulun ve hemen kiralamaya başlayın
        </Typography>
      </Box>

      <Grid container spacing={3}>
        {/* Filtreler - Masaüstü */}
        <Grid item xs={12} md={3} sx={{ display: { xs: 'none', md: 'block' } }}>
          <Paper elevation={2} sx={{ p: 3, borderRadius: 2, position: 'sticky', top: 20 }}>
            <Typography variant="h6" sx={{ mb: 2, fontWeight: 'bold' }}>
              <FilterIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
              Filtreler
            </Typography>
            
            <Box sx={{ mb: 3 }}>
              <Typography gutterBottom>Marka</Typography>
              <FormControl fullWidth size="small">
                <Select
                  name="brand"
                  value={filters.brand}
                  onChange={handleFilterChange}
                  displayEmpty
                >
                  {brands.map((brand) => (
                    <MenuItem key={brand} value={brand}>
                      {brand}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>

            <Box sx={{ mb: 3 }}>
              <Typography gutterBottom>Yakıt Türü</Typography>
              <FormControl fullWidth size="small">
                <Select
                  name="fuelType"
                  value={filters.fuelType}
                  onChange={handleFilterChange}
                  displayEmpty
                >
                  {fuelTypes.map((type) => (
                    <MenuItem key={type} value={type}>
                      {type}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>

            <Box sx={{ mb: 3 }}>
              <Typography gutterBottom>Vites Türü</Typography>
              <FormControl fullWidth size="small">
                <Select
                  name="transmission"
                  value={filters.transmission}
                  onChange={handleFilterChange}
                  displayEmpty
                >
                  {transmissionTypes.map((type) => (
                    <MenuItem key={type} value={type}>
                      {type}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>

            <Box sx={{ mb: 3 }}>
              <Typography gutterBottom>
                Günlük Fiyat Aralığı (₺)
              </Typography>
              <Slider
                value={filters.priceRange}
                onChange={handlePriceChange}
                valueLabelDisplay="auto"
                min={0}
                max={5000}
                step={100}
                valueLabelFormat={(value) => `₺${value}`}
                sx={{ mt: 3 }}
              />
              <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                <Typography variant="body2" color="text.secondary">
                  ₺{filters.priceRange[0]}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  ₺{filters.priceRange[1]}
                </Typography>
              </Box>
            </Box>

            <Button
              fullWidth
              variant="outlined"
              onClick={() => {
                setFilters({
                  search: '',
                  brand: 'Tüm Markalar',
                  fuelType: 'Tüm Yakıtlar',
                  transmission: 'Tüm Vitesler',
                  priceRange: [0, 5000],
                  sortBy: 'popularity'
                });
              }}
            >
              Filtreleri Sıfırla
            </Button>
          </Paper>
        </Grid>

        {/* Araç Listesi */}
        <Grid item xs={12} md={9}>
          <Box sx={{ mb: 3, display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, gap: 2 }}>
            <TextField
              fullWidth
              variant="outlined"
              placeholder="Araç ara..."
              name="search"
              value={filters.search}
              onChange={handleFilterChange}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon />
                  </InputAdornment>
                ),
              }}
              sx={{
                flexGrow: 1,
                '& .MuiOutlinedInput-root': {
                  borderRadius: 2,
                },
              }}
            />
            <FormControl sx={{ minWidth: 200 }} size="small">
              <InputLabel>Sırala</InputLabel>
              <Select
                name="sortBy"
                value={filters.sortBy}
                onChange={handleFilterChange}
                label="Sırala"
                sx={{ borderRadius: 2 }}
              >
                <MenuItem value="popularity">Popülerlik</MenuItem>
                <MenuItem value="price-asc">Fiyat (Düşükten Yükseğe)</MenuItem>
                <MenuItem value="price-desc">Fiyat (Yüksekten Düşüğe)</MenuItem>
                <MenuItem value="year-desc">Yeniden Eskiye</MenuItem>
              </Select>
            </FormControl>
            
            {/* Mobil Filtre Butonu */}
            <Button
              variant="outlined"
              startIcon={<FilterIcon />}
              onClick={() => setMobileFiltersOpen(true)}
              sx={{
                display: { xs: 'flex', md: 'none' },
                borderRadius: 2,
                py: 1.5,
              }}
            >
              Filtreler
            </Button>
          </Box>

          {loading ? (
            <Grid container spacing={3}>
              {[...Array(6)].map((_, index) => (
                <Grid item xs={12} sm={6} lg={4} key={index}>
                  <Skeleton variant="rectangular" height={200} sx={{ borderRadius: 2, mb: 1 }} />
                  <Skeleton variant="text" width="60%" />
                  <Skeleton variant="text" width="40%" />
                  <Skeleton variant="text" width="80%" />
                </Grid>
              ))}
            </Grid>
          ) : filteredCars.length > 0 ? (
            <>
              <Grid container spacing={3}>
                {paginatedCars.map((car) => (
                  <Grid item xs={12} sm={6} lg={4} key={car.id}>
                    <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column', borderRadius: 2, overflow: 'hidden', transition: 'transform 0.3s, box-shadow 0.3s', '&:hover': { transform: 'translateY(-4px)', boxShadow: theme.shadows[6] } }}>
                      <Box sx={{ position: 'relative' }}>
                        <CardMedia
                          component="img"
                          height="180"
                          image={car.image}
                          alt={`${car.brand} ${car.model}`}
                        />
                        <Box sx={{ position: 'absolute', top: 10, right: 10, display: 'flex', alignItems: 'center', backgroundColor: 'rgba(0, 0, 0, 0.7)', color: 'white', px: 1, py: 0.5, borderRadius: 1 }}>
                          <StarIcon sx={{ color: '#ffc107', fontSize: '1rem', mr: 0.5 }} />
                          <Typography variant="body2">
                            {car.rating} ({car.reviews})
                          </Typography>
                        </Box>
                      </Box>
                      <CardContent sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                          <Typography variant="h6" component="h2" sx={{ fontWeight: 'bold' }}>
                            {car.brand} {car.model}
                          </Typography>
                          <Typography variant="h6" color="primary" sx={{ fontWeight: 'bold', whiteSpace: 'nowrap', ml: 1 }}>
                            ₺{car.price.toLocaleString('tr-TR')}/gün
                          </Typography>
                        </Box>
                        
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                          {car.year} Model • {car.transmission} • {car.fuel}
                        </Typography>
                        
                        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
                          <Chip 
                            icon={<PersonIcon fontSize="small" />} 
                            label={`${car.seats} Kişi`} 
                            size="small" 
                            variant="outlined"
                          />
                          {car.ac && (
                            <Chip 
                              icon={<AcIcon fontSize="small" />} 
                              label="Klima" 
                              size="small" 
                              variant="outlined"
                            />
                          )}
                          <Chip 
                            icon={<TransmissionIcon fontSize="small" />} 
                            label={car.transmission} 
                            size="small" 
                            variant="outlined"
                          />
                          <Chip 
                            icon={<FuelIcon fontSize="small" />} 
                            label={car.fuel} 
                            size="small" 
                            variant="outlined"
                          />
                        </Box>
                        
                        <Box sx={{ mt: 'auto', pt: 2 }}>
                          <Button 
                            fullWidth 
                            variant="contained"
                            onClick={() => handleRentNow(car.id)}
                            sx={{ py: 1, borderRadius: 2, textTransform: 'none', fontWeight: 'bold' }}
                          >
                            Hemen Kirala
                          </Button>
                        </Box>
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
              </Grid>
              
              {/* Sayfalama */}
              {pageCount > 1 && (
                <Box sx={{ mt: 4, display: 'flex', justifyContent: 'center' }}>
                  <Pagination 
                    count={pageCount} 
                    page={page} 
                    onChange={handlePageChange} 
                    color="primary" 
                    size="large"
                    showFirstButton 
                    showLastButton
                    sx={{ '& .MuiPaginationItem-root': { borderRadius: 2 } }}
                  />
                </Box>
              )}
            </>
          ) : (
            <Box sx={{ textAlign: 'center', py: 8 }}>
              <Typography variant="h6" color="text.secondary" sx={{ mb: 2 }}>
                Aradığınız kriterlere uygun araç bulunamadı.
              </Typography>
              <Button 
                variant="outlined" 
                onClick={() => {
                  setFilters({
                    search: '',
                    brand: 'Tüm Markalar',
                    fuelType: 'Tüm Yakıtlar',
                    transmission: 'Tüm Vitesler',
                    priceRange: [0, 5000],
                    sortBy: 'popularity'
                  });
                }}
              >
                Filtreleri Sıfırla
              </Button>
            </Box>
          )}
        </Grid>
      </Grid>

      {/* Mobil Filtre Drawer */}
      <Drawer
        anchor="right"
        open={mobileFiltersOpen}
        onClose={() => setMobileFiltersOpen(false)}
        sx={{ '& .MuiDrawer-paper': { width: '85%', maxWidth: 350, p: 2 } }}
      >
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h6" sx={{ fontWeight: 'bold' }}>
            Filtreler
          </Typography>
          <IconButton onClick={() => setMobileFiltersOpen(false)}>
            <CloseIcon />
          </IconButton>
        </Box>
        
        <Divider sx={{ my: 2 }} />
        
        <Box sx={{ mb: 3 }}>
          <Typography gutterBottom>Marka</Typography>
          <FormControl fullWidth size="small">
            <Select
              name="brand"
              value={filters.brand}
              onChange={handleFilterChange}
              displayEmpty
            >
              {brands.map((brand) => (
                <MenuItem key={brand} value={brand}>
                  {brand}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>

        <Box sx={{ mb: 3 }}>
          <Typography gutterBottom>Yakıt Türü</Typography>
          <FormControl fullWidth size="small">
            <Select
              name="fuelType"
              value={filters.fuelType}
              onChange={handleFilterChange}
              displayEmpty
            >
              {fuelTypes.map((type) => (
                <MenuItem key={type} value={type}>
                  {type}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>

        <Box sx={{ mb: 3 }}>
          <Typography gutterBottom>Vites Türü</Typography>
          <FormControl fullWidth size="small">
            <Select
              name="transmission"
              value={filters.transmission}
              onChange={handleFilterChange}
              displayEmpty
            >
              {transmissionTypes.map((type) => (
                <MenuItem key={type} value={type}>
                  {type}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>

        <Box sx={{ mb: 3 }}>
          <Typography gutterBottom>
            Günlük Fiyat Aralığı (₺)
          </Typography>
          <Slider
            value={filters.priceRange}
            onChange={handlePriceChange}
            valueLabelDisplay="auto"
            min={0}
            max={5000}
            step={100}
            valueLabelFormat={(value) => `₺${value}`}
            sx={{ mt: 3 }}
          />
          <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
            <Typography variant="body2" color="text.secondary">
              ₺{filters.priceRange[0]}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              ₺{filters.priceRange[1]}
            </Typography>
          </Box>
        </Box>

        <Button
          fullWidth
          variant="contained"
          onClick={() => setMobileFiltersOpen(false)}
          sx={{ mt: 2 }}
        >
          Filtreleri Uygula
        </Button>
        
        <Button
          fullWidth
          variant="outlined"
          onClick={() => {
            setFilters({
              search: '',
              brand: 'Tüm Markalar',
              fuelType: 'Tüm Yakıtlar',
              transmission: 'Tüm Vitesler',
              priceRange: [0, 5000],
              sortBy: 'popularity'
            });
          }}
          sx={{ mt: 1 }}
        >
          Filtreleri Sıfırla
        </Button>
      </Drawer>
    </Container>
  );
};

export default CarsPage;

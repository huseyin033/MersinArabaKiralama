import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useRental } from '../../contexts/RentalContext';
import { useNotification } from '../../contexts/NotificationContext';
import {
  Container,
  Paper,
  Typography,
  Box,
  Button,
  Grid,
  Divider,
  Card,
  CardContent,
  CardMedia,
  CardActions,
  Chip,
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  CircularProgress,
  Alert,
  Badge
} from '@mui/material';
import {
  Event as EventIcon,
  DirectionsCar as CarIcon,
  AccessTime as TimeIcon,
  AttachMoney as MoneyIcon,
  Receipt as ReceiptIcon,
  Cancel as CancelIcon,
  Edit as EditIcon,
  Print as PrintIcon,
  Star as StarIcon,
  StarBorder as StarBorderIcon,
  RateReview as RateReviewIcon,
  CheckCircle as CheckCircleIcon,
  Close as CloseIcon,
  ArrowBack as ArrowBackIcon
} from '@mui/icons-material';
import { format, differenceInDays, parseISO } from 'date-fns';
import { tr } from 'date-fns/locale';

const ReservationsPage = () => {
  const { user } = useAuth();
  const { getReservations, cancelReservation } = useRental();
  const { showNotification } = useNotification();
  const navigate = useNavigate();
  
  const [reservations, setReservations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState(0);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(5);
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false);
  const [selectedReservation, setSelectedReservation] = useState(null);
  const [canceling, setCanceling] = useState(false);
  
  // Filtrelenmiş rezervasyonlar
  const filteredReservations = reservations.filter(reservation => {
    const now = new Date();
    const startDate = new Date(reservation.startDate);
    const endDate = new Date(reservation.endDate);
    
    switch(activeTab) {
      case 0: // Tümü
        return true;
      case 1: // Yaklaşan
        return startDate > now;
      case 2: // Devam Eden
        return startDate <= now && endDate >= now;
      case 3: // Geçmiş
        return endDate < now;
      case 4: // İptal Edilen
        return reservation.status === 'cancelled';
      default:
        return true;
    }
  });

  // Sayfalama için görüntülenecek rezervasyonlar
  const paginatedReservations = filteredReservations.slice(
    page * rowsPerPage,
    page * rowsPerPage + rowsPerPage
  );
  
  useEffect(() => {
    const fetchReservations = async () => {
      try {
        setLoading(true);
        const data = await getReservations();
        setReservations(data);
      } catch (err) {
        console.error('Rezervasyonlar yüklenirken hata oluştu:', err);
        setError('Rezervasyonlar yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.');
      } finally {
        setLoading(false);
      }
    };
    
    if (user) {
      fetchReservations();
    }
  }, [user]);
  
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
    setPage(0); // Sekme değiştiğinde sayfayı sıfırla
  };
  
  const handleChangePage = (event, newPage) => {
    setPage(newPage);
  };
  
  const handleChangeRowsPerPage = (event) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };
  
  const handleOpenCancelDialog = (reservation) => {
    setSelectedReservation(reservation);
    setCancelDialogOpen(true);
  };
  
  const handleCloseCancelDialog = () => {
    setCancelDialogOpen(false);
    setSelectedReservation(null);
  };
  
  const handleCancelReservation = async () => {
    if (!selectedReservation) return;
    
    try {
      setCanceling(true);
      await cancelReservation(selectedReservation.id);
      
      // Rezervasyon durumunu güncelle
      setReservations(prev => 
        prev.map(res => 
          res.id === selectedReservation.id 
            ? { ...res, status: 'cancelled' } 
            : res
        )
      );
      
      showNotification('Rezervasyon başarıyla iptal edildi', 'success');
      handleCloseCancelDialog();
    } catch (error) {
      console.error('Rezervasyon iptal edilirken hata oluştu:', error);
      showNotification('Rezervasyon iptal edilirken bir hata oluştu', 'error');
    } finally {
      setCanceling(false);
    }
  };
  
  const handlePrintReservation = (reservationId) => {
    // Burada yazdırma işlemi yapılacak
    console.log('Yazdırılıyor:', reservationId);
    // Örnek: window.print();
  };
  
  const handleRateReservation = (reservationId) => {
    // Değerlendirme sayfasına yönlendir
    navigate(`/degerlendir/${reservationId}`);
  };
  
  const getStatusChip = (status, startDate, endDate) => {
    const now = new Date();
    const start = new Date(startDate);
    const end = new Date(endDate);
    
    if (status === 'cancelled') {
      return <Chip label="İptal Edildi" color="error" size="small" />;
    }
    
    if (now < start) {
      return <Chip label="Onay Bekliyor" color="warning" size="small" />;
    } else if (now >= start && now <= end) {
      return <Chip label="Devam Ediyor" color="success" size="small" />;
    } else if (now > end) {
      return <Chip label="Tamamlandı" color="info" size="small" />;
    }
    
    return <Chip label={status} size="small" />;
  };
  
  const formatDate = (dateString) => {
    return format(parseISO(dateString), 'dd MMMM yyyy EEEE', { locale: tr });
  };
  
  const formatTime = (dateString) => {
    return format(parseISO(dateString), 'HH:mm');
  };
  
  const calculateTotalDays = (startDate, endDate) => {
    return differenceInDays(new Date(endDate), new Date(startDate)) + 1;
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
      <Container maxWidth="lg" sx={{ py: 4 }}>
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
  
  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Rezervasyonlarım</Typography>
        <Button 
          variant="contained" 
          color="primary" 
          startIcon={<CarIcon />}
          onClick={() => navigate('/araclar')}
        >
          Yeni Rezervasyon Yap
        </Button>
      </Box>
      
      <Paper sx={{ mb: 3, overflow: 'hidden' }}>
        <Tabs
          value={activeTab}
          onChange={handleTabChange}
          variant="scrollable"
          scrollButtons="auto"
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          <Tab 
            label={
              <Box display="flex" alignItems="center">
                <ReceiptIcon sx={{ mr: 1 }} />
                Tümü
                {reservations.length > 0 && (
                  <Box component="span" sx={{ ml: 1, px: 1, py: 0.5, bgcolor: 'action.selected', borderRadius: 10, fontSize: '0.75rem' }}>
                    {reservations.length}
                  </Box>
                )}
              </Box>
            } 
          />
          <Tab 
            label={
              <Box display="flex" alignItems="center">
                <EventIcon sx={{ mr: 1 }} />
                Yaklaşan
                {reservations.filter(r => new Date(r.startDate) > new Date()).length > 0 && (
                  <Box component="span" sx={{ ml: 1, px: 1, py: 0.5, bgcolor: 'warning.light', color: 'warning.contrastText', borderRadius: 10, fontSize: '0.75rem' }}>
                    {reservations.filter(r => new Date(r.startDate) > new Date()).length}
                  </Box>
                )}
              </Box>
            } 
          />
          <Tab 
            label={
              <Box display="flex" alignItems="center">
                <TimeIcon sx={{ mr: 1 }} />
                Devam Eden
                {reservations.filter(r => new Date(r.startDate) <= new Date() && new Date(r.endDate) >= new Date() && r.status !== 'cancelled').length > 0 && (
                  <Box component="span" sx={{ ml: 1, px: 1, py: 0.5, bgcolor: 'success.light', color: 'success.contrastText', borderRadius: 10, fontSize: '0.75rem' }}>
                    {reservations.filter(r => new Date(r.startDate) <= new Date() && new Date(r.endDate) >= new Date() && r.status !== 'cancelled').length}
                  </Box>
                )}
              </Box>
            } 
          />
          <Tab 
            label={
              <Box display="flex" alignItems="center">
                <CheckCircleIcon sx={{ mr: 1 }} />
                Geçmiş
                {reservations.filter(r => new Date(r.endDate) < new Date() && r.status !== 'cancelled').length > 0 && (
                  <Box component="span" sx={{ ml: 1, px: 1, py: 0.5, bgcolor: 'info.light', color: 'info.contrastText', borderRadius: 10, fontSize: '0.75rem' }}>
                    {reservations.filter(r => new Date(r.endDate) < new Date() && r.status !== 'cancelled').length}
                  </Box>
                )}
              </Box>
            } 
          />
          <Tab 
            label={
              <Box display="flex" alignItems="center">
                <CancelIcon sx={{ mr: 1 }} />
                İptal Edilen
                {reservations.filter(r => r.status === 'cancelled').length > 0 && (
                  <Box component="span" sx={{ ml: 1, px: 1, py: 0.5, bgcolor: 'error.light', color: 'error.contrastText', borderRadius: 10, fontSize: '0.75rem' }}>
                    {reservations.filter(r => r.status === 'cancelled').length}
                  </Box>
                )}
              </Box>
            } 
          />
        </Tabs>
      </Paper>
      
      {filteredReservations.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <EventIcon sx={{ fontSize: 60, color: 'text.secondary', mb: 2 }} />
          <Typography variant="h6" gutterBottom>
            {activeTab === 0 
              ? 'Henüz rezervasyonunuz bulunmuyor.' 
              : activeTab === 1 
                ? 'Yaklaşan rezervasyonunuz bulunmuyor.'
                : activeTab === 2 
                  ? 'Devam eden rezervasyonunuz bulunmuyor.'
                  : activeTab === 3
                    ? 'Geçmiş rezervasyonunuz bulunmuyor.'
                    : 'İptal edilmiş rezervasyonunuz bulunmuyor.'
            }
          </Typography>
          <Typography color="text.secondary" paragraph>
            {activeTab === 0 
              ? 'Hemen bir araç kiralayarak ilk rezervasyonunuzu yapabilirsiniz.'
              : activeTab === 4
                ? 'İptal edilmiş rezervasyonlarınız burada görüntülenecektir.'
                : 'Yeni bir rezervasyon yapmak için aşağıdaki butonu kullanabilirsiniz.'
            }
          </Typography>
          {activeTab !== 4 && (
            <Button 
              variant="contained" 
              color="primary" 
              startIcon={<CarIcon />}
              onClick={() => navigate('/araclar')}
              sx={{ mt: 2 }}
            >
              Araçları Görüntüle
            </Button>
          )}
        </Paper>
      ) : (
        <>
          <TableContainer component={Paper} sx={{ mb: 2 }}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Rezervasyon No</TableCell>
                  <TableCell>Araç</TableCell>
                  <TableCell>Kiralama Tarihleri</TableCell>
                  <TableCell>Toplam Tutar</TableCell>
                  <TableCell>Durum</TableCell>
                  <TableCell align="right">İşlemler</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {paginatedReservations.map((reservation) => (
                  <TableRow key={reservation.id} hover>
                    <TableCell>#{reservation.reservationNumber}</TableCell>
                    <TableCell>
                      <Box display="flex" alignItems="center">
                        <Box 
                          component="img"
                          src={reservation.car.images?.[0] || '/default-car.jpg'}
                          alt={`${reservation.car.brand} ${reservation.car.model}`}
                          sx={{ width: 60, height: 40, objectFit: 'cover', borderRadius: 1, mr: 2 }}
                        />
                        <Box>
                          <Typography variant="subtitle2">{reservation.car.brand} {reservation.car.model}</Typography>
                          <Typography variant="body2" color="text.secondary">
                            {reservation.car.plateNumber}
                          </Typography>
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Box>
                        <Typography variant="body2">
                          {formatDate(reservation.startDate)}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {formatTime(reservation.startDate)} - {formatTime(reservation.endDate)}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {calculateTotalDays(reservation.startDate, reservation.endDate)} gün
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography variant="subtitle2">{reservation.totalPrice} ₺</Typography>
                      <Typography variant="body2" color="text.secondary">
                        {reservation.paymentStatus === 'paid' ? 'Ödendi' : 'Ödenmedi'}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      {getStatusChip(reservation.status, reservation.startDate, reservation.endDate)}
                    </TableCell>
                    <TableCell align="right">
                      <Box display="flex" justifyContent="flex-end" gap={1}>
                        <Tooltip title="Detayları Görüntüle">
                          <IconButton 
                            size="small" 
                            color="primary" 
                            onClick={() => navigate(`/rezervasyon/${reservation.id}`)}
                          >
                            <ReceiptIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        
                        <Tooltip title="Yazdır">
                          <IconButton 
                            size="small" 
                            color="default"
                            onClick={() => handlePrintReservation(reservation.id)}
                          >
                            <PrintIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        
                        {reservation.status !== 'cancelled' && new Date(reservation.startDate) > new Date() && (
                          <Tooltip title="İptal Et">
                            <IconButton 
                              size="small" 
                              color="error"
                              onClick={() => handleOpenCancelDialog(reservation)}
                            >
                              <CancelIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        )}
                        
                        {reservation.status !== 'cancelled' && new Date(reservation.endDate) < new Date() && !reservation.isRated && (
                          <Tooltip title="Değerlendir">
                            <IconButton 
                              size="small" 
                              color="warning"
                              onClick={() => handleRateReservation(reservation.id)}
                            >
                              <RateReviewIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        )}
                      </Box>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
          
          <TablePagination
            rowsPerPageOptions={[5, 10, 25]}
            component="div"
            count={filteredReservations.length}
            rowsPerPage={rowsPerPage}
            page={page}
            onPageChange={handleChangePage}
            onRowsPerPageChange={handleChangeRowsPerPage}
            labelRowsPerPage="Sayfa başına satır:"
            labelDisplayedRows={({ from, to, count }) => `${from}-${to} / ${count !== -1 ? count : `more than ${to}`}`}
          />
        </>
      )}
      
      {/* Rezervasyon İptal Onay Diyaloğu */}
      <Dialog
        open={cancelDialogOpen}
        onClose={handleCloseCancelDialog}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Rezervasyon İptali</DialogTitle>
        <DialogContent>
          <DialogContentText>
            <strong>#{selectedReservation?.reservationNumber}</strong> numaralı rezervasyonu iptal etmek istediğinize emin misiniz?
            <br /><br />
            <strong>İptal Koşulları:</strong>
            <ul>
              <li>Rezervasyon başlangıcına 24 saatten az kaldıysa, toplam tutarın %50'si iade edilir.</li>
              <li>Rezervasyon başlangıcına 48 saatten az kaldıysa, toplam tutarın %75'i iade edilir.</li>
              <li>Rezervasyon başlangıcına 72 saatten fazla kaldıysa, toplam tutarın tamamı iade edilir.</li>
            </ul>
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button 
            onClick={handleCloseCancelDialog} 
            color="inherit"
            disabled={canceling}
          >
            Vazgeç
          </Button>
          <Button 
            onClick={handleCancelReservation} 
            color="error" 
            variant="contained"
            disabled={canceling}
            startIcon={canceling ? <CircularProgress size={20} color="inherit" /> : <CancelIcon />}
          >
            {canceling ? 'İptal Ediliyor...' : 'Rezervasyonu İptal Et'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default ReservationsPage;

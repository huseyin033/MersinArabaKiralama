import React, { createContext, useState, useCallback, useRef, useEffect } from 'react';
import { Snackbar, Alert, Slide } from '@mui/material';

// Bildirim tipleri
const NOTIFICATION_TYPES = {
  SUCCESS: 'success',
  ERROR: 'error',
  WARNING: 'warning',
  INFO: 'info'
};

// Varsayılan ayarlar
const DEFAULT_OPTIONS = {
  autoHideDuration: 5000, // 5 saniye
  anchorOrigin: { vertical: 'top', horizontal: 'right' },
  transition: Slide,
  maxSnack: 3 // Aynı anda maksimum gösterilecek bildirim sayısı
};

// Context oluştur
const NotificationContext = createContext(null);

// Özel hook
const useNotification = () => {
  return useContext(NotificationContext);
};

// Bildirim bileşeni
const Notification = ({ id, message, type, options, onClose }) => {
  const { autoHideDuration, ...otherOptions } = options;
  
  return (
    <Snackbar
      key={id}
      open={true}
      autoHideDuration={autoHideDuration}
      onClose={(event, reason) => {
        if (reason === 'clickaway') return;
        onClose(id);
      }}
      TransitionComponent={options.transition}
      anchorOrigin={options.anchorOrigin}
      sx={{ marginTop: '20px' }}
    >
      <Alert 
        onClose={() => onClose(id)} 
        severity={type} 
        variant="filled"
        sx={{ width: '100%', boxShadow: 3 }}
      >
        {message}
      </Alert>
    </Snackbar>
  );
};

// Notification Provider bileşeni
const NotificationProvider = ({ children }) => {
  const [notifications, setNotifications] = useState([]);
  const queue = useRef([]);
  
  // Bildirim kapatıldığında
  const closeNotification = useCallback((id) => {
    setNotifications(prev => 
      prev.map(notification => 
        notification.id === id 
          ? { ...notification, isOpen: false } 
          : notification
      )
    );
    
    // Animasyon bittikten sonra kaldır
    setTimeout(() => {
      setNotifications(prev => prev.filter(n => n.id !== id));
      
      // Kuyrukta bekleyen bildirim varsa göster
      if (queue.current.length > 0) {
        const nextNotification = queue.current.shift();
        showNotification(nextNotification.message, nextNotification.type, nextNotification.options);
      }
    }, 300);
  }, []);
  
  // Bildirim göster
  const showNotification = useCallback((message, type = NOTIFICATION_TYPES.INFO, options = {}) => {
    const id = Date.now().toString();
    const notificationOptions = { ...DEFAULT_OPTIONS, ...options };
    
    // Maksimum bildirim sayısına ulaşıldıysa kuyruğa ekle
    if (notifications.length >= notificationOptions.maxSnack) {
      queue.current.push({ message, type, options: notificationOptions });
      return id;
    }
    
    // Yeni bildirimi ekle
    setNotifications(prev => [
      ...prev, 
      { 
        id, 
        message, 
        type, 
        options: notificationOptions,
        isOpen: true 
      }
    ]);
    
    return id;
  }, [notifications.length]);
  
  // Başarı bildirimi
  const success = useCallback((message, options = {}) => {
    return showNotification(message, NOTIFICATION_TYPES.SUCCESS, options);
  }, [showNotification]);
  
  // Hata bildirimi
  const error = useCallback((message, options = {}) => {
    return showNotification(message, NOTIFICATION_TYPES.ERROR, options);
  }, [showNotification]);
  
  // Uyarı bildirimi
  const warning = useCallback((message, options = {}) => {
    return showNotification(message, NOTIFICATION_TYPES.WARNING, options);
  }, [showNotification]);
  
  // Bilgi bildirimi
  const info = useCallback((message, options = {}) => {
    return showNotification(message, NOTIFICATION_TYPES.INFO, options);
  }, [showNotification]);
  
  // Context değeri
  const value = {
    showNotification,
    success,
    error,
    warning,
    info,
    closeNotification,
    NOTIFICATION_TYPES
  };

  return (
    <NotificationContext.Provider value={value}>
      {children}
      {notifications.map(notification => (
        <Notification
          key={notification.id}
          id={notification.id}
          message={notification.message}
          type={notification.type}
          options={notification.options}
          onClose={closeNotification}
        />
      ))}
    </NotificationContext.Provider>
  );
};

export { NotificationProvider, useNotification, NOTIFICATION_TYPES };

// Kullanım örneği:
// 1. Uygulamanın en üst seviyesinde:
//    <NotificationProvider>
//      <App />
//    </NotificationProvider>
//
// 2. Bileşen içinde:
//    const { success, error, warning, info } = useNotification();
//    
//    // Kullanım:
//    success('İşlem başarıyla tamamlandı!');
//    error('Bir hata oluştu!');
//    warning('Dikkat! Bu işlem geri alınamaz.');
//    info('Bilgilendirme mesajı');

import React from 'react';
import { AuthProvider } from '../contexts/AuthContext';
import { RentalProvider } from '../contexts/RentalContext';
import { NotificationProvider } from '../contexts/NotificationContext';

/**
 * Tüm uygulama sağlayıcılarını bir araya getiren bileşen.
 * Bu bileşen, uygulamanın kök seviyesinde kullanılmalıdır.
 */
const AppProvider = ({ children }) => {
  return (
    <NotificationProvider>
      <AuthProvider>
        <RentalProvider>
          {children}
        </RentalProvider>
      </AuthProvider>
    </NotificationProvider>
  );
};

export default AppProvider;

// Kullanım örneği:
// index.js veya App.js içinde:
// import AppProvider from './providers/AppProvider';
// 
// ReactDOM.render(
//   <React.StrictMode>
//     <AppProvider>
//       <App />
//     </AppProvider>
//   </React.StrictMode>,
//   document.getElementById('root')
// );

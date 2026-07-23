import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { SessionProvider } from './session/session_provider.jsx';
import ProtectedRoute from './session/protected_route.jsx';
import PublicRoute from './session/public_route.jsx';
import Landing from './pages/landing.jsx';
import Incentivadores from './pages/incentivadores.jsx';
import Login from './pages/login.jsx';
import Register from './pages/register.jsx';
import Dashboard from './pages/dashboard.jsx';
import TokenWallet from './pages/token_wallet.jsx';
import Profile from './pages/profile.jsx';
import RideTracker from './pages/ride_tracker.jsx';
import MyRides from './pages/my_rides.jsx';
import RideValidation from './pages/ride_validation.jsx';

export default function App() {
  return (
    <SessionProvider>
      <BrowserRouter>
        <Toaster />
        <Routes>
          <Route path="/" element={<Landing />} />
          <Route path="/incentivadores" element={<Incentivadores />} />
          <Route path="/login" element={<PublicRoute><Login /></PublicRoute>} />
          <Route path="/register" element={<PublicRoute><Register /></PublicRoute>} />
          <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
          <Route path="/wallet" element={<ProtectedRoute><TokenWallet /></ProtectedRoute>} />
          <Route path="/profile" element={<ProtectedRoute><Profile /></ProtectedRoute>} />
          <Route path="/ride-tracker" element={<ProtectedRoute><RideTracker /></ProtectedRoute>} />
          <Route path="/my-rides" element={<ProtectedRoute><MyRides /></ProtectedRoute>} />
          <Route path="/admin/rides" element={<ProtectedRoute><RideValidation /></ProtectedRoute>} />
          <Route path="*" element={<Landing />} />
        </Routes>
      </BrowserRouter>
    </SessionProvider>
  );
}

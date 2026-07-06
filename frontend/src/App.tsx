import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashboardPage';
import TravelPlanFormPage from './pages/TravelPlanFormPage';
import TravelPlanDetailPage from './pages/TravelPlanDetailPage';
import DestinationFormPage from './pages/DestinationFormPage';
import ActivityFormPage from './pages/ActivityFormPage';
import ExpenseFormPage from './pages/ExpenseFormPage';
import SharePage from './pages/SharePage';
import AdminPage from './pages/AdminPage';
import SharedPlanPage from './pages/SharedPlanPage';

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          {/* Javne rute - ne zahtevaju login */}
          <Route path="/" element={<Navigate to="/login" />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/shared/:token" element={<SharedPlanPage />} />

          {/* Zaštićene rute - zahtevaju ulogovanog korisnika (bilo koje uloge) */}
          <Route element={<ProtectedRoute />}>
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/travel-plans/create" element={<TravelPlanFormPage />} />
            <Route path="/travel-plans/:id/edit" element={<TravelPlanFormPage />} />
            <Route path="/travel-plans/:id" element={<TravelPlanDetailPage />} />
            <Route path="/travel-plans/:planId/destinations/create" element={<DestinationFormPage />} />
            <Route path="/travel-plans/:planId/destinations/:id/edit" element={<DestinationFormPage />} />
            <Route path="/travel-plans/:planId/activities/create" element={<ActivityFormPage />} />
            <Route path="/travel-plans/:planId/activities/:id/edit" element={<ActivityFormPage />} />
            <Route path="/travel-plans/:planId/expenses/create" element={<ExpenseFormPage />} />
            <Route path="/travel-plans/:planId/expenses/:id/edit" element={<ExpenseFormPage />} />
            <Route path="/travel-plans/:planId/share" element={<SharePage />} />
          </Route>

          {/* Zaštićena ruta - zahteva Admin ulogu */}
          <Route element={<ProtectedRoute adminOnly />}>
            <Route path="/admin" element={<AdminPage />} />
          </Route>

          {/* Nepoznata putanja - vrati na dashboard umesto praznog ekrana */}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;
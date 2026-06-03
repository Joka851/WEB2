import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
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
          <Route path="/" element={<Navigate to="/login" />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/admin" element={<AdminPage />} />
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
          <Route path="/shared/:token" element={<SharedPlanPage />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;
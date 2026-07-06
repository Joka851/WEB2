import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface ProtectedRouteProps {
  adminOnly?: boolean;
}

/**
 * Omotava jednu ili više ruta i zahteva da korisnik bude ulogovan pre nego
 * što se prikažu. Ako je adminOnly=true, dodatno zahteva Admin ulogu.
 *
 * Koristi se u App.tsx kao roditeljska <Route> sa ugnježdenim <Route>-ovima
 * unutra - React Router onda automatski prikazuje <Outlet /> mesto koje
 * odgovarajuće ugnježdene rute treba da popune.
 */
const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ adminOnly = false }) => {
  const { isAuthenticated, isAdmin, isLoading } = useAuth();
  const location = useLocation();

  // Dok se AuthContext još učitava (čita localStorage pri prvom renderu),
  // ne donosimo odluku o preusmeravanju - inače bi već ulogovan korisnik
  // bio nakratko izbačen na login pri svakom F5 osvežavanju stranice.
  if (isLoading) {
    return <div className="page"><p style={{ color: 'var(--ink-soft)' }}>Loading...</p></div>;
  }

  if (!isAuthenticated) {
    return <Navigate to={`/login?redirect=${encodeURIComponent(location.pathname)}`} replace />;
  }

  if (adminOnly && !isAdmin) {
    return <Navigate to="/dashboard" replace />;
  }

  return <Outlet />;
};

export default ProtectedRoute;
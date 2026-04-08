import { Suspense, lazy } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AppLayout } from './components/layout/AppLayout';
import { ProtectedRoute, RoleRoute } from './components/layout/ProtectedRoute';
import { PageLoader } from './components/ui/Spinner';
import { ToastNotifications } from './components/ui/ToastNotifications';

// Eager load login since it's the entry point
import { LoginPage } from './pages/auth/LoginPage';

// Lazy load all protected pages for better performance
const DashboardPage = lazy(() => import('./pages/DashboardPage'));
const ReservationsPage = lazy(() => import('./pages/ReservationsPage'));
const RoomsPage = lazy(() => import('./pages/RoomsPage'));
const GuestsPage = lazy(() => import('./pages/GuestsPage'));
const UsersPage = lazy(() => import('./pages/UsersPage'));
const PropertiesPage = lazy(() => import('./pages/PropertiesPage'));
const HousekeepingPage = lazy(() => import('./pages/HousekeepingPage'));
const ReportsPage = lazy(() => import('./pages/ReportsPage'));
const SettingsPage = lazy(() => import('./pages/SettingsPage'));

function App() {
  return (
    <BrowserRouter>
      {/* Global toast — renders on every page including login */}
      <ToastNotifications />
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />

        {/* Protected routes */}
        <Route
          element={
            <ProtectedRoute>
              <AppLayout />
            </ProtectedRoute>
          }
        >
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route
            path="/dashboard"
            element={
              <Suspense fallback={<PageLoader />}>
                <DashboardPage />
              </Suspense>
            }
          />
          <Route
            path="/reservations"
            element={
              <Suspense fallback={<PageLoader />}>
                <ReservationsPage />
              </Suspense>
            }
          />
          <Route
            path="/rooms"
            element={
              <Suspense fallback={<PageLoader />}>
                <RoomsPage />
              </Suspense>
            }
          />
          <Route
            path="/guests"
            element={
              <Suspense fallback={<PageLoader />}>
                <GuestsPage />
              </Suspense>
            }
          />
          <Route
            path="/users"
            element={
              <RoleRoute allowedRoles={['SuperAdmin', 'Admin', 'Developer']}>
                <Suspense fallback={<PageLoader />}>
                  <UsersPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/properties"
            element={
              <RoleRoute allowedRoles={['SuperAdmin', 'Admin', 'Developer']}>
                <Suspense fallback={<PageLoader />}>
                  <PropertiesPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/housekeeping"
            element={
              <Suspense fallback={<PageLoader />}>
                <HousekeepingPage />
              </Suspense>
            }
          />
          <Route
            path="/reports"
            element={
              <RoleRoute allowedRoles={['SuperAdmin', 'Admin', 'Developer']}>
                <Suspense fallback={<PageLoader />}>
                  <ReportsPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/settings"
            element={
              <Suspense fallback={<PageLoader />}>
                <SettingsPage />
              </Suspense>
            }
          />
        </Route>

        {/* 404 - redirect to dashboard */}
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;

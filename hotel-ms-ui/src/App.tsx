import { Suspense, lazy, type ComponentType } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AppLayout } from './components/layout/AppLayout';
import { ProtectedRoute, RoleRoute } from './components/layout/ProtectedRoute';
import { PageLoader } from './components/ui/Spinner';
import { ToastNotifications } from './components/ui/ToastNotifications';
import { useAuthStore } from './lib/store';

// Eager load auth pages since they're the entry point
import { LoginPage } from './pages/auth/LoginPage';
import { ResetPasswordPage } from './pages/auth/ResetPasswordPage';
import { ActivatePage } from './pages/auth/ActivatePage';
import { SetupWorkspacePage } from './pages/auth/SetupWorkspacePage';
import ChangePasswordPage from './pages/auth/ChangePasswordPage';

// Lazy load all protected pages for better performance
const DashboardPage    = lazy(() => import('./pages/DashboardPage'));
const ReservationsPage = lazy(() => import('./pages/ReservationsPage'));
const RoomsPage        = lazy(() => import('./pages/RoomsPage'));
const GuestsPage       = lazy(() => import('./pages/GuestsPage'));
const UsersPage        = lazy(() => import('./pages/UsersPage'));
const PropertiesPage   = lazy(() => import('./pages/PropertiesPage'));
const HousekeepingPage = lazy(() => import('./pages/HousekeepingPage'));
const ReportsPage      = lazy(() => import('./pages/ReportsPage'));
const SettingsPage     = lazy(() => import('./pages/SettingsPage'));
const BillingPage      = lazy(() => import('./pages/BillingPage'));
const DiscountsPage    = lazy(() => import('./pages/DiscountsPage'));
const ServiceRequestsPage = lazy(() => import('./pages/ServiceRequestsPage'));
const AuditLogsPage    = lazy(() => import('./pages/AuditLogsPage'));
const TenantsPage      = lazy(() => import('./pages/TenantsPage'));
const SubscriptionPage = lazy(() => import('./pages/SubscriptionPage') as Promise<{ default: ComponentType }>);

// Role constants — single source of truth
const DEV_ROLES     = ['Developer'];
const STAFF_ROLES   = ['SuperAdmin', 'Admin', 'FrontDesk', 'Housekeeping', 'Developer'];
const ADMIN_ROLES   = ['SuperAdmin', 'Admin', 'Developer'];
const DESK_ROLES    = ['SuperAdmin', 'Admin', 'FrontDesk', 'Developer'];
const HK_ROLES      = ['SuperAdmin', 'Admin', 'FrontDesk', 'Housekeeping', 'Developer'];
const BOOKING_ROLES = [...DESK_ROLES, 'Guest'];
const ROOMS_ROLES   = [...HK_ROLES,  'Guest'];

/** Redirects to the correct home page based on the user's role. */
function RoleAwareHome() {
  const { user } = useAuthStore();
  const roles: string[] = user?.roles ?? [];
  const isGuest = roles.includes('Guest') && !STAFF_ROLES.some((r) => roles.includes(r));
  return <Navigate to={isGuest ? '/reservations' : '/dashboard'} replace />;
}

function App() {
  return (
    <BrowserRouter>
      <ToastNotifications />
      <Routes>
        {/* Public routes */}
        <Route path="/login"          element={<LoginPage />} />
        <Route path="/activate"       element={<ActivatePage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />

        {/* First-login password change (provisioned tenants) */}
        <Route
          path="/change-password"
          element={
            <ProtectedRoute>
              <ChangePasswordPage />
            </ProtectedRoute>
          }
        />

        {/* Authenticated-but-unactivated workspace setup (self-register flow) */}
        <Route
          path="/setup"
          element={
            <ProtectedRoute>
              <SetupWorkspacePage />
            </ProtectedRoute>
          }
        />

        {/* Protected routes */}
        <Route
          element={
            <ProtectedRoute>
              <AppLayout />
            </ProtectedRoute>
          }
        >
          {/* Root redirect — role-aware */}
          <Route path="/" element={<RoleAwareHome />} />

          {/* Staff + admin pages */}
          <Route
            path="/dashboard"
            element={
              <RoleRoute allowedRoles={STAFF_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <DashboardPage />
                </Suspense>
              </RoleRoute>
            }
          />

          {/* Accessible by staff + guest */}
          <Route
            path="/reservations"
            element={
              <RoleRoute allowedRoles={BOOKING_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <ReservationsPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/rooms"
            element={
              <RoleRoute allowedRoles={ROOMS_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <RoomsPage />
                </Suspense>
              </RoleRoute>
            }
          />

          {/* Staff only */}
          <Route
            path="/guests"
            element={
              <RoleRoute allowedRoles={DESK_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <GuestsPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/housekeeping"
            element={
              <RoleRoute allowedRoles={HK_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <HousekeepingPage />
                </Suspense>
              </RoleRoute>
            }
          />

          {/* Admin only */}
          <Route
            path="/users"
            element={
              <RoleRoute allowedRoles={ADMIN_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <UsersPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/properties"
            element={
              <RoleRoute allowedRoles={ADMIN_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <PropertiesPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/reports"
            element={
              <RoleRoute allowedRoles={ADMIN_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <ReportsPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/settings"
            element={
              <RoleRoute allowedRoles={ADMIN_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <SettingsPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/billing"
            element={
              <RoleRoute allowedRoles={DESK_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <BillingPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/discounts"
            element={
              <RoleRoute allowedRoles={ADMIN_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <DiscountsPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/service-requests"
            element={
              <RoleRoute allowedRoles={BOOKING_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <ServiceRequestsPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/audit-logs"
            element={
              <RoleRoute allowedRoles={ADMIN_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <AuditLogsPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/tenants"
            element={
              <RoleRoute allowedRoles={DEV_ROLES}>
                <Suspense fallback={<PageLoader />}>
                  <TenantsPage />
                </Suspense>
              </RoleRoute>
            }
          />
          <Route
            path="/subscription"
            element={
              <RoleRoute allowedRoles={['SuperAdmin', 'Admin', 'Developer']}>
                <Suspense fallback={<PageLoader />}>
                  <SubscriptionPage />
                </Suspense>
              </RoleRoute>
            }
          />
        </Route>

        {/* 404 */}
        <Route path="*" element={<RoleAwareHome />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;

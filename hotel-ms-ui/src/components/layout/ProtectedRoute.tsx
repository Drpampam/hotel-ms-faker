import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../../lib/store';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isAuthenticated, token, user, tenantId } = useAuthStore();
  const location = useLocation();

  // Check both store state and localStorage token
  const storedToken = localStorage.getItem('hotel_ms_token');
  const hasAccess = isAuthenticated || !!storedToken || !!token;

  if (!hasAccess) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Must change password before accessing anything
  if (user?.mustChangePassword && location.pathname !== '/change-password') {
    return <Navigate to="/change-password" replace />;
  }

  // Self-registered users with no tenant yet go to workspace setup
  const needsSetup = hasAccess && user != null && !user.mustChangePassword && (!tenantId || tenantId <= 0);
  if (needsSetup && location.pathname !== '/setup') {
    return <Navigate to="/setup" replace />;
  }

  return <>{children}</>;
}

interface RoleRouteProps {
  children: React.ReactNode;
  allowedRoles: string[];
}

/** Wraps a route that should only be accessible to specific roles.
 *  Guests land on /reservations; all other unauthorised roles land on /dashboard. */
export function RoleRoute({ children, allowedRoles }: RoleRouteProps) {
  const { user } = useAuthStore();
  const userRoles: string[] = user?.roles ?? [];
  const hasRole = allowedRoles.some((r) => userRoles.includes(r));

  if (!hasRole) {
    const isGuest = userRoles.includes('Guest');
    return <Navigate to={isGuest ? '/reservations' : '/dashboard'} replace />;
  }

  return <>{children}</>;
}

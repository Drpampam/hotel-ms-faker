import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../../lib/store';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isAuthenticated, token } = useAuthStore();
  const location = useLocation();

  // Check both store state and localStorage token
  const storedToken = localStorage.getItem('hotel_ms_token');
  const hasAccess = isAuthenticated || !!storedToken || !!token;

  if (!hasAccess) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
}

interface RoleRouteProps {
  children: React.ReactNode;
  allowedRoles: string[];
}

/** Wraps a route that should only be accessible to specific roles.
 *  Redirects to /dashboard if the current user's role is not in allowedRoles. */
export function RoleRoute({ children, allowedRoles }: RoleRouteProps) {
  const { user } = useAuthStore();
  const userRoles: string[] = user?.roles ?? [];
  const hasRole = allowedRoles.some((r) => userRoles.includes(r));

  if (!hasRole) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
}

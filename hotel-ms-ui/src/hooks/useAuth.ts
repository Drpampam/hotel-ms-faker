import { useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore, useToast } from '../lib/store';
import { authService } from '../services/auth.service';
import type { LoginRequest } from '../types';

export function useAuth() {
  const { user, token, isAuthenticated, isLoading, setAuth, setLoading, logout: storeLogout } = useAuthStore();
  const navigate = useNavigate();
  const toast = useToast();

  const login = useCallback(
    async (credentials: LoginRequest) => {
      setLoading(true);
      try {
        const { token: authToken, refreshToken, user: authUser } = await authService.login(credentials);

        setAuth(authUser, authToken);
        // Store refresh token separately
        if (refreshToken) {
          localStorage.setItem('hotel_ms_refresh_token', refreshToken);
        }
        toast.success('Welcome back!', `Signed in as ${authUser.email}`);
        navigate('/dashboard');
        return { success: true };
      } catch (error: unknown) {
        setLoading(false);
        const message =
          (error as { response?: { data?: { message?: string } } })?.response?.data?.message ||
          (error as Error)?.message ||
          'Invalid email or password';
        toast.error('Login failed', message);
        return { success: false, message };
      }
    },
    [setAuth, setLoading, navigate, toast]
  );

  const logout = useCallback(() => {
    authService.logout();
    storeLogout();
    navigate('/login');
    toast.info('Signed out', 'You have been logged out successfully');
  }, [storeLogout, navigate, toast]);

  return {
    user,
    token,
    isAuthenticated,
    isLoading,
    login,
    logout,
  };
}

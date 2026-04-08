import api from '../lib/axios';
import type { LoginRequest, BackendLoginResponse, AuthUser } from '../types';

export const authService = {
  /**
   * Login: POST /api/v1/User/login
   * Backend returns token in response header "Token", user info in body.data
   */
  async login(credentials: LoginRequest): Promise<{ token: string; refreshToken: string; user: AuthUser; message?: string }> {
    const response = await api.post<BackendLoginResponse>('/api/v1/User/login', {
      email: credentials.email,
      password: credentials.password,
      rememberMe: credentials.rememberMe ?? false,
    });

    // Token is returned in the response header "Token"
    const token = response.headers['token'] as string | undefined;
    const refreshToken = response.headers['refreshtoken'] as string | undefined;

    if (!token) {
      throw new Error(response.data?.message ?? 'Login failed: no token received');
    }

    const data = response.data?.data;
    const user: AuthUser = {
      email: data?.email ?? credentials.email,
      fullName: data?.fullName ?? credentials.email,
      roles: data?.roles ?? [],
      tenantId: 1,
      picture: data?.picture,
    };

    return { token, refreshToken: refreshToken ?? '', user };
  },

  /**
   * GET /api/v1/User/get-user-by-email?email=...
   */
  async getUserByEmail(email: string): Promise<AuthUser | null> {
    try {
      const response = await api.get<{ status: boolean; data?: { fullName?: string; email?: string; userRoles?: Array<{ name: string }> } }>(
        '/api/v1/User/get-user-by-email',
        { params: { email } }
      );
      const data = response.data?.data;
      if (!data) return null;
      return {
        email: data.email ?? email,
        fullName: data.fullName ?? email,
        roles: (data.userRoles ?? []).map((r) => r.name),
        tenantId: 1,
      };
    } catch {
      return null;
    }
  },

  /**
   * POST /api/v1/User/refresh-token?currentRefreshToken=...
   */
  async refreshToken(currentRefreshToken: string): Promise<{ token: string; refreshToken: string }> {
    const response = await api.post('/api/v1/User/refresh-token', null, {
      params: { currentRefreshToken },
    });
    const token = response.headers['token'] as string;
    const newRefreshToken = response.headers['refreshtoken'] as string;
    return { token, refreshToken: newRefreshToken };
  },

  /**
   * POST /api/v1/User/forgot-password — sends password reset email
   */
  async forgotPassword(email: string): Promise<{ success: boolean; message: string }> {
    const response = await api.post<{ status: boolean; message?: string }>(
      '/api/v1/User/forgot-password',
      { email }
    );
    return {
      success: response.data?.status ?? false,
      message: response.data?.message ?? 'Reset link sent.',
    };
  },

  /**
   * POST /api/v1/User/reset-password — resets password with token
   */
  async resetPassword(email: string, token: string, newPassword: string): Promise<{ success: boolean; message: string }> {
    const response = await api.post<{ status: boolean; message?: string }>(
      '/api/v1/User/reset-password',
      { email, token, newPassword }
    );
    return {
      success: response.data?.status ?? false,
      message: response.data?.message ?? 'Password reset.',
    };
  },

  logout() {
    localStorage.removeItem('hotel_ms_token');
    localStorage.removeItem('hotel_ms_user');
    localStorage.removeItem('hotel-ms-auth');
  },

  getStoredToken(): string | null {
    return localStorage.getItem('hotel_ms_token');
  },
};

import axios, { type InternalAxiosRequestConfig } from 'axios';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://hotel-ms-api-tfvt.onrender.com';

const api = axios.create({
  baseURL: BASE_URL,
  timeout: 60000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// --- Token refresh state ---
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (err: unknown) => void;
}> = [];

function processQueue(error: unknown, token: string | null) {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error || !token) reject(error);
    else resolve(token);
  });
  failedQueue = [];
}

function clearAuth() {
  localStorage.removeItem('hotel_ms_token');
  localStorage.removeItem('hotel_ms_user');
  localStorage.removeItem('hotel_ms_refresh_token');
  localStorage.removeItem('hotel-ms-auth');
}

async function attemptRefresh(): Promise<string> {
  const refreshToken = localStorage.getItem('hotel_ms_refresh_token');
  if (!refreshToken) throw new Error('No refresh token');

  const res = await fetch(
    `${BASE_URL}/api/v1/User/refresh-token?currentRefreshToken=${encodeURIComponent(refreshToken)}`,
    { method: 'POST' }
  );
  if (!res.ok) throw new Error('Refresh failed');

  const newToken = res.headers.get('token');
  const newRefreshToken = res.headers.get('refreshtoken');
  if (!newToken) throw new Error('No token in refresh response');

  localStorage.setItem('hotel_ms_token', newToken);
  if (newRefreshToken) localStorage.setItem('hotel_ms_refresh_token', newRefreshToken);

  // Update persisted Zustand store so future page loads pick up the new token
  try {
    const stored = localStorage.getItem('hotel-ms-auth');
    if (stored) {
      const parsed = JSON.parse(stored) as { state?: Record<string, unknown> };
      if (parsed.state) {
        parsed.state.token = newToken;
        localStorage.setItem('hotel-ms-auth', JSON.stringify(parsed));
      }
    }
  } catch {
    // non-fatal
  }

  return newToken;
}

// Request interceptor — attach token + tenant
api.interceptors.request.use(
  (config) => {
    let token: string | null = null;
    let tenantId: number | null = null;
    try {
      const stored = localStorage.getItem('hotel-ms-auth');
      if (stored) {
        const parsed = JSON.parse(stored) as { state?: { token?: string; tenantId?: number } };
        token = parsed?.state?.token ?? null;
        tenantId = parsed?.state?.tenantId ?? null;
      }
    } catch {
      // ignore parse errors
    }
    if (!token) token = localStorage.getItem('hotel_ms_token');

    if (token) config.headers.Authorization = `Bearer ${token}`;
    if (tenantId && tenantId > 0) config.headers['X-Tenant-Id'] = String(tenantId);
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor
api.interceptors.response.use(
  (response) => {
    const body = response.data as { status?: boolean; message?: string } | undefined;
    if (body && body.status === false) {
      return Promise.reject(new Error(body.message || 'Request failed'));
    }
    return response;
  },
  async (error) => {
    const status = error.response?.status as number | undefined;
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    if (status === 401 && !originalRequest._retry) {
      const refreshToken = localStorage.getItem('hotel_ms_refresh_token');

      if (!refreshToken) {
        clearAuth();
        window.location.href = '/login';
        return Promise.reject(error);
      }

      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const newToken = await attemptRefresh();
        processQueue(null, newToken);
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        clearAuth();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    if (status === 403) {
      import('./store').then(({ useNotificationStore }) => {
        useNotificationStore.getState().addNotification({
          type: 'error',
          title: 'Permission denied',
          message: 'You do not have access to perform this action.',
        });
      });
      return Promise.reject(error);
    }

    if (status === 400) {
      const body = error.response?.data as {
        message?: string;
        data?: { field?: string; message: string }[];
      } | undefined;

      if (Array.isArray(body?.data) && body!.data!.length > 0) {
        const lines = body!.data!.map((e) =>
          e.field ? `${e.field}: ${e.message}` : e.message
        );
        return Promise.reject(new Error(lines.join('\n')));
      }
      return Promise.reject(new Error(body?.message || 'Validation failed. Please check your input.'));
    }

    if (status === 500) {
      const body = error.response?.data as { message?: string } | undefined;
      return Promise.reject(new Error(body?.message || 'A server error occurred. Please try again.'));
    }

    return Promise.reject(error);
  }
);

export default api;

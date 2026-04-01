import axios from 'axios';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://hotel-ms-api-tfvt.onrender.com';

const api = axios.create({
  baseURL: BASE_URL,
  timeout: 60000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor — attach token + tenant ID to every request
api.interceptors.request.use(
  (config) => {
    // Token: prefer zustand persisted store, fall back to raw localStorage
    let token: string | null = null;
    try {
      const stored = localStorage.getItem('hotel-ms-auth');
      if (stored) {
        const parsed = JSON.parse(stored) as { state?: { token?: string; tenantId?: number } };
        token = parsed?.state?.token ?? null;
      }
    } catch {
      // ignore parse errors
    }
    if (!token) {
      token = localStorage.getItem('hotel_ms_token');
    }

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Tenant ID: from zustand store or default to 1
    let tenantId = 1;
    try {
      const stored = localStorage.getItem('hotel-ms-auth');
      if (stored) {
        const parsed = JSON.parse(stored) as { state?: { tenantId?: number } };
        tenantId = parsed?.state?.tenantId ?? 1;
      }
    } catch {
      // ignore
    }
    config.headers['X-Tenant-Id'] = String(tenantId);

    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor — on 401 clear auth and redirect to login
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('hotel_ms_token');
      localStorage.removeItem('hotel_ms_user');
      localStorage.removeItem('hotel_ms_refresh_token');
      localStorage.removeItem('hotel-ms-auth');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;

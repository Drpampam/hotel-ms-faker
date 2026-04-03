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

    // Do not send X-Tenant-Id — the middleware defaults to 'public' schema
    // which is where all data lives. Sending a numeric tenant ID routes queries
    // to an empty tenant-specific schema (e.g. tenant_1) causing 42P01 errors.

    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor
api.interceptors.response.use(
  (response) => {
    // Business-logic failures: HTTP 200 but status:false in body
    const body = response.data as { status?: boolean; message?: string } | undefined;
    if (body && body.status === false) {
      return Promise.reject(new Error(body.message || 'Request failed'));
    }
    return response;
  },
  (error) => {
    const status = error.response?.status as number | undefined;

    if (status === 401) {
      localStorage.removeItem('hotel_ms_token');
      localStorage.removeItem('hotel_ms_user');
      localStorage.removeItem('hotel_ms_refresh_token');
      localStorage.removeItem('hotel-ms-auth');
      window.location.href = '/login';
      return Promise.reject(error);
    }

    if (status === 400) {
      // FluentValidation / data-annotation errors: { message, data: [{field, message}] }
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

import axios from 'axios';
import { useAdminStore } from './store';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://hotel-ms-api-tfvt.onrender.com';

const api = axios.create({
  baseURL: BASE_URL,
  timeout: 60000,
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = useAdminStore.getState().token;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (response) => {
    const body = response.data as { status?: boolean; message?: string } | undefined;
    if (body && body.status === false) {
      return Promise.reject(new Error(body.message || 'Request failed'));
    }
    return response;
  },
  (error) => {
    const status = error.response?.status as number | undefined;
    if (status === 401) {
      useAdminStore.getState().logout();
      window.location.href = '/login';
    }
    const body = error.response?.data as { message?: string } | undefined;
    return Promise.reject(new Error(body?.message || error.message || 'Request failed'));
  }
);

export default api;

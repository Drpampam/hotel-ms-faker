import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface AdminUser {
  email: string;
  fullName: string;
  roles: string[];
}

interface AdminStore {
  user: AdminUser | null;
  token: string | null;
  isAuthenticated: boolean;
  setAuth: (user: AdminUser, token: string) => void;
  logout: () => void;
}

export const useAdminStore = create<AdminStore>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      isAuthenticated: false,
      setAuth: (user, token) => {
        localStorage.setItem('admin_ms_token', token);
        set({ user, token, isAuthenticated: true });
      },
      logout: () => {
        localStorage.removeItem('admin_ms_token');
        localStorage.removeItem('hotel-ms-admin-auth');
        set({ user: null, token: null, isAuthenticated: false });
      },
    }),
    {
      name: 'hotel-ms-admin-auth',
      partialize: (s) => ({ user: s.user, token: s.token, isAuthenticated: s.isAuthenticated }),
    }
  )
);

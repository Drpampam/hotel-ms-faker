import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { AuthUser, Theme } from '../types';

// Auth Store
interface AuthStore {
  user: AuthUser | null;
  token: string | null;
  tenantId: number;
  isAuthenticated: boolean;
  isLoading: boolean;
  setAuth: (user: AuthUser | null, token: string) => void;
  setUser: (user: AuthUser) => void;
  setLoading: (loading: boolean) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      tenantId: 1,
      isAuthenticated: false,
      isLoading: false,
      setAuth: (user, token) => {
        localStorage.setItem('hotel_ms_token', token);
        if (user) {
          localStorage.setItem('hotel_ms_user', JSON.stringify(user));
        }
        set({
          user,
          token,
          tenantId: user?.tenantId ?? 1,
          isAuthenticated: true,
          isLoading: false,
        });
      },
      setUser: (user) => set({ user, tenantId: user.tenantId ?? 1 }),
      setLoading: (isLoading) => set({ isLoading }),
      logout: () => {
        localStorage.removeItem('hotel_ms_token');
        localStorage.removeItem('hotel_ms_user');
        localStorage.removeItem('hotel_ms_refresh_token');
        localStorage.removeItem('hotel-ms-auth');
        set({ user: null, token: null, tenantId: 1, isAuthenticated: false, isLoading: false });
      },
    }),
    {
      name: 'hotel-ms-auth',
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        tenantId: state.tenantId,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);

// Theme Store
interface ThemeStore {
  theme: Theme;
  toggleTheme: () => void;
  setTheme: (theme: Theme) => void;
}

export const useThemeStore = create<ThemeStore>()(
  persist(
    (set, get) => ({
      theme: 'light',
      toggleTheme: () => {
        const newTheme = get().theme === 'light' ? 'dark' : 'light';
        document.documentElement.classList.toggle('dark', newTheme === 'dark');
        set({ theme: newTheme });
      },
      setTheme: (theme) => {
        document.documentElement.classList.toggle('dark', theme === 'dark');
        set({ theme });
      },
    }),
    {
      name: 'hotel-ms-theme',
    }
  )
);

// Sidebar Store
interface SidebarStore {
  isCollapsed: boolean;
  isMobileOpen: boolean;
  toggleCollapse: () => void;
  toggleMobile: () => void;
  closeMobile: () => void;
}

export const useSidebarStore = create<SidebarStore>()((set) => ({
  isCollapsed: false,
  isMobileOpen: false,
  toggleCollapse: () => set((state) => ({ isCollapsed: !state.isCollapsed })),
  toggleMobile: () => set((state) => ({ isMobileOpen: !state.isMobileOpen })),
  closeMobile: () => set({ isMobileOpen: false }),
}));

// Notification Store
interface Notification {
  id: string;
  type: 'success' | 'error' | 'info' | 'warning';
  title: string;
  message?: string;
  duration?: number;
}

interface NotificationStore {
  notifications: Notification[];
  addNotification: (notification: Omit<Notification, 'id'>) => void;
  removeNotification: (id: string) => void;
  clearAll: () => void;
}

export const useNotificationStore = create<NotificationStore>()((set) => ({
  notifications: [],
  addNotification: (notification) => {
    const id = Date.now().toString();
    const newNotification = { ...notification, id };
    set((state) => ({ notifications: [...state.notifications, newNotification] }));

    const duration = notification.duration ?? 4000;
    if (duration > 0) {
      setTimeout(() => {
        set((state) => ({
          notifications: state.notifications.filter((n) => n.id !== id),
        }));
      }, duration);
    }
  },
  removeNotification: (id) =>
    set((state) => ({
      notifications: state.notifications.filter((n) => n.id !== id),
    })),
  clearAll: () => set({ notifications: [] }),
}));

// Helper hook for notifications
export function useToast() {
  const { addNotification } = useNotificationStore();

  return {
    success: (title: string, message?: string) =>
      addNotification({ type: 'success', title, message }),
    error: (title: string, message?: string) =>
      addNotification({ type: 'error', title, message }),
    info: (title: string, message?: string) =>
      addNotification({ type: 'info', title, message }),
    warning: (title: string, message?: string) =>
      addNotification({ type: 'warning', title, message }),
  };
}

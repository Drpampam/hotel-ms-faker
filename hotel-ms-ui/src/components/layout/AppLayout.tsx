import { Outlet } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Header } from './Header';
import { useSidebarStore, useNotificationStore } from '../../lib/store';
import { cn } from '../../lib/utils';
import { CheckCircle, XCircle, AlertCircle, Info, X } from 'lucide-react';

function ToastNotifications() {
  const { notifications, removeNotification } = useNotificationStore();

  const icons = {
    success: <CheckCircle className="h-5 w-5 text-emerald-500" />,
    error: <XCircle className="h-5 w-5 text-red-500" />,
    warning: <AlertCircle className="h-5 w-5 text-amber-500" />,
    info: <Info className="h-5 w-5 text-blue-500" />,
  };

  const borders = {
    success: 'border-l-4 border-l-emerald-500',
    error: 'border-l-4 border-l-red-500',
    warning: 'border-l-4 border-l-amber-500',
    info: 'border-l-4 border-l-blue-500',
  };

  if (notifications.length === 0) return null;

  return (
    <div className="fixed top-4 right-4 z-[100] flex flex-col gap-2 max-w-sm w-full pointer-events-none">
      {notifications.map((notification) => (
        <div
          key={notification.id}
          className={cn(
            'flex items-start gap-3 p-4 rounded-xl bg-white dark:bg-slate-800 shadow-lg',
            'border border-slate-200 dark:border-slate-700',
            'animate-slide-up pointer-events-auto',
            borders[notification.type]
          )}
        >
          <div className="flex-shrink-0 mt-0.5">{icons[notification.type]}</div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-semibold text-slate-900 dark:text-slate-100">
              {notification.title}
            </p>
            {notification.message && (
              <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
                {notification.message}
              </p>
            )}
          </div>
          <button
            onClick={() => removeNotification(notification.id)}
            className="flex-shrink-0 p-0.5 rounded text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors"
          >
            <X className="h-4 w-4" />
          </button>
        </div>
      ))}
    </div>
  );
}

export function AppLayout() {
  const { isCollapsed } = useSidebarStore();

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-900">
      <Sidebar />
      <div
        className={cn(
          'flex flex-col min-h-screen transition-all duration-300',
          'lg:pl-64',
          isCollapsed && 'lg:pl-[68px]'
        )}
      >
        <Header />
        <main className="flex-1 overflow-x-hidden">
          <Outlet />
        </main>
      </div>
      <ToastNotifications />
    </div>
  );
}

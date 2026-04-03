import { useRef, useState, useEffect } from 'react';
import { Menu, Sun, Moon, Bell, Search, CheckCircle, XCircle, AlertCircle, Info, Trash2, CheckCheck } from 'lucide-react';
import { useSidebarStore, useNotificationHistoryStore } from '../../lib/store';
import type { NotificationHistoryItem } from '../../lib/store';
import { useTheme } from '../../hooks/useTheme';
import { Button } from '../ui/Button';
import { SearchModal } from './SearchModal';
import { useLocation } from 'react-router-dom';
import { cn } from '../../lib/utils';

const pageTitles: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/reservations': 'Reservations',
  '/rooms': 'Rooms',
  '/guests': 'Guests',
  '/housekeeping': 'Housekeeping',
  '/properties': 'Properties',
  '/users': 'Users',
  '/reports': 'Reports',
  '/settings': 'Settings',
};

const typeIcon: Record<NotificationHistoryItem['type'], React.ReactNode> = {
  success: <CheckCircle className="h-4 w-4 text-emerald-500 flex-shrink-0 mt-0.5" />,
  error:   <XCircle    className="h-4 w-4 text-red-500    flex-shrink-0 mt-0.5" />,
  warning: <AlertCircle className="h-4 w-4 text-amber-500 flex-shrink-0 mt-0.5" />,
  info:    <Info        className="h-4 w-4 text-blue-500   flex-shrink-0 mt-0.5" />,
};

function timeAgo(ts: number): string {
  const diff = Math.floor((Date.now() - ts) / 1000);
  if (diff < 60) return 'just now';
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
  if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
  return `${Math.floor(diff / 86400)}d ago`;
}

export function Header() {
  const { toggleMobile } = useSidebarStore();
  const { isDark, toggleTheme } = useTheme();
  const location = useLocation();
  const { history, unreadCount, markAllRead, markRead, clearHistory } = useNotificationHistoryStore();

  const [notifOpen, setNotifOpen] = useState(false);
  const [searchOpen, setSearchOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const bellRef = useRef<HTMLButtonElement>(null);

  // Close notification dropdown on outside click
  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (
        dropdownRef.current && !dropdownRef.current.contains(e.target as Node) &&
        bellRef.current && !bellRef.current.contains(e.target as Node)
      ) {
        setNotifOpen(false);
      }
    }
    if (notifOpen) document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, [notifOpen]);

  // Ctrl+K / ⌘K opens search
  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
        e.preventDefault();
        setSearchOpen((prev) => !prev);
      }
    }
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, []);

  const handleBellClick = () => {
    setNotifOpen((prev) => !prev);
    if (!notifOpen && unreadCount > 0) markAllRead();
  };

  const pathKey = Object.keys(pageTitles).find(
    (key) => location.pathname === key || location.pathname.startsWith(key + '/')
  );
  const pageTitle = pathKey ? pageTitles[pathKey] : 'HotelMS';

  const now = new Date();
  const greeting =
    now.getHours() < 12 ? 'Good morning' : now.getHours() < 18 ? 'Good afternoon' : 'Good evening';

  return (
    <>
      <header className="sticky top-0 z-30 h-16 flex items-center gap-4 px-4 lg:px-6 bg-white/80 dark:bg-slate-900/80 backdrop-blur-md border-b border-slate-200 dark:border-slate-800">
        {/* Mobile menu toggle */}
        <button
          onClick={toggleMobile}
          className="lg:hidden p-2 rounded-lg text-slate-500 hover:text-slate-900 dark:hover:text-slate-100 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
        >
          <Menu className="h-5 w-5" />
        </button>

        {/* Page title */}
        <div className="flex-1">
          <h1 className="text-xl font-bold text-slate-900 dark:text-slate-100">{pageTitle}</h1>
          <p className="text-xs text-slate-500 dark:text-slate-400 hidden sm:block">
            {greeting} — {now.toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
          </p>
        </div>

        {/* Search button */}
        <button
          onClick={() => setSearchOpen(true)}
          className="hidden md:flex items-center gap-2 px-3 py-2 text-sm text-slate-500 dark:text-slate-400 bg-slate-100 dark:bg-slate-800 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors w-48"
        >
          <Search className="h-4 w-4 flex-shrink-0" />
          <span>Quick search...</span>
          <kbd className="ml-auto text-xs bg-white dark:bg-slate-700 border border-slate-200 dark:border-slate-600 rounded px-1">⌘K</kbd>
        </button>

        {/* Mobile search icon */}
        <button
          onClick={() => setSearchOpen(true)}
          className="md:hidden p-2 rounded-lg text-slate-500 hover:text-slate-900 dark:hover:text-slate-100 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
        >
          <Search className="h-5 w-5" />
        </button>

        {/* Notifications */}
        <div className="relative">
          <button
            ref={bellRef}
            onClick={handleBellClick}
            className={cn(
              'relative p-2 rounded-lg transition-colors',
              notifOpen
                ? 'text-indigo-600 bg-indigo-50 dark:bg-indigo-900/30'
                : 'text-slate-500 hover:text-slate-900 dark:hover:text-slate-100 hover:bg-slate-100 dark:hover:bg-slate-800'
            )}
            title="Notifications"
          >
            <Bell className="h-5 w-5" />
            {unreadCount > 0 && (
              <span className="absolute top-1 right-1 min-w-[16px] h-4 flex items-center justify-center bg-red-500 rounded-full ring-2 ring-white dark:ring-slate-900 text-[10px] font-bold text-white px-0.5">
                {unreadCount > 99 ? '99+' : unreadCount}
              </span>
            )}
            {unreadCount === 0 && history.length > 0 && (
              <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-slate-300 dark:bg-slate-600 rounded-full ring-2 ring-white dark:ring-slate-900" />
            )}
          </button>

          {/* Notification dropdown */}
          {notifOpen && (
            <div
              ref={dropdownRef}
              className="absolute right-0 top-full mt-2 w-80 sm:w-96 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-700 rounded-xl shadow-xl z-50 overflow-hidden"
            >
              <div className="flex items-center justify-between px-4 py-3 border-b border-slate-100 dark:border-slate-800">
                <div>
                  <p className="text-sm font-semibold text-slate-900 dark:text-slate-100">Notifications</p>
                  {history.length > 0 && (
                    <p className="text-xs text-slate-500 dark:text-slate-400">{history.length} total</p>
                  )}
                </div>
                {history.length > 0 && (
                  <div className="flex items-center gap-1">
                    <button
                      onClick={markAllRead}
                      className="p-1.5 rounded-lg text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 transition-colors"
                      title="Mark all as read"
                    >
                      <CheckCheck className="h-4 w-4" />
                    </button>
                    <button
                      onClick={clearHistory}
                      className="p-1.5 rounded-lg text-slate-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
                      title="Clear all"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                )}
              </div>

              <div className="max-h-[420px] overflow-y-auto divide-y divide-slate-50 dark:divide-slate-800">
                {history.length === 0 ? (
                  <div className="flex flex-col items-center justify-center py-10 px-4 text-center">
                    <Bell className="h-8 w-8 text-slate-300 dark:text-slate-600 mb-2" />
                    <p className="text-sm text-slate-500 dark:text-slate-400">No notifications yet</p>
                    <p className="text-xs text-slate-400 dark:text-slate-500 mt-1">
                      Actions like check-ins, payments, and updates will appear here
                    </p>
                  </div>
                ) : (
                  history.map((item) => (
                    <button
                      key={item.id}
                      onClick={() => markRead(item.id)}
                      className={cn(
                        'w-full flex items-start gap-3 px-4 py-3 text-left transition-colors hover:bg-slate-50 dark:hover:bg-slate-800/50',
                        !item.read && 'bg-indigo-50/40 dark:bg-indigo-900/10'
                      )}
                    >
                      {typeIcon[item.type]}
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center justify-between gap-2">
                          <p className={cn(
                            'text-sm truncate',
                            item.read
                              ? 'text-slate-600 dark:text-slate-400 font-normal'
                              : 'text-slate-900 dark:text-slate-100 font-medium'
                          )}>
                            {item.title}
                          </p>
                          {!item.read && (
                            <span className="flex-shrink-0 w-2 h-2 rounded-full bg-indigo-500" />
                          )}
                        </div>
                        {item.message && (
                          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5 line-clamp-2">
                            {item.message}
                          </p>
                        )}
                        <p className="text-xs text-slate-400 dark:text-slate-500 mt-1">
                          {timeAgo(item.timestamp)}
                        </p>
                      </div>
                    </button>
                  ))
                )}
              </div>
            </div>
          )}
        </div>

        {/* Theme toggle */}
        <Button variant="ghost" size="icon" onClick={toggleTheme} title={isDark ? 'Switch to light mode' : 'Switch to dark mode'}>
          {isDark ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
        </Button>
      </header>

      {/* Search modal — rendered outside header so it overlays everything */}
      <SearchModal isOpen={searchOpen} onClose={() => setSearchOpen(false)} />
    </>
  );
}

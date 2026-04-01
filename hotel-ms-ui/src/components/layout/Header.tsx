import { Menu, Sun, Moon, Bell, Search } from 'lucide-react';
import { useSidebarStore } from '../../lib/store';
import { useTheme } from '../../hooks/useTheme';
import { Button } from '../ui/Button';
import { useLocation } from 'react-router-dom';

const pageTitles: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/reservations': 'Reservations',
  '/rooms': 'Rooms',
  '/guests': 'Guests',
  '/housekeeping': 'Housekeeping',
  '/properties': 'Properties',
  '/users': 'Users',
  '/settings': 'Settings',
};

export function Header() {
  const { toggleMobile } = useSidebarStore();
  const { isDark, toggleTheme } = useTheme();
  const location = useLocation();

  const pathKey = Object.keys(pageTitles).find(
    (key) => location.pathname === key || location.pathname.startsWith(key + '/')
  );
  const pageTitle = pathKey ? pageTitles[pathKey] : 'HotelMS';

  const now = new Date();
  const greeting =
    now.getHours() < 12 ? 'Good morning' : now.getHours() < 18 ? 'Good afternoon' : 'Good evening';

  return (
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

      {/* Search button (decorative) */}
      <button className="hidden md:flex items-center gap-2 px-3 py-2 text-sm text-slate-500 dark:text-slate-400 bg-slate-100 dark:bg-slate-800 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors w-48">
        <Search className="h-4 w-4" />
        <span>Quick search...</span>
        <kbd className="ml-auto text-xs bg-white dark:bg-slate-700 border border-slate-200 dark:border-slate-600 rounded px-1">⌘K</kbd>
      </button>

      {/* Notifications */}
      <button className="relative p-2 rounded-lg text-slate-500 hover:text-slate-900 dark:hover:text-slate-100 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors">
        <Bell className="h-5 w-5" />
        <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-indigo-600 rounded-full ring-2 ring-white dark:ring-slate-900" />
      </button>

      {/* Theme toggle */}
      <Button variant="ghost" size="icon" onClick={toggleTheme} title={isDark ? 'Switch to light mode' : 'Switch to dark mode'}>
        {isDark ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
      </Button>
    </header>
  );
}

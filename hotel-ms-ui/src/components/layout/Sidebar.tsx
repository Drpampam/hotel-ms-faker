import { NavLink, useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  CalendarCheck,
  BedDouble,
  Users,
  UserCircle,
  Building2,
  Sparkles,
  BarChart3,
  Settings,
  LogOut,
  ChevronLeft,
  ChevronRight,
  Hotel,
  X,
} from 'lucide-react';
import { cn } from '../../lib/utils';
import { useSidebarStore } from '../../lib/store';
import { useAuth } from '../../hooks/useAuth';
import { getInitials, getFullName } from '../../lib/utils';

type NavItem = {
  label: string;
  path: string;
  icon: React.ElementType;
  roles?: string[]; // undefined = accessible by all authenticated users
};

const ALL_NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard',    path: '/dashboard',    icon: LayoutDashboard, roles: ['SuperAdmin', 'Admin', 'FrontDesk', 'Housekeeping', 'Developer'] },
  { label: 'Reservations', path: '/reservations', icon: CalendarCheck,   roles: ['SuperAdmin', 'Admin', 'FrontDesk', 'Developer', 'Guest'] },
  { label: 'Rooms',        path: '/rooms',        icon: BedDouble,       roles: ['SuperAdmin', 'Admin', 'FrontDesk', 'Housekeeping', 'Developer', 'Guest'] },
  { label: 'Guests',       path: '/guests',       icon: UserCircle,      roles: ['SuperAdmin', 'Admin', 'FrontDesk', 'Developer'] },
  { label: 'Housekeeping', path: '/housekeeping', icon: Sparkles,        roles: ['SuperAdmin', 'Admin', 'FrontDesk', 'Housekeeping', 'Developer'] },
  { label: 'Properties',   path: '/properties',   icon: Building2,       roles: ['SuperAdmin', 'Admin', 'Developer'] },
  { label: 'Users',        path: '/users',        icon: Users,           roles: ['SuperAdmin', 'Admin', 'Developer'] },
  { label: 'Reports',      path: '/reports',      icon: BarChart3,       roles: ['SuperAdmin', 'Admin', 'Developer'] },
  { label: 'Settings',     path: '/settings',     icon: Settings,        roles: ['SuperAdmin', 'Admin', 'Developer'] },
];

export function Sidebar() {
  const { isCollapsed, isMobileOpen, toggleCollapse, closeMobile } = useSidebarStore();
  const { user, logout } = useAuth();
  const location = useLocation();

  const userRoles: string[] = user?.roles ?? [];
  const navItems = ALL_NAV_ITEMS.filter(
    (item) => !item.roles || item.roles.some((r) => userRoles.includes(r))
  );

  const nameParts = (user?.fullName ?? '').trim().split(/\s+/);
  const initials = user
    ? getInitials(nameParts[0] || 'U', nameParts.slice(1).join(' ') || 'S')
    : 'US';
  const fullName = user?.fullName || 'User';

  return (
    <>
      {/* Mobile overlay */}
      {isMobileOpen && (
        <div
          className="fixed inset-0 bg-black/50 z-40 lg:hidden"
          onClick={closeMobile}
        />
      )}

      {/* Sidebar */}
      <aside
        className={cn(
          'fixed top-0 left-0 h-full z-50 flex flex-col',
          'bg-white dark:bg-slate-900 border-r border-slate-200 dark:border-slate-800',
          'transition-all duration-300 ease-in-out',
          // Desktop
          'lg:translate-x-0',
          isCollapsed ? 'lg:w-[68px]' : 'lg:w-64',
          // Mobile
          isMobileOpen ? 'translate-x-0 w-64' : '-translate-x-full lg:translate-x-0'
        )}
      >
        {/* Logo */}
        <div
          className={cn(
            'flex items-center h-16 px-4 border-b border-slate-200 dark:border-slate-800 flex-shrink-0',
            isCollapsed ? 'justify-center' : 'justify-between'
          )}
        >
          <div className="flex items-center gap-2.5 overflow-hidden">
            <div className="flex-shrink-0 w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center shadow-sm">
              <Hotel className="h-5 w-5 text-white" />
            </div>
            {!isCollapsed && (
              <div className="overflow-hidden">
                <span className="text-lg font-bold text-slate-900 dark:text-slate-100 whitespace-nowrap">
                  Hotel<span className="text-indigo-600">MS</span>
                </span>
              </div>
            )}
          </div>

          {/* Mobile close / Desktop collapse */}
          <button
            onClick={isMobileOpen ? closeMobile : toggleCollapse}
            className={cn(
              'p-1.5 rounded-lg text-slate-400 hover:text-slate-600 dark:hover:text-slate-300',
              'hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors',
              isCollapsed && 'hidden lg:hidden'
            )}
          >
            <X className="h-4 w-4 lg:hidden" />
            <ChevronLeft className="h-4 w-4 hidden lg:block" />
          </button>
        </div>

        {/* Collapsed expand button */}
        {isCollapsed && (
          <button
            onClick={toggleCollapse}
            className="hidden lg:flex absolute -right-3 top-16 mt-3 w-6 h-6 rounded-full bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 items-center justify-center shadow-sm hover:shadow-md transition-shadow z-10"
          >
            <ChevronRight className="h-3 w-3 text-slate-600 dark:text-slate-400" />
          </button>
        )}

        {/* Nav */}
        <nav className="flex-1 overflow-y-auto py-4 px-3 space-y-1">
          {navItems.map(({ label, path, icon: Icon }) => {
            const isActive =
              path === '/dashboard'
                ? location.pathname === '/dashboard' || location.pathname === '/'
                : location.pathname.startsWith(path);

            return (
              <NavLink
                key={path}
                to={path}
                onClick={() => closeMobile()}
                title={isCollapsed ? label : undefined}
                className={cn(
                  'sidebar-item group',
                  isActive ? 'sidebar-item-active' : 'sidebar-item-inactive',
                  isCollapsed && 'lg:justify-center lg:px-0'
                )}
              >
                <Icon
                  className={cn(
                    'h-5 w-5 flex-shrink-0 transition-colors',
                    isActive
                      ? 'text-indigo-600 dark:text-indigo-400'
                      : 'text-slate-500 dark:text-slate-400 group-hover:text-slate-700 dark:group-hover:text-slate-200'
                  )}
                />
                {!isCollapsed && (
                  <span className="truncate">{label}</span>
                )}
                {isCollapsed && (
                  <span className="absolute left-full ml-2 px-2 py-1 text-xs font-medium bg-slate-900 dark:bg-slate-700 text-white rounded-md opacity-0 group-hover:opacity-100 pointer-events-none whitespace-nowrap lg:block hidden transition-opacity z-50">
                    {label}
                  </span>
                )}
              </NavLink>
            );
          })}
        </nav>

        {/* User section */}
        <div className="flex-shrink-0 p-3 border-t border-slate-200 dark:border-slate-800">
          <div
            className={cn(
              'flex items-center gap-3 p-2.5 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors cursor-pointer group',
              isCollapsed && 'lg:justify-center'
            )}
          >
            <div className="flex-shrink-0 w-8 h-8 rounded-full bg-indigo-600 flex items-center justify-center">
              <span className="text-xs font-semibold text-white">{initials}</span>
            </div>
            {!isCollapsed && (
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-slate-900 dark:text-slate-100 truncate">
                  {fullName || user?.email || 'User'}
                </p>
                <p className="text-xs text-slate-500 dark:text-slate-400 truncate">
                  {user?.roles?.[0] ?? 'Admin'}
                </p>
              </div>
            )}
            {!isCollapsed && (
              <button
                onClick={logout}
                title="Sign out"
                className="flex-shrink-0 p-1.5 rounded-md text-slate-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors opacity-0 group-hover:opacity-100"
              >
                <LogOut className="h-4 w-4" />
              </button>
            )}
          </div>
          {isCollapsed && (
            <button
              onClick={logout}
              title="Sign out"
              className="hidden lg:flex w-full mt-1 p-2 rounded-lg text-slate-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors justify-center"
            >
              <LogOut className="h-4 w-4" />
            </button>
          )}
        </div>
      </aside>
    </>
  );
}

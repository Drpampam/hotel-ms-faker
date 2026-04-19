import { Outlet } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Header } from './Header';
import { SubscriptionBanner } from './SubscriptionBanner';
import { useSidebarStore } from '../../lib/store';
import { cn } from '../../lib/utils';

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
        <SubscriptionBanner />
        <Header />
        <main className="flex-1 overflow-x-hidden">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

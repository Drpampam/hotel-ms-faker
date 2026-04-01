import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  BedDouble,
  CalendarCheck,
  Users,
  DollarSign,
  Plus,
  ArrowRight,
  TrendingUp,
  CheckCircle,
  Clock,
} from 'lucide-react';
import { StatsCard } from '../components/dashboard/StatsCard';
import { OccupancyChart } from '../components/dashboard/OccupancyChart';
import { RecentReservations } from '../components/dashboard/RecentReservations';
import { Button } from '../components/ui/Button';
import type { Reservation, DashboardStats, OccupancyDataPoint } from '../types';
import { reservationService } from '../services/reservation.service';
import { roomService } from '../services/room.service';
import { guestService } from '../services/guest.service';
import { formatCurrency } from '../lib/utils';

const MOCK_STATS: DashboardStats = {
  totalRooms: 48,
  availableRooms: 19,
  occupiedRooms: 27,
  activeReservations: 34,
  totalGuests: 128,
  revenueThisMonth: 42680,
  occupancyRate: 72,
  checkInsToday: 8,
  checkOutsToday: 5,
};

function generateChartData(): OccupancyDataPoint[] {
  return Array.from({ length: 7 }, (_, i) => {
    const date = new Date();
    date.setDate(date.getDate() - (6 - i));
    return {
      date: date.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' }),
      reservations: Math.floor(Math.random() * 25) + 8,
      checkIns: Math.floor(Math.random() * 12) + 2,
      checkOuts: Math.floor(Math.random() * 12) + 2,
    };
  });
}

export function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats>(MOCK_STATS);
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [chartData] = useState<OccupancyDataPoint[]>(generateChartData());
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      setIsLoading(true);
      try {
        const [reservationList, roomList, guestList] = await Promise.allSettled([
          reservationService.getAll(),
          roomService.getAll(),
          guestService.getAll(),
        ]);

        const rooms = roomList.status === 'fulfilled' ? roomList.value : [];
        const resList = reservationList.status === 'fulfilled' ? reservationList.value : [];
        const guests = guestList.status === 'fulfilled' ? guestList.value : [];

        if (rooms.length > 0 || resList.length > 0 || guests.length > 0) {
          const available = rooms.filter((r) => r.status === 'Available').length;
          const occupied = rooms.filter((r) => r.status === 'Occupied').length;
          const active = resList.filter(
            (r) => r.status === 'Confirmed' || r.status === 'CheckedIn'
          ).length;
          const revenue = resList
            .filter((r) => r.status !== 'Cancelled')
            .reduce((sum, r) => sum + (r.totalAmount ?? 0), 0);

          setStats({
            totalRooms: rooms.length || MOCK_STATS.totalRooms,
            availableRooms: available || MOCK_STATS.availableRooms,
            occupiedRooms: occupied || MOCK_STATS.occupiedRooms,
            activeReservations: active || MOCK_STATS.activeReservations,
            totalGuests: guests.length || MOCK_STATS.totalGuests,
            revenueThisMonth: revenue || MOCK_STATS.revenueThisMonth,
            occupancyRate:
              rooms.length > 0
                ? Math.round((occupied / rooms.length) * 100)
                : MOCK_STATS.occupancyRate,
            checkInsToday: MOCK_STATS.checkInsToday,
            checkOutsToday: MOCK_STATS.checkOutsToday,
          });
          setReservations(resList);
        }
      } catch {
        // Silently use mock data
      } finally {
        setIsLoading(false);
      }
    };
    fetchData();
  }, []);

  const quickActions = [
    {
      label: 'New Reservation',
      to: '/reservations',
      icon: <Plus className="h-4 w-4" />,
      color: 'bg-indigo-600 hover:bg-indigo-700 text-white',
    },
    {
      label: 'Check-in Guest',
      to: '/reservations',
      icon: <CheckCircle className="h-4 w-4" />,
      color: 'bg-emerald-600 hover:bg-emerald-700 text-white',
    },
    {
      label: 'View Rooms',
      to: '/rooms',
      icon: <BedDouble className="h-4 w-4" />,
      color: 'bg-slate-100 hover:bg-slate-200 dark:bg-slate-700 dark:hover:bg-slate-600 text-slate-900 dark:text-white',
    },
    {
      label: 'Guest List',
      to: '/guests',
      icon: <Users className="h-4 w-4" />,
      color: 'bg-slate-100 hover:bg-slate-200 dark:bg-slate-700 dark:hover:bg-slate-600 text-slate-900 dark:text-white',
    },
  ];

  return (
    <div className="page-container">
      {/* Header */}
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Overview</h2>
          <p className="page-subtitle">Monitor your hotel performance at a glance</p>
        </div>
        <Link to="/reservations">
          <Button leftIcon={<Plus className="h-4 w-4" />}>New Reservation</Button>
        </Link>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-8">
        <StatsCard
          title="Total Rooms"
          value={stats.totalRooms}
          icon={<BedDouble className="h-6 w-6" />}
          change={0}
          changeLabel="vs last month"
          color="indigo"
          isLoading={isLoading}
          suffix={`/ ${stats.availableRooms} available`}
        />
        <StatsCard
          title="Active Reservations"
          value={stats.activeReservations}
          icon={<CalendarCheck className="h-6 w-6" />}
          change={12}
          changeLabel="vs last week"
          color="blue"
          isLoading={isLoading}
        />
        <StatsCard
          title="Total Guests"
          value={stats.totalGuests}
          icon={<Users className="h-6 w-6" />}
          change={8}
          changeLabel="vs last month"
          color="violet"
          isLoading={isLoading}
        />
        <StatsCard
          title="Revenue This Month"
          value={formatCurrency(stats.revenueThisMonth)}
          icon={<DollarSign className="h-6 w-6" />}
          change={15}
          changeLabel="vs last month"
          color="emerald"
          isLoading={isLoading}
        />
      </div>

      {/* Secondary stats */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
        {[
          {
            label: 'Occupancy Rate',
            value: `${stats.occupancyRate}%`,
            icon: <TrendingUp className="h-5 w-5 text-indigo-600" />,
            bg: 'bg-indigo-50 dark:bg-indigo-900/20',
          },
          {
            label: 'Check-ins Today',
            value: stats.checkInsToday,
            icon: <CheckCircle className="h-5 w-5 text-emerald-600" />,
            bg: 'bg-emerald-50 dark:bg-emerald-900/20',
          },
          {
            label: 'Check-outs Today',
            value: stats.checkOutsToday,
            icon: <Clock className="h-5 w-5 text-amber-600" />,
            bg: 'bg-amber-50 dark:bg-amber-900/20',
          },
        ].map((item) => (
          <div
            key={item.label}
            className="flex items-center gap-4 p-4 bg-white dark:bg-slate-800 rounded-xl border border-slate-100 dark:border-slate-700 shadow-sm"
          >
            <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${item.bg}`}>
              {item.icon}
            </div>
            <div>
              <p className="text-xs text-slate-500 dark:text-slate-400">{item.label}</p>
              <p className="text-xl font-bold text-slate-900 dark:text-slate-100">{item.value}</p>
            </div>
          </div>
        ))}
      </div>

      {/* Main content grid */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6 mb-8">
        <div className="xl:col-span-2">
          <OccupancyChart data={chartData} isLoading={isLoading} />
        </div>

        {/* Quick Actions */}
        <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-100 dark:border-slate-700 p-6 shadow-sm">
          <div className="flex items-center justify-between mb-5">
            <h3 className="text-base font-semibold text-slate-900 dark:text-slate-100">
              Quick Actions
            </h3>
          </div>
          <div className="space-y-2.5">
            {quickActions.map((action) => (
              <Link
                key={action.label}
                to={action.to}
                className={`flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-medium transition-all duration-150 active:scale-[0.98] ${action.color}`}
              >
                {action.icon}
                {action.label}
                <ArrowRight className="h-4 w-4 ml-auto" />
              </Link>
            ))}
          </div>

          {/* Room Status Mini Overview */}
          <div className="mt-6 pt-5 border-t border-slate-100 dark:border-slate-700">
            <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide mb-3">
              Room Status
            </p>
            <div className="space-y-2.5">
              {[
                { label: 'Available', count: stats.availableRooms, color: 'bg-emerald-500', pct: Math.round((stats.availableRooms / stats.totalRooms) * 100) },
                { label: 'Occupied', count: stats.occupiedRooms, color: 'bg-blue-500', pct: Math.round((stats.occupiedRooms / stats.totalRooms) * 100) },
                { label: 'Maintenance', count: stats.totalRooms - stats.availableRooms - stats.occupiedRooms, color: 'bg-amber-500', pct: 100 - Math.round((stats.availableRooms / stats.totalRooms) * 100) - Math.round((stats.occupiedRooms / stats.totalRooms) * 100) },
              ].map((s) => (
                <div key={s.label}>
                  <div className="flex items-center justify-between text-xs mb-1">
                    <span className="text-slate-600 dark:text-slate-400 flex items-center gap-1.5">
                      <span className={`w-2 h-2 rounded-full ${s.color}`} />
                      {s.label}
                    </span>
                    <span className="font-medium text-slate-800 dark:text-slate-200">{s.count}</span>
                  </div>
                  <div className="h-1.5 bg-slate-100 dark:bg-slate-700 rounded-full overflow-hidden">
                    <div
                      className={`h-full rounded-full ${s.color} transition-all duration-700`}
                      style={{ width: `${Math.max(0, s.pct)}%` }}
                    />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Recent Reservations */}
      <RecentReservations reservations={reservations} isLoading={isLoading} />
    </div>
  );
}

export default DashboardPage;

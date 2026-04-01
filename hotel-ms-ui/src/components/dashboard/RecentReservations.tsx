import { Link } from 'react-router-dom';
import { ArrowRight } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '../ui/Card';
import { Badge } from '../ui/Badge';
import { Spinner } from '../ui/Spinner';
import type { Reservation } from '../../types';
import { formatDate, formatCurrency } from '../../lib/utils';

interface RecentReservationsProps {
  reservations: Reservation[];
  isLoading?: boolean;
}

const statusVariant: Record<string, 'warning' | 'info' | 'success' | 'default' | 'danger' | 'secondary'> = {
  Pending: 'warning',
  Confirmed: 'info',
  CheckedIn: 'success',
  CheckedOut: 'default',
  Cancelled: 'danger',
  NoShow: 'secondary',
};

const MOCK_RESERVATIONS: Reservation[] = [
  {
    id: '1',
    guestName: 'James Morrison',
    roomNumber: '201',
    checkInDate: new Date(Date.now() + 86400000).toISOString(),
    checkOutDate: new Date(Date.now() + 3 * 86400000).toISOString(),
    status: 'Confirmed',
    totalAmount: 450,
    guestId: '1',
    roomId: '1',
    tenantId: 1,
    createdAt: new Date().toISOString(),
  },
  {
    id: '2',
    guestName: 'Sarah Chen',
    roomNumber: '305',
    checkInDate: new Date().toISOString(),
    checkOutDate: new Date(Date.now() + 2 * 86400000).toISOString(),
    status: 'CheckedIn',
    totalAmount: 320,
    guestId: '2',
    roomId: '2',
    tenantId: 1,
    createdAt: new Date().toISOString(),
  },
  {
    id: '3',
    guestName: 'Robert Williams',
    roomNumber: '102',
    checkInDate: new Date(Date.now() - 86400000).toISOString(),
    checkOutDate: new Date(Date.now() + 86400000).toISOString(),
    status: 'CheckedIn',
    totalAmount: 280,
    guestId: '3',
    roomId: '3',
    tenantId: 1,
    createdAt: new Date().toISOString(),
  },
  {
    id: '4',
    guestName: 'Emily Davis',
    roomNumber: '414',
    checkInDate: new Date(Date.now() + 2 * 86400000).toISOString(),
    checkOutDate: new Date(Date.now() + 5 * 86400000).toISOString(),
    status: 'Pending',
    totalAmount: 750,
    guestId: '4',
    roomId: '4',
    tenantId: 1,
    createdAt: new Date().toISOString(),
  },
  {
    id: '5',
    guestName: 'Michael Scott',
    roomNumber: '510',
    checkInDate: new Date(Date.now() - 3 * 86400000).toISOString(),
    checkOutDate: new Date(Date.now() - 86400000).toISOString(),
    status: 'CheckedOut',
    totalAmount: 560,
    guestId: '5',
    roomId: '5',
    tenantId: 1,
    createdAt: new Date().toISOString(),
  },
];

export function RecentReservations({ reservations, isLoading = false }: RecentReservationsProps) {
  const displayData = reservations.length > 0 ? reservations.slice(0, 5) : MOCK_RESERVATIONS;

  return (
    <Card padding="none">
      <div className="p-6 pb-4">
        <CardHeader>
          <CardTitle>Recent Reservations</CardTitle>
          <Link
            to="/reservations"
            className="flex items-center gap-1 text-xs font-medium text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-300 transition-colors"
          >
            View all <ArrowRight className="h-3.5 w-3.5" />
          </Link>
        </CardHeader>
      </div>

      {isLoading ? (
        <div className="flex items-center justify-center py-12">
          <Spinner />
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-t border-slate-100 dark:border-slate-700/50">
                {['Guest', 'Room', 'Check-in', 'Check-out', 'Amount', 'Status'].map((h) => (
                  <th
                    key={h}
                    className="px-6 py-3 text-left text-xs font-semibold text-slate-400 dark:text-slate-500 uppercase tracking-wide"
                  >
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-700/50">
              {displayData.map((r) => (
                <tr
                  key={r.id}
                  className="hover:bg-slate-50 dark:hover:bg-slate-700/30 transition-colors"
                >
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-2.5">
                      <div className="w-7 h-7 rounded-full bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center flex-shrink-0">
                        <span className="text-xs font-semibold text-indigo-600 dark:text-indigo-400">
                          {(r.guestName ?? r.guest
                            ? (r.guestName ?? `${r.guest?.firstName} ${r.guest?.lastName}`)
                            : 'G'
                          )
                            .split(' ')
                            .map((n: string) => n[0])
                            .join('')
                            .slice(0, 2)
                            .toUpperCase()}
                        </span>
                      </div>
                      <span className="font-medium text-slate-800 dark:text-slate-200 truncate max-w-[120px]">
                        {r.guestName ?? (r.guest ? `${r.guest.firstName} ${r.guest.lastName}` : '—')}
                      </span>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-slate-600 dark:text-slate-400">
                    <span className="font-mono text-xs bg-slate-100 dark:bg-slate-700 px-1.5 py-0.5 rounded">
                      {r.roomNumber ?? r.room?.roomNumber ?? '—'}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-slate-600 dark:text-slate-400 whitespace-nowrap">
                    {formatDate(r.checkInDate)}
                  </td>
                  <td className="px-6 py-4 text-slate-600 dark:text-slate-400 whitespace-nowrap">
                    {formatDate(r.checkOutDate)}
                  </td>
                  <td className="px-6 py-4 font-semibold text-slate-900 dark:text-slate-100 whitespace-nowrap">
                    {formatCurrency(r.totalAmount)}
                  </td>
                  <td className="px-6 py-4">
                    <Badge variant={statusVariant[r.status] ?? 'default'} dot>
                      {r.status}
                    </Badge>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Card>
  );
}

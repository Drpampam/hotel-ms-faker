import { useState, useEffect, useCallback } from 'react';
import { useSlowConnection } from '../hooks/useSlowConnection';
import {
  Plus,
  Search,
  Filter,
  Eye,
  Calendar,
  User,
  BedDouble,
  DollarSign,
  X,
} from 'lucide-react';
import { reservationService } from '../services/reservation.service';
import { guestService } from '../services/guest.service';
import { roomService } from '../services/room.service';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Select } from '../components/ui/Select';
import { Badge } from '../components/ui/Badge';
import { Table } from '../components/ui/Table';
import { Modal } from '../components/ui/Modal';
import { Card } from '../components/ui/Card';
import { useToast } from '../lib/store';
import type { Reservation, Guest, Room, CreateReservationRequest } from '../types';
import { formatDate, formatCurrency, calculateNights, RESERVATION_STATUS_COLORS } from '../lib/utils';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { cn } from '../lib/utils';

const STATUS_OPTIONS = [
  { value: '', label: 'All Statuses' },
  { value: 'Pending', label: 'Pending' },
  { value: 'Confirmed', label: 'Confirmed' },
  { value: 'CheckedIn', label: 'Checked In' },
  { value: 'CheckedOut', label: 'Checked Out' },
  { value: 'Cancelled', label: 'Cancelled' },
  { value: 'NoShow', label: 'No Show' },
];

const statusVariant: Record<string, 'warning' | 'info' | 'success' | 'default' | 'danger' | 'secondary'> = {
  Pending: 'warning',
  Confirmed: 'info',
  CheckedIn: 'success',
  CheckedOut: 'default',
  Cancelled: 'danger',
  NoShow: 'secondary',
};

const today = new Date().toISOString().split('T')[0];

const createSchema = z.object({
  guestId: z.string().min(1, 'Guest is required'),
  roomId: z.string().min(1, 'Room is required'),
  checkInDate: z.string()
    .min(1, 'Check-in date is required')
    .refine((d) => d >= today, { message: 'Check-in date cannot be in the past' }),
  checkOutDate: z.string().min(1, 'Check-out date is required'),
  adults: z.number().min(1, 'At least 1 adult required').max(10).default(1),
  children: z.number().min(0).max(10).default(0),
  specialRequests: z.string().max(500, 'Special requests cannot exceed 500 characters').optional(),
}).refine((d) => !d.checkInDate || !d.checkOutDate || d.checkOutDate > d.checkInDate, {
  message: 'Check-out date must be after check-in date',
  path: ['checkOutDate'],
});

type CreateFormData = z.infer<typeof createSchema>;


export function ReservationsPage() {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [guests, setGuests] = useState<Guest[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [viewReservation, setViewReservation] = useState<Reservation | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const toast = useToast();
  useSlowConnection(isLoading);

  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors },
  } = useForm<CreateFormData>({
    resolver: zodResolver(createSchema),
    defaultValues: { adults: 1, children: 0 },
  });

  const checkIn = watch('checkInDate');
  const checkOut = watch('checkOutDate');
  const selectedRoomId = watch('roomId');
  const selectedRoom = rooms.find((r) => String(r.id) === selectedRoomId);
  const nights = checkIn && checkOut ? calculateNights(checkIn, checkOut) : 0;
  const estimatedTotal = selectedRoom && nights > 0 ? selectedRoom.pricePerNight * nights : 0;

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    try {
      const [resList, guestList, roomList] = await Promise.allSettled([
        reservationService.getAll(),
        guestService.getAll(),
        roomService.getAll(),
      ]);
      setReservations(resList.status === 'fulfilled' ? resList.value : []);
      setGuests(guestList.status === 'fulfilled' ? guestList.value : []);
      setRooms(roomList.status === 'fulfilled' ? roomList.value : []);
    } catch {
      // leave state as empty arrays
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const filtered = reservations.filter((r) => {
    const name = (r.guestName ?? r.guest ? `${r.guest?.firstName} ${r.guest?.lastName}` : '').toLowerCase();
    const matchSearch =
      !search ||
      name.includes(search.toLowerCase()) ||
      (r.roomNumber ?? r.room?.roomNumber ?? '').includes(search) ||
      (r.reservationNumber ?? '').toLowerCase().includes(search.toLowerCase());
    const matchStatus = !statusFilter || r.status === statusFilter;
    return matchSearch && matchStatus;
  });

  const onSubmit = async (data: CreateFormData) => {
    setIsSubmitting(true);
    try {
      const payload: CreateReservationRequest = {
        guestId: Number(data.guestId),
        roomId: Number(data.roomId),
        checkInDate: data.checkInDate,
        checkOutDate: data.checkOutDate,
        specialRequests: data.specialRequests,
      };
      await reservationService.create(payload);
      toast.success('Reservation created', 'The reservation has been created successfully');
      setIsCreateOpen(false);
      reset();
      await fetchData();
    } catch (err) {
      toast.error('Failed to create reservation', err instanceof Error ? err.message : 'Please check the details and try again');
    } finally {
      setIsSubmitting(false);
    }
  };

  const columns = [
    {
      key: 'reservationNumber',
      header: 'Reference',
      render: (r: Reservation) => (
        <span className="font-mono text-xs bg-slate-100 dark:bg-slate-700 px-2 py-1 rounded-md">
          {r.reservationNumber ?? `#${String(r.id).padStart(6, '0')}`}
        </span>
      ),
    },
    {
      key: 'guestName',
      header: 'Guest',
      render: (r: Reservation) => {
        const name = r.guestName ?? (r.guest ? `${r.guest.firstName} ${r.guest.lastName}` : '—');
        return (
          <div className="flex items-center gap-2.5">
            <div className="w-7 h-7 rounded-full bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center flex-shrink-0">
              <span className="text-xs font-semibold text-indigo-600 dark:text-indigo-400">
                {name.split(' ').map((n: string) => n[0]).join('').slice(0, 2).toUpperCase()}
              </span>
            </div>
            <span className="font-medium text-slate-800 dark:text-slate-200 truncate">{name}</span>
          </div>
        );
      },
    },
    {
      key: 'roomNumber',
      header: 'Room',
      render: (r: Reservation) => (
        <span className="font-mono text-xs bg-slate-100 dark:bg-slate-700 px-2 py-1 rounded">
          {r.roomNumber ?? r.room?.roomNumber ?? '—'}
        </span>
      ),
    },
    {
      key: 'checkInDate',
      header: 'Check-in',
      render: (r: Reservation) => formatDate(r.checkInDate),
    },
    {
      key: 'checkOutDate',
      header: 'Check-out',
      render: (r: Reservation) => formatDate(r.checkOutDate),
    },
    {
      key: 'totalAmount',
      header: 'Amount',
      render: (r: Reservation) => (
        <span className="font-semibold text-slate-900 dark:text-slate-100">
          {formatCurrency(r.totalAmount ?? 0)}
        </span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (r: Reservation) => (
        <Badge variant={statusVariant[r.status] ?? 'default'} dot>
          {r.status}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: '',
      render: (r: Reservation) => (
        <Button
          variant="ghost"
          size="sm"
          leftIcon={<Eye className="h-3.5 w-3.5" />}
          onClick={(e) => {
            e.stopPropagation();
            setViewReservation(r);
          }}
        >
          View
        </Button>
      ),
    },
  ];

  return (
    <div className="page-container">
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Reservations</h2>
          <p className="page-subtitle">{reservations.length} total reservations</p>
        </div>
        <Button leftIcon={<Plus className="h-4 w-4" />} onClick={() => setIsCreateOpen(true)}>
          New Reservation
        </Button>
      </div>

      {/* Filters */}
      <Card className="mb-6" padding="sm">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              type="text"
              placeholder="Search by guest, room, or reference..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full h-10 pl-10 pr-4 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-slate-900 dark:text-slate-100 placeholder:text-slate-400 transition-all"
            />
            {search && (
              <button onClick={() => setSearch('')} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600">
                <X className="h-4 w-4" />
              </button>
            )}
          </div>
          <div className="sm:w-48">
            <Select
              options={STATUS_OPTIONS}
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
            />
          </div>
          <Button variant="outline" leftIcon={<Filter className="h-4 w-4" />}>
            More Filters
          </Button>
        </div>
      </Card>

      {/* Table */}
      <Card padding="none">
        <Table
          data={filtered}
          columns={columns}
          isLoading={isLoading}
          emptyMessage="No reservations found"
          emptyDescription="Create your first reservation to get started"
        />
      </Card>

      {/* Create Modal */}
      <Modal
        isOpen={isCreateOpen}
        onClose={() => { setIsCreateOpen(false); reset(); }}
        title="New Reservation"
        description="Fill in the details to create a new reservation"
        size="lg"
        footer={
          <>
            <Button variant="outline" onClick={() => { setIsCreateOpen(false); reset(); }} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button isLoading={isSubmitting} onClick={handleSubmit(onSubmit)}>
              Create Reservation
            </Button>
          </>
        }
      >
        <form className="space-y-5" onSubmit={handleSubmit(onSubmit)}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {/* Guest */}
            <div className="sm:col-span-2">
              <Select
                label="Guest"
                required
                options={[
                  { value: '', label: 'Select a guest...' },
                  ...guests.map((g) => ({
                    value: String(g.id),
                    label: `${g.firstName || g.fullName || '—'} — ${g.email ?? ''}`,
                  })),
                ]}
                {...register('guestId')}
                error={errors.guestId?.message}
              />
              {guests.length === 0 && (
                <p className="mt-1 text-xs text-amber-500">No guests found. Add guests in the Guests section first.</p>
              )}
            </div>

            {/* Room */}
            <div className="sm:col-span-2">
              <Select
                label="Room"
                required
                options={[
                  { value: '', label: 'Select a room...' },
                  ...rooms
                    .filter((r) => r.status === 'Available')
                    .map((r) => ({
                      value: String(r.id),
                      label: `Room ${r.roomNumber ?? r.number} — ${r.type}${r.floor ? ` (Floor ${r.floor})` : ''} — ${formatCurrency(r.pricePerNight)}/night`,
                    })),
                ]}
                {...register('roomId')}
                error={errors.roomId?.message}
              />
              {rooms.filter((r) => r.status === 'Available').length === 0 && (
                <p className="mt-1 text-xs text-amber-500">No available rooms found.</p>
              )}
            </div>

            {/* Dates */}
            <Input
              label="Check-in Date"
              type="date"
              required
              {...register('checkInDate')}
              error={errors.checkInDate?.message}
              min={new Date().toISOString().split('T')[0]}
            />
            <Input
              label="Check-out Date"
              type="date"
              required
              {...register('checkOutDate')}
              error={errors.checkOutDate?.message}
              min={checkIn || new Date().toISOString().split('T')[0]}
            />

            {/* Adults / Children */}
            <Input
              label="Adults"
              type="number"
              min={1}
              max={10}
              {...register('adults', { valueAsNumber: true })}
              error={errors.adults?.message}
            />
            <Input
              label="Children"
              type="number"
              min={0}
              max={10}
              {...register('children', { valueAsNumber: true })}
              error={errors.children?.message}
            />

            {/* Special Requests */}
            <div className="sm:col-span-2">
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">
                Special Requests
              </label>
              <textarea
                {...register('specialRequests')}
                placeholder="Any special requirements or notes..."
                rows={3}
                className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-3 py-2 text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
              />
            </div>
          </div>

          {/* Estimated total */}
          {estimatedTotal > 0 && (
            <div className="p-4 bg-indigo-50 dark:bg-indigo-900/20 rounded-xl border border-indigo-100 dark:border-indigo-800">
              <div className="flex items-center justify-between text-sm">
                <span className="text-indigo-700 dark:text-indigo-300 font-medium">
                  Estimated Total ({nights} night{nights !== 1 ? 's' : ''})
                </span>
                <span className="text-xl font-bold text-indigo-700 dark:text-indigo-300">
                  {formatCurrency(estimatedTotal)}
                </span>
              </div>
            </div>
          )}
        </form>
      </Modal>

      {/* View Modal */}
      <Modal
        isOpen={!!viewReservation}
        onClose={() => setViewReservation(null)}
        title="Reservation Details"
        size="lg"
      >
        {viewReservation && (
          <div className="space-y-5">
            <div className="flex items-start justify-between">
              <div>
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">Reference</p>
                <p className="font-mono font-semibold text-slate-900 dark:text-slate-100">
                  {viewReservation.reservationNumber ?? `#${String(viewReservation.id).padStart(6, '0')}`}
                </p>
              </div>
              <Badge variant={statusVariant[viewReservation.status] ?? 'default'} dot>
                {viewReservation.status}
              </Badge>
            </div>

            <div className="grid grid-cols-2 gap-4">
              {[
                { icon: <User className="h-4 w-4" />, label: 'Guest', value: viewReservation.guestName ?? '—' },
                { icon: <BedDouble className="h-4 w-4" />, label: 'Room', value: viewReservation.roomNumber ?? viewReservation.room?.roomNumber ?? '—' },
                { icon: <Calendar className="h-4 w-4" />, label: 'Check-in', value: formatDate(viewReservation.checkInDate) },
                { icon: <Calendar className="h-4 w-4" />, label: 'Check-out', value: formatDate(viewReservation.checkOutDate) },
                { icon: <User className="h-4 w-4" />, label: 'Guests', value: `${viewReservation.adults ?? 1} adult(s), ${viewReservation.children ?? 0} child(ren)` },
                { icon: <DollarSign className="h-4 w-4" />, label: 'Total Amount', value: formatCurrency(viewReservation.totalAmount ?? 0) },
              ].map((item) => (
                <div key={item.label} className="p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                  <div className="flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400 mb-1">
                    {item.icon} {item.label}
                  </div>
                  <p className="font-semibold text-slate-900 dark:text-slate-100 text-sm">{item.value}</p>
                </div>
              ))}
            </div>

            {viewReservation.specialRequests && (
              <div className="p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">Special Requests</p>
                <p className="text-sm text-slate-800 dark:text-slate-200">{viewReservation.specialRequests}</p>
              </div>
            )}

            <div className="p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
              <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">Created</p>
              <p className="text-sm text-slate-800 dark:text-slate-200">{formatDate(viewReservation.createdAt)}</p>
            </div>

            {/* Status Update */}
            <div className="pt-4 border-t border-slate-200 dark:border-slate-700">
              <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide mb-3">Update Status</p>
              <div className="flex flex-wrap gap-2">
                {['Confirmed', 'CheckedIn', 'CheckedOut', 'Cancelled'].map((s) => (
                  <button
                    key={s}
                    onClick={async () => {
                      try {
                        await reservationService.updateStatus(viewReservation.id, s);
                        toast.success('Status updated', `Reservation status changed to ${s}`);
                        setViewReservation({ ...viewReservation, status: s as Reservation['status'] });
                        await fetchData();
                      } catch (err) {
                        toast.error('Update failed', err instanceof Error ? err.message : 'Could not update reservation status');
                      }
                    }}
                    className={cn(
                      'px-3 py-1.5 rounded-lg text-xs font-medium transition-all',
                      viewReservation.status === s
                        ? 'ring-2 ring-indigo-500 ring-offset-1'
                        : 'hover:opacity-80',
                      RESERVATION_STATUS_COLORS[s]
                    )}
                  >
                    {s === 'CheckedIn' ? 'Check In' : s === 'CheckedOut' ? 'Check Out' : s}
                  </button>
                ))}
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}

export default ReservationsPage;

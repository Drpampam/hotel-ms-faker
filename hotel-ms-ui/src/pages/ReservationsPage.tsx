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
  Edit2,
  CreditCard,
  CheckCircle,
  Clock,
  XCircle,
} from 'lucide-react';
import { reservationService } from '../services/reservation.service';
import { paymentService } from '../services/payment.service';
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
import type { Reservation, Guest, Room, CreateReservationRequest, Payment } from '../types';
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

const PAYMENT_METHODS = [
  { value: '', label: 'Select payment method...' },
  { value: 'Cash', label: 'Cash' },
  { value: 'Card', label: 'Credit / Debit Card' },
  { value: 'BankTransfer', label: 'Bank Transfer' },
  { value: 'Online', label: 'Online Payment' },
  { value: 'Other', label: 'Other' },
];

const statusVariant: Record<string, 'warning' | 'info' | 'success' | 'default' | 'danger' | 'secondary'> = {
  Pending: 'warning',
  Confirmed: 'info',
  CheckedIn: 'success',
  CheckedOut: 'default',
  Cancelled: 'danger',
  NoShow: 'secondary',
};

const paymentStateVariant: Record<string, 'success' | 'warning' | 'danger' | 'info' | 'default'> = {
  Completed: 'success',
  Processing: 'warning',
  Pending: 'info',
  Failed: 'danger',
  Refunded: 'default',
};

const today = new Date().toISOString().split('T')[0];

// ── Create reservation schema ────────────────────────────────────────────────
const createSchema = z.object({
  guestId: z.string().min(1, 'Guest is required'),
  roomId: z.string().min(1, 'Room is required'),
  checkInDate: z.string()
    .min(1, 'Check-in date is required')
    .refine((d) => d >= today, { message: 'Check-in date cannot be in the past' }),
  checkOutDate: z.string().min(1, 'Check-out date is required'),
  adults: z.number().min(1, 'At least 1 adult required').max(10).default(1),
  children: z.number().min(0).max(10).default(0),
  specialRequests: z.string().max(500).optional(),
}).refine((d) => !d.checkInDate || !d.checkOutDate || d.checkOutDate > d.checkInDate, {
  message: 'Check-out date must be after check-in date',
  path: ['checkOutDate'],
});
type CreateFormData = z.infer<typeof createSchema>;

// ── Edit reservation schema ──────────────────────────────────────────────────
const editSchema = z.object({
  roomId: z.string().min(1, 'Room is required'),
  checkInDate: z.string().min(1, 'Check-in date is required'),
  checkOutDate: z.string().min(1, 'Check-out date is required'),
  specialRequests: z.string().max(500).optional(),
}).refine((d) => !d.checkInDate || !d.checkOutDate || d.checkOutDate > d.checkInDate, {
  message: 'Check-out date must be after check-in date',
  path: ['checkOutDate'],
});
type EditFormData = z.infer<typeof editSchema>;

// ── Payment capture schema ───────────────────────────────────────────────────
const paymentSchema = z.object({
  paymentMethod: z.string().min(1, 'Payment method is required'),
  amount: z.number({ invalid_type_error: 'Amount is required' }).min(0.01, 'Amount must be greater than 0'),
  transactionId: z.string().optional(),
});
type PaymentFormData = z.infer<typeof paymentSchema>;

export function ReservationsPage() {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [guests, setGuests] = useState<Guest[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [viewReservation, setViewReservation] = useState<Reservation | null>(null);
  const [editReservation, setEditReservation] = useState<Reservation | null>(null);
  const [paymentReservation, setPaymentReservation] = useState<Reservation | null>(null);
  const [reservationPayments, setReservationPayments] = useState<Payment[]>([]);
  const [isLoadingPayments, setIsLoadingPayments] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const toast = useToast();
  useSlowConnection(isLoading);

  // ── Create form ─────────────────────────────────────────────────────────────
  const {
    register: registerCreate,
    handleSubmit: handleCreate,
    reset: resetCreate,
    watch: watchCreate,
    formState: { errors: createErrors },
  } = useForm<CreateFormData>({ resolver: zodResolver(createSchema), defaultValues: { adults: 1, children: 0 } });

  const checkInCreate = watchCreate('checkInDate');
  const checkOutCreate = watchCreate('checkOutDate');
  const selectedRoomIdCreate = watchCreate('roomId');
  const selectedRoomCreate = rooms.find((r) => String(r.id) === selectedRoomIdCreate);
  const nightsCreate = checkInCreate && checkOutCreate ? calculateNights(checkInCreate, checkOutCreate) : 0;
  const estimatedTotalCreate = selectedRoomCreate && nightsCreate > 0 ? selectedRoomCreate.pricePerNight * nightsCreate : 0;

  // ── Edit form ───────────────────────────────────────────────────────────────
  const {
    register: registerEdit,
    handleSubmit: handleEdit,
    reset: resetEdit,
    watch: watchEdit,
    formState: { errors: editErrors },
  } = useForm<EditFormData>({ resolver: zodResolver(editSchema) });

  const checkInEdit = watchEdit('checkInDate');
  const checkOutEdit = watchEdit('checkOutDate');
  const selectedRoomIdEdit = watchEdit('roomId');
  const selectedRoomEdit = rooms.find((r) => String(r.id) === selectedRoomIdEdit);
  const nightsEdit = checkInEdit && checkOutEdit ? calculateNights(checkInEdit, checkOutEdit) : 0;
  const estimatedTotalEdit = selectedRoomEdit && nightsEdit > 0 ? selectedRoomEdit.pricePerNight * nightsEdit : 0;

  // ── Payment form ────────────────────────────────────────────────────────────
  const {
    register: registerPayment,
    handleSubmit: handlePayment,
    reset: resetPayment,
    formState: { errors: paymentErrors },
  } = useForm<PaymentFormData>({ resolver: zodResolver(paymentSchema) });

  // ── Data fetching ───────────────────────────────────────────────────────────
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

  useEffect(() => { fetchData(); }, [fetchData]);

  // Load payments when view modal opens
  useEffect(() => {
    if (!viewReservation) { setReservationPayments([]); return; }
    setIsLoadingPayments(true);
    paymentService.getByReservation(viewReservation.id)
      .then(setReservationPayments)
      .catch(() => setReservationPayments([]))
      .finally(() => setIsLoadingPayments(false));
  }, [viewReservation]);

  // Pre-fill edit form when edit modal opens
  useEffect(() => {
    if (!editReservation) return;
    resetEdit({
      roomId: String(editReservation.roomId),
      checkInDate: editReservation.checkInDate?.split('T')[0] ?? '',
      checkOutDate: editReservation.checkOutDate?.split('T')[0] ?? '',
      specialRequests: editReservation.specialRequests ?? '',
    });
  }, [editReservation, resetEdit]);

  // Pre-fill payment amount when payment modal opens
  useEffect(() => {
    if (!paymentReservation) return;
    resetPayment({
      paymentMethod: '',
      amount: paymentReservation.totalAmount ?? paymentReservation.totalPrice ?? 0,
      transactionId: '',
    });
  }, [paymentReservation, resetPayment]);

  // ── Filtered list ───────────────────────────────────────────────────────────
  const filtered = reservations.filter((r) => {
    const name = (r.guestName ?? (r.guest ? `${r.guest.firstName} ${r.guest.lastName}` : '')).toLowerCase();
    const matchSearch =
      !search ||
      name.includes(search.toLowerCase()) ||
      (r.roomNumber ?? r.room?.roomNumber ?? '').includes(search) ||
      (r.reservationNumber ?? '').toLowerCase().includes(search.toLowerCase());
    const matchStatus = !statusFilter || r.status === statusFilter;
    return matchSearch && matchStatus;
  });

  // ── Handlers ────────────────────────────────────────────────────────────────
  const onCreateSubmit = async (data: CreateFormData) => {
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
      resetCreate();
      await fetchData();
    } catch (err) {
      toast.error('Failed to create reservation', err instanceof Error ? err.message : 'Please check the details and try again');
    } finally {
      setIsSubmitting(false);
    }
  };

  const onEditSubmit = async (data: EditFormData) => {
    if (!editReservation) return;
    setIsSubmitting(true);
    try {
      await reservationService.update({
        id: editReservation.id,
        roomId: Number(data.roomId),
        checkInDate: data.checkInDate,
        checkOutDate: data.checkOutDate,
        specialRequests: data.specialRequests,
      });
      toast.success('Reservation updated', 'Changes saved successfully');
      setEditReservation(null);
      setViewReservation(null);
      await fetchData();
    } catch (err) {
      toast.error('Update failed', err instanceof Error ? err.message : 'Could not save changes');
    } finally {
      setIsSubmitting(false);
    }
  };

  const onPaymentSubmit = async (data: PaymentFormData) => {
    if (!paymentReservation) return;
    setIsSubmitting(true);
    try {
      await paymentService.capture({
        reservationId: paymentReservation.id,
        paymentMethod: data.paymentMethod,
        amount: data.amount,
        transactionId: data.transactionId || undefined,
      });
      // Advance reservation to CheckedOut
      await reservationService.updateStatus(paymentReservation.id, 'CheckedOut');
      toast.success('Payment captured & checked out', `Payment of ${formatCurrency(data.amount)} recorded successfully`);
      setPaymentReservation(null);
      setViewReservation(null);
      resetPayment();
      await fetchData();
    } catch (err) {
      toast.error('Payment failed', err instanceof Error ? err.message : 'Could not process payment');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleStatusChange = async (reservation: Reservation, status: string) => {
    if (status === 'CheckedOut') {
      // Intercept — show payment modal instead
      setPaymentReservation(reservation);
      return;
    }
    try {
      await reservationService.updateStatus(reservation.id, status);
      toast.success('Status updated', `Reservation status changed to ${status}`);
      setViewReservation({ ...reservation, status: status as Reservation['status'] });
      await fetchData();
    } catch (err) {
      toast.error('Update failed', err instanceof Error ? err.message : 'Could not update reservation status');
    }
  };

  // ── Columns ─────────────────────────────────────────────────────────────────
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
          onClick={(e) => { e.stopPropagation(); setViewReservation(r); }}
        >
          View
        </Button>
      ),
    },
  ];

  // ── Render ──────────────────────────────────────────────────────────────────
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
            <Select options={STATUS_OPTIONS} value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} />
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

      {/* ── Create Modal ───────────────────────────────────────────────────── */}
      <Modal
        isOpen={isCreateOpen}
        onClose={() => { setIsCreateOpen(false); resetCreate(); }}
        title="New Reservation"
        description="Fill in the details to create a new reservation"
        size="lg"
        footer={
          <>
            <Button variant="outline" onClick={() => { setIsCreateOpen(false); resetCreate(); }} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button isLoading={isSubmitting} onClick={handleCreate(onCreateSubmit)}>
              Create Reservation
            </Button>
          </>
        }
      >
        <form className="space-y-5" onSubmit={handleCreate(onCreateSubmit)}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="sm:col-span-2">
              <Select
                label="Guest"
                required
                options={[
                  { value: '', label: 'Select a guest...' },
                  ...guests.map((g) => ({ value: String(g.id), label: `${g.firstName || g.fullName || '—'} — ${g.email ?? ''}` })),
                ]}
                {...registerCreate('guestId')}
                error={createErrors.guestId?.message}
              />
              {guests.length === 0 && (
                <p className="mt-1 text-xs text-amber-500">No guests found. Add guests in the Guests section first.</p>
              )}
            </div>
            <div className="sm:col-span-2">
              <Select
                label="Room"
                required
                options={[
                  { value: '', label: 'Select a room...' },
                  ...rooms.filter((r) => r.status === 'Available').map((r) => ({
                    value: String(r.id),
                    label: `Room ${r.roomNumber ?? r.number} — ${r.type}${r.floor ? ` (Floor ${r.floor})` : ''} — ${formatCurrency(r.pricePerNight)}/night`,
                  })),
                ]}
                {...registerCreate('roomId')}
                error={createErrors.roomId?.message}
              />
            </div>
            <Input label="Check-in Date" type="date" required {...registerCreate('checkInDate')} error={createErrors.checkInDate?.message} min={today} />
            <Input label="Check-out Date" type="date" required {...registerCreate('checkOutDate')} error={createErrors.checkOutDate?.message} min={checkInCreate || today} />
            <Input label="Adults" type="number" min={1} max={10} {...registerCreate('adults', { valueAsNumber: true })} error={createErrors.adults?.message} />
            <Input label="Children" type="number" min={0} max={10} {...registerCreate('children', { valueAsNumber: true })} error={createErrors.children?.message} />
            <div className="sm:col-span-2">
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">Special Requests</label>
              <textarea
                {...registerCreate('specialRequests')}
                placeholder="Any special requirements or notes..."
                rows={3}
                className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-3 py-2 text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
              />
            </div>
          </div>
          {estimatedTotalCreate > 0 && (
            <div className="p-4 bg-indigo-50 dark:bg-indigo-900/20 rounded-xl border border-indigo-100 dark:border-indigo-800">
              <div className="flex items-center justify-between text-sm">
                <span className="text-indigo-700 dark:text-indigo-300 font-medium">
                  Estimated Total ({nightsCreate} night{nightsCreate !== 1 ? 's' : ''})
                </span>
                <span className="text-xl font-bold text-indigo-700 dark:text-indigo-300">
                  {formatCurrency(estimatedTotalCreate)}
                </span>
              </div>
            </div>
          )}
        </form>
      </Modal>

      {/* ── View Modal ─────────────────────────────────────────────────────── */}
      <Modal
        isOpen={!!viewReservation && !editReservation && !paymentReservation}
        onClose={() => setViewReservation(null)}
        title="Reservation Details"
        size="lg"
      >
        {viewReservation && (
          <div className="space-y-5">
            {/* Header row */}
            <div className="flex items-start justify-between">
              <div>
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">Reference</p>
                <p className="font-mono font-semibold text-slate-900 dark:text-slate-100">
                  {viewReservation.reservationNumber ?? `#${String(viewReservation.id).padStart(6, '0')}`}
                </p>
              </div>
              <div className="flex items-center gap-2">
                <Badge variant={statusVariant[viewReservation.status] ?? 'default'} dot>
                  {viewReservation.status}
                </Badge>
                <button
                  onClick={() => setEditReservation(viewReservation)}
                  className="p-1.5 rounded-lg text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 transition-colors"
                  title="Edit reservation"
                >
                  <Edit2 className="h-4 w-4" />
                </button>
              </div>
            </div>

            {/* Detail grid */}
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

            {/* Payment history */}
            <div className="border-t border-slate-200 dark:border-slate-700 pt-4">
              <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide mb-3 flex items-center gap-1.5">
                <CreditCard className="h-3.5 w-3.5" /> Payment History
              </p>
              {isLoadingPayments ? (
                <p className="text-sm text-slate-400">Loading payments...</p>
              ) : reservationPayments.length === 0 ? (
                <p className="text-sm text-slate-400 italic">No payments recorded yet.</p>
              ) : (
                <div className="space-y-2">
                  {reservationPayments.map((p) => (
                    <div key={p.id} className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-700/50 rounded-lg">
                      <div>
                        <p className="text-sm font-medium text-slate-800 dark:text-slate-200">{p.paymentMethod}</p>
                        <p className="text-xs text-slate-500 dark:text-slate-400">{formatDate(p.paymentDate)}</p>
                        {p.transactionId && (
                          <p className="text-xs text-slate-400 font-mono">{p.transactionId}</p>
                        )}
                      </div>
                      <div className="text-right">
                        <p className="text-sm font-bold text-slate-900 dark:text-slate-100">{formatCurrency(p.amount)}</p>
                        <Badge variant={paymentStateVariant[p.paymentState] ?? 'default'} size="sm">
                          {p.paymentState === 'Completed' ? (
                            <span className="flex items-center gap-1"><CheckCircle className="h-3 w-3" />{p.paymentState}</span>
                          ) : p.paymentState === 'Failed' ? (
                            <span className="flex items-center gap-1"><XCircle className="h-3 w-3" />{p.paymentState}</span>
                          ) : (
                            <span className="flex items-center gap-1"><Clock className="h-3 w-3" />{p.paymentState}</span>
                          )}
                        </Badge>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Status update actions */}
            <div className="border-t border-slate-200 dark:border-slate-700 pt-4">
              <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide mb-3">Update Status</p>
              <div className="flex flex-wrap gap-2">
                {['Confirmed', 'CheckedIn', 'CheckedOut', 'Cancelled'].map((s) => (
                  <button
                    key={s}
                    onClick={() => handleStatusChange(viewReservation, s)}
                    className={cn(
                      'px-3 py-1.5 rounded-lg text-xs font-medium transition-all flex items-center gap-1.5',
                      viewReservation.status === s ? 'ring-2 ring-indigo-500 ring-offset-1' : 'hover:opacity-80',
                      RESERVATION_STATUS_COLORS[s]
                    )}
                  >
                    {s === 'CheckedOut' && <CreditCard className="h-3 w-3" />}
                    {s === 'CheckedIn' ? 'Check In' : s === 'CheckedOut' ? 'Check Out & Pay' : s}
                  </button>
                ))}
              </div>
            </div>
          </div>
        )}
      </Modal>

      {/* ── Edit Modal ─────────────────────────────────────────────────────── */}
      <Modal
        isOpen={!!editReservation}
        onClose={() => setEditReservation(null)}
        title="Edit Reservation"
        description="Correct reservation details — dates and room can only be changed before check-in"
        size="lg"
        footer={
          <>
            <Button variant="outline" onClick={() => setEditReservation(null)} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button isLoading={isSubmitting} onClick={handleEdit(onEditSubmit)}>
              Save Changes
            </Button>
          </>
        }
      >
        <form className="space-y-5" onSubmit={handleEdit(onEditSubmit)}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="sm:col-span-2">
              <Select
                label="Room"
                required
                options={[
                  { value: '', label: 'Select a room...' },
                  // Include current room even if not available, plus all available rooms
                  ...rooms
                    .filter((r) => r.status === 'Available' || r.id === editReservation?.roomId)
                    .map((r) => ({
                      value: String(r.id),
                      label: `Room ${r.roomNumber ?? r.number} — ${r.type} — ${formatCurrency(r.pricePerNight)}/night${r.id === editReservation?.roomId ? ' (current)' : ''}`,
                    })),
                ]}
                {...registerEdit('roomId')}
                error={editErrors.roomId?.message}
              />
            </div>
            <Input label="Check-in Date" type="date" required {...registerEdit('checkInDate')} error={editErrors.checkInDate?.message} />
            <Input label="Check-out Date" type="date" required {...registerEdit('checkOutDate')} error={editErrors.checkOutDate?.message} min={checkInEdit} />
            <div className="sm:col-span-2">
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">Special Requests</label>
              <textarea
                {...registerEdit('specialRequests')}
                placeholder="Any special requirements or notes..."
                rows={3}
                className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-3 py-2 text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
              />
            </div>
          </div>
          {estimatedTotalEdit > 0 && (
            <div className="p-4 bg-indigo-50 dark:bg-indigo-900/20 rounded-xl border border-indigo-100 dark:border-indigo-800">
              <div className="flex items-center justify-between text-sm">
                <span className="text-indigo-700 dark:text-indigo-300 font-medium">
                  New Estimated Total ({nightsEdit} night{nightsEdit !== 1 ? 's' : ''})
                </span>
                <span className="text-xl font-bold text-indigo-700 dark:text-indigo-300">
                  {formatCurrency(estimatedTotalEdit)}
                </span>
              </div>
            </div>
          )}
        </form>
      </Modal>

      {/* ── Payment Capture Modal ─────────────────────────────────────────── */}
      <Modal
        isOpen={!!paymentReservation}
        onClose={() => { setPaymentReservation(null); resetPayment(); }}
        title="Capture Payment & Check Out"
        description="Record payment details before checking out the guest"
        size="md"
        footer={
          <>
            <Button variant="outline" onClick={() => { setPaymentReservation(null); resetPayment(); }} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button
              isLoading={isSubmitting}
              onClick={handlePayment(onPaymentSubmit)}
              leftIcon={<CreditCard className="h-4 w-4" />}
            >
              Confirm Payment & Check Out
            </Button>
          </>
        }
      >
        {paymentReservation && (
          <form className="space-y-5" onSubmit={handlePayment(onPaymentSubmit)}>
            {/* Summary */}
            <div className="p-4 bg-slate-50 dark:bg-slate-700/50 rounded-xl space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-slate-500 dark:text-slate-400">Guest</span>
                <span className="font-medium text-slate-800 dark:text-slate-200">{paymentReservation.guestName ?? '—'}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-slate-500 dark:text-slate-400">Room</span>
                <span className="font-medium text-slate-800 dark:text-slate-200">{paymentReservation.roomNumber ?? '—'}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-slate-500 dark:text-slate-400">Stay</span>
                <span className="font-medium text-slate-800 dark:text-slate-200">
                  {formatDate(paymentReservation.checkInDate)} → {formatDate(paymentReservation.checkOutDate)}
                </span>
              </div>
              <div className="flex justify-between text-sm border-t border-slate-200 dark:border-slate-600 pt-2 mt-2">
                <span className="font-semibold text-slate-700 dark:text-slate-300">Total Due</span>
                <span className="font-bold text-lg text-indigo-600 dark:text-indigo-400">
                  {formatCurrency(paymentReservation.totalAmount ?? 0)}
                </span>
              </div>
            </div>

            {/* Payment fields */}
            <Select
              label="Payment Method"
              required
              options={PAYMENT_METHODS}
              {...registerPayment('paymentMethod')}
              error={paymentErrors.paymentMethod?.message}
            />
            <Input
              label="Amount"
              type="number"
              step="0.01"
              required
              {...registerPayment('amount', { valueAsNumber: true })}
              error={paymentErrors.amount?.message}
            />
            <Input
              label="Transaction / Reference ID"
              placeholder="Optional — for card or bank transfer"
              {...registerPayment('transactionId')}
              error={paymentErrors.transactionId?.message}
            />
          </form>
        )}
      </Modal>
    </div>
  );
}

export default ReservationsPage;

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
  Receipt,
  Trash2,
  FileText,
  Printer,
} from 'lucide-react';
import { reservationService } from '../services/reservation.service';
import { paymentService } from '../services/payment.service';
import { billingService } from '../services/billing.service';
import { guestService } from '../services/guest.service';
import { roomService } from '../services/room.service';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Select } from '../components/ui/Select';
import { Badge } from '../components/ui/Badge';
import { Table } from '../components/ui/Table';
import { Modal } from '../components/ui/Modal';
import { Card } from '../components/ui/Card';
import { useToast, useAuthStore } from '../lib/store';
import type { Reservation, ReservationExpense, Guest, Room, CreateReservationRequest, Payment, Invoice } from '../types';
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

// ── Expense schema ───────────────────────────────────────────────────────────
const expenseSchema = z.object({
  description: z.string().min(1, 'Description is required').max(255),
  category: z.string().optional(),
  quantity: z.number({ invalid_type_error: 'Quantity is required' }).int().min(1, 'Quantity must be at least 1'),
  unitPrice: z.number({ invalid_type_error: 'Unit price is required' }).min(0.01, 'Price must be greater than 0'),
});
type ExpenseFormData = z.infer<typeof expenseSchema>;

const EXPENSE_CATEGORIES = [
  { value: '', label: 'Select category...' },
  { value: 'Food', label: 'Food & Beverage' },
  { value: 'Minibar', label: 'Minibar' },
  { value: 'Laundry', label: 'Laundry' },
  { value: 'Spa', label: 'Spa & Wellness' },
  { value: 'Transport', label: 'Transport' },
  { value: 'Phone', label: 'Phone / Internet' },
  { value: 'Other', label: 'Other' },
];

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
  const [reservationExpenses, setReservationExpenses] = useState<ReservationExpense[]>([]);
  const [isLoadingExpenses, setIsLoadingExpenses] = useState(false);
  const [showAddExpense, setShowAddExpense] = useState(false);
  const [reservationInvoice, setReservationInvoice] = useState<Invoice | null>(null);
  const [isLoadingInvoice, setIsLoadingInvoice] = useState(false);
  const [isGeneratingInvoice, setIsGeneratingInvoice] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const toast = useToast();
  const { user } = useAuthStore();
  const isGuestRole = user?.roles?.includes('Guest') ?? false;
  useSlowConnection(isLoading);

  // ── Create form ─────────────────────────────────────────────────────────────
  const {
    register: registerCreate,
    handleSubmit: handleCreate,
    reset: resetCreate,
    watch: watchCreate,
    setValue: setValueCreate,
    formState: { errors: createErrors },
  } = useForm<CreateFormData>({ resolver: zodResolver(createSchema), defaultValues: { adults: 1, children: 0 } });

  const checkInCreate = watchCreate('checkInDate');
  const checkOutCreate = watchCreate('checkOutDate');
  const selectedRoomIdCreate = watchCreate('roomId');
  const selectedRoomCreate = rooms.find((r) => String(r.id) === selectedRoomIdCreate);
  const nightsCreate = checkInCreate && checkOutCreate ? calculateNights(checkInCreate, checkOutCreate) : 0;
  const estimatedTotalCreate = selectedRoomCreate && nightsCreate > 0 ? selectedRoomCreate.pricePerNight * nightsCreate : 0;

  // Compute blocked room IDs for the selected date range
  const blockedRoomIds = new Set(
    (checkInCreate && checkOutCreate)
      ? reservations
          .filter((r) =>
            ['Pending', 'Confirmed', 'CheckedIn'].includes(r.status) &&
            r.checkInDate < checkOutCreate &&
            r.checkOutDate > checkInCreate
          )
          .map((r) => r.roomId)
      : []
  );
  const availableRoomsForDates = rooms.filter((r) =>
    r.roomState === 'Available' && !blockedRoomIds.has(r.id)
  );

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

  // ── Expense form ─────────────────────────────────────────────────────────────
  const {
    register: registerExpense,
    handleSubmit: handleExpense,
    reset: resetExpense,
    watch: watchExpense,
    formState: { errors: expenseErrors },
  } = useForm<ExpenseFormData>({ resolver: zodResolver(expenseSchema), defaultValues: { quantity: 1 } });

  const expenseQty = watchExpense('quantity');
  const expensePrice = watchExpense('unitPrice');
  const expenseLineTotal = (expenseQty > 0 && expensePrice > 0) ? expenseQty * expensePrice : 0;

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
      const loadedGuests = guestList.status === 'fulfilled' ? guestList.value : [];
      setGuests(loadedGuests);
      setRooms(roomList.status === 'fulfilled' ? roomList.value : []);

      // Auto-select the logged-in guest when the user has the Guest role
      if (isGuestRole && user?.email) {
        const myGuest = loadedGuests.find(
          (g) => (g.email ?? '').toLowerCase() === user.email.toLowerCase()
        );
        if (myGuest) {
          setValueCreate('guestId', String(myGuest.id), { shouldValidate: true });
        }
      }
    } catch {
      // leave state as empty arrays
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => { fetchData(); }, [fetchData]);

  // Load payments and expenses when view modal opens
  useEffect(() => {
    if (!viewReservation) {
      setReservationPayments([]);
      setReservationExpenses([]);
      setReservationInvoice(null);
      setShowAddExpense(false);
      return;
    }
    setIsLoadingPayments(true);
    setIsLoadingExpenses(true);
    setIsLoadingInvoice(true);
    paymentService.getByReservation(viewReservation.id)
      .then(setReservationPayments)
      .catch(() => setReservationPayments([]))
      .finally(() => setIsLoadingPayments(false));
    reservationService.getExpenses(viewReservation.id)
      .then(setReservationExpenses)
      .catch(() => setReservationExpenses([]))
      .finally(() => setIsLoadingExpenses(false));
    billingService.getByReservation(viewReservation.id)
      .then((inv) => setReservationInvoice(inv))
      .catch(() => setReservationInvoice(null))
      .finally(() => setIsLoadingInvoice(false));
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

  // Pre-fill payment amount when payment modal opens (use grandTotal = room + expenses)
  useEffect(() => {
    if (!paymentReservation) return;
    const total = paymentReservation.grandTotal ?? paymentReservation.totalAmount ?? paymentReservation.totalPrice ?? 0;
    resetPayment({
      paymentMethod: '',
      amount: total,
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

  const onAddExpenseSubmit = async (data: ExpenseFormData) => {
    if (!viewReservation) return;
    setIsSubmitting(true);
    try {
      const newExpense = await reservationService.addExpense(viewReservation.id, {
        description: data.description,
        category: data.category || undefined,
        quantity: data.quantity,
        unitPrice: data.unitPrice,
      });
      setReservationExpenses((prev) => [...prev, newExpense]);
      // Update viewReservation totals locally
      const newExpensesTotal = reservationExpenses.reduce((s, e) => s + e.amount, 0) + newExpense.amount;
      setViewReservation({ ...viewReservation, expensesTotal: newExpensesTotal, grandTotal: viewReservation.totalPrice + newExpensesTotal });
      resetExpense({ quantity: 1, unitPrice: 0, description: '', category: '' });
      setShowAddExpense(false);
      toast.success('Expense added', `${data.description} (${formatCurrency(newExpense.amount)}) added to reservation`);
    } catch (err) {
      toast.error('Failed to add expense', err instanceof Error ? err.message : 'Please try again');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteExpense = async (expenseId: number) => {
    if (!viewReservation) return;
    try {
      await reservationService.deleteExpense(viewReservation.id, expenseId);
      const updated = reservationExpenses.filter((e) => e.id !== expenseId);
      setReservationExpenses(updated);
      const newExpensesTotal = updated.reduce((s, e) => s + e.amount, 0);
      setViewReservation({ ...viewReservation, expensesTotal: newExpensesTotal, grandTotal: viewReservation.totalPrice + newExpensesTotal });
      toast.success('Expense removed', 'Expense deleted from reservation');
    } catch (err) {
      toast.error('Failed to delete expense', err instanceof Error ? err.message : 'Please try again');
    }
  };

  const handleRefundPayment = async (paymentId: number) => {
    if (!confirm('Issue a refund for this payment? This cannot be undone.')) return;
    try {
      const updated = await paymentService.refund(paymentId);
      setReservationPayments((prev) => prev.map((p) => (p.id === paymentId ? updated : p)));
      toast.success('Refund issued', 'Payment has been marked as refunded');
    } catch (err) {
      toast.error('Refund failed', err instanceof Error ? err.message : 'Could not process refund');
    }
  };

  const handleGenerateInvoice = async () => {
    if (!viewReservation) return;
    setIsGeneratingInvoice(true);
    try {
      const invoice = await billingService.generateInvoice(viewReservation.id);
      setReservationInvoice(invoice);
      toast.success('Invoice generated', invoice.invoiceNumber);
    } catch (err) {
      toast.error('Failed to generate invoice', err instanceof Error ? err.message : 'Please try again');
    } finally {
      setIsGeneratingInvoice(false);
    }
  };

  const handlePrintInvoice = (invoice: Invoice) => {
    const { formatCurrency: fc, formatDate: fd } = { formatCurrency, formatDate };
    const lineItemsHtml = invoice.lineItems.map((item) => `
      <tr>
        <td style="padding:8px 4px;border-bottom:1px solid #e2e8f0">${item.description}</td>
        <td style="padding:8px 4px;border-bottom:1px solid #e2e8f0;color:#64748b">${item.category}</td>
        <td style="padding:8px 4px;border-bottom:1px solid #e2e8f0;text-align:right">${item.quantity} × ${fc(item.unitPrice)}</td>
        <td style="padding:8px 4px;border-bottom:1px solid #e2e8f0;text-align:right;font-weight:600">${fc(item.amount)}</td>
      </tr>`).join('');

    const html = `<!DOCTYPE html><html><head><title>Invoice ${invoice.invoiceNumber}</title>
      <style>
        body{font-family:Inter,sans-serif;margin:0;padding:32px;color:#0f172a;font-size:14px}
        h1{font-size:22px;font-weight:700;margin:0 0 4px}
        .meta{color:#64748b;font-size:13px;margin-bottom:24px}
        .section-label{font-size:11px;font-weight:600;color:#64748b;text-transform:uppercase;letter-spacing:.05em;margin-bottom:8px}
        .guest-box{background:#f8fafc;border-radius:8px;padding:12px 16px;margin-bottom:24px}
        table{width:100%;border-collapse:collapse;margin-bottom:24px}
        th{text-align:left;padding:8px 4px;border-bottom:2px solid #e2e8f0;font-size:12px;color:#64748b;font-weight:600}
        .total-row td{padding:6px 4px;font-size:13px}
        .grand-total td{padding:10px 4px;font-size:15px;font-weight:700;border-top:2px solid #e2e8f0;color:#4f46e5}
        @media print{@page{margin:20mm}}
      </style></head><body>
      <h1>${invoice.invoiceNumber}</h1>
      <div class="meta">Issued ${fd(invoice.issueDate)} · Due ${fd(invoice.dueDate)} · Status: ${invoice.status}</div>
      ${invoice.guestName ? `<div class="section-label">Billed To</div>
        <div class="guest-box"><strong>${invoice.guestName}</strong>${invoice.guestEmail ? `<br><span style="color:#64748b">${invoice.guestEmail}</span>` : ''}</div>` : ''}
      <div class="section-label">Line Items</div>
      <table><thead><tr><th>Description</th><th>Category</th><th style="text-align:right">Qty × Price</th><th style="text-align:right">Amount</th></tr></thead>
        <tbody>${lineItemsHtml}</tbody></table>
      <table style="width:300px;margin-left:auto">
        <tbody>
          <tr class="total-row"><td>Subtotal</td><td style="text-align:right">${fc(invoice.subTotal)}</td></tr>
          ${invoice.discountAmount > 0 ? `<tr class="total-row"><td style="color:#16a34a">Discount</td><td style="text-align:right;color:#16a34a">-${fc(invoice.discountAmount)}</td></tr>` : ''}
          <tr class="total-row"><td>Tax (10%)</td><td style="text-align:right">${fc(invoice.taxAmount)}</td></tr>
          <tr class="grand-total"><td>Total</td><td style="text-align:right">${fc(invoice.totalAmount)}</td></tr>
        </tbody>
      </table>
    </body></html>`;

    const win = window.open('', '_blank', 'width=800,height=600');
    if (!win) return;
    win.document.write(html);
    win.document.close();
    win.onload = () => { win.print(); };
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
    ...(!isGuestRole ? [{
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
    }] : []),
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
          {formatCurrency(r.grandTotal ?? r.totalAmount ?? 0)}
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
        <Button leftIcon={<Plus className="h-4 w-4" />} onClick={() => {
          setIsCreateOpen(true);
          // Re-apply the guest auto-selection after form reset
          if (isGuestRole && user?.email) {
            const myGuest = guests.find(
              (g) => (g.email ?? '').toLowerCase() === user.email.toLowerCase()
            );
            if (myGuest) {
              setTimeout(() => setValueCreate('guestId', String(myGuest.id), { shouldValidate: true }), 0);
            }
          }
        }}>
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
            {isGuestRole ? (
              // Guest users are automatically set as the guest — show a read-only label
              <div className="sm:col-span-2 p-3 rounded-lg bg-indigo-50 dark:bg-indigo-900/20 border border-indigo-100 dark:border-indigo-800">
                <p className="text-xs font-medium text-indigo-600 dark:text-indigo-400 mb-0.5">Booking as</p>
                <p className="text-sm font-semibold text-slate-900 dark:text-slate-100">
                  {guests.find((g) => (g.email ?? '').toLowerCase() === (user?.email ?? '').toLowerCase())
                    ? `${guests.find((g) => (g.email ?? '').toLowerCase() === (user?.email ?? '').toLowerCase())!.firstName || ''} ${guests.find((g) => (g.email ?? '').toLowerCase() === (user?.email ?? '').toLowerCase())!.lastName || ''}`.trim() || user?.fullName || user?.email
                    : user?.fullName || user?.email}
                </p>
                {/* hidden input keeps guestId in the form */}
                <input type="hidden" {...registerCreate('guestId')} />
              </div>
            ) : (
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
            )}
            <div className="sm:col-span-2">
              <Select
                label={`Room${checkInCreate && checkOutCreate ? ` (${availableRoomsForDates.length} available for selected dates)` : ''}`}
                required
                options={[
                  { value: '', label: checkInCreate && checkOutCreate ? 'Select an available room...' : 'Select check-in/out dates first...' },
                  ...(checkInCreate && checkOutCreate ? availableRoomsForDates : rooms.filter((r) => r.roomState === 'Available')).map((r) => ({
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
                { icon: <DollarSign className="h-4 w-4" />, label: 'Total Amount', value: formatCurrency(viewReservation.grandTotal ?? viewReservation.totalAmount ?? 0) },
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

            {/* ── Expenses — staff only ──────────────────────────────────── */}
            {!isGuestRole && (
            <div className="border-t border-slate-200 dark:border-slate-700 pt-4">
              <div className="flex items-center justify-between mb-3">
                <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide flex items-center gap-1.5">
                  <Receipt className="h-3.5 w-3.5" /> Expenses
                  {reservationExpenses.length > 0 && (
                    <span className="ml-1 text-indigo-600 dark:text-indigo-400">
                      ({formatCurrency(reservationExpenses.reduce((s, e) => s + e.amount, 0))})
                    </span>
                  )}
                </p>
                {!showAddExpense && (
                  <button
                    onClick={() => { setShowAddExpense(true); resetExpense({ quantity: 1 }); }}
                    className="flex items-center gap-1 text-xs text-indigo-500 hover:text-indigo-700 dark:hover:text-indigo-300 transition-colors"
                  >
                    <Plus className="h-3.5 w-3.5" /> Add
                  </button>
                )}
              </div>

              {/* Add expense inline form */}
              {showAddExpense && (
                <form
                  onSubmit={handleExpense(onAddExpenseSubmit)}
                  className="mb-3 p-3 bg-indigo-50 dark:bg-indigo-900/20 rounded-xl border border-indigo-100 dark:border-indigo-800 space-y-3"
                >
                  <div className="grid grid-cols-2 gap-2">
                    <div className="col-span-2">
                      <Input
                        label="Description"
                        placeholder="e.g. Room service dinner"
                        {...registerExpense('description')}
                        error={expenseErrors.description?.message}
                      />
                    </div>
                    <Select
                      label="Category"
                      options={EXPENSE_CATEGORIES}
                      {...registerExpense('category')}
                    />
                    <Input
                      label="Unit Price"
                      type="number"
                      step="0.01"
                      placeholder="0.00"
                      {...registerExpense('unitPrice', { valueAsNumber: true })}
                      error={expenseErrors.unitPrice?.message}
                    />
                    <Input
                      label="Quantity"
                      type="number"
                      min={1}
                      {...registerExpense('quantity', { valueAsNumber: true })}
                      error={expenseErrors.quantity?.message}
                    />
                    <div className="flex items-end pb-1">
                      {expenseLineTotal > 0 && (
                        <p className="text-sm font-bold text-indigo-700 dark:text-indigo-300">
                          = {formatCurrency(expenseLineTotal)}
                        </p>
                      )}
                    </div>
                  </div>
                  <div className="flex gap-2 justify-end">
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      onClick={() => { setShowAddExpense(false); resetExpense({ quantity: 1 }); }}
                    >
                      Cancel
                    </Button>
                    <Button type="submit" size="sm" isLoading={isSubmitting}>
                      Add Expense
                    </Button>
                  </div>
                </form>
              )}

              {isLoadingExpenses ? (
                <p className="text-sm text-slate-400">Loading expenses...</p>
              ) : reservationExpenses.length === 0 ? (
                <p className="text-sm text-slate-400 italic">No expenses added yet.</p>
              ) : (
                <div className="space-y-1.5">
                  {reservationExpenses.map((expense) => (
                    <div key={expense.id} className="flex items-center justify-between p-2.5 bg-slate-50 dark:bg-slate-700/50 rounded-lg group">
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-slate-800 dark:text-slate-200 truncate">{expense.description}</p>
                        <p className="text-xs text-slate-500 dark:text-slate-400">
                          {expense.category && <span className="mr-2">{expense.category}</span>}
                          {expense.quantity} × {formatCurrency(expense.unitPrice)}
                        </p>
                      </div>
                      <div className="flex items-center gap-2 ml-2">
                        <span className="text-sm font-semibold text-slate-800 dark:text-slate-200">{formatCurrency(expense.amount)}</span>
                        <button
                          onClick={() => handleDeleteExpense(expense.id)}
                          className="opacity-0 group-hover:opacity-100 p-1 text-red-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded transition-all"
                          title="Remove expense"
                        >
                          <Trash2 className="h-3.5 w-3.5" />
                        </button>
                      </div>
                    </div>
                  ))}
                  {/* Totals summary */}
                  <div className="mt-2 pt-2 border-t border-slate-200 dark:border-slate-600 space-y-1">
                    <div className="flex justify-between text-xs text-slate-500 dark:text-slate-400">
                      <span>Room charges</span>
                      <span>{formatCurrency(viewReservation.totalPrice)}</span>
                    </div>
                    <div className="flex justify-between text-xs text-slate-500 dark:text-slate-400">
                      <span>Expenses</span>
                      <span>{formatCurrency(reservationExpenses.reduce((s, e) => s + e.amount, 0))}</span>
                    </div>
                    <div className="flex justify-between text-sm font-bold text-slate-800 dark:text-slate-100 pt-1">
                      <span>Grand Total</span>
                      <span className="text-indigo-600 dark:text-indigo-400">
                        {formatCurrency(viewReservation.totalPrice + reservationExpenses.reduce((s, e) => s + e.amount, 0))}
                      </span>
                    </div>
                  </div>
                </div>
              )}
            </div>
            )} {/* end !isGuestRole expenses */}

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
                      <div className="text-right flex flex-col items-end gap-1">
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
                        {!isGuestRole && p.paymentState === 'Completed' && (
                          <button
                            onClick={() => handleRefundPayment(p.id)}
                            className="text-xs text-red-400 hover:text-red-600 transition-colors"
                          >
                            Refund
                          </button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Invoice section */}
            {!isGuestRole && (
              <div className="border-t border-slate-200 dark:border-slate-700 pt-4">
                <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide mb-3 flex items-center gap-1.5">
                  <FileText className="h-3.5 w-3.5" /> Invoice
                </p>
                {isLoadingInvoice ? (
                  <p className="text-sm text-slate-400">Loading invoice...</p>
                ) : reservationInvoice ? (
                  <div className="p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl flex items-center justify-between gap-3">
                    <div>
                      <p className="font-mono text-sm font-semibold text-slate-800 dark:text-slate-200">{reservationInvoice.invoiceNumber}</p>
                      <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
                        {reservationInvoice.status} · {formatCurrency(reservationInvoice.totalAmount)}
                      </p>
                    </div>
                    <div className="flex items-center gap-2 shrink-0">
                      <Badge variant={reservationInvoice.status === 'Paid' ? 'success' : reservationInvoice.status === 'Void' ? 'danger' : 'warning'} dot>
                        {reservationInvoice.status}
                      </Badge>
                      <button
                        onClick={() => handlePrintInvoice(reservationInvoice)}
                        title="Print invoice"
                        className="p-1.5 rounded-lg text-slate-500 hover:text-slate-800 dark:hover:text-slate-200 hover:bg-slate-200 dark:hover:bg-slate-600 transition-colors"
                      >
                        <Printer className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                ) : viewReservation.status === 'CheckedOut' ? (
                  <Button
                    size="sm"
                    variant="outline"
                    leftIcon={<FileText className="h-3.5 w-3.5" />}
                    isLoading={isGeneratingInvoice}
                    onClick={handleGenerateInvoice}
                  >
                    Generate Invoice
                  </Button>
                ) : (
                  <p className="text-sm text-slate-400 italic">Invoice is generated at check-out.</p>
                )}
              </div>
            )}

            {/* Status update actions — staff only */}
            {!isGuestRole && (
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
            )}
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
              {(paymentReservation.expensesTotal ?? 0) > 0 && (
                <>
                  <div className="flex justify-between text-sm">
                    <span className="text-slate-500 dark:text-slate-400">Room charges</span>
                    <span className="font-medium text-slate-800 dark:text-slate-200">{formatCurrency(paymentReservation.totalPrice)}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-slate-500 dark:text-slate-400">Expenses</span>
                    <span className="font-medium text-slate-800 dark:text-slate-200">{formatCurrency(paymentReservation.expensesTotal ?? 0)}</span>
                  </div>
                </>
              )}
              <div className="flex justify-between text-sm border-t border-slate-200 dark:border-slate-600 pt-2 mt-2">
                <span className="font-semibold text-slate-700 dark:text-slate-300">Total Due</span>
                <span className="font-bold text-lg text-indigo-600 dark:text-indigo-400">
                  {formatCurrency(paymentReservation.grandTotal ?? paymentReservation.totalAmount ?? 0)}
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

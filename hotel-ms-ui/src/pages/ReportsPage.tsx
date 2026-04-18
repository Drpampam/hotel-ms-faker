import { useState, useCallback, useEffect } from 'react';
import {
  BarChart3, TrendingUp, BedDouble, CalendarCheck, Sparkles,
  CreditCard, RefreshCw, ArrowUpRight, ArrowDownRight, Clock,
  CheckCircle, XCircle, AlertCircle, Receipt, Search, Download,
} from 'lucide-react';
import { Card, CardHeader, CardTitle } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { useToast } from '../lib/store';
import { reportService } from '../services/report.service';
import type {
  OccupancyReport, RevenueSummary, ReservationStats,
  HousekeepingStats, PaymentBreakdown, FrontDeskSummary, ExpenseReport,
} from '../services/report.service';
import { cn, formatCurrency, downloadCSV } from '../lib/utils';

// ─── helpers ────────────────────────────────────────────────────────────────

function today() {
  return new Date().toISOString().split('T')[0];
}
function monthStart() {
  const d = new Date();
  d.setDate(1);
  return d.toISOString().split('T')[0];
}

function StatCard({
  label, value, sub, icon: Icon, color = 'text-indigo-600', highlight = false,
}: {
  label: string; value: string | number; sub?: string;
  icon: React.ElementType; color?: string; highlight?: boolean;
}) {
  return (
    <div className={cn(
      'bg-white dark:bg-slate-800 rounded-xl border p-4 shadow-sm',
      highlight
        ? 'border-indigo-300 dark:border-indigo-700'
        : 'border-slate-100 dark:border-slate-700',
    )}>
      <div className="flex items-start justify-between gap-2">
        <div>
          <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">{label}</p>
          <p className={cn('text-2xl font-bold', color)}>{value}</p>
          {sub && <p className="text-xs text-slate-400 dark:text-slate-500 mt-0.5">{sub}</p>}
        </div>
        <div className={cn('p-2 rounded-lg bg-slate-50 dark:bg-slate-700/50', color)}>
          <Icon className="h-5 w-5" />
        </div>
      </div>
    </div>
  );
}

function ProgressBar({ label, value, total, color = 'bg-indigo-500' }: {
  label: string; value: number; total: number; color?: string;
}) {
  const pct = total > 0 ? Math.round((value / total) * 100) : 0;
  return (
    <div>
      <div className="flex justify-between text-xs text-slate-500 dark:text-slate-400 mb-1">
        <span>{label}</span>
        <span>{value} ({pct}%)</span>
      </div>
      <div className="h-2 bg-slate-100 dark:bg-slate-700 rounded-full overflow-hidden">
        <div className={cn('h-full rounded-full transition-all', color)} style={{ width: `${pct}%` }} />
      </div>
    </div>
  );
}

function DateRangeBar({
  from, to, onFrom, onTo, onApply, isLoading,
}: {
  from: string; to: string;
  onFrom: (v: string) => void; onTo: (v: string) => void;
  onApply: () => void; isLoading: boolean;
}) {
  return (
    <div className="flex flex-wrap items-end gap-3 mb-6">
      <Input label="From" type="date" value={from} onChange={(e) => onFrom(e.target.value)} className="w-36" />
      <Input label="To" type="date" value={to} onChange={(e) => onTo(e.target.value)} className="w-36" />
      <Button onClick={onApply} isLoading={isLoading} leftIcon={<RefreshCw className="h-4 w-4" />}>
        Run Report
      </Button>
    </div>
  );
}

// ─── tab definitions ─────────────────────────────────────────────────────────

type Tab = 'frontdesk' | 'occupancy' | 'revenue' | 'reservations' | 'housekeeping' | 'payments' | 'expenses';

const TABS: { key: Tab; label: string; icon: React.ElementType }[] = [
  { key: 'frontdesk',    label: "Today's Summary", icon: BarChart3 },
  { key: 'occupancy',    label: 'Occupancy',        icon: BedDouble },
  { key: 'revenue',      label: 'Revenue',          icon: TrendingUp },
  { key: 'reservations', label: 'Reservations',     icon: CalendarCheck },
  { key: 'housekeeping', label: 'Housekeeping',     icon: Sparkles },
  { key: 'payments',     label: 'Payments',         icon: CreditCard },
  { key: 'expenses',     label: 'Expenses',         icon: Receipt },
];

// ─── page ────────────────────────────────────────────────────────────────────

export function ReportsPage() {
  const toast = useToast();
  const [activeTab, setActiveTab] = useState<Tab>('frontdesk');
  const [isLoading, setIsLoading] = useState(false);

  // date range state (shared across tabs that need it)
  const [from, setFrom] = useState(monthStart());
  const [to, setTo]     = useState(today());
  const [hkDate, setHkDate] = useState(today());

  // report data
  const [frontDesk,    setFrontDesk]    = useState<FrontDeskSummary | null>(null);
  const [occupancy,    setOccupancy]    = useState<OccupancyReport | null>(null);
  const [revenue,      setRevenue]      = useState<RevenueSummary | null>(null);
  const [reservations, setReservations] = useState<ReservationStats | null>(null);
  const [housekeeping, setHousekeeping] = useState<HousekeepingStats | null>(null);
  const [payments,     setPayments]     = useState<PaymentBreakdown | null>(null);
  const [expenses,     setExpenses]     = useState<ExpenseReport | null>(null);
  const [expenseSearch, setExpenseSearch] = useState('');

  const loadFrontDesk = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await reportService.getFrontDeskSummary(today());
      setFrontDesk(data);
    } catch {
      toast.error('Failed to load', "Could not fetch today's summary");
    } finally {
      setIsLoading(false);
    }
  }, [toast]);

  const loadOccupancy = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await reportService.getOccupancy(from, to);
      setOccupancy(data);
    } catch {
      toast.error('Failed to load', 'Could not fetch occupancy report');
    } finally {
      setIsLoading(false);
    }
  }, [from, to, toast]);

  const loadRevenue = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await reportService.getRevenue(from, to);
      setRevenue(data);
    } catch {
      toast.error('Failed to load', 'Could not fetch revenue report');
    } finally {
      setIsLoading(false);
    }
  }, [from, to, toast]);

  const loadReservations = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await reportService.getReservationStats(from, to);
      setReservations(data);
    } catch {
      toast.error('Failed to load', 'Could not fetch reservation stats');
    } finally {
      setIsLoading(false);
    }
  }, [from, to, toast]);

  const loadHousekeeping = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await reportService.getHousekeepingStats(hkDate);
      setHousekeeping(data);
    } catch {
      toast.error('Failed to load', 'Could not fetch housekeeping stats');
    } finally {
      setIsLoading(false);
    }
  }, [hkDate, toast]);

  const loadPayments = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await reportService.getPaymentBreakdown(from, to);
      setPayments(data);
    } catch {
      toast.error('Failed to load', 'Could not fetch payment breakdown');
    } finally {
      setIsLoading(false);
    }
  }, [from, to, toast]);

  const loadExpenses = useCallback(async (search?: string) => {
    setIsLoading(true);
    try {
      const trimmed = (search ?? expenseSearch).trim();
      const isNumeric = /^\d+$/.test(trimmed);
      const data = await reportService.getExpenseReport(
        from, to,
        isNumeric ? undefined : trimmed || undefined,
        isNumeric ? Number(trimmed) : undefined,
      );
      setExpenses(data);
    } catch {
      toast.error('Failed to load', 'Could not fetch expense report');
    } finally {
      setIsLoading(false);
    }
  }, [from, to, expenseSearch, toast]);

  // Auto-load when tab changes
  useEffect(() => {
    if (activeTab === 'frontdesk')    loadFrontDesk();
    if (activeTab === 'occupancy')    loadOccupancy();
    if (activeTab === 'revenue')      loadRevenue();
    if (activeTab === 'reservations') loadReservations();
    if (activeTab === 'housekeeping') loadHousekeeping();
    if (activeTab === 'payments')     loadPayments();
    if (activeTab === 'expenses')     loadExpenses();
  }, [activeTab]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleRunReport = () => {
    if (activeTab === 'occupancy')    loadOccupancy();
    if (activeTab === 'revenue')      loadRevenue();
    if (activeTab === 'reservations') loadReservations();
    if (activeTab === 'payments')     loadPayments();
    if (activeTab === 'housekeeping') loadHousekeeping();
    if (activeTab === 'expenses')     loadExpenses();
  };

  const handleExportCSV = () => {
    const prefix = `hotel-ms-${from}-to-${to}`;
    if (activeTab === 'occupancy' && occupancy) {
      downloadCSV(`${prefix}-occupancy.csv`, [{
        totalRooms: occupancy.totalRooms,
        occupiedRooms: occupancy.occupiedRooms,
        availableRooms: occupancy.availableRooms,
        cleaningRooms: occupancy.cleaningRooms,
        maintenanceRooms: occupancy.maintenanceRooms,
        occupancyRate: occupancy.occupancyRate,
      }]);
    } else if (activeTab === 'revenue' && revenue) {
      downloadCSV(`${prefix}-revenue.csv`, [{
        totalRevenue: revenue.totalRevenue,
        roomRevenue: revenue.roomRevenue,
        taxCollected: revenue.taxCollected,
        totalDiscountsApplied: revenue.totalDiscountsApplied,
        paidInvoicesCount: revenue.paidInvoicesCount,
        pendingInvoicesCount: revenue.pendingInvoicesCount,
      }]);
    } else if (activeTab === 'reservations' && reservations) {
      downloadCSV(`${prefix}-reservations.csv`, [{
        totalReservations: reservations.totalReservations,
        confirmedReservations: reservations.confirmedReservations,
        checkedInCount: reservations.checkedInCount,
        checkedOutCount: reservations.checkedOutCount,
        pendingReservations: reservations.pendingReservations,
        cancelledCount: reservations.cancelledCount,
        noShowCount: reservations.noShowCount,
        averageStayDays: reservations.averageStayDays,
      }]);
    } else if (activeTab === 'payments' && payments) {
      downloadCSV(`${prefix}-payments.csv`, payments.byMethod.map((m) => ({
        method: m.method,
        count: m.count,
        amount: m.amount,
      })));
    } else if (activeTab === 'expenses' && expenses) {
      downloadCSV(`${prefix}-expenses.csv`, expenses.items.map((item) => ({
        date: item.creationDate,
        reservationId: item.reservationId,
        guestName: item.guestName ?? '',
        guestEmail: item.guestEmail ?? '',
        room: item.roomNumber ?? '',
        description: item.description,
        category: item.category ?? '',
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        amount: item.amount,
        addedBy: item.createdBy ?? '',
      })));
    } else if (activeTab === 'housekeeping' && housekeeping) {
      downloadCSV(`hotel-ms-${hkDate}-housekeeping.csv`, [{
        date: hkDate,
        totalTasks: housekeeping.totalTasks,
        pendingTasks: housekeeping.pendingTasks,
        inProgressTasks: housekeeping.inProgressTasks,
        completedTasks: housekeeping.completedTasks,
        skippedTasks: housekeeping.skippedTasks,
        completionRate: housekeeping.completionRate,
      }]);
    }
  };

  const canExport = (activeTab === 'occupancy' && !!occupancy)
    || (activeTab === 'revenue' && !!revenue)
    || (activeTab === 'reservations' && !!reservations)
    || (activeTab === 'payments' && !!payments)
    || (activeTab === 'expenses' && !!expenses)
    || (activeTab === 'housekeeping' && !!housekeeping);

  return (
    <div className="page-container">
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Reports</h2>
          <p className="page-subtitle">Operational and financial reports across the hotel</p>
        </div>
        {canExport && (
          <Button variant="outline" leftIcon={<Download className="h-4 w-4" />} onClick={handleExportCSV}>
            Export CSV
          </Button>
        )}
      </div>

      {/* Tab bar */}
      <div className="flex flex-wrap gap-1 mb-6 bg-slate-100 dark:bg-slate-800 p-1 rounded-xl">
        {TABS.map(({ key, label, icon: Icon }) => (
          <button
            key={key}
            onClick={() => setActiveTab(key)}
            className={cn(
              'flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-medium transition-all',
              activeTab === key
                ? 'bg-white dark:bg-slate-700 text-indigo-600 dark:text-indigo-400 shadow-sm'
                : 'text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-200',
            )}
          >
            <Icon className="h-4 w-4" />
            <span className="hidden sm:inline">{label}</span>
          </button>
        ))}
      </div>

      {/* ── Today's Summary ───────────────────────────────────── */}
      {activeTab === 'frontdesk' && (
        <div>
          <div className="flex items-center justify-between mb-6">
            <p className="text-sm text-slate-500 dark:text-slate-400">
              {new Date().toLocaleDateString('en-GB', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
            </p>
            <Button variant="outline" size="sm" onClick={loadFrontDesk} isLoading={isLoading} leftIcon={<RefreshCw className="h-4 w-4" />}>
              Refresh
            </Button>
          </div>

          {frontDesk ? (
            <>
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-6">
                <StatCard label="Currently Occupied" value={frontDesk.currentlyOccupied} icon={BedDouble} color="text-indigo-600" highlight />
                <StatCard label="Expected Arrivals" value={frontDesk.expectedArrivals} sub={`${frontDesk.actualCheckIns} checked in`} icon={ArrowDownRight} color="text-emerald-600" />
                <StatCard label="Expected Departures" value={frontDesk.expectedDepartures} sub={`${frontDesk.actualCheckOuts} checked out`} icon={ArrowUpRight} color="text-amber-600" />
                <StatCard label="Pending Service Requests" value={frontDesk.pendingServiceRequests} icon={AlertCircle} color="text-red-500" />
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <Card>
                  <CardHeader><CardTitle>Arrivals Today</CardTitle></CardHeader>
                  <div className="p-4 space-y-3">
                    <ProgressBar label="Checked In" value={frontDesk.actualCheckIns} total={frontDesk.expectedArrivals} color="bg-emerald-500" />
                    <ProgressBar label="Pending Arrivals" value={frontDesk.expectedArrivals - frontDesk.actualCheckIns} total={frontDesk.expectedArrivals} color="bg-amber-400" />
                  </div>
                </Card>
                <Card>
                  <CardHeader><CardTitle>Departures Today</CardTitle></CardHeader>
                  <div className="p-4 space-y-3">
                    <ProgressBar label="Checked Out" value={frontDesk.actualCheckOuts} total={frontDesk.expectedDepartures} color="bg-indigo-500" />
                    <ProgressBar label="Still In Room" value={frontDesk.expectedDepartures - frontDesk.actualCheckOuts} total={frontDesk.expectedDepartures} color="bg-amber-400" />
                  </div>
                </Card>
                <Card>
                  <CardHeader><CardTitle>Housekeeping Today</CardTitle></CardHeader>
                  <div className="p-4">
                    <p className="text-3xl font-bold text-amber-500 mb-1">{frontDesk.pendingHousekeepingTasks}</p>
                    <p className="text-sm text-slate-500 dark:text-slate-400">Pending tasks scheduled for today</p>
                  </div>
                </Card>
              </div>
            </>
          ) : (
            <EmptyState isLoading={isLoading} />
          )}
        </div>
      )}

      {/* ── Occupancy ──────────────────────────────────────────── */}
      {activeTab === 'occupancy' && (
        <div>
          <DateRangeBar from={from} to={to} onFrom={setFrom} onTo={setTo} onApply={handleRunReport} isLoading={isLoading} />
          {occupancy ? (
            <>
              <div className="grid grid-cols-2 sm:grid-cols-5 gap-4 mb-6">
                <StatCard label="Total Rooms"    value={occupancy.totalRooms}       icon={BedDouble}   color="text-slate-600" />
                <StatCard label="Occupied"        value={occupancy.occupiedRooms}    icon={CheckCircle} color="text-indigo-600" highlight />
                <StatCard label="Available"       value={occupancy.availableRooms}   icon={ArrowUpRight} color="text-emerald-600" />
                <StatCard label="Cleaning"        value={occupancy.cleaningRooms}    icon={Sparkles}    color="text-amber-500" />
                <StatCard label="Maintenance"     value={occupancy.maintenanceRooms} icon={AlertCircle} color="text-red-500" />
              </div>
              <Card>
                <CardHeader><CardTitle>Occupancy Rate — {occupancy.occupancyRate}%</CardTitle></CardHeader>
                <div className="p-4 space-y-4">
                  <div>
                    <div className="flex justify-between text-sm font-medium mb-2">
                      <span className="text-slate-700 dark:text-slate-300">Overall Occupancy</span>
                      <span className="text-indigo-600 dark:text-indigo-400">{occupancy.occupancyRate}%</span>
                    </div>
                    <div className="h-4 bg-slate-100 dark:bg-slate-700 rounded-full overflow-hidden">
                      <div className="h-full bg-indigo-500 rounded-full transition-all" style={{ width: `${occupancy.occupancyRate}%` }} />
                    </div>
                  </div>
                  <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 pt-2">
                    {[
                      { label: 'Occupied',    value: occupancy.occupiedRooms,    color: 'bg-indigo-500' },
                      { label: 'Available',   value: occupancy.availableRooms,   color: 'bg-emerald-500' },
                      { label: 'Cleaning',    value: occupancy.cleaningRooms,    color: 'bg-amber-400' },
                      { label: 'Maintenance', value: occupancy.maintenanceRooms, color: 'bg-red-400' },
                    ].map(({ label, value, color }) => (
                      <div key={label} className="flex items-center gap-2 text-sm">
                        <div className={cn('w-3 h-3 rounded-full flex-shrink-0', color)} />
                        <span className="text-slate-500 dark:text-slate-400">{label}:</span>
                        <span className="font-semibold text-slate-700 dark:text-slate-300">{value}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </Card>
            </>
          ) : (
            <EmptyState isLoading={isLoading} />
          )}
        </div>
      )}

      {/* ── Revenue ────────────────────────────────────────────── */}
      {activeTab === 'revenue' && (
        <div>
          <DateRangeBar from={from} to={to} onFrom={setFrom} onTo={setTo} onApply={handleRunReport} isLoading={isLoading} />
          {revenue ? (
            <>
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 mb-6">
                <StatCard label="Total Revenue"       value={formatCurrency(revenue.totalRevenue)}          icon={TrendingUp}  color="text-emerald-600" highlight />
                <StatCard label="Room Revenue"         value={formatCurrency(revenue.roomRevenue)}           icon={BedDouble}   color="text-indigo-600" />
                <StatCard label="Tax Collected"        value={formatCurrency(revenue.taxCollected)}          icon={BarChart3}   color="text-slate-600" />
                <StatCard label="Discounts Applied"    value={formatCurrency(revenue.totalDiscountsApplied)} icon={XCircle}     color="text-amber-500" />
                <StatCard label="Paid Invoices"        value={revenue.paidInvoicesCount}                     icon={CheckCircle} color="text-emerald-600" />
                <StatCard label="Pending Invoices"     value={revenue.pendingInvoicesCount}                  icon={Clock}       color="text-amber-500" />
              </div>
              <Card>
                <CardHeader><CardTitle>Revenue Breakdown</CardTitle></CardHeader>
                <div className="p-4 space-y-4">
                  {[
                    { label: 'Room Revenue',         value: revenue.roomRevenue,           max: revenue.totalRevenue, color: 'bg-indigo-500' },
                    { label: 'Tax Collected',         value: revenue.taxCollected,          max: revenue.totalRevenue, color: 'bg-slate-400' },
                    { label: 'Discounts Applied',     value: revenue.totalDiscountsApplied, max: revenue.totalRevenue, color: 'bg-amber-400' },
                  ].map(({ label, value, max, color }) => (
                    <div key={label}>
                      <div className="flex justify-between text-xs text-slate-500 dark:text-slate-400 mb-1">
                        <span>{label}</span>
                        <span>{formatCurrency(value)}</span>
                      </div>
                      <div className="h-2 bg-slate-100 dark:bg-slate-700 rounded-full overflow-hidden">
                        <div className={cn('h-full rounded-full', color)} style={{ width: max > 0 ? `${Math.min((value / max) * 100, 100)}%` : '0%' }} />
                      </div>
                    </div>
                  ))}
                </div>
              </Card>
            </>
          ) : (
            <EmptyState isLoading={isLoading} />
          )}
        </div>
      )}

      {/* ── Reservations ──────────────────────────────────────── */}
      {activeTab === 'reservations' && (
        <div>
          <DateRangeBar from={from} to={to} onFrom={setFrom} onTo={setTo} onApply={handleRunReport} isLoading={isLoading} />
          {reservations ? (
            <>
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-6">
                <StatCard label="Total"       value={reservations.totalReservations}    icon={CalendarCheck} color="text-indigo-600" highlight />
                <StatCard label="Confirmed"   value={reservations.confirmedReservations} icon={CheckCircle}  color="text-emerald-600" />
                <StatCard label="Checked In"  value={reservations.checkedInCount}        icon={BedDouble}    color="text-blue-500" />
                <StatCard label="Checked Out" value={reservations.checkedOutCount}       icon={ArrowUpRight} color="text-slate-500" />
                <StatCard label="Pending"     value={reservations.pendingReservations}   icon={Clock}        color="text-amber-500" />
                <StatCard label="Cancelled"   value={reservations.cancelledCount}        icon={XCircle}      color="text-red-500" />
                <StatCard label="No Show"     value={reservations.noShowCount}           icon={AlertCircle}  color="text-slate-400" />
                <StatCard label="Avg Stay"    value={`${reservations.averageStayDays}d`} icon={CalendarCheck} color="text-indigo-500" />
              </div>
              <Card>
                <CardHeader><CardTitle>Status Distribution</CardTitle></CardHeader>
                <div className="p-4 space-y-3">
                  {[
                    { label: 'Confirmed',   value: reservations.confirmedReservations, color: 'bg-emerald-500' },
                    { label: 'Checked In',  value: reservations.checkedInCount,         color: 'bg-blue-500' },
                    { label: 'Checked Out', value: reservations.checkedOutCount,        color: 'bg-slate-400' },
                    { label: 'Pending',     value: reservations.pendingReservations,    color: 'bg-amber-400' },
                    { label: 'Cancelled',   value: reservations.cancelledCount,         color: 'bg-red-400' },
                    { label: 'No Show',     value: reservations.noShowCount,            color: 'bg-slate-300' },
                  ].map(({ label, value, color }) => (
                    <ProgressBar key={label} label={label} value={value} total={reservations.totalReservations} color={color} />
                  ))}
                </div>
              </Card>
            </>
          ) : (
            <EmptyState isLoading={isLoading} />
          )}
        </div>
      )}

      {/* ── Housekeeping ──────────────────────────────────────── */}
      {activeTab === 'housekeeping' && (
        <div>
          <div className="flex flex-wrap items-end gap-3 mb-6">
            <Input label="Date" type="date" value={hkDate} onChange={(e) => setHkDate(e.target.value)} className="w-40" />
            <Button onClick={loadHousekeeping} isLoading={isLoading} leftIcon={<RefreshCw className="h-4 w-4" />}>
              Run Report
            </Button>
          </div>
          {housekeeping ? (
            <>
              <div className="grid grid-cols-2 sm:grid-cols-5 gap-4 mb-6">
                <StatCard label="Total Tasks"   value={housekeeping.totalTasks}       icon={Sparkles}    color="text-slate-600" />
                <StatCard label="Pending"        value={housekeeping.pendingTasks}     icon={Clock}       color="text-amber-500" />
                <StatCard label="In Progress"    value={housekeeping.inProgressTasks}  icon={RefreshCw}   color="text-blue-500" />
                <StatCard label="Completed"      value={housekeeping.completedTasks}   icon={CheckCircle} color="text-emerald-600" highlight />
                <StatCard label="Skipped"        value={housekeeping.skippedTasks}     icon={XCircle}     color="text-slate-400" />
              </div>
              <Card>
                <CardHeader>
                  <CardTitle>Completion Rate — {housekeeping.completionRate}%</CardTitle>
                </CardHeader>
                <div className="p-4 space-y-4">
                  <div>
                    <div className="h-4 bg-slate-100 dark:bg-slate-700 rounded-full overflow-hidden">
                      <div className="h-full bg-emerald-500 rounded-full transition-all" style={{ width: `${housekeeping.completionRate}%` }} />
                    </div>
                  </div>
                  <div className="space-y-3 pt-2">
                    {[
                      { label: 'Completed',   value: housekeeping.completedTasks,  color: 'bg-emerald-500' },
                      { label: 'Pending',     value: housekeeping.pendingTasks,    color: 'bg-amber-400' },
                      { label: 'In Progress', value: housekeeping.inProgressTasks, color: 'bg-blue-500' },
                      { label: 'Skipped',     value: housekeeping.skippedTasks,    color: 'bg-slate-300' },
                    ].map(({ label, value, color }) => (
                      <ProgressBar key={label} label={label} value={value} total={housekeeping.totalTasks} color={color} />
                    ))}
                  </div>
                </div>
              </Card>
            </>
          ) : (
            <EmptyState isLoading={isLoading} />
          )}
        </div>
      )}

      {/* ── Payments ──────────────────────────────────────────── */}
      {activeTab === 'payments' && (
        <div>
          <DateRangeBar from={from} to={to} onFrom={setFrom} onTo={setTo} onApply={handleRunReport} isLoading={isLoading} />
          {payments ? (
            <>
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 mb-6">
                <StatCard label="Total Transactions" value={payments.totalPayments}             icon={CreditCard}  color="text-indigo-600" highlight />
                <StatCard label="Total Amount"        value={formatCurrency(payments.totalAmount)} icon={TrendingUp} color="text-emerald-600" />
              </div>
              <Card>
                <CardHeader><CardTitle>By Payment Method</CardTitle></CardHeader>
                <div className="p-4">
                  {payments.byMethod.length === 0 ? (
                    <p className="text-sm text-slate-400 dark:text-slate-500 text-center py-4">No payment data for this period</p>
                  ) : (
                    <div className="space-y-4">
                      {payments.byMethod.map((m) => (
                        <div key={m.method} className="flex items-center justify-between gap-4">
                          <div className="flex-1">
                            <div className="flex justify-between text-sm mb-1">
                              <span className="font-medium text-slate-700 dark:text-slate-300 capitalize">{m.method}</span>
                              <span className="text-slate-500 dark:text-slate-400">{m.count} txn · {formatCurrency(m.amount)}</span>
                            </div>
                            <div className="h-2 bg-slate-100 dark:bg-slate-700 rounded-full overflow-hidden">
                              <div
                                className="h-full bg-indigo-500 rounded-full"
                                style={{ width: payments.totalAmount > 0 ? `${(m.amount / payments.totalAmount) * 100}%` : '0%' }}
                              />
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </Card>
            </>
          ) : (
            <EmptyState isLoading={isLoading} />
          )}
        </div>
      )}

      {/* ── Expenses ──────────────────────────────────────────── */}
      {activeTab === 'expenses' && (
        <div>
          {/* Filters */}
          <div className="flex flex-wrap items-end gap-3 mb-6">
            <Input label="From" type="date" value={from} onChange={(e) => setFrom(e.target.value)} className="w-36" />
            <Input label="To"   type="date" value={to}   onChange={(e) => setTo(e.target.value)}   className="w-36" />
            <div className="flex-1 min-w-[200px]">
              <label className="block text-xs font-medium text-slate-500 dark:text-slate-400 mb-1">
                Search guest name or reservation ID
              </label>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />
                <input
                  type="text"
                  value={expenseSearch}
                  onChange={(e) => setExpenseSearch(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && loadExpenses()}
                  placeholder="e.g. John Doe or 42"
                  className="w-full h-10 pl-9 pr-4 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-sm text-slate-700 dark:text-slate-200 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>
            </div>
            <Button onClick={() => loadExpenses()} isLoading={isLoading} leftIcon={<RefreshCw className="h-4 w-4" />}>
              Run Report
            </Button>
          </div>

          {expenses ? (
            <>
              {/* Summary stats */}
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 mb-6">
                <StatCard label="Total Expenses"  value={expenses.totalItems}               icon={Receipt}    color="text-indigo-600" highlight />
                <StatCard label="Total Amount"    value={formatCurrency(expenses.totalAmount)} icon={TrendingUp} color="text-emerald-600" />
                <StatCard label="Categories"      value={expenses.byCategory.length}        icon={BarChart3}  color="text-slate-600" />
              </div>

              {/* By category breakdown */}
              {expenses.byCategory.length > 0 && (
                <Card className="mb-6">
                  <CardHeader><CardTitle>By Category</CardTitle></CardHeader>
                  <div className="p-4 space-y-3">
                    {expenses.byCategory.map((c) => (
                      <div key={c.category}>
                        <div className="flex justify-between text-xs text-slate-500 dark:text-slate-400 mb-1">
                          <span className="font-medium">{c.category}</span>
                          <span>{c.count} item{c.count !== 1 ? 's' : ''} · {formatCurrency(c.amount)}</span>
                        </div>
                        <div className="h-2 bg-slate-100 dark:bg-slate-700 rounded-full overflow-hidden">
                          <div
                            className="h-full bg-indigo-500 rounded-full"
                            style={{ width: expenses.totalAmount > 0 ? `${Math.min((c.amount / expenses.totalAmount) * 100, 100)}%` : '0%' }}
                          />
                        </div>
                      </div>
                    ))}
                  </div>
                </Card>
              )}

              {/* Expense rows table */}
              <Card>
                <CardHeader><CardTitle>Expense Transactions ({expenses.totalItems})</CardTitle></CardHeader>
                {expenses.items.length === 0 ? (
                  <p className="text-sm text-slate-400 dark:text-slate-500 text-center py-8">No expenses found for this period</p>
                ) : (
                  <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b border-slate-100 dark:border-slate-700">
                          <th className="text-left px-4 py-3 text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Date & Time</th>
                          <th className="text-left px-4 py-3 text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Res. ID</th>
                          <th className="text-left px-4 py-3 text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Guest</th>
                          <th className="text-left px-4 py-3 text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Room</th>
                          <th className="text-left px-4 py-3 text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Description</th>
                          <th className="text-left px-4 py-3 text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Category</th>
                          <th className="text-right px-4 py-3 text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Qty</th>
                          <th className="text-right px-4 py-3 text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Unit Price</th>
                          <th className="text-right px-4 py-3 text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Amount</th>
                          <th className="text-left px-4 py-3 text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider">Added By</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-slate-50 dark:divide-slate-700/50">
                        {expenses.items.map((item) => (
                          <tr key={item.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30 transition-colors">
                            <td className="px-4 py-3 text-slate-500 dark:text-slate-400 whitespace-nowrap">
                              {new Date(item.creationDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })}
                              <br />
                              <span className="text-xs text-slate-400 dark:text-slate-500">
                                {new Date(item.creationDate).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}
                              </span>
                            </td>
                            <td className="px-4 py-3 font-mono text-indigo-600 dark:text-indigo-400 whitespace-nowrap">
                              #{item.reservationId}
                            </td>
                            <td className="px-4 py-3">
                              <p className="font-medium text-slate-700 dark:text-slate-200">{item.guestName ?? '—'}</p>
                              {item.guestEmail && (
                                <p className="text-xs text-slate-400 dark:text-slate-500">{item.guestEmail}</p>
                              )}
                            </td>
                            <td className="px-4 py-3 text-slate-600 dark:text-slate-300 whitespace-nowrap">
                              {item.roomNumber ?? '—'}
                            </td>
                            <td className="px-4 py-3 text-slate-700 dark:text-slate-200 max-w-[180px] truncate" title={item.description}>
                              {item.description}
                            </td>
                            <td className="px-4 py-3">
                              {item.category ? (
                                <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-400">
                                  {item.category}
                                </span>
                              ) : (
                                <span className="text-slate-400 dark:text-slate-500 text-xs">—</span>
                              )}
                            </td>
                            <td className="px-4 py-3 text-right text-slate-600 dark:text-slate-300">{item.quantity}</td>
                            <td className="px-4 py-3 text-right text-slate-600 dark:text-slate-300">{formatCurrency(item.unitPrice)}</td>
                            <td className="px-4 py-3 text-right font-semibold text-slate-800 dark:text-slate-100">{formatCurrency(item.amount)}</td>
                            <td className="px-4 py-3 text-xs text-slate-400 dark:text-slate-500 whitespace-nowrap">{item.createdBy ?? '—'}</td>
                          </tr>
                        ))}
                      </tbody>
                      <tfoot>
                        <tr className="border-t-2 border-slate-200 dark:border-slate-600 bg-slate-50 dark:bg-slate-700/30">
                          <td colSpan={8} className="px-4 py-3 text-right text-sm font-semibold text-slate-700 dark:text-slate-200">
                            Total
                          </td>
                          <td className="px-4 py-3 text-right text-sm font-bold text-emerald-600 dark:text-emerald-400">
                            {formatCurrency(expenses.totalAmount)}
                          </td>
                          <td />
                        </tr>
                      </tfoot>
                    </table>
                  </div>
                )}
              </Card>
            </>
          ) : (
            <EmptyState isLoading={isLoading} />
          )}
        </div>
      )}
    </div>
  );
}

function EmptyState({ isLoading }: { isLoading: boolean }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-slate-400 dark:text-slate-500">
      {isLoading ? (
        <>
          <RefreshCw className="h-8 w-8 animate-spin mb-3" />
          <p className="text-sm">Loading report…</p>
        </>
      ) : (
        <>
          <BarChart3 className="h-10 w-10 mb-3 opacity-40" />
          <p className="text-sm">Select a date range and click Run Report</p>
        </>
      )}
    </div>
  );
}

export default ReportsPage;

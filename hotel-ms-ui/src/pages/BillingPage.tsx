import { useState, useEffect, useCallback } from 'react';
import { FileText, Plus, CheckCircle, XCircle, Clock, Search, X, Printer } from 'lucide-react';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { Table } from '../components/ui/Table';
import { Modal } from '../components/ui/Modal';
import { useToast, useAuthStore } from '../lib/store';
import { billingService } from '../services/billing.service';
import type { Invoice } from '../types';
import { formatDate, formatCurrency } from '../lib/utils';

const STATUS_VARIANT: Record<string, 'success' | 'danger' | 'warning' | 'default'> = {
  Issued: 'warning',
  Paid: 'success',
  Void: 'danger',
};

export function BillingPage() {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [viewInvoice, setViewInvoice] = useState<Invoice | null>(null);
  const [isBusy, setIsBusy] = useState(false);
  const toast = useToast();
  const { user } = useAuthStore();
  const isAdmin = user?.roles?.some((r) => ['Admin', 'SuperAdmin', 'Developer'].includes(r)) ?? false;

  const fetchInvoices = useCallback(async () => {
    setIsLoading(true);
    try {
      const result = await billingService.getAll({ status: statusFilter || undefined });
      setInvoices(result.data);
    } catch {
      toast.error('Failed to load invoices', 'Could not fetch billing records');
    } finally {
      setIsLoading(false);
    }
  }, [statusFilter, toast]);

  useEffect(() => { fetchInvoices(); }, [fetchInvoices]);

  const handleMarkPaid = async (invoice: Invoice) => {
    setIsBusy(true);
    try {
      const updated = await billingService.markPaid(invoice.id);
      setInvoices((prev) => prev.map((i) => (i.id === invoice.id ? updated : i)));
      setViewInvoice(updated);
      toast.success('Invoice marked as paid', invoice.invoiceNumber);
    } catch (err) {
      toast.error('Failed', err instanceof Error ? err.message : 'Could not mark invoice paid');
    } finally {
      setIsBusy(false);
    }
  };

  const handleVoid = async (invoice: Invoice) => {
    if (!confirm(`Void invoice ${invoice.invoiceNumber}? This cannot be undone.`)) return;
    setIsBusy(true);
    try {
      const updated = await billingService.voidInvoice(invoice.id);
      setInvoices((prev) => prev.map((i) => (i.id === invoice.id ? updated : i)));
      setViewInvoice(updated);
      toast.success('Invoice voided', invoice.invoiceNumber);
    } catch (err) {
      toast.error('Failed', err instanceof Error ? err.message : 'Could not void invoice');
    } finally {
      setIsBusy(false);
    }
  };

  const handlePrint = () => {
    window.print();
  };

  const filtered = invoices.filter((inv) => {
    if (!search) return true;
    const q = search.toLowerCase();
    return (
      inv.invoiceNumber.toLowerCase().includes(q) ||
      (inv.guestName ?? '').toLowerCase().includes(q) ||
      (inv.guestEmail ?? '').toLowerCase().includes(q)
    );
  });

  const columns = [
    {
      key: 'invoiceNumber',
      header: 'Invoice #',
      render: (inv: Invoice) => (
        <span className="font-mono text-xs bg-slate-100 dark:bg-slate-700 px-2 py-1 rounded-md">
          {inv.invoiceNumber}
        </span>
      ),
    },
    {
      key: 'guest',
      header: 'Guest',
      render: (inv: Invoice) => (
        <div>
          <p className="font-medium text-slate-800 dark:text-slate-200">{inv.guestName ?? '—'}</p>
          <p className="text-xs text-slate-500 dark:text-slate-400">{inv.guestEmail ?? ''}</p>
        </div>
      ),
    },
    {
      key: 'issueDate',
      header: 'Issued',
      render: (inv: Invoice) => formatDate(inv.issueDate),
    },
    {
      key: 'totalAmount',
      header: 'Total',
      render: (inv: Invoice) => (
        <span className="font-semibold text-slate-900 dark:text-slate-100">
          {formatCurrency(inv.totalAmount)}
        </span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (inv: Invoice) => (
        <Badge variant={STATUS_VARIANT[inv.status] ?? 'default'} dot>
          {inv.status}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: '',
      render: (inv: Invoice) => (
        <Button variant="ghost" size="sm" leftIcon={<FileText className="h-3.5 w-3.5" />}
          onClick={(e) => { e.stopPropagation(); setViewInvoice(inv); }}>
          View
        </Button>
      ),
    },
  ];

  return (
    <div className="page-container">
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Billing & Invoices</h2>
          <p className="page-subtitle">{invoices.length} invoice{invoices.length !== 1 ? 's' : ''}</p>
        </div>
      </div>

      {/* Filters */}
      <Card className="mb-6" padding="sm">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              type="text"
              placeholder="Search by invoice number, guest name or email..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full h-10 pl-10 pr-10 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900 dark:text-slate-100 placeholder:text-slate-400"
            />
            {search && (
              <button onClick={() => setSearch('')} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600">
                <X className="h-4 w-4" />
              </button>
            )}
          </div>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="sm:w-40 h-10 px-3 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          >
            <option value="">All Statuses</option>
            <option value="Issued">Issued</option>
            <option value="Paid">Paid</option>
            <option value="Void">Void</option>
          </select>
          <Button variant="outline" leftIcon={<Plus className="h-4 w-4" />} onClick={fetchInvoices}>
            Refresh
          </Button>
        </div>
      </Card>

      <Card padding="none">
        <Table
          data={filtered}
          columns={columns}
          isLoading={isLoading}
          emptyMessage="No invoices found"
          emptyDescription="Invoices are generated from the Reservations page at checkout"
          onRowClick={(inv) => setViewInvoice(inv as unknown as Invoice)}
        />
      </Card>

      {/* Invoice detail modal */}
      <Modal
        isOpen={!!viewInvoice}
        onClose={() => setViewInvoice(null)}
        title={viewInvoice?.invoiceNumber ?? 'Invoice'}
        description={viewInvoice ? `Issued ${formatDate(viewInvoice.issueDate)} · Due ${formatDate(viewInvoice.dueDate)}` : ''}
        size="lg"
        footer={
          viewInvoice ? (
            <div className="flex gap-2 flex-wrap justify-end">
              <Button variant="outline" leftIcon={<Printer className="h-4 w-4" />} onClick={handlePrint}>
                Print
              </Button>
              {isAdmin && viewInvoice.status === 'Issued' && (
                <>
                  <Button
                    variant="outline"
                    leftIcon={<XCircle className="h-4 w-4" />}
                    isLoading={isBusy}
                    onClick={() => handleVoid(viewInvoice)}
                    className="text-red-600 border-red-300 hover:bg-red-50"
                  >
                    Void Invoice
                  </Button>
                  <Button
                    leftIcon={<CheckCircle className="h-4 w-4" />}
                    isLoading={isBusy}
                    onClick={() => handleMarkPaid(viewInvoice)}
                  >
                    Mark as Paid
                  </Button>
                </>
              )}
              {viewInvoice.status !== 'Issued' && (
                <Button variant="outline" onClick={() => setViewInvoice(null)}>Close</Button>
              )}
            </div>
          ) : null
        }
      >
        {viewInvoice && (
          <div className="space-y-5 print-invoice">
            {/* Status banner */}
            <div className="flex items-center justify-between">
              <Badge variant={STATUS_VARIANT[viewInvoice.status] ?? 'default'} dot>
                {viewInvoice.status === 'Issued' && <Clock className="h-3 w-3 mr-1" />}
                {viewInvoice.status === 'Paid' && <CheckCircle className="h-3 w-3 mr-1" />}
                {viewInvoice.status === 'Void' && <XCircle className="h-3 w-3 mr-1" />}
                {viewInvoice.status}
              </Badge>
              <span className="text-xs text-slate-400">Reservation #{viewInvoice.reservationId}</span>
            </div>

            {/* Guest info */}
            {(viewInvoice.guestName || viewInvoice.guestEmail) && (
              <div className="p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">Billed To</p>
                <p className="font-semibold text-slate-900 dark:text-slate-100">{viewInvoice.guestName ?? '—'}</p>
                {viewInvoice.guestEmail && <p className="text-sm text-slate-500 dark:text-slate-400">{viewInvoice.guestEmail}</p>}
              </div>
            )}

            {/* Line items */}
            <div>
              <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide mb-3">Line Items</p>
              <div className="space-y-2">
                {viewInvoice.lineItems.map((item, i) => (
                  <div key={i} className="flex items-start justify-between p-2.5 bg-slate-50 dark:bg-slate-700/50 rounded-lg">
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-slate-800 dark:text-slate-200">{item.description}</p>
                      <p className="text-xs text-slate-500 dark:text-slate-400">
                        {item.category} · {item.quantity} × {formatCurrency(item.unitPrice)}
                      </p>
                    </div>
                    <span className="text-sm font-semibold text-slate-800 dark:text-slate-200 ml-3">{formatCurrency(item.amount)}</span>
                  </div>
                ))}
                {viewInvoice.lineItems.length === 0 && (
                  <p className="text-sm text-slate-400 italic">No line items</p>
                )}
              </div>
            </div>

            {/* Totals */}
            <div className="border-t border-slate-200 dark:border-slate-700 pt-4 space-y-2">
              <div className="flex justify-between text-sm text-slate-600 dark:text-slate-400">
                <span>Subtotal</span><span>{formatCurrency(viewInvoice.subTotal)}</span>
              </div>
              {viewInvoice.discountAmount > 0 && (
                <div className="flex justify-between text-sm text-emerald-600 dark:text-emerald-400">
                  <span>Discount</span><span>-{formatCurrency(viewInvoice.discountAmount)}</span>
                </div>
              )}
              <div className="flex justify-between text-sm text-slate-600 dark:text-slate-400">
                <span>Tax (10%)</span><span>{formatCurrency(viewInvoice.taxAmount)}</span>
              </div>
              <div className="flex justify-between text-base font-bold text-slate-900 dark:text-slate-100 pt-2 border-t border-slate-200 dark:border-slate-700">
                <span>Total</span>
                <span className="text-indigo-600 dark:text-indigo-400">{formatCurrency(viewInvoice.totalAmount)}</span>
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}

export default BillingPage;

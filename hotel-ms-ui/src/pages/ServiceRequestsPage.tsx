import { useState, useEffect, useCallback } from 'react';
import { Search, X, RefreshCw, Plus } from 'lucide-react';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { Table } from '../components/ui/Table';
import { Modal } from '../components/ui/Modal';
import { useToast, useAuthStore } from '../lib/store';
import { serviceRequestService } from '../services/serviceRequest.service';
import { reservationService } from '../services/reservation.service';
import type { ServiceRequest, ServiceRequestState, Reservation } from '../types';
import { formatDate } from '../lib/utils';

const STATE_VARIANT: Record<ServiceRequestState, 'warning' | 'default' | 'success' | 'danger'> = {
  Pending: 'warning',
  InProgress: 'default',
  Completed: 'success',
  Cancelled: 'danger',
};

const SERVICE_TYPES = [
  'Housekeeping',
  'Room Service',
  'Maintenance',
  'Extra Towels/Bedding',
  'Wake-up Call',
  'Luggage Assistance',
  'Transportation',
  'Other',
];

export function ServiceRequestsPage() {
  const [requests, setRequests] = useState<ServiceRequest[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [stateFilter, setStateFilter] = useState('');
  const [viewRequest, setViewRequest] = useState<ServiceRequest | null>(null);
  const [isBusy, setIsBusy] = useState(false);

  // Create modal (guests)
  const [showCreate, setShowCreate] = useState(false);
  const [guestReservations, setGuestReservations] = useState<Reservation[]>([]);
  const [createForm, setCreateForm] = useState({ reservationId: '', serviceType: '', notes: '' });
  const [isCreating, setIsCreating] = useState(false);

  const toast = useToast();
  const { user } = useAuthStore();
  const isGuestRole = (user?.roles ?? []).includes('Guest') &&
    !['Admin', 'SuperAdmin', 'FrontDesk', 'Housekeeping', 'Developer'].some((r) => (user?.roles ?? []).includes(r));

  const fetchRequests = useCallback(async () => {
    setIsLoading(true);
    try {
      const result = await serviceRequestService.getAll({ state: stateFilter || undefined });
      setRequests(result);
    } catch {
      toast.error('Failed to load service requests');
    } finally {
      setIsLoading(false);
    }
  }, [stateFilter, toast]);

  useEffect(() => { fetchRequests(); }, [fetchRequests]);

  const openCreate = async () => {
    setCreateForm({ reservationId: '', serviceType: '', notes: '' });
    if (isGuestRole && guestReservations.length === 0) {
      try {
        const res = await reservationService.getAll({});
        setGuestReservations(res.data.filter((r) => ['Confirmed', 'CheckedIn'].includes(r.status)));
      } catch {
        // non-fatal — guest can still type their reservation ID
      }
    }
    setShowCreate(true);
  };

  const handleCreate = async () => {
    if (!createForm.reservationId || !createForm.serviceType) {
      toast.error('Required fields missing', 'Please select a reservation and service type');
      return;
    }
    setIsCreating(true);
    try {
      const created = await serviceRequestService.create({
        reservationId: Number(createForm.reservationId),
        serviceType: createForm.serviceType,
        notes: createForm.notes || undefined,
      });
      setRequests((prev) => [created, ...prev]);
      setShowCreate(false);
      toast.success('Service request submitted', `#${created.id} — ${created.requestType}`);
    } catch (err) {
      toast.error('Failed to submit', err instanceof Error ? err.message : 'Please try again');
    } finally {
      setIsCreating(false);
    }
  };

  const handleTransition = async (request: ServiceRequest, trigger: string) => {
    setIsBusy(true);
    try {
      const updated = await serviceRequestService.transition(request.id, trigger);
      setRequests((prev) => prev.map((r) => (r.id === request.id ? updated : r)));
      setViewRequest(updated);
      toast.success('Status updated', `Request #${request.id}`);
    } catch (err) {
      toast.error('Failed', err instanceof Error ? err.message : 'Could not update status');
    } finally {
      setIsBusy(false);
    }
  };

  const filtered = requests.filter((r) => {
    if (!search) return true;
    const q = search.toLowerCase();
    return (
      r.requestType.toLowerCase().includes(q) ||
      (r.roomNumber ?? '').toLowerCase().includes(q) ||
      (r.description ?? '').toLowerCase().includes(q)
    );
  });

  const columns = [
    {
      key: 'id',
      header: 'ID',
      render: (r: ServiceRequest) => (
        <span className="font-mono text-xs bg-slate-100 dark:bg-slate-700 px-2 py-1 rounded-md">#{r.id}</span>
      ),
    },
    {
      key: 'type',
      header: 'Type',
      render: (r: ServiceRequest) => (
        <div>
          <p className="font-medium text-slate-800 dark:text-slate-200">{r.requestType}</p>
          {r.description && <p className="text-xs text-slate-500 dark:text-slate-400 truncate max-w-xs">{r.description}</p>}
        </div>
      ),
    },
    {
      key: 'reservation',
      header: 'Reservation',
      render: (r: ServiceRequest) => (
        <span className="text-sm text-slate-600 dark:text-slate-400">#{r.reservationId}</span>
      ),
    },
    {
      key: 'state',
      header: 'Status',
      render: (r: ServiceRequest) => (
        <Badge variant={STATE_VARIANT[r.state] ?? 'default'} dot>
          {r.state}
        </Badge>
      ),
    },
    {
      key: 'date',
      header: 'Created',
      render: (r: ServiceRequest) => formatDate(r.creationDate),
    },
    {
      key: 'actions',
      header: '',
      render: (r: ServiceRequest) => (
        <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setViewRequest(r); }}>
          View
        </Button>
      ),
    },
  ];

  const getNextActions = (state: ServiceRequestState) => {
    switch (state) {
      case 'Pending': return [{ label: 'Start', trigger: 'Start', variant: 'default' as const }];
      case 'InProgress': return [
        { label: 'Complete', trigger: 'Complete', variant: 'default' as const },
        { label: 'Cancel', trigger: 'Cancel', variant: 'outline' as const },
      ];
      default: return [];
    }
  };

  const inputClass = 'w-full h-10 px-3 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900 dark:text-slate-100 placeholder:text-slate-400';

  return (
    <div className="page-container">
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Service Requests</h2>
          <p className="page-subtitle">{requests.length} request{requests.length !== 1 ? 's' : ''}</p>
        </div>
        <div className="flex gap-2">
          {isGuestRole && (
            <Button leftIcon={<Plus className="h-4 w-4" />} onClick={openCreate}>
              New Request
            </Button>
          )}
          <Button variant="outline" leftIcon={<RefreshCw className="h-4 w-4" />} onClick={fetchRequests}>
            Refresh
          </Button>
        </div>
      </div>

      <Card className="mb-6" padding="sm">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              type="text"
              placeholder="Search by type or description..."
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
            value={stateFilter}
            onChange={(e) => setStateFilter(e.target.value)}
            className="sm:w-44 h-10 px-3 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          >
            <option value="">All Statuses</option>
            <option value="Requested">Pending</option>
            <option value="InProgress">In Progress</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </Card>

      <Card padding="none">
        <Table
          data={filtered}
          columns={columns}
          isLoading={isLoading}
          emptyMessage="No service requests"
          emptyDescription={isGuestRole ? 'Submit a new request using the button above' : 'Service requests are created from reservations'}
          onRowClick={(r) => setViewRequest(r as unknown as ServiceRequest)}
        />
      </Card>

      {/* View modal */}
      <Modal
        isOpen={!!viewRequest}
        onClose={() => setViewRequest(null)}
        title={`Service Request #${viewRequest?.id}`}
        description={viewRequest ? `${viewRequest.requestType} · Reservation #${viewRequest.reservationId}` : ''}
        size="md"
        footer={
          viewRequest ? (
            <div className="flex gap-2 flex-wrap justify-end">
              {!isGuestRole && getNextActions(viewRequest.state).map(({ label, trigger, variant }) => (
                <Button key={trigger} variant={variant} isLoading={isBusy}
                  onClick={() => handleTransition(viewRequest, trigger)}
                  className={trigger === 'Cancel' ? 'text-red-600 border-red-300 hover:bg-red-50' : ''}>
                  {label}
                </Button>
              ))}
              <Button variant="outline" onClick={() => setViewRequest(null)}>Close</Button>
            </div>
          ) : null
        }
      >
        {viewRequest && (
          <div className="space-y-4">
            <div className="flex items-center gap-3">
              <Badge variant={STATE_VARIANT[viewRequest.state] ?? 'default'} dot>{viewRequest.state}</Badge>
            </div>

            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">Request Type</p>
                <p className="font-medium text-slate-800 dark:text-slate-200">{viewRequest.requestType}</p>
              </div>
              <div>
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">Reservation</p>
                <p className="font-medium text-slate-800 dark:text-slate-200">#{viewRequest.reservationId}</p>
              </div>
              <div>
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">Created</p>
                <p className="font-medium text-slate-800 dark:text-slate-200">{formatDate(viewRequest.creationDate)}</p>
              </div>
              {viewRequest.lastModifiedDate && (
                <div>
                  <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">Completed</p>
                  <p className="font-medium text-slate-800 dark:text-slate-200">{formatDate(viewRequest.lastModifiedDate)}</p>
                </div>
              )}
            </div>

            {viewRequest.notes && (
              <div className="p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">Notes</p>
                <p className="text-sm text-slate-700 dark:text-slate-300">{viewRequest.notes}</p>
              </div>
            )}
          </div>
        )}
      </Modal>

      {/* Create modal (guests) */}
      <Modal
        isOpen={showCreate}
        onClose={() => setShowCreate(false)}
        title="New Service Request"
        description="Submit a request for your current stay"
        size="md"
        footer={
          <div className="flex gap-2 justify-end">
            <Button variant="outline" onClick={() => setShowCreate(false)}>Cancel</Button>
            <Button isLoading={isCreating} onClick={handleCreate}>Submit Request</Button>
          </div>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
              Reservation <span className="text-red-500">*</span>
            </label>
            {guestReservations.length > 0 ? (
              <select
                value={createForm.reservationId}
                onChange={(e) => setCreateForm((f) => ({ ...f, reservationId: e.target.value }))}
                className={inputClass}
              >
                <option value="">Select your reservation...</option>
                {guestReservations.map((r) => (
                  <option key={r.id} value={r.id}>
                    #{r.id} — Room {r.roomNumber} · {r.status}
                  </option>
                ))}
              </select>
            ) : (
              <input
                type="number"
                placeholder="Enter your reservation ID"
                value={createForm.reservationId}
                onChange={(e) => setCreateForm((f) => ({ ...f, reservationId: e.target.value }))}
                className={inputClass}
              />
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
              Service Type <span className="text-red-500">*</span>
            </label>
            <select
              value={createForm.serviceType}
              onChange={(e) => setCreateForm((f) => ({ ...f, serviceType: e.target.value }))}
              className={inputClass}
            >
              <option value="">Select a service type...</option>
              {SERVICE_TYPES.map((t) => (
                <option key={t} value={t}>{t}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Notes</label>
            <textarea
              rows={3}
              placeholder="Any additional details..."
              value={createForm.notes}
              onChange={(e) => setCreateForm((f) => ({ ...f, notes: e.target.value }))}
              className="w-full px-3 py-2 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900 dark:text-slate-100 placeholder:text-slate-400 resize-none"
            />
          </div>
        </div>
      </Modal>
    </div>
  );
}

export default ServiceRequestsPage;

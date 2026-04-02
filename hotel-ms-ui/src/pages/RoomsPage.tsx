import { useState, useEffect, useCallback } from 'react';
import {
  Search, LayoutGrid, List, BedDouble, Users, DollarSign, X, Plus,
  RefreshCw, ChevronRight, Zap, Wrench, Wind, LogIn, LogOut, Eye
} from 'lucide-react';
import { roomService } from '../services/room.service';
import { propertyService } from '../services/property.service';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { Card } from '../components/ui/Card';
import { Select } from '../components/ui/Select';
import { Table } from '../components/ui/Table';
import { Modal } from '../components/ui/Modal';
import { Input } from '../components/ui/Input';
import { useToast, useAuthStore } from '../lib/store';
import type { Room, Property, RoomTrigger } from '../types';
import { formatCurrency, ROOM_STATUS_COLORS, ROOM_STATUS_DOT, cn } from '../lib/utils';

const TYPE_OPTIONS = [
  { value: '', label: 'All Types' },
  { value: 'Standard', label: 'Standard' },
  { value: 'Deluxe', label: 'Deluxe' },
  { value: 'Suite', label: 'Suite' },
  { value: 'Presidential', label: 'Presidential' },
  { value: 'Twin', label: 'Twin' },
  { value: 'Double', label: 'Double' },
];

const STATUS_OPTIONS = [
  { value: '', label: 'All Statuses' },
  { value: 'Available', label: 'Available' },
  { value: 'Occupied', label: 'Occupied' },
  { value: 'Maintenance', label: 'Maintenance' },
  { value: 'Cleaning', label: 'Cleaning' },
];

const TRIGGER_CONFIG: Record<RoomTrigger, { label: string; icon: React.ReactNode; className: string }> = {
  CheckIn:          { label: 'Check In',          icon: <LogIn className="h-4 w-4" />,  className: 'bg-blue-600 hover:bg-blue-700 text-white' },
  CheckOut:         { label: 'Check Out',         icon: <LogOut className="h-4 w-4" />, className: 'bg-emerald-600 hover:bg-emerald-700 text-white' },
  SetCleaning:      { label: 'Start Cleaning',    icon: <Wind className="h-4 w-4" />,   className: 'bg-violet-600 hover:bg-violet-700 text-white' },
  FinishCleaning:   { label: 'Finish Cleaning',   icon: <Zap className="h-4 w-4" />,    className: 'bg-teal-600 hover:bg-teal-700 text-white' },
  SetMaintenance:   { label: 'Set Maintenance',   icon: <Wrench className="h-4 w-4" />, className: 'bg-amber-600 hover:bg-amber-700 text-white' },
  FinishMaintenance:{ label: 'End Maintenance',   icon: <Zap className="h-4 w-4" />,    className: 'bg-slate-600 hover:bg-slate-700 text-white' },
};

// ── Room Card ──────────────────────────────────────────────────────────────────
function RoomCard({ room, onView }: { room: Room; onView: (r: Room) => void }) {
  return (
    <Card hover className="group relative overflow-hidden cursor-pointer" padding="none" onClick={() => onView(room)}>
      <div className={cn('h-1.5 w-full', ROOM_STATUS_DOT[room.status] ?? 'bg-slate-300')} />
      <div className="p-5">
        <div className="flex items-start justify-between mb-3">
          <div>
            <div className="flex items-baseline gap-2">
              <span className="text-2xl font-bold text-slate-900 dark:text-slate-100">{room.roomNumber}</span>
              {room.floor && <span className="text-xs text-slate-500 dark:text-slate-400">Floor {room.floor}</span>}
            </div>
            <p className="text-sm text-slate-600 dark:text-slate-400 mt-0.5">{room.type ?? 'Standard'}</p>
          </div>
          <Badge className={cn(ROOM_STATUS_COLORS[room.status] ?? '')} dot>{room.status}</Badge>
        </div>

        <div className="grid grid-cols-2 gap-3 my-3">
          <div className="flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400">
            <Users className="h-3.5 w-3.5" />
            <span>{room.capacity ?? 2} guests</span>
          </div>
          <div className="flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400">
            <DollarSign className="h-3.5 w-3.5" />
            <span>{formatCurrency(room.pricePerNight)}/night</span>
          </div>
        </div>

        <div className="flex items-center justify-between pt-3 border-t border-slate-100 dark:border-slate-700">
          <span className="text-xs text-slate-400">Click to manage</span>
          <ChevronRight className="h-4 w-4 text-slate-400 group-hover:text-indigo-500 transition-colors" />
        </div>
      </div>
    </Card>
  );
}

// ── Room Detail Modal ──────────────────────────────────────────────────────────
function RoomDetailModal({
  room,
  onClose,
  onTriggered,
}: {
  room: Room | null;
  onClose: () => void;
  onTriggered: (updated: Room) => void;
}) {
  const [triggers, setTriggers] = useState<RoomTrigger[]>([]);
  const [loadingTrigger, setLoadingTrigger] = useState<RoomTrigger | null>(null);
  const toast = useToast();

  useEffect(() => {
    if (!room) return;
    const id = Number(room.id);
    if (!id) return;
    roomService.getAvailableTriggers(id).then(setTriggers).catch(() => setTriggers([]));
  }, [room]);

  const fireTrigger = async (trigger: RoomTrigger) => {
    if (!room) return;
    setLoadingTrigger(trigger);
    try {
      const updated = await roomService.changeState(Number(room.id), trigger);
      toast.success('Room updated', `${TRIGGER_CONFIG[trigger].label} completed`);
      onTriggered(updated);
      onClose();
    } catch {
      toast.error('Action failed', 'Could not change room state');
    } finally {
      setLoadingTrigger(null);
    }
  };

  if (!room) return null;

  return (
    <Modal isOpen={!!room} onClose={onClose} title={`Room ${room.roomNumber}`} size="md"
      footer={<Button variant="outline" onClick={onClose}>Close</Button>}
    >
      <div className="space-y-5">
        {/* Status banner */}
        <div className={cn('flex items-center gap-3 p-4 rounded-xl border',
          room.status === 'Available' ? 'bg-emerald-50 dark:bg-emerald-900/20 border-emerald-200 dark:border-emerald-800' :
          room.status === 'Occupied'  ? 'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800' :
          room.status === 'Maintenance' ? 'bg-amber-50 dark:bg-amber-900/20 border-amber-200 dark:border-amber-800' :
          'bg-violet-50 dark:bg-violet-900/20 border-violet-200 dark:border-violet-800'
        )}>
          <div className={cn('w-3 h-3 rounded-full', ROOM_STATUS_DOT[room.status])} />
          <div>
            <p className="text-sm font-semibold text-slate-900 dark:text-slate-100">Current State: {room.status}</p>
            <p className="text-xs text-slate-500 dark:text-slate-400">{room.type} · Floor {room.floor ?? '—'} · {room.capacity ?? 2} guests max</p>
          </div>
        </div>

        {/* Details grid */}
        <div className="grid grid-cols-2 gap-3 text-sm">
          <div className="bg-slate-50 dark:bg-slate-700/50 rounded-lg p-3">
            <p className="text-xs text-slate-500 dark:text-slate-400 mb-0.5">Rate per Night</p>
            <p className="font-bold text-slate-900 dark:text-slate-100">{formatCurrency(room.pricePerNight)}</p>
          </div>
          <div className="bg-slate-50 dark:bg-slate-700/50 rounded-lg p-3">
            <p className="text-xs text-slate-500 dark:text-slate-400 mb-0.5">Room ID</p>
            <p className="font-bold text-slate-900 dark:text-slate-100">#{room.id}</p>
          </div>
        </div>

        {/* State machine actions */}
        <div>
          <p className="text-sm font-semibold text-slate-700 dark:text-slate-300 mb-3">Available Actions</p>
          {triggers.length === 0 ? (
            <p className="text-sm text-slate-400 text-center py-4">No actions available for this state</p>
          ) : (
            <div className="grid grid-cols-1 gap-2">
              {triggers.map((trigger) => {
                const cfg = TRIGGER_CONFIG[trigger];
                if (!cfg) return null;
                return (
                  <button
                    key={trigger}
                    onClick={() => fireTrigger(trigger)}
                    disabled={!!loadingTrigger}
                    className={cn(
                      'flex items-center gap-3 w-full px-4 py-3 rounded-xl font-medium text-sm transition-all',
                      cfg.className,
                      loadingTrigger === trigger ? 'opacity-70 cursor-wait' : 'hover:scale-[1.01] active:scale-[0.99]'
                    )}
                  >
                    {loadingTrigger === trigger ? (
                      <RefreshCw className="h-4 w-4 animate-spin" />
                    ) : cfg.icon}
                    {cfg.label}
                  </button>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </Modal>
  );
}

// ── Add Room Form ──────────────────────────────────────────────────────────────
interface AddRoomForm {
  number: string;
  type: string;
  floor: string;
  capacity: string;
  pricePerNight: string;
  propertyId: string;
  description: string;
}

const EMPTY_FORM: AddRoomForm = { number: '', type: 'Standard', floor: '1', capacity: '2', pricePerNight: '', propertyId: '', description: '' };

// ── Page ──────────────────────────────────────────────────────────────────────
export function RoomsPage() {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [properties, setProperties] = useState<Property[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [view, setView] = useState<'grid' | 'list'>('grid');
  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [isAddOpen, setIsAddOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [detailRoom, setDetailRoom] = useState<Room | null>(null);
  const [form, setForm] = useState<AddRoomForm>(EMPTY_FORM);
  const toast = useToast();
  const { tenantId } = useAuthStore();

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    try {
      const [roomData, propData] = await Promise.all([
        roomService.getAll({ pageSize: 200 } as Parameters<typeof roomService.getAll>[0]),
        propertyService.getAll({ tenantId: tenantId ?? undefined }),
      ]);
      setRooms(roomData);
      setProperties(propData);
    } catch {
      toast.error('Load failed', 'Could not fetch rooms');
    } finally {
      setIsLoading(false);
    }
  }, [tenantId, toast]);

  useEffect(() => { fetchData(); }, [fetchData]);

  const handleAdd = async () => {
    if (!form.number.trim()) { toast.error('Validation', 'Room number is required'); return; }
    if (!form.pricePerNight) { toast.error('Validation', 'Price is required'); return; }
    setIsSubmitting(true);
    try {
      const created = await roomService.create({
        number: form.number,
        type: form.type,
        floor: Number(form.floor),
        capacity: Number(form.capacity),
        pricePerNight: Number(form.pricePerNight),
        propertyId: form.propertyId ? Number(form.propertyId) : (properties[0] ? Number(properties[0].id) : 1),
        description: form.description || undefined,
      } as Parameters<typeof roomService.create>[0]);
      setRooms((prev) => [created, ...prev]);
      toast.success('Room created', `Room ${form.number} added successfully`);
      setIsAddOpen(false);
      setForm(EMPTY_FORM);
    } catch {
      toast.error('Failed', 'Could not create room');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleTriggered = (updated: Room) => {
    setRooms((prev) => prev.map((r) => (r.id === updated.id ? updated : r)));
  };

  const filtered = rooms.filter((r) => {
    const matchSearch = !search || r.roomNumber.includes(search) || (r.type ?? '').toLowerCase().includes(search.toLowerCase());
    const matchType = !typeFilter || r.type === typeFilter;
    const matchStatus = !statusFilter || r.status === statusFilter;
    return matchSearch && matchType && matchStatus;
  });

  const statusCounts = {
    Available:   rooms.filter((r) => r.status === 'Available').length,
    Occupied:    rooms.filter((r) => r.status === 'Occupied').length,
    Maintenance: rooms.filter((r) => r.status === 'Maintenance').length,
    Cleaning:    rooms.filter((r) => r.status === 'Cleaning').length,
  };

  const columns = [
    { key: 'roomNumber', header: 'Room', render: (r: Room) => <span className="font-mono font-bold">{r.roomNumber}</span> },
    { key: 'type', header: 'Type', render: (r: Room) => r.type ?? '—' },
    { key: 'floor', header: 'Floor', render: (r: Room) => r.floor ? `Floor ${r.floor}` : '—' },
    { key: 'capacity', header: 'Capacity', render: (r: Room) => `${r.capacity ?? 2} guests` },
    { key: 'pricePerNight', header: 'Rate/Night', render: (r: Room) => formatCurrency(r.pricePerNight) },
    { key: 'status', header: 'Status', render: (r: Room) => <Badge className={ROOM_STATUS_COLORS[r.status] ?? ''} dot>{r.status}</Badge> },
    {
      key: 'actions', header: '',
      render: (r: Room) => (
        <button onClick={() => setDetailRoom(r)} className="flex items-center gap-1 text-xs text-indigo-600 hover:underline font-medium">
          <Eye className="h-3.5 w-3.5" /> Manage
        </button>
      ),
    },
  ];

  return (
    <div className="page-container">
      {/* Header */}
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Rooms</h2>
          <p className="page-subtitle">{rooms.length} total rooms across all properties</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" leftIcon={<RefreshCw className="h-4 w-4" />} onClick={fetchData} isLoading={isLoading}>Refresh</Button>
          <div className="flex items-center gap-1 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg p-1">
            <button onClick={() => setView('grid')} className={cn('p-1.5 rounded-md transition-colors', view === 'grid' ? 'bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600' : 'text-slate-500 hover:text-slate-700')}><LayoutGrid className="h-4 w-4" /></button>
            <button onClick={() => setView('list')} className={cn('p-1.5 rounded-md transition-colors', view === 'list' ? 'bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600' : 'text-slate-500 hover:text-slate-700')}><List className="h-4 w-4" /></button>
          </div>
          <Button leftIcon={<Plus className="h-4 w-4" />} onClick={() => setIsAddOpen(true)}>Add Room</Button>
        </div>
      </div>

      {/* Status counters — clickable filters */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-6">
        {Object.entries(statusCounts).map(([status, count]) => (
          <button key={status} onClick={() => setStatusFilter(statusFilter === status ? '' : status)}
            className={cn('p-3 rounded-xl border text-left transition-all',
              statusFilter === status
                ? 'border-indigo-300 dark:border-indigo-600 bg-indigo-50 dark:bg-indigo-900/20 ring-2 ring-indigo-200 dark:ring-indigo-800'
                : 'bg-white dark:bg-slate-800 border-slate-100 dark:border-slate-700 hover:shadow-md hover:-translate-y-0.5'
            )}
          >
            <div className="flex items-center gap-2 mb-1">
              <span className={cn('w-2.5 h-2.5 rounded-full', ROOM_STATUS_DOT[status] ?? 'bg-slate-300')} />
              <span className="text-xs font-medium text-slate-500 dark:text-slate-400">{status}</span>
            </div>
            {isLoading ? <div className="h-7 w-8 bg-slate-200 dark:bg-slate-700 rounded animate-pulse" /> :
              <span className="text-2xl font-bold text-slate-900 dark:text-slate-100">{count}</span>}
          </button>
        ))}
      </div>

      {/* Filters */}
      <Card className="mb-6" padding="sm">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input type="text" placeholder="Search room number or type…" value={search} onChange={(e) => setSearch(e.target.value)}
              className="w-full h-10 pl-10 pr-4 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-slate-900 dark:text-slate-100 placeholder:text-slate-400" />
            {search && <button onClick={() => setSearch('')} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"><X className="h-4 w-4" /></button>}
          </div>
          <div className="sm:w-40"><Select options={TYPE_OPTIONS} value={typeFilter} onChange={(e) => setTypeFilter(e.target.value)} /></div>
          <div className="sm:w-40"><Select options={STATUS_OPTIONS} value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} /></div>
        </div>
      </Card>

      <p className="text-sm text-slate-500 dark:text-slate-400 mb-4">Showing {filtered.length} of {rooms.length} rooms</p>

      {/* Grid / List */}
      {view === 'grid' ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          {isLoading ? Array.from({ length: 8 }).map((_, i) => <div key={i} className="h-52 bg-slate-200 dark:bg-slate-700 rounded-2xl animate-pulse" />) :
            filtered.length === 0 ? (
              <div className="col-span-full py-20 text-center text-slate-400">
                <BedDouble className="h-12 w-12 mx-auto mb-3 opacity-30" />
                <p className="font-medium">No rooms found</p>
                <p className="text-sm">Try adjusting your filters or add a new room</p>
              </div>
            ) : filtered.map((room) => <RoomCard key={room.id} room={room} onView={setDetailRoom} />)}
        </div>
      ) : (
        <Card padding="none">
          <Table data={filtered} columns={columns} isLoading={isLoading} emptyMessage="No rooms found" />
        </Card>
      )}

      {/* Room Detail Modal */}
      <RoomDetailModal room={detailRoom} onClose={() => setDetailRoom(null)} onTriggered={handleTriggered} />

      {/* Add Room Modal */}
      <Modal isOpen={isAddOpen} onClose={() => { setIsAddOpen(false); setForm(EMPTY_FORM); }} title="Add New Room" size="lg"
        footer={
          <>
            <Button variant="outline" onClick={() => { setIsAddOpen(false); setForm(EMPTY_FORM); }}>Cancel</Button>
            <Button isLoading={isSubmitting} onClick={handleAdd}>Add Room</Button>
          </>
        }
      >
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <Input label="Room Number" required placeholder="e.g. 201" value={form.number} onChange={(e) => setForm((f) => ({ ...f, number: e.target.value }))} />
            <Input label="Floor" type="number" min={1} value={form.floor} onChange={(e) => setForm((f) => ({ ...f, floor: e.target.value }))} />
            <Select label="Type" required options={TYPE_OPTIONS.filter((o) => o.value)} value={form.type} onChange={(e) => setForm((f) => ({ ...f, type: e.target.value }))} />
            <Input label="Capacity (guests)" type="number" min={1} max={10} value={form.capacity} onChange={(e) => setForm((f) => ({ ...f, capacity: e.target.value }))} />
            <Input label="Price per Night ($)" type="number" required min={1} value={form.pricePerNight} onChange={(e) => setForm((f) => ({ ...f, pricePerNight: e.target.value }))} />
            {properties.length > 0 && (
              <Select label="Property" options={[{ value: '', label: 'Select property…' }, ...properties.map((p) => ({ value: String(p.id), label: p.name }))]}
                value={form.propertyId} onChange={(e) => setForm((f) => ({ ...f, propertyId: e.target.value }))} />
            )}
          </div>
          <Input label="Description" placeholder="Room description (optional)" value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
        </div>
      </Modal>
    </div>
  );
}

export default RoomsPage;

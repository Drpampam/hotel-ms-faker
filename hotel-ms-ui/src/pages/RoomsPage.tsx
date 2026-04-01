import { useState, useEffect, useCallback } from 'react';
import { useForm } from 'react-hook-form';
import {
  Search,
  LayoutGrid,
  List,
  BedDouble,
  Users,
  DollarSign,
  X,
  Plus,
} from 'lucide-react';
import { roomService } from '../services/room.service';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { Card } from '../components/ui/Card';
import { Select } from '../components/ui/Select';
import { Table } from '../components/ui/Table';
import { Modal } from '../components/ui/Modal';
import { Input } from '../components/ui/Input';
import { useToast } from '../lib/store';
import type { Room } from '../types';
import { formatCurrency, ROOM_STATUS_COLORS, ROOM_STATUS_DOT, cn } from '../lib/utils';

const MOCK_ROOMS: Room[] = Array.from({ length: 12 }, (_, i) => ({
  id: `room-${i + 1}`,
  roomNumber: String(100 + (Math.floor(i / 3) + 1) * 100 + (i % 3 + 1)),
  type: (['Standard', 'Deluxe', 'Suite', 'Presidential'] as Room['type'][])[i % 4],
  status: (
    ['Available', 'Occupied', 'Available', 'Maintenance', 'Available', 'Occupied', 'Available', 'Reserved', 'Available', 'Occupied', 'Available', 'Cleaning'] as Room['status'][]
  )[i],
  floor: Math.floor(i / 3) + 1,
  pricePerNight: [120, 180, 350, 800, 120, 180, 350, 800, 120, 180, 350, 800][i],
  capacity: [2, 2, 4, 4, 2, 2, 4, 4, 2, 2, 4, 4][i],
  amenities: ['WiFi', 'TV', 'AC', 'Mini Bar'].slice(0, (i % 4) + 1),
  tenantId: 1,
  description: `Comfortable ${(['Standard', 'Deluxe', 'Suite', 'Presidential'] as string[])[i % 4]} room with modern amenities`,
}));

const FLOOR_OPTIONS = [
  { value: '', label: 'All Floors' },
  { value: '1', label: 'Floor 1' },
  { value: '2', label: 'Floor 2' },
  { value: '3', label: 'Floor 3' },
  { value: '4', label: 'Floor 4' },
  { value: '5', label: 'Floor 5' },
];

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
  { value: 'Reserved', label: 'Reserved' },
  { value: 'Cleaning', label: 'Cleaning' },
];

interface RoomFormData {
  roomNumber: string;
  floor: number;
  type: string;
  status: string;
  pricePerNight: number;
  capacity: number;
  description?: string;
}

function RoomCard({
  room,
  onStatusChange,
}: {
  room: Room;
  onStatusChange: (id: string, status: string) => void;
}) {
  return (
    <Card hover className="group relative overflow-hidden" padding="none">
      {/* Status bar */}
      <div className={cn('h-1.5 w-full', ROOM_STATUS_DOT[room.status])} />
      <div className="p-5">
        <div className="flex items-start justify-between mb-3">
          <div>
            <div className="flex items-baseline gap-2">
              <span className="text-2xl font-bold text-slate-900 dark:text-slate-100">
                {room.roomNumber}
              </span>
              <span className="text-xs text-slate-500 dark:text-slate-400">Floor {room.floor}</span>
            </div>
            <p className="text-sm text-slate-600 dark:text-slate-400 mt-0.5">{room.type}</p>
          </div>
          <Badge className={cn(ROOM_STATUS_COLORS[room.status])} dot>
            {room.status}
          </Badge>
        </div>

        <div className="grid grid-cols-2 gap-3 my-4">
          <div className="flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400">
            <Users className="h-3.5 w-3.5" />
            <span>{room.capacity} guests</span>
          </div>
          <div className="flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400">
            <DollarSign className="h-3.5 w-3.5" />
            <span>{formatCurrency(room.pricePerNight)}/night</span>
          </div>
        </div>

        {room.amenities && room.amenities.length > 0 && (
          <div className="flex flex-wrap gap-1 mb-4">
            {room.amenities.slice(0, 3).map((a) => (
              <span
                key={a}
                className="text-xs bg-slate-100 dark:bg-slate-700 text-slate-600 dark:text-slate-400 px-1.5 py-0.5 rounded"
              >
                {a}
              </span>
            ))}
            {room.amenities.length > 3 && (
              <span className="text-xs text-slate-400">+{room.amenities.length - 3}</span>
            )}
          </div>
        )}

        <div className="flex gap-2 pt-3 border-t border-slate-100 dark:border-slate-700">
          {room.status === 'Available' ? (
            <button
              onClick={() => onStatusChange(room.id, 'Occupied')}
              className="flex-1 text-xs py-1.5 rounded-lg bg-blue-50 hover:bg-blue-100 dark:bg-blue-900/20 dark:hover:bg-blue-900/30 text-blue-600 dark:text-blue-400 transition-colors font-medium"
            >
              Mark Occupied
            </button>
          ) : room.status === 'Occupied' ? (
            <button
              onClick={() => onStatusChange(room.id, 'Available')}
              className="flex-1 text-xs py-1.5 rounded-lg bg-emerald-50 hover:bg-emerald-100 dark:bg-emerald-900/20 dark:hover:bg-emerald-900/30 text-emerald-600 dark:text-emerald-400 transition-colors font-medium"
            >
              Mark Available
            </button>
          ) : (
            <button
              onClick={() => onStatusChange(room.id, 'Available')}
              className="flex-1 text-xs py-1.5 rounded-lg bg-slate-100 hover:bg-slate-200 dark:bg-slate-700 dark:hover:bg-slate-600 text-slate-600 dark:text-slate-400 transition-colors font-medium"
            >
              Set Available
            </button>
          )}
          <button
            onClick={() => onStatusChange(room.id, 'Maintenance')}
            className="text-xs px-2 py-1.5 rounded-lg bg-amber-50 hover:bg-amber-100 dark:bg-amber-900/20 dark:hover:bg-amber-900/30 text-amber-600 dark:text-amber-400 transition-colors font-medium"
          >
            Maint.
          </button>
        </div>
      </div>
    </Card>
  );
}

export function RoomsPage() {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [view, setView] = useState<'grid' | 'list'>('grid');
  const [search, setSearch] = useState('');
  const [floorFilter, setFloorFilter] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [isAddOpen, setIsAddOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const toast = useToast();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RoomFormData>({
    defaultValues: { status: 'Available', capacity: 2, floor: 1 },
  });

  const fetchRooms = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await roomService.getAll();
      setRooms(data.length > 0 ? data : MOCK_ROOMS);
    } catch {
      setRooms(MOCK_ROOMS);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchRooms();
  }, [fetchRooms]);

  const handleStatusChange = async (id: string, status: string) => {
    try {
      await roomService.updateStatus(id, status);
    } catch {
      // Silently continue with local update
    }
    setRooms((prev) =>
      prev.map((r) => (r.id === id ? { ...r, status: status as Room['status'] } : r))
    );
    toast.success('Room updated', `Room status changed to ${status}`);
  };

  const filtered = rooms.filter((r) => {
    const matchSearch =
      !search ||
      r.roomNumber.includes(search) ||
      r.type.toLowerCase().includes(search.toLowerCase());
    const matchFloor = !floorFilter || r.floor === Number(floorFilter);
    const matchType = !typeFilter || r.type === typeFilter;
    const matchStatus = !statusFilter || r.status === statusFilter;
    return matchSearch && matchFloor && matchType && matchStatus;
  });

  const onAddRoom = async (data: RoomFormData) => {
    setIsSubmitting(true);
    try {
      await roomService.create(data as Partial<Room>);
      toast.success('Room created', 'The room has been added successfully');
      setIsAddOpen(false);
      reset();
      await fetchRooms();
    } catch {
      toast.error('Failed to create room', 'Please check the details and try again');
    } finally {
      setIsSubmitting(false);
    }
  };

  const statusCounts = {
    Available: rooms.filter((r) => r.status === 'Available').length,
    Occupied: rooms.filter((r) => r.status === 'Occupied').length,
    Maintenance: rooms.filter((r) => r.status === 'Maintenance').length,
  };

  const columns = [
    {
      key: 'roomNumber',
      header: 'Room No.',
      render: (r: Room) => <span className="font-mono font-bold">{r.roomNumber}</span>,
    },
    { key: 'type', header: 'Type' },
    { key: 'floor', header: 'Floor', render: (r: Room) => `Floor ${r.floor}` },
    { key: 'capacity', header: 'Capacity', render: (r: Room) => `${r.capacity} guests` },
    {
      key: 'pricePerNight',
      header: 'Rate/Night',
      render: (r: Room) => formatCurrency(r.pricePerNight),
    },
    {
      key: 'status',
      header: 'Status',
      render: (r: Room) => (
        <Badge className={ROOM_STATUS_COLORS[r.status]} dot>
          {r.status}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: '',
      render: (r: Room) => (
        <div className="flex gap-3">
          {r.status !== 'Available' && (
            <button
              onClick={() => handleStatusChange(r.id, 'Available')}
              className="text-xs text-emerald-600 hover:underline"
            >
              Available
            </button>
          )}
          {r.status !== 'Occupied' && (
            <button
              onClick={() => handleStatusChange(r.id, 'Occupied')}
              className="text-xs text-blue-600 hover:underline"
            >
              Occupied
            </button>
          )}
          {r.status !== 'Maintenance' && (
            <button
              onClick={() => handleStatusChange(r.id, 'Maintenance')}
              className="text-xs text-amber-600 hover:underline"
            >
              Maint.
            </button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="page-container">
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Rooms</h2>
          <p className="page-subtitle">{rooms.length} total rooms</p>
        </div>
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-1 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg p-1">
            <button
              onClick={() => setView('grid')}
              className={cn(
                'p-1.5 rounded-md transition-colors',
                view === 'grid'
                  ? 'bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600'
                  : 'text-slate-500 hover:text-slate-700'
              )}
            >
              <LayoutGrid className="h-4 w-4" />
            </button>
            <button
              onClick={() => setView('list')}
              className={cn(
                'p-1.5 rounded-md transition-colors',
                view === 'list'
                  ? 'bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600'
                  : 'text-slate-500 hover:text-slate-700'
              )}
            >
              <List className="h-4 w-4" />
            </button>
          </div>
          <Button leftIcon={<Plus className="h-4 w-4" />} onClick={() => setIsAddOpen(true)}>
            Add Room
          </Button>
        </div>
      </div>

      {/* Status summary */}
      <div className="grid grid-cols-3 gap-3 mb-6">
        {Object.entries(statusCounts).map(([status, count]) => (
          <button
            key={status}
            onClick={() => setStatusFilter(statusFilter === status ? '' : status)}
            className={cn(
              'p-3 rounded-xl border text-left transition-all',
              statusFilter === status
                ? 'border-indigo-300 dark:border-indigo-600 bg-indigo-50 dark:bg-indigo-900/20 ring-2 ring-indigo-200 dark:ring-indigo-800'
                : 'bg-white dark:bg-slate-800 border-slate-100 dark:border-slate-700 hover:shadow-sm'
            )}
          >
            <div className="flex items-center gap-2 mb-1">
              <span className={cn('w-2.5 h-2.5 rounded-full', ROOM_STATUS_DOT[status])} />
              <span className="text-xs font-medium text-slate-500 dark:text-slate-400">{status}</span>
            </div>
            <span className="text-2xl font-bold text-slate-900 dark:text-slate-100">{count}</span>
          </button>
        ))}
      </div>

      {/* Filters */}
      <Card className="mb-6" padding="sm">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              type="text"
              placeholder="Search by room number or type..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full h-10 pl-10 pr-4 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-slate-900 dark:text-slate-100 placeholder:text-slate-400 transition-all"
            />
            {search && (
              <button
                onClick={() => setSearch('')}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
              >
                <X className="h-4 w-4" />
              </button>
            )}
          </div>
          <div className="sm:w-36">
            <Select
              options={FLOOR_OPTIONS}
              value={floorFilter}
              onChange={(e) => setFloorFilter(e.target.value)}
            />
          </div>
          <div className="sm:w-40">
            <Select
              options={TYPE_OPTIONS}
              value={typeFilter}
              onChange={(e) => setTypeFilter(e.target.value)}
            />
          </div>
          <div className="sm:w-40">
            <Select
              options={STATUS_OPTIONS}
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
            />
          </div>
        </div>
      </Card>

      {/* Results count */}
      <p className="text-sm text-slate-500 dark:text-slate-400 mb-4">
        Showing {filtered.length} of {rooms.length} rooms
      </p>

      {/* Grid / List view */}
      {view === 'grid' ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          {isLoading
            ? Array.from({ length: 8 }).map((_, i) => (
                <div
                  key={i}
                  className="h-56 bg-slate-200 dark:bg-slate-700 rounded-2xl animate-pulse"
                />
              ))
            : filtered.map((room) => (
                <RoomCard key={room.id} room={room} onStatusChange={handleStatusChange} />
              ))}
          {!isLoading && filtered.length === 0 && (
            <div className="col-span-full py-20 text-center text-slate-500 dark:text-slate-400">
              <BedDouble className="h-12 w-12 mx-auto mb-3 opacity-30" />
              <p className="font-medium">No rooms found</p>
              <p className="text-sm">Try adjusting your filters</p>
            </div>
          )}
        </div>
      ) : (
        <Card padding="none">
          <Table
            data={filtered}
            columns={columns}
            isLoading={isLoading}
            emptyMessage="No rooms found"
          />
        </Card>
      )}

      {/* Add Room Modal */}
      <Modal
        isOpen={isAddOpen}
        onClose={() => {
          setIsAddOpen(false);
          reset();
        }}
        title="Add New Room"
        size="lg"
        footer={
          <>
            <Button
              variant="outline"
              onClick={() => {
                setIsAddOpen(false);
                reset();
              }}
            >
              Cancel
            </Button>
            <Button isLoading={isSubmitting} onClick={handleSubmit(onAddRoom)}>
              Add Room
            </Button>
          </>
        }
      >
        <form className="space-y-4" onSubmit={handleSubmit(onAddRoom)}>
          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Room Number"
              required
              placeholder="e.g. 201"
              {...register('roomNumber', { required: 'Room number is required' })}
              error={errors.roomNumber?.message}
            />
            <Input
              label="Floor"
              type="number"
              required
              min={1}
              {...register('floor', { required: 'Floor is required', valueAsNumber: true })}
              error={errors.floor?.message}
            />
            <Select
              label="Type"
              required
              options={TYPE_OPTIONS.filter((o) => o.value)}
              {...register('type', { required: 'Type is required' })}
              error={errors.type?.message}
            />
            <Select
              label="Status"
              options={STATUS_OPTIONS.filter((o) => o.value)}
              {...register('status')}
            />
            <Input
              label="Price per Night ($)"
              type="number"
              required
              min={1}
              {...register('pricePerNight', {
                required: 'Price is required',
                valueAsNumber: true,
              })}
              error={errors.pricePerNight?.message}
            />
            <Input
              label="Capacity (guests)"
              type="number"
              required
              min={1}
              max={10}
              {...register('capacity', { required: 'Capacity is required', valueAsNumber: true })}
              error={errors.capacity?.message}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">
              Description
            </label>
            <textarea
              {...register('description')}
              rows={2}
              placeholder="Room description..."
              className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-3 py-2 text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
            />
          </div>
        </form>
      </Modal>
    </div>
  );
}

export default RoomsPage;

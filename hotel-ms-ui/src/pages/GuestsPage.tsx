import { useState, useEffect, useCallback } from 'react';
import { Search, Plus, User, Phone, Mail, Globe, X, Eye } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { guestService } from '../services/guest.service';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Card } from '../components/ui/Card';
import { Table } from '../components/ui/Table';
import { Modal } from '../components/ui/Modal';
import { useToast } from '../lib/store';
import type { Guest, CreateGuestRequest } from '../types';
import { formatDate, getInitials } from '../lib/utils';

const MOCK_GUESTS: Guest[] = [
  { id: '1', firstName: 'James', lastName: 'Morrison', email: 'james.morrison@example.com', phoneNumber: '+1 555-0101', country: 'United States', nationality: 'American', tenantId: 1, createdAt: new Date(Date.now() - 10 * 86400000).toISOString(), totalReservations: 3 },
  { id: '2', firstName: 'Sarah', lastName: 'Chen', email: 'sarah.chen@example.com', phoneNumber: '+1 555-0102', country: 'Canada', nationality: 'Canadian', tenantId: 1, createdAt: new Date(Date.now() - 20 * 86400000).toISOString(), totalReservations: 1 },
  { id: '3', firstName: 'Robert', lastName: 'Williams', email: 'r.williams@example.com', phoneNumber: '+44 20 7946 0101', country: 'United Kingdom', nationality: 'British', tenantId: 1, createdAt: new Date(Date.now() - 5 * 86400000).toISOString(), totalReservations: 5 },
  { id: '4', firstName: 'Emily', lastName: 'Davis', email: 'emily.davis@example.com', phoneNumber: '+61 2 8765 4321', country: 'Australia', nationality: 'Australian', tenantId: 1, createdAt: new Date(Date.now() - 45 * 86400000).toISOString(), totalReservations: 2 },
  { id: '5', firstName: 'Michael', lastName: 'Scott', email: 'm.scott@dundermifflin.com', phoneNumber: '+1 570-555-0199', country: 'United States', nationality: 'American', tenantId: 1, createdAt: new Date(Date.now() - 60 * 86400000).toISOString(), totalReservations: 7 },
  { id: '6', firstName: 'Anna', lastName: 'Johnson', email: 'anna.j@example.com', phoneNumber: '+49 30 12345678', country: 'Germany', nationality: 'German', tenantId: 1, createdAt: new Date(Date.now() - 15 * 86400000).toISOString(), totalReservations: 1 },
];

const createGuestSchema = z.object({
  firstName: z.string().min(1, 'First name is required').max(50),
  lastName: z.string().min(1, 'Last name is required').max(50),
  email: z.string().email('Invalid email address'),
  phoneNumber: z.string().optional(),
  address: z.string().optional(),
  city: z.string().optional(),
  country: z.string().optional(),
  nationality: z.string().optional(),
  idType: z.string().optional(),
  idNumber: z.string().optional(),
});

type CreateGuestFormData = z.infer<typeof createGuestSchema>;

export function GuestsPage() {
  const [guests, setGuests] = useState<Guest[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [viewGuest, setViewGuest] = useState<Guest | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const toast = useToast();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateGuestFormData>({
    resolver: zodResolver(createGuestSchema),
  });

  const fetchGuests = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await guestService.getAll();
      setGuests(data.length > 0 ? data : MOCK_GUESTS);
    } catch {
      setGuests(MOCK_GUESTS);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchGuests();
  }, [fetchGuests]);

  const filtered = guests.filter((g) => {
    if (!search) return true;
    const query = search.toLowerCase();
    return (
      g.firstName.toLowerCase().includes(query) ||
      g.lastName.toLowerCase().includes(query) ||
      g.email.toLowerCase().includes(query) ||
      (g.phoneNumber ?? '').toLowerCase().includes(query) ||
      (g.country ?? '').toLowerCase().includes(query)
    );
  });

  const onSubmit = async (data: CreateGuestFormData) => {
    setIsSubmitting(true);
    try {
      const payload: CreateGuestRequest = {
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        phoneNumber: data.phoneNumber,
        address: data.address,
        city: data.city,
        country: data.country,
        nationality: data.nationality,
        idType: data.idType,
        idNumber: data.idNumber,
      };
      await guestService.create(payload);
      toast.success('Guest created', `${data.firstName} ${data.lastName} has been added`);
      setIsCreateOpen(false);
      reset();
      await fetchGuests();
    } catch {
      toast.error('Failed to create guest', 'Please check the details and try again');
    } finally {
      setIsSubmitting(false);
    }
  };

  const columns = [
    {
      key: 'name',
      header: 'Guest',
      render: (g: Guest) => (
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-full bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center flex-shrink-0">
            <span className="text-sm font-semibold text-indigo-600 dark:text-indigo-400">
              {getInitials(g.firstName, g.lastName)}
            </span>
          </div>
          <div>
            <p className="font-medium text-slate-900 dark:text-slate-100">
              {g.firstName} {g.lastName}
            </p>
            <p className="text-xs text-slate-500 dark:text-slate-400">{g.email}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'phoneNumber',
      header: 'Phone',
      render: (g: Guest) => (
        <span className="text-slate-600 dark:text-slate-400">{g.phoneNumber ?? '—'}</span>
      ),
    },
    {
      key: 'country',
      header: 'Country',
      render: (g: Guest) => (
        <div className="flex items-center gap-1.5 text-slate-600 dark:text-slate-400">
          <Globe className="h-3.5 w-3.5" />
          {g.country ?? '—'}
        </div>
      ),
    },
    {
      key: 'totalReservations',
      header: 'Reservations',
      render: (g: Guest) => (
        <span className="inline-flex items-center justify-center w-7 h-7 rounded-full bg-indigo-100 dark:bg-indigo-900/30 text-xs font-semibold text-indigo-600 dark:text-indigo-400">
          {g.totalReservations ?? 0}
        </span>
      ),
    },
    {
      key: 'createdAt',
      header: 'Registered',
      render: (g: Guest) => formatDate(g.createdAt),
    },
    {
      key: 'actions',
      header: '',
      render: (g: Guest) => (
        <Button
          variant="ghost"
          size="sm"
          leftIcon={<Eye className="h-3.5 w-3.5" />}
          onClick={(e) => {
            e.stopPropagation();
            setViewGuest(g);
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
          <h2 className="page-title">Guests</h2>
          <p className="page-subtitle">{guests.length} registered guests</p>
        </div>
        <Button leftIcon={<Plus className="h-4 w-4" />} onClick={() => setIsCreateOpen(true)}>
          Add Guest
        </Button>
      </div>

      {/* Search */}
      <Card className="mb-6" padding="sm">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
          <input
            type="text"
            placeholder="Search guests by name, email, phone, or country..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full h-10 pl-10 pr-10 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-slate-900 dark:text-slate-100 placeholder:text-slate-400 transition-all"
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
      </Card>

      {/* Results */}
      {search && (
        <p className="text-sm text-slate-500 dark:text-slate-400 mb-4">
          {filtered.length} result{filtered.length !== 1 ? 's' : ''} for &quot;{search}&quot;
        </p>
      )}

      {/* Table */}
      <Card padding="none">
        <Table
          data={filtered}
          columns={columns}
          isLoading={isLoading}
          emptyMessage="No guests found"
          emptyDescription="Add your first guest to get started"
          onRowClick={(g) => setViewGuest(g as unknown as Guest)}
        />
      </Card>

      {/* Create Guest Modal */}
      <Modal
        isOpen={isCreateOpen}
        onClose={() => { setIsCreateOpen(false); reset(); }}
        title="Add New Guest"
        description="Enter the guest's personal information"
        size="lg"
        footer={
          <>
            <Button variant="outline" onClick={() => { setIsCreateOpen(false); reset(); }} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button isLoading={isSubmitting} onClick={handleSubmit(onSubmit)}>
              Add Guest
            </Button>
          </>
        }
      >
        <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Input label="First Name" required placeholder="John" {...register('firstName')} error={errors.firstName?.message} />
            <Input label="Last Name" required placeholder="Doe" {...register('lastName')} error={errors.lastName?.message} />
            <div className="sm:col-span-2">
              <Input label="Email Address" required type="email" placeholder="john@example.com" leftIcon={<Mail className="h-4 w-4" />} {...register('email')} error={errors.email?.message} />
            </div>
            <Input label="Phone Number" type="tel" placeholder="+1 555-0100" leftIcon={<Phone className="h-4 w-4" />} {...register('phoneNumber')} error={errors.phoneNumber?.message} />
            <Input label="Nationality" placeholder="e.g. American" {...register('nationality')} />
            <Input label="Country" placeholder="e.g. United States" leftIcon={<Globe className="h-4 w-4" />} {...register('country')} />
            <Input label="City" placeholder="e.g. New York" {...register('city')} />
            <Input label="Address" placeholder="Street address" {...register('address')} />
            <div className="sm:col-span-2 grid grid-cols-2 gap-4">
              <Input label="ID Type" placeholder="e.g. Passport, Driver's License" {...register('idType')} />
              <Input label="ID Number" placeholder="ID document number" {...register('idNumber')} />
            </div>
          </div>
        </form>
      </Modal>

      {/* Guest Profile Modal */}
      <Modal
        isOpen={!!viewGuest}
        onClose={() => setViewGuest(null)}
        title="Guest Profile"
        size="md"
      >
        {viewGuest && (
          <div className="space-y-5">
            {/* Avatar */}
            <div className="flex items-center gap-4">
              <div className="w-16 h-16 rounded-2xl bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center">
                <span className="text-2xl font-bold text-indigo-600 dark:text-indigo-400">
                  {getInitials(viewGuest.firstName, viewGuest.lastName)}
                </span>
              </div>
              <div>
                <h3 className="text-xl font-bold text-slate-900 dark:text-slate-100">
                  {viewGuest.firstName} {viewGuest.lastName}
                </h3>
                <p className="text-sm text-slate-500 dark:text-slate-400">
                  Guest since {formatDate(viewGuest.createdAt)}
                </p>
              </div>
            </div>

            <div className="space-y-3">
              {[
                { icon: <Mail className="h-4 w-4" />, label: 'Email', value: viewGuest.email },
                { icon: <Phone className="h-4 w-4" />, label: 'Phone', value: viewGuest.phoneNumber ?? '—' },
                { icon: <Globe className="h-4 w-4" />, label: 'Country', value: viewGuest.country ?? '—' },
                { icon: <User className="h-4 w-4" />, label: 'Nationality', value: viewGuest.nationality ?? '—' },
              ].map((item) => (
                <div key={item.label} className="flex items-center gap-3 p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                  <span className="text-slate-400 flex-shrink-0">{item.icon}</span>
                  <div>
                    <p className="text-xs text-slate-500 dark:text-slate-400">{item.label}</p>
                    <p className="text-sm font-medium text-slate-900 dark:text-slate-100">{item.value}</p>
                  </div>
                </div>
              ))}
            </div>

            <div className="flex items-center justify-between p-4 bg-indigo-50 dark:bg-indigo-900/20 rounded-xl border border-indigo-100 dark:border-indigo-800">
              <div>
                <p className="text-xs text-indigo-600 dark:text-indigo-400">Total Reservations</p>
                <p className="text-3xl font-bold text-indigo-700 dark:text-indigo-300">
                  {viewGuest.totalReservations ?? 0}
                </p>
              </div>
              <div className="w-12 h-12 rounded-xl bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center">
                <User className="h-6 w-6 text-indigo-600 dark:text-indigo-400" />
              </div>
            </div>

            {(viewGuest.idType || viewGuest.idNumber) && (
              <div className="p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">
                  {viewGuest.idType ?? 'ID'}
                </p>
                <p className="text-sm font-medium text-slate-900 dark:text-slate-100">
                  {viewGuest.idNumber ?? '—'}
                </p>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
}

export default GuestsPage;

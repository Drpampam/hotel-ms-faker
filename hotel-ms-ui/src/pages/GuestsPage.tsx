import { useState, useEffect, useCallback } from 'react';
import { useSlowConnection } from '../hooks/useSlowConnection';
import { Search, Plus, User, Phone, Mail, Globe, X, Eye, Edit, Save } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { guestService } from '../services/guest.service';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Card } from '../components/ui/Card';
import { Table } from '../components/ui/Table';
import { Modal } from '../components/ui/Modal';
import { Badge } from '../components/ui/Badge';
import { useToast, useAuthStore } from '../lib/store';
import type { Guest } from '../types';
import { formatDate, getInitials } from '../lib/utils';

const createGuestSchema = z.object({
  fullName: z.string().min(2, 'Full name must be at least 2 characters'),
  email: z.string().email('Enter a valid email address').optional().or(z.literal('')),
  phoneNumber: z.string().optional(),
  nationality: z.string().optional(),
  passportNumber: z.string().optional(),
  dateOfBirth: z.string().optional(),
  preferredRoomType: z.string().optional(),
  specialRequests: z.string().optional(),
});

type CreateGuestFormData = z.infer<typeof createGuestSchema>;

const editGuestSchema = z.object({
  nationality: z.string().optional(),
  passportNumber: z.string().optional(),
  dateOfBirth: z.string().optional(),
  preferredRoomType: z.string().optional(),
  specialRequests: z.string().optional(),
});

type EditGuestFormData = z.infer<typeof editGuestSchema>;

function guestDisplayName(g: Guest): string {
  if (g.fullName?.trim()) return g.fullName.trim();
  if (g.firstName || g.lastName) return `${g.firstName} ${g.lastName}`.trim();
  return g.email ?? `Guest #${g.id}`;
}

export function GuestsPage() {
  const [guests, setGuests] = useState<Guest[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [viewGuest, setViewGuest] = useState<Guest | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [isEditSubmitting, setIsEditSubmitting] = useState(false);
  const toast = useToast();
  const { tenantId } = useAuthStore();
  useSlowConnection(isLoading);

  const createForm = useForm<CreateGuestFormData>({ resolver: zodResolver(createGuestSchema) });
  const editForm = useForm<EditGuestFormData>({ resolver: zodResolver(editGuestSchema) });

  const fetchGuests = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await guestService.getAll({ tenantId: tenantId ?? undefined });
      setGuests(data);
    } catch (err) {
      toast.error('Failed to load guests', err instanceof Error ? err.message : 'Could not fetch guest profiles');
    } finally {
      setIsLoading(false);
    }
  }, [tenantId, toast]);

  useEffect(() => { fetchGuests(); }, [fetchGuests]);

  const openView = (g: Guest) => {
    setViewGuest(g);
    setIsEditing(false);
    editForm.reset({
      nationality: g.nationality ?? '',
      passportNumber: g.passportNumber ?? g.idNumber ?? '',
      dateOfBirth: g.dateOfBirth ? g.dateOfBirth.split('T')[0] : '',
      preferredRoomType: g.preferredRoomType ?? '',
      specialRequests: g.specialRequests ?? '',
    });
  };

  const filtered = guests.filter((g) => {
    if (!search) return true;
    const q = search.toLowerCase();
    return (
      guestDisplayName(g).toLowerCase().includes(q) ||
      (g.email ?? '').toLowerCase().includes(q) ||
      (g.phoneNumber ?? '').toLowerCase().includes(q) ||
      (g.nationality ?? '').toLowerCase().includes(q)
    );
  });

  const onCreateSubmit = createForm.handleSubmit(async (data) => {
    try {
      await guestService.create({
        fullName: data.fullName,
        email: data.email || undefined,
        phoneNumber: data.phoneNumber || undefined,
        nationality: data.nationality || undefined,
        passportNumber: data.passportNumber || undefined,
        dateOfBirth: data.dateOfBirth || undefined,
        preferredRoomType: data.preferredRoomType || undefined,
        specialRequests: data.specialRequests || undefined,
        tenantId: tenantId ?? undefined,
      });
      toast.success('Guest added', `${data.fullName} has been registered`);
      setIsCreateOpen(false);
      createForm.reset();
      await fetchGuests();
    } catch (err) {
      toast.error('Failed to add guest', err instanceof Error ? err.message : 'Could not create guest profile');
    }
  });

  const onEditSubmit = editForm.handleSubmit(async (data) => {
    if (!viewGuest) return;
    setIsEditSubmitting(true);
    try {
      const updated = await guestService.update({
        id: viewGuest.id,
        nationality: data.nationality || undefined,
        passportNumber: data.passportNumber || undefined,
        dateOfBirth: data.dateOfBirth || undefined,
        preferredRoomType: data.preferredRoomType || undefined,
        specialRequests: data.specialRequests || undefined,
      });
      setGuests((prev) => prev.map((g) => (g.id === viewGuest.id ? { ...g, ...updated } : g)));
      setViewGuest({ ...viewGuest, ...updated });
      setIsEditing(false);
      toast.success('Profile updated', 'Guest profile has been saved');
    } catch (err) {
      toast.error('Update failed', err instanceof Error ? err.message : 'Could not update guest profile');
    } finally {
      setIsEditSubmitting(false);
    }
  });

  const tierVariant = (tier?: string) => {
    if (tier === 'Gold') return 'warning' as const;
    if (tier === 'Platinum') return 'secondary' as const;
    return 'info' as const;
  };

  const columns = [
    {
      key: 'name',
      header: 'Guest',
      render: (g: Guest) => {
        const name = guestDisplayName(g);
        const parts = name.split(' ');
        return (
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 rounded-full bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center flex-shrink-0">
              <span className="text-sm font-semibold text-indigo-600 dark:text-indigo-400">
                {getInitials(parts[0] ?? '', parts.slice(1).join(' '))}
              </span>
            </div>
            <div>
              <p className="font-medium text-slate-900 dark:text-slate-100">{name}</p>
              <p className="text-xs text-slate-500 dark:text-slate-400">{g.email ?? '—'}</p>
            </div>
          </div>
        );
      },
    },
    {
      key: 'phoneNumber',
      header: 'Phone',
      render: (g: Guest) => (
        <span className="text-slate-600 dark:text-slate-400">{g.phoneNumber ?? '—'}</span>
      ),
    },
    {
      key: 'nationality',
      header: 'Nationality',
      render: (g: Guest) => (
        <div className="flex items-center gap-1.5 text-slate-600 dark:text-slate-400">
          <Globe className="h-3.5 w-3.5" />
          {g.nationality ?? '—'}
        </div>
      ),
    },
    {
      key: 'loyaltyTier',
      header: 'Loyalty',
      render: (g: Guest) => (
        <Badge variant={tierVariant(g.loyaltyTier)} dot>
          {g.loyaltyTier ?? 'Bronze'}
        </Badge>
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
          onClick={(e) => { e.stopPropagation(); openView(g); }}
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
          <p className="page-subtitle">{guests.length} registered guest{guests.length !== 1 ? 's' : ''}</p>
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
            placeholder="Search by name, email, phone, or nationality…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full h-10 pl-10 pr-10 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-slate-900 dark:text-slate-100 placeholder:text-slate-400 transition-all"
          />
          {search && (
            <button onClick={() => setSearch('')} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600">
              <X className="h-4 w-4" />
            </button>
          )}
        </div>
      </Card>

      {search && (
        <p className="text-sm text-slate-500 dark:text-slate-400 mb-4">
          {filtered.length} result{filtered.length !== 1 ? 's' : ''} for &quot;{search}&quot;
        </p>
      )}

      <Card padding="none">
        <Table
          data={filtered}
          columns={columns}
          isLoading={isLoading}
          emptyMessage="No guests found"
          emptyDescription='Click "Add Guest" to register your first guest'
          onRowClick={(g) => openView(g as unknown as Guest)}
        />
      </Card>

      {/* Create Guest Modal */}
      <Modal
        isOpen={isCreateOpen}
        onClose={() => { setIsCreateOpen(false); createForm.reset(); }}
        title="Add Guest"
        description="Register a walk-in guest or hotel customer"
        size="lg"
        footer={
          <>
            <Button variant="outline" onClick={() => { setIsCreateOpen(false); createForm.reset(); }} disabled={createForm.formState.isSubmitting}>Cancel</Button>
            <Button isLoading={createForm.formState.isSubmitting} leftIcon={<Plus className="h-4 w-4" />} onClick={onCreateSubmit}>Add Guest</Button>
          </>
        }
      >
        <form className="space-y-4" onSubmit={onCreateSubmit}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="sm:col-span-2">
              <Input
                label="Full Name"
                required
                placeholder="e.g. John Smith"
                leftIcon={<User className="h-4 w-4" />}
                {...createForm.register('fullName')}
                error={createForm.formState.errors.fullName?.message}
              />
            </div>
            <Input
              label="Email Address"
              type="email"
              placeholder="guest@example.com"
              leftIcon={<Mail className="h-4 w-4" />}
              {...createForm.register('email')}
              error={createForm.formState.errors.email?.message}
            />
            <Input
              label="Phone Number"
              type="tel"
              placeholder="+1 555-0100"
              leftIcon={<Phone className="h-4 w-4" />}
              {...createForm.register('phoneNumber')}
            />
          </div>

          <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide pt-1">Travel Details (optional)</p>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Input
              label="Nationality"
              placeholder="e.g. American"
              leftIcon={<Globe className="h-4 w-4" />}
              {...createForm.register('nationality')}
            />
            <Input
              label="Passport / ID Number"
              placeholder="Passport or ID number"
              {...createForm.register('passportNumber')}
            />
            <Input
              label="Date of Birth"
              type="date"
              {...createForm.register('dateOfBirth')}
            />
            <Input
              label="Preferred Room Type"
              placeholder="e.g. Deluxe, Suite"
              {...createForm.register('preferredRoomType')}
            />
            <div className="sm:col-span-2">
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">Special Requests</label>
              <textarea
                {...createForm.register('specialRequests')}
                rows={2}
                placeholder="Any standing preferences…"
                className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-3 py-2 text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
              />
            </div>
          </div>
        </form>
      </Modal>

      {/* View / Edit Guest Modal */}
      <Modal
        isOpen={!!viewGuest}
        onClose={() => { setViewGuest(null); setIsEditing(false); }}
        title={isEditing ? 'Edit Guest Profile' : 'Guest Profile'}
        size="md"
        footer={
          isEditing ? (
            <>
              <Button variant="outline" onClick={() => setIsEditing(false)} disabled={isEditSubmitting}>Cancel</Button>
              <Button isLoading={isEditSubmitting} leftIcon={<Save className="h-4 w-4" />} onClick={onEditSubmit}>Save Changes</Button>
            </>
          ) : (
            <>
              <Button variant="outline" onClick={() => setViewGuest(null)}>Close</Button>
              <Button leftIcon={<Edit className="h-4 w-4" />} onClick={() => setIsEditing(true)}>Edit Profile</Button>
            </>
          )
        }
      >
        {viewGuest && !isEditing && (
          <div className="space-y-5">
            <div className="flex items-center gap-4">
              <div className="w-16 h-16 rounded-2xl bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center">
                <span className="text-2xl font-bold text-indigo-600 dark:text-indigo-400">
                  {(() => { const n = guestDisplayName(viewGuest).split(' '); return getInitials(n[0] ?? '', n.slice(1).join(' ')); })()}
                </span>
              </div>
              <div>
                <h3 className="text-xl font-bold text-slate-900 dark:text-slate-100">{guestDisplayName(viewGuest)}</h3>
                <p className="text-sm text-slate-500 dark:text-slate-400">Guest since {formatDate(viewGuest.createdAt)}</p>
                <Badge variant={tierVariant(viewGuest.loyaltyTier)} dot className="mt-1">
                  {viewGuest.loyaltyTier ?? 'Bronze'} — {viewGuest.loyaltyPoints} pts
                </Badge>
              </div>
            </div>

            <div className="space-y-3">
              {[
                { icon: <Mail className="h-4 w-4" />, label: 'Email', value: viewGuest.email ?? '—' },
                { icon: <Phone className="h-4 w-4" />, label: 'Phone', value: viewGuest.phoneNumber ?? '—' },
                { icon: <Globe className="h-4 w-4" />, label: 'Nationality', value: viewGuest.nationality ?? '—' },
                { icon: <User className="h-4 w-4" />, label: 'Passport / ID', value: viewGuest.passportNumber ?? viewGuest.idNumber ?? '—' },
                { icon: <User className="h-4 w-4" />, label: 'Preferred Room', value: viewGuest.preferredRoomType ?? '—' },
                { icon: <User className="h-4 w-4" />, label: 'Date of Birth', value: viewGuest.dateOfBirth ? formatDate(viewGuest.dateOfBirth) : '—' },
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

            {viewGuest.specialRequests && (
              <div className="p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl">
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">Special Requests</p>
                <p className="text-sm text-slate-800 dark:text-slate-200">{viewGuest.specialRequests}</p>
              </div>
            )}
          </div>
        )}

        {viewGuest && isEditing && (
          <form className="space-y-4" onSubmit={onEditSubmit}>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <Input label="Nationality" placeholder="e.g. American" leftIcon={<Globe className="h-4 w-4" />} {...editForm.register('nationality')} error={editForm.formState.errors.nationality?.message} />
              <Input label="Passport / ID Number" placeholder="Passport or ID number" {...editForm.register('passportNumber')} error={editForm.formState.errors.passportNumber?.message} />
              <Input label="Date of Birth" type="date" {...editForm.register('dateOfBirth')} error={editForm.formState.errors.dateOfBirth?.message} />
              <Input label="Preferred Room Type" placeholder="e.g. Deluxe, Suite" {...editForm.register('preferredRoomType')} error={editForm.formState.errors.preferredRoomType?.message} />
              <div className="sm:col-span-2">
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">Special Requests</label>
                <textarea
                  {...editForm.register('specialRequests')}
                  rows={3}
                  placeholder="Any standing preferences…"
                  className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-3 py-2 text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
                />
              </div>
            </div>
          </form>
        )}
      </Modal>
    </div>
  );
}

export default GuestsPage;

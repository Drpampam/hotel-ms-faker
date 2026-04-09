import { useState, useEffect, useCallback } from 'react';
import {
  Search, Plus, UserCog, Shield, X, Eye, EyeOff, Save, Edit2,
  Trash2, Lock, Clock, AlertTriangle, CheckCircle, XCircle, Users,
} from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { userService } from '../services/user.service';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Badge } from '../components/ui/Badge';
import { Card } from '../components/ui/Card';
import { Table } from '../components/ui/Table';
import { Modal } from '../components/ui/Modal';
import { useToast } from '../lib/store';
import type { User, CreateUserRequest } from '../types';
import { formatDate, getInitials, ROLE_COLORS } from '../lib/utils';

const ALL_ROLES = ['Admin', 'SuperAdmin', 'FrontDesk', 'Housekeeping', 'Developer'];
const SHIFT_OPTIONS = ['Morning', 'Afternoon', 'Night'];
const DEPARTMENT_OPTIONS = [
  'Front Desk', 'Housekeeping', 'Management',
  'Food & Beverage', 'Security', 'Maintenance',
];

// ── Schemas ───────────────────────────────────────────────────────────────────

const createUserSchema = z.object({
  email: z.string().email('Invalid email'),
  password: z.string().min(6, 'Minimum 6 characters'),
  fullName: z.string().min(2, 'Required'),
  phoneNumber: z.string().min(1, 'Required'),
  role: z.enum(['Admin', 'SuperAdmin', 'FrontDesk', 'Housekeeping', 'Developer'] as const),
});

const editUserSchema = z.object({
  fullName: z.string().min(2, 'Required'),
  roles: z.array(z.string()).min(1, 'Select at least one role'),
});

const changePasswordSchema = z
  .object({
    newPassword: z.string().min(6, 'Minimum 6 characters'),
    confirmPassword: z.string().min(1, 'Required'),
  })
  .refine((d) => d.newPassword === d.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

const changeShiftSchema = z.object({
  shift: z.string().optional(),
  department: z.string().optional(),
});

type CreateUserForm = z.infer<typeof createUserSchema>;
type EditUserForm = z.infer<typeof editUserSchema>;
type ChangePasswordForm = z.infer<typeof changePasswordSchema>;
type ChangeShiftForm = z.infer<typeof changeShiftSchema>;
type ManageTab = 'details' | 'password' | 'shift' | 'danger';

// ── Small helpers ─────────────────────────────────────────────────────────────

function Avatar({ user, size = 'md' }: { user: User; size?: 'sm' | 'md' | 'lg' }) {
  const dim = size === 'sm' ? 'w-8 h-8 text-xs' : size === 'lg' ? 'w-14 h-14 text-xl' : 'w-10 h-10 text-sm';
  return (
    <div className={`${dim} rounded-full bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center flex-shrink-0 font-semibold text-indigo-600 dark:text-indigo-400 overflow-hidden`}>
      {user.picture
        ? <img src={user.picture} alt="" className="w-full h-full object-cover" />
        : getInitials(user.firstName, user.lastName)}
    </div>
  );
}

function RoleTag({ role }: { role: string }) {
  const color = (ROLE_COLORS as Record<string, string>)[role] ?? 'default';
  return <Badge variant={color as 'default'}>{role}</Badge>;
}

function FieldRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="py-3 border-b border-slate-100 dark:border-slate-700 last:border-0 grid grid-cols-5 gap-2">
      <span className="col-span-2 text-sm text-slate-500 dark:text-slate-400">{label}</span>
      <span className="col-span-3 text-sm font-medium text-slate-900 dark:text-slate-100">{value}</span>
    </div>
  );
}

function NativeSelect({
  label,
  options,
  ...props
}: { label: string; options: string[] } & React.SelectHTMLAttributes<HTMLSelectElement>) {
  return (
    <div>
      <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">{label}</label>
      <select
        {...props}
        className="w-full h-10 px-3 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 transition-all"
      >
        <option value="">None</option>
        {options.map((o) => <option key={o} value={o}>{o}</option>)}
      </select>
    </div>
  );
}

// ── Main Component ────────────────────────────────────────────────────────────

export default function UsersPage() {
  const toast = useToast();

  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');

  const [showCreate, setShowCreate] = useState(false);
  const [creating, setCreating] = useState(false);

  const [managingUser, setManagingUser] = useState<User | null>(null);
  const [manageTab, setManageTab] = useState<ManageTab>('details');
  const [editMode, setEditMode] = useState(false);
  const [showPw, setShowPw] = useState(false);
  const [showConfirmPw, setShowConfirmPw] = useState(false);
  const [deleteEmail, setDeleteEmail] = useState('');
  const [busy, setBusy] = useState(false);

  // ── Data ──────────────────────────────────────────────────────────────────

  const loadUsers = useCallback(async () => {
    setLoading(true);
    try {
      setUsers(await userService.getAll({ pageSize: 200 }));
    } catch (err) {
      toast.error('Failed to load users', err instanceof Error ? err.message : undefined);
    } finally {
      setLoading(false);
    }
  }, [toast]);

  useEffect(() => { loadUsers(); }, [loadUsers]);

  // ── Forms ─────────────────────────────────────────────────────────────────

  const createForm = useForm<CreateUserForm>({ resolver: zodResolver(createUserSchema) });
  const editForm = useForm<EditUserForm>({ resolver: zodResolver(editUserSchema) });
  const pwForm = useForm<ChangePasswordForm>({ resolver: zodResolver(changePasswordSchema) });
  const shiftForm = useForm<ChangeShiftForm>({ resolver: zodResolver(changeShiftSchema) });

  // ── Create ────────────────────────────────────────────────────────────────

  const handleCreate = async (data: CreateUserForm) => {
    setCreating(true);
    try {
      await userService.create(data as CreateUserRequest);
      toast.success('User created', `${data.fullName} has been added`);
      setShowCreate(false);
      createForm.reset();
      loadUsers();
    } catch (err) {
      toast.error('Failed to create user', err instanceof Error ? err.message : undefined);
    } finally {
      setCreating(false);
    }
  };

  // ── Open / close manage ───────────────────────────────────────────────────

  const openManage = (user: User) => {
    setManagingUser(user);
    setManageTab('details');
    setEditMode(false);
    setDeleteEmail('');
    editForm.reset({
      fullName: user.fullName ?? `${user.firstName} ${user.lastName}`.trim(),
      roles: user.userRoles?.map((r) => r.name) ?? [user.role],
    });
    shiftForm.reset({ shift: user.shift ?? '', department: user.department ?? '' });
    pwForm.reset();
  };

  const closeManage = () => {
    setManagingUser(null);
    setEditMode(false);
    setDeleteEmail('');
  };

  // ── Actions ───────────────────────────────────────────────────────────────

  const handleEditSave = async (data: EditUserForm) => {
    if (!managingUser) return;
    setBusy(true);
    try {
      await userService.update({ email: managingUser.email, fullName: data.fullName, roles: data.roles });
      toast.success('User updated', 'Profile changes have been saved');
      setEditMode(false);
      setManagingUser((prev) => prev ? { ...prev, fullName: data.fullName } : prev);
      loadUsers();
    } catch (err) {
      toast.error('Update failed', err instanceof Error ? err.message : undefined);
    } finally {
      setBusy(false);
    }
  };

  const handleToggleActive = async () => {
    if (!managingUser) return;
    setBusy(true);
    try {
      if (managingUser.isActive) {
        await userService.deactivate(managingUser.email);
        toast.success('User deactivated', `${managingUser.fullName ?? managingUser.email} can no longer log in`);
      } else {
        await userService.activate(managingUser.email);
        toast.success('User activated', `${managingUser.fullName ?? managingUser.email} can now log in`);
      }
      await loadUsers();
      closeManage();
    } catch (err) {
      toast.error('Status change failed', err instanceof Error ? err.message : undefined);
    } finally {
      setBusy(false);
    }
  };

  const handleChangePassword = async (data: ChangePasswordForm) => {
    if (!managingUser) return;
    setBusy(true);
    try {
      await userService.adminChangePassword(managingUser.email, data.newPassword);
      toast.success('Password changed', 'The user can now log in with the new password');
      pwForm.reset();
    } catch (err) {
      toast.error('Password change failed', err instanceof Error ? err.message : undefined);
    } finally {
      setBusy(false);
    }
  };

  const handleChangeShift = async (data: ChangeShiftForm) => {
    if (!managingUser) return;
    setBusy(true);
    try {
      await userService.changeShift(managingUser.email, data.shift ?? null, data.department ?? null);
      toast.success('Shift updated', 'Staff assignment has been saved');
      setManagingUser((prev) =>
        prev ? { ...prev, shift: data.shift ?? undefined, department: data.department ?? undefined } : prev
      );
      loadUsers();
    } catch (err) {
      toast.error('Shift update failed', err instanceof Error ? err.message : undefined);
    } finally {
      setBusy(false);
    }
  };

  const handleDelete = async () => {
    if (!managingUser || deleteEmail !== managingUser.email) return;
    setBusy(true);
    try {
      await userService.deleteUser(managingUser.email);
      toast.success('User deleted', 'The account has been removed from the system');
      closeManage();
      loadUsers();
    } catch (err) {
      toast.error('Delete failed', err instanceof Error ? err.message : undefined);
    } finally {
      setBusy(false);
    }
  };

  // ── Filter ────────────────────────────────────────────────────────────────

  const filtered = users.filter((u) => {
    const name = (u.fullName ?? `${u.firstName} ${u.lastName}`).toLowerCase();
    const q = search.toLowerCase();
    const matchSearch = !q || name.includes(q) || u.email.toLowerCase().includes(q);
    const matchRole =
      !roleFilter || u.role === roleFilter || u.userRoles?.some((r) => r.name === roleFilter);
    return matchSearch && matchRole;
  });

  const activeCount = users.filter((u) => u.isActive).length;
  const inactiveCount = users.length - activeCount;

  // ── Columns ───────────────────────────────────────────────────────────────

  const columns = [
    {
      key: 'name',
      header: 'User',
      render: (u: User) => (
        <div className="flex items-center gap-3">
          <Avatar user={u} size="sm" />
          <div className="min-w-0">
            <p className="font-medium text-slate-900 dark:text-slate-100 truncate">
              {u.fullName ?? `${u.firstName} ${u.lastName}`}
            </p>
            <p className="text-xs text-slate-500 dark:text-slate-400 truncate">{u.email}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'role',
      header: 'Role',
      render: (u: User) => (
        <div className="flex flex-wrap gap-1">
          {(u.userRoles?.length ?? 0) > 0
            ? u.userRoles!.map((r) => <RoleTag key={r.id} role={r.name} />)
            : <RoleTag role={u.role} />}
        </div>
      ),
    },
    {
      key: 'shift',
      header: 'Shift',
      render: (u: User) =>
        u.shift ? (
          <div className="text-sm">
            <span className="font-medium text-slate-800 dark:text-slate-200">{u.shift}</span>
            {u.department && (
              <span className="text-slate-500 dark:text-slate-400 ml-1 text-xs">· {u.department}</span>
            )}
          </div>
        ) : (
          <span className="text-slate-400 dark:text-slate-600 text-sm">—</span>
        ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (u: User) => (
        <Badge variant={u.isActive ? 'success' : 'danger'} dot>
          {u.isActive ? 'Active' : 'Inactive'}
        </Badge>
      ),
    },
    {
      key: 'createdAt',
      header: 'Joined',
      render: (u: User) => (
        <span className="text-sm text-slate-500 dark:text-slate-400">{formatDate(u.createdAt)}</span>
      ),
    },
    {
      key: 'actions',
      header: '',
      render: (u: User) => (
        <Button
          variant="ghost"
          size="sm"
          leftIcon={<UserCog className="h-3.5 w-3.5" />}
          onClick={(e) => { e.stopPropagation(); openManage(u); }}
        >
          Manage
        </Button>
      ),
    },
  ];

  // ── JSX ───────────────────────────────────────────────────────────────────

  return (
    <div className="page-container">
      {/* Header */}
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Users</h2>
          <p className="page-subtitle">{users.length} staff member{users.length !== 1 ? 's' : ''}</p>
        </div>
        <Button leftIcon={<Plus className="h-4 w-4" />} onClick={() => setShowCreate(true)}>
          Add User
        </Button>
      </div>

      {/* Stat cards */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-6">
        {[
          { label: 'Total', value: users.length, Icon: Users, color: 'text-indigo-600 dark:text-indigo-400', bg: 'bg-indigo-50 dark:bg-indigo-900/20' },
          { label: 'Active', value: activeCount, Icon: CheckCircle, color: 'text-emerald-600 dark:text-emerald-400', bg: 'bg-emerald-50 dark:bg-emerald-900/20' },
          { label: 'Inactive', value: inactiveCount, Icon: XCircle, color: 'text-rose-600 dark:text-rose-400', bg: 'bg-rose-50 dark:bg-rose-900/20' },
          { label: 'Roles', value: ALL_ROLES.length, Icon: Shield, color: 'text-violet-600 dark:text-violet-400', bg: 'bg-violet-50 dark:bg-violet-900/20' },
        ].map(({ label, value, Icon, color, bg }) => (
          <Card key={label} padding="sm">
            <div className="flex items-center gap-3">
              <div className={`w-10 h-10 rounded-lg ${bg} flex items-center justify-center flex-shrink-0`}>
                <Icon className={`w-5 h-5 ${color}`} />
              </div>
              <div>
                <p className="text-2xl font-bold text-slate-900 dark:text-slate-100 leading-none">{value}</p>
                <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">{label}</p>
              </div>
            </div>
          </Card>
        ))}
      </div>

      {/* Search + filter */}
      <Card padding="sm" className="mb-6">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search by name or email…"
              className="w-full h-10 pl-10 pr-10 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-slate-900 dark:text-slate-100 placeholder:text-slate-400 transition-all"
            />
            {search && (
              <button onClick={() => setSearch('')} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600">
                <X className="h-4 w-4" />
              </button>
            )}
          </div>
          <select
            value={roleFilter}
            onChange={(e) => setRoleFilter(e.target.value)}
            className="h-10 px-3 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 sm:w-44 transition-all"
          >
            <option value="">All Roles</option>
            {ALL_ROLES.map((r) => <option key={r} value={r}>{r}</option>)}
          </select>
        </div>
        {search && (
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-2">
            {filtered.length} result{filtered.length !== 1 ? 's' : ''} for &quot;{search}&quot;
          </p>
        )}
      </Card>

      {/* Table */}
      <Card padding="none">
        <Table
          data={filtered}
          columns={columns}
          isLoading={loading}
          emptyMessage="No users found"
          emptyDescription='Click "Add User" to create the first staff account'
          onRowClick={(u) => openManage(u as unknown as User)}
        />
      </Card>

      {/* ─── Create User Modal ─────────────────────────────────────────────── */}
      <Modal
        isOpen={showCreate}
        onClose={() => { setShowCreate(false); createForm.reset(); }}
        title="Add New User"
        description="Create a staff account with login access"
        size="md"
        footer={
          <>
            <Button variant="outline" onClick={() => { setShowCreate(false); createForm.reset(); }}>
              Cancel
            </Button>
            <Button
              leftIcon={<Plus className="h-4 w-4" />}
              isLoading={creating}
              onClick={createForm.handleSubmit(handleCreate)}
            >
              Create User
            </Button>
          </>
        }
      >
        <form onSubmit={createForm.handleSubmit(handleCreate)} className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Input
              label="Full Name"
              placeholder="Jane Doe"
              {...createForm.register('fullName')}
              error={createForm.formState.errors.fullName?.message}
            />
            <Input
              label="Email"
              type="email"
              placeholder="jane@hotel.com"
              {...createForm.register('email')}
              error={createForm.formState.errors.email?.message}
            />
            <Input
              label="Password"
              type="password"
              placeholder="Min. 6 characters"
              {...createForm.register('password')}
              error={createForm.formState.errors.password?.message}
            />
            <Input
              label="Phone Number"
              placeholder="+1 555 000 0000"
              {...createForm.register('phoneNumber')}
              error={createForm.formState.errors.phoneNumber?.message}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">Role</label>
            <select
              {...createForm.register('role')}
              className="w-full h-10 px-3 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 transition-all"
            >
              <option value="">Select role…</option>
              {ALL_ROLES.map((r) => <option key={r} value={r}>{r}</option>)}
            </select>
            {createForm.formState.errors.role && (
              <p className="mt-1 text-xs text-rose-500">{createForm.formState.errors.role.message}</p>
            )}
          </div>
        </form>
      </Modal>

      {/* ─── Manage User Modal ─────────────────────────────────────────────── */}
      {managingUser && (
        <Modal
          isOpen
          onClose={closeManage}
          title="Manage User"
          size="lg"
        >
          {/* Profile strip */}
          <div className="flex items-center gap-4 p-4 mb-2 rounded-xl bg-slate-50 dark:bg-slate-800/60">
            <Avatar user={managingUser} size="lg" />
            <div className="flex-1 min-w-0">
              <p className="font-semibold text-slate-900 dark:text-slate-100 text-base truncate">
                {managingUser.fullName ?? `${managingUser.firstName} ${managingUser.lastName}`}
              </p>
              <p className="text-sm text-slate-500 dark:text-slate-400 truncate">{managingUser.email}</p>
              <div className="flex flex-wrap gap-1 mt-1.5">
                <Badge variant={managingUser.isActive ? 'success' : 'danger'} dot>
                  {managingUser.isActive ? 'Active' : 'Inactive'}
                </Badge>
                {managingUser.shift && (
                  <Badge variant="info">{managingUser.shift}</Badge>
                )}
              </div>
            </div>
          </div>

          {/* Tabs */}
          <div className="flex gap-0.5 border-b border-slate-200 dark:border-slate-700 mb-5 mt-4">
            {(
              [
                { id: 'details', label: 'Details', Icon: Edit2 },
                { id: 'password', label: 'Password', Icon: Lock },
                { id: 'shift', label: 'Shift', Icon: Clock },
                { id: 'danger', label: 'Danger', Icon: AlertTriangle },
              ] as { id: ManageTab; label: string; Icon: React.ElementType }[]
            ).map(({ id, label, Icon }) => (
              <button
                key={id}
                onClick={() => { setManageTab(id); setEditMode(false); }}
                className={`flex items-center gap-1.5 px-4 py-2.5 text-sm font-medium border-b-2 -mb-px transition-colors whitespace-nowrap ${
                  manageTab === id
                    ? id === 'danger'
                      ? 'border-rose-500 text-rose-600 dark:text-rose-400'
                      : 'border-indigo-500 text-indigo-600 dark:text-indigo-400'
                    : 'border-transparent text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-300'
                }`}
              >
                <Icon className="h-3.5 w-3.5" />
                {label}
              </button>
            ))}
          </div>

          {/* ── Details tab ── */}
          {manageTab === 'details' && (
            <div>
              {!editMode ? (
                <>
                  <div className="rounded-lg border border-slate-100 dark:border-slate-700 px-4 mb-4">
                    <FieldRow label="Full name" value={managingUser.fullName ?? `${managingUser.firstName} ${managingUser.lastName}`} />
                    <FieldRow label="Email" value={managingUser.email} />
                    <FieldRow label="Phone" value={managingUser.phoneNumber ?? '—'} />
                    <FieldRow
                      label="Roles"
                      value={
                        <div className="flex flex-wrap gap-1">
                          {(managingUser.userRoles?.length ?? 0) > 0
                            ? managingUser.userRoles!.map((r) => <RoleTag key={r.id} role={r.name} />)
                            : <RoleTag role={managingUser.role} />}
                        </div>
                      }
                    />
                    <FieldRow label="Joined" value={formatDate(managingUser.createdAt)} />
                    <FieldRow
                      label="Last active"
                      value={managingUser.lastActiveDate ? formatDate(managingUser.lastActiveDate) : '—'}
                    />
                  </div>
                  <div className="flex gap-3">
                    <Button
                      variant="outline"
                      leftIcon={<Edit2 className="h-4 w-4" />}
                      onClick={() => setEditMode(true)}
                    >
                      Edit
                    </Button>
                    <Button
                      variant={managingUser.isActive ? 'danger' : 'success'}
                      leftIcon={managingUser.isActive
                        ? <XCircle className="h-4 w-4" />
                        : <CheckCircle className="h-4 w-4" />}
                      isLoading={busy}
                      onClick={handleToggleActive}
                    >
                      {managingUser.isActive ? 'Deactivate' : 'Activate'}
                    </Button>
                  </div>
                </>
              ) : (
                <form onSubmit={editForm.handleSubmit(handleEditSave)} className="space-y-4">
                  <Input
                    label="Full Name"
                    {...editForm.register('fullName')}
                    error={editForm.formState.errors.fullName?.message}
                  />
                  <div>
                    <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                      Roles
                    </label>
                    <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
                      {ALL_ROLES.map((role) => {
                        const current = editForm.watch('roles') ?? [];
                        return (
                          <label key={role} className="flex items-center gap-2 cursor-pointer p-2.5 rounded-lg border border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors">
                            <input
                              type="checkbox"
                              checked={current.includes(role)}
                              onChange={(e) => {
                                const next = e.target.checked
                                  ? [...current, role]
                                  : current.filter((r) => r !== role);
                                editForm.setValue('roles', next, { shouldValidate: true });
                              }}
                              className="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
                            />
                            <span className="text-sm text-slate-700 dark:text-slate-300">{role}</span>
                          </label>
                        );
                      })}
                    </div>
                    {editForm.formState.errors.roles && (
                      <p className="mt-1 text-xs text-rose-500">{editForm.formState.errors.roles.message}</p>
                    )}
                  </div>
                  <div className="flex gap-3 pt-1">
                    <Button variant="outline" leftIcon={<X className="h-4 w-4" />} onClick={() => setEditMode(false)}>
                      Cancel
                    </Button>
                    <Button leftIcon={<Save className="h-4 w-4" />} type="submit" isLoading={busy}>
                      Save Changes
                    </Button>
                  </div>
                </form>
              )}
            </div>
          )}

          {/* ── Password tab ── */}
          {manageTab === 'password' && (
            <form onSubmit={pwForm.handleSubmit(handleChangePassword)} className="space-y-4">
              <p className="text-sm text-slate-500 dark:text-slate-400">
                Set a new password for{' '}
                <span className="font-medium text-slate-700 dark:text-slate-300">{managingUser.email}</span>.
                The user can log in immediately with the new password.
              </p>
              <div className="relative">
                <Input
                  label="New Password"
                  type={showPw ? 'text' : 'password'}
                  placeholder="Min. 6 characters"
                  {...pwForm.register('newPassword')}
                  error={pwForm.formState.errors.newPassword?.message}
                />
                <button
                  type="button"
                  tabIndex={-1}
                  className="absolute right-3 top-8 text-slate-400 hover:text-slate-600"
                  onClick={() => setShowPw((v) => !v)}
                >
                  {showPw ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
              <div className="relative">
                <Input
                  label="Confirm Password"
                  type={showConfirmPw ? 'text' : 'password'}
                  placeholder="Repeat password"
                  {...pwForm.register('confirmPassword')}
                  error={pwForm.formState.errors.confirmPassword?.message}
                />
                <button
                  type="button"
                  tabIndex={-1}
                  className="absolute right-3 top-8 text-slate-400 hover:text-slate-600"
                  onClick={() => setShowConfirmPw((v) => !v)}
                >
                  {showConfirmPw ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
              <Button
                type="submit"
                leftIcon={<Lock className="h-4 w-4" />}
                isLoading={busy}
                className="w-full"
              >
                Change Password
              </Button>
            </form>
          )}

          {/* ── Shift tab ── */}
          {manageTab === 'shift' && (
            <form onSubmit={shiftForm.handleSubmit(handleChangeShift)} className="space-y-4">
              {managingUser.shift || managingUser.department ? (
                <div className="flex items-center gap-2 p-3 rounded-lg bg-indigo-50 dark:bg-indigo-900/20 border border-indigo-100 dark:border-indigo-800 text-sm">
                  <Clock className="h-4 w-4 text-indigo-500 flex-shrink-0" />
                  <span className="text-indigo-700 dark:text-indigo-300">
                    Currently: <strong>{managingUser.shift ?? 'No shift'}</strong>
                    {managingUser.department && <> · {managingUser.department}</>}
                  </span>
                </div>
              ) : (
                <p className="text-sm text-slate-500 dark:text-slate-400">
                  No shift assigned yet. Use the dropdowns below to assign one.
                </p>
              )}
              <NativeSelect
                label="Shift"
                options={SHIFT_OPTIONS}
                {...shiftForm.register('shift')}
              />
              <NativeSelect
                label="Department"
                options={DEPARTMENT_OPTIONS}
                {...shiftForm.register('department')}
              />
              <Button
                type="submit"
                leftIcon={<Clock className="h-4 w-4" />}
                isLoading={busy}
                className="w-full"
              >
                Update Shift
              </Button>
            </form>
          )}

          {/* ── Danger tab ── */}
          {manageTab === 'danger' && (
            <div className="space-y-4">
              <div className="flex gap-3 p-4 rounded-xl bg-rose-50 dark:bg-rose-900/20 border border-rose-200 dark:border-rose-800">
                <AlertTriangle className="h-5 w-5 text-rose-600 dark:text-rose-400 flex-shrink-0 mt-0.5" />
                <div className="text-sm">
                  <p className="font-semibold text-rose-800 dark:text-rose-300">Delete User Account</p>
                  <p className="text-rose-700 dark:text-rose-400 mt-1">
                    This permanently removes the account. Historical records (reservations, payments,
                    audit logs) are preserved for compliance.
                  </p>
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">
                  Type <code className="px-1 py-0.5 rounded bg-slate-100 dark:bg-slate-800 text-xs">{managingUser.email}</code> to confirm
                </label>
                <input
                  value={deleteEmail}
                  onChange={(e) => setDeleteEmail(e.target.value)}
                  placeholder={managingUser.email}
                  className="w-full h-10 px-3 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-slate-100 text-sm focus:outline-none focus:ring-2 focus:ring-rose-500 transition-all"
                />
              </div>
              <Button
                variant="danger"
                leftIcon={<Trash2 className="h-4 w-4" />}
                isLoading={busy}
                disabled={deleteEmail !== managingUser.email}
                onClick={handleDelete}
                className="w-full"
              >
                Delete User Permanently
              </Button>
            </div>
          )}
        </Modal>
      )}
    </div>
  );
}

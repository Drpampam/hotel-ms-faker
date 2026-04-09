import { useState, useEffect, useCallback } from 'react';
import {
  Search, Plus, UserCog, Shield, X, Eye, EyeOff, Save, Edit,
  Trash2, Lock, Clock, AlertTriangle, CheckCircle, XCircle,
} from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { userService } from '../services/user.service';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Select } from '../components/ui/Select';
import { Badge } from '../components/ui/Badge';
import { Card } from '../components/ui/Card';
import { Table } from '../components/ui/Table';
import { Modal } from '../components/ui/Modal';
import { useToast } from '../lib/store';
import type { User, CreateUserRequest } from '../types';
import { formatDate, getInitials, ROLE_COLORS } from '../lib/utils';

const ALL_ROLES = ['Admin', 'SuperAdmin', 'FrontDesk', 'Housekeeping', 'Developer'];
const SHIFT_OPTIONS = ['Morning', 'Afternoon', 'Night'];
const DEPARTMENT_OPTIONS = ['Front Desk', 'Housekeeping', 'Management', 'Food & Beverage', 'Security', 'Maintenance'];
const ROLE_OPTIONS = [{ value: '', label: 'All Roles' }, ...ALL_ROLES.map((r) => ({ value: r, label: r }))];

// ── Schemas ──────────────────────────────────────────────────────────────────

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

const changePasswordSchema = z.object({
  newPassword: z.string().min(6, 'Minimum 6 characters'),
  confirmPassword: z.string().min(6, 'Required'),
}).refine((d) => d.newPassword === d.confirmPassword, {
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

// ── Helpers ───────────────────────────────────────────────────────────────────

function RoleTag({ role }: { role: string }) {
  const color = (ROLE_COLORS as Record<string, string>)[role] ?? 'default';
  return <Badge variant={color as 'default'}>{role}</Badge>;
}

// ── Main Component ────────────────────────────────────────────────────────────

export default function UsersPage() {
  const toast = useToast();

  // List state
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');

  // Create modal
  const [showCreate, setShowCreate] = useState(false);
  const [creating, setCreating] = useState(false);

  // Manage modal
  const [managingUser, setManagingUser] = useState<User | null>(null);
  const [manageTab, setManageTab] = useState<ManageTab>('details');
  const [editMode, setEditMode] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [deleteConfirmEmail, setDeleteConfirmEmail] = useState('');
  const [actionLoading, setActionLoading] = useState(false);

  // ── Data loading ──────────────────────────────────────────────────────────

  const loadUsers = useCallback(async () => {
    setLoading(true);
    try {
      const data = await userService.getAll({ pageSize: 200 });
      setUsers(data);
    } catch {
      toast.error('Failed to load users');
    } finally {
      setLoading(false);
    }
  }, [toast]);

  useEffect(() => { loadUsers(); }, [loadUsers]);

  // ── Forms ─────────────────────────────────────────────────────────────────

  const createForm = useForm<CreateUserForm>({ resolver: zodResolver(createUserSchema) });
  const editForm = useForm<EditUserForm>({ resolver: zodResolver(editUserSchema) });
  const passwordForm = useForm<ChangePasswordForm>({ resolver: zodResolver(changePasswordSchema) });
  const shiftForm = useForm<ChangeShiftForm>({ resolver: zodResolver(changeShiftSchema) });

  // ── Create user ───────────────────────────────────────────────────────────

  const handleCreate = async (data: CreateUserForm) => {
    setCreating(true);
    try {
      await userService.create(data as CreateUserRequest);
      toast.success('User created successfully');
      setShowCreate(false);
      createForm.reset();
      loadUsers();
    } catch {
      toast.error('Failed to create user');
    } finally {
      setCreating(false);
    }
  };

  // ── Open manage modal ─────────────────────────────────────────────────────

  const openManage = (user: User) => {
    setManagingUser(user);
    setManageTab('details');
    setEditMode(false);
    setDeleteConfirmEmail('');
    editForm.reset({
      fullName: user.fullName ?? `${user.firstName} ${user.lastName}`.trim(),
      roles: user.userRoles?.map((r) => r.name) ?? [user.role],
    });
    shiftForm.reset({ shift: user.shift ?? '', department: user.department ?? '' });
    passwordForm.reset();
  };

  const closeManage = () => {
    setManagingUser(null);
    setEditMode(false);
    setDeleteConfirmEmail('');
  };

  // ── Edit details ──────────────────────────────────────────────────────────

  const handleEditSave = async (data: EditUserForm) => {
    if (!managingUser) return;
    setActionLoading(true);
    try {
      await userService.update({ email: managingUser.email, fullName: data.fullName, roles: data.roles });
      toast.success('User updated');
      setEditMode(false);
      await loadUsers();
      // Refresh the managing user from updated list
      setManagingUser((prev) => prev ? { ...prev, fullName: data.fullName } : prev);
    } catch {
      toast.error('Failed to update user');
    } finally {
      setActionLoading(false);
    }
  };

  // ── Toggle active ─────────────────────────────────────────────────────────

  const handleToggleActive = async (user: User) => {
    setActionLoading(true);
    try {
      if (user.isActive) {
        await userService.deactivate(user.email);
        toast.success(`${user.fullName ?? user.email} deactivated`);
      } else {
        await userService.activate(user.email);
        toast.success(`${user.fullName ?? user.email} activated`);
      }
      await loadUsers();
      closeManage();
    } catch {
      toast.error('Failed to update user status');
    } finally {
      setActionLoading(false);
    }
  };

  // ── Change password ───────────────────────────────────────────────────────

  const handleChangePassword = async (data: ChangePasswordForm) => {
    if (!managingUser) return;
    setActionLoading(true);
    try {
      await userService.adminChangePassword(managingUser.email, data.newPassword);
      toast.success('Password changed successfully');
      passwordForm.reset();
    } catch {
      toast.error('Failed to change password');
    } finally {
      setActionLoading(false);
    }
  };

  // ── Change shift ──────────────────────────────────────────────────────────

  const handleChangeShift = async (data: ChangeShiftForm) => {
    if (!managingUser) return;
    setActionLoading(true);
    try {
      await userService.changeShift(managingUser.email, data.shift ?? null, data.department ?? null);
      toast.success('Shift updated');
      await loadUsers();
    } catch {
      toast.error('Failed to update shift');
    } finally {
      setActionLoading(false);
    }
  };

  // ── Delete user ───────────────────────────────────────────────────────────

  const handleDelete = async () => {
    if (!managingUser || deleteConfirmEmail !== managingUser.email) return;
    setActionLoading(true);
    try {
      await userService.deleteUser(managingUser.email);
      toast.success('User deleted');
      closeManage();
      loadUsers();
    } catch {
      toast.error('Failed to delete user');
    } finally {
      setActionLoading(false);
    }
  };

  // ── Filter ────────────────────────────────────────────────────────────────

  const filtered = users.filter((u) => {
    const name = (u.fullName ?? `${u.firstName} ${u.lastName}`).toLowerCase();
    const matchSearch = name.includes(search.toLowerCase()) || u.email.toLowerCase().includes(search.toLowerCase());
    const matchRole = !roleFilter || u.role === roleFilter || u.userRoles?.some((r) => r.name === roleFilter);
    return matchSearch && matchRole;
  });

  // ── Table columns ─────────────────────────────────────────────────────────

  const columns = [
    {
      key: 'name',
      header: 'User',
      render: (u: User) => (
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-full bg-primary-100 dark:bg-primary-900/30 flex items-center justify-center flex-shrink-0">
            {u.picture ? (
              <img src={u.picture} alt="" className="w-9 h-9 rounded-full object-cover" />
            ) : (
              <span className="text-sm font-semibold text-primary-600 dark:text-primary-400">
                {getInitials(u.firstName, u.lastName)}
              </span>
            )}
          </div>
          <div>
            <p className="font-medium text-gray-900 dark:text-white">
              {u.fullName ?? `${u.firstName} ${u.lastName}`}
            </p>
            <p className="text-xs text-gray-500 dark:text-gray-400">{u.email}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'role',
      header: 'Role',
      render: (u: User) => (
        <div className="flex flex-wrap gap-1">
          {u.userRoles && u.userRoles.length > 0
            ? u.userRoles.map((r) => <RoleTag key={r.id} role={r.name} />)
            : <RoleTag role={u.role} />}
        </div>
      ),
    },
    {
      key: 'shift',
      header: 'Shift / Dept',
      render: (u: User) => (
        <div className="text-sm">
          {u.shift ? (
            <>
              <span className="font-medium text-gray-800 dark:text-gray-200">{u.shift}</span>
              {u.department && <span className="text-gray-500 dark:text-gray-400"> · {u.department}</span>}
            </>
          ) : (
            <span className="text-gray-400 dark:text-gray-600">—</span>
          )}
        </div>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (u: User) => (
        <Badge variant={u.isActive ? 'success' : 'danger'}>
          {u.isActive ? 'Active' : 'Inactive'}
        </Badge>
      ),
    },
    {
      key: 'createdAt',
      header: 'Created',
      render: (u: User) => (
        <span className="text-sm text-gray-500 dark:text-gray-400">
          {formatDate(u.createdAt)}
        </span>
      ),
    },
    {
      key: 'actions',
      header: '',
      render: (u: User) => (
        <Button size="sm" variant="ghost" onClick={() => openManage(u)}>
          <UserCog className="w-4 h-4" />
          <span className="ml-1">Manage</span>
        </Button>
      ),
    },
  ];

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Users</h1>
          <p className="text-gray-500 dark:text-gray-400 text-sm mt-1">
            Manage hotel staff and their permissions
          </p>
        </div>
        <Button onClick={() => setShowCreate(true)}>
          <Plus className="w-4 h-4 mr-2" />
          Add User
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <div className="flex flex-col sm:flex-row gap-4 p-4">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search by name or email..."
              className="w-full pl-10 pr-4 py-2 text-sm border border-gray-200 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>
          <Select
            value={roleFilter}
            onChange={(e) => setRoleFilter(e.target.value)}
            options={ROLE_OPTIONS}
            className="sm:w-48"
          />
        </div>
      </Card>

      {/* Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        {[
          { label: 'Total Users', value: users.length, icon: UserCog, color: 'text-blue-600' },
          { label: 'Active', value: users.filter((u) => u.isActive).length, icon: CheckCircle, color: 'text-green-600' },
          { label: 'Inactive', value: users.filter((u) => !u.isActive).length, icon: XCircle, color: 'text-red-500' },
          { label: 'Roles', value: ALL_ROLES.length, icon: Shield, color: 'text-purple-600' },
        ].map((s) => (
          <Card key={s.label} className="p-4">
            <div className="flex items-center gap-3">
              <s.icon className={`w-8 h-8 ${s.color}`} />
              <div>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">{s.value}</p>
                <p className="text-xs text-gray-500 dark:text-gray-400">{s.label}</p>
              </div>
            </div>
          </Card>
        ))}
      </div>

      {/* Table */}
      <Card>
        <Table columns={columns} data={filtered} isLoading={loading} emptyMessage="No users found" />
      </Card>

      {/* ── Create User Modal ── */}
      <Modal isOpen={showCreate} onClose={() => { setShowCreate(false); createForm.reset(); }} title="Add New User" size="md">
        <form onSubmit={createForm.handleSubmit(handleCreate)} className="space-y-4">
          <Input label="Full Name" {...createForm.register('fullName')} error={createForm.formState.errors.fullName?.message} />
          <Input label="Email" type="email" {...createForm.register('email')} error={createForm.formState.errors.email?.message} />
          <Input label="Password" type="password" {...createForm.register('password')} error={createForm.formState.errors.password?.message} />
          <Input label="Phone Number" {...createForm.register('phoneNumber')} error={createForm.formState.errors.phoneNumber?.message} />
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Role</label>
            <select
              {...createForm.register('role')}
              className="w-full px-3 py-2 border border-gray-200 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="">Select role...</option>
              {ALL_ROLES.map((r) => <option key={r} value={r}>{r}</option>)}
            </select>
            {createForm.formState.errors.role && (
              <p className="mt-1 text-xs text-red-500">{createForm.formState.errors.role.message}</p>
            )}
          </div>
          <div className="flex gap-3 pt-2">
            <Button type="button" variant="outline" className="flex-1" onClick={() => { setShowCreate(false); createForm.reset(); }}>
              Cancel
            </Button>
            <Button type="submit" className="flex-1" isLoading={creating}>
              Create User
            </Button>
          </div>
        </form>
      </Modal>

      {/* ── Manage User Modal ── */}
      {managingUser && (
        <Modal isOpen={!!managingUser} onClose={closeManage} title="Manage User" size="lg">
          {/* User header */}
          <div className="flex items-center gap-4 pb-4 mb-4 border-b border-gray-200 dark:border-gray-700">
            <div className="w-12 h-12 rounded-full bg-primary-100 dark:bg-primary-900/30 flex items-center justify-center flex-shrink-0">
              {managingUser.picture ? (
                <img src={managingUser.picture} alt="" className="w-12 h-12 rounded-full object-cover" />
              ) : (
                <span className="text-lg font-bold text-primary-600 dark:text-primary-400">
                  {getInitials(managingUser.firstName, managingUser.lastName)}
                </span>
              )}
            </div>
            <div className="flex-1 min-w-0">
              <p className="font-semibold text-gray-900 dark:text-white truncate">
                {managingUser.fullName ?? `${managingUser.firstName} ${managingUser.lastName}`}
              </p>
              <p className="text-sm text-gray-500 dark:text-gray-400 truncate">{managingUser.email}</p>
            </div>
            <Badge variant={managingUser.isActive ? 'success' : 'danger'}>
              {managingUser.isActive ? 'Active' : 'Inactive'}
            </Badge>
          </div>

          {/* Tabs */}
          <div className="flex border-b border-gray-200 dark:border-gray-700 mb-6 gap-1">
            {([
              { id: 'details', label: 'Details', icon: Edit },
              { id: 'password', label: 'Password', icon: Lock },
              { id: 'shift', label: 'Shift', icon: Clock },
              { id: 'danger', label: 'Danger', icon: AlertTriangle },
            ] as { id: ManageTab; label: string; icon: React.ElementType }[]).map((tab) => (
              <button
                key={tab.id}
                onClick={() => { setManageTab(tab.id); setEditMode(false); }}
                className={`flex items-center gap-1.5 px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
                  manageTab === tab.id
                    ? tab.id === 'danger'
                      ? 'border-red-500 text-red-600 dark:text-red-400'
                      : 'border-primary-500 text-primary-600 dark:text-primary-400'
                    : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300'
                }`}
              >
                <tab.icon className="w-4 h-4" />
                {tab.label}
              </button>
            ))}
          </div>

          {/* ── Details Tab ── */}
          {manageTab === 'details' && (
            <div className="space-y-4">
              {!editMode ? (
                <>
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <p className="text-gray-500 dark:text-gray-400">Full Name</p>
                      <p className="font-medium text-gray-900 dark:text-white mt-0.5">
                        {managingUser.fullName ?? `${managingUser.firstName} ${managingUser.lastName}`}
                      </p>
                    </div>
                    <div>
                      <p className="text-gray-500 dark:text-gray-400">Email</p>
                      <p className="font-medium text-gray-900 dark:text-white mt-0.5">{managingUser.email}</p>
                    </div>
                    <div>
                      <p className="text-gray-500 dark:text-gray-400">Phone</p>
                      <p className="font-medium text-gray-900 dark:text-white mt-0.5">
                        {managingUser.phoneNumber ?? '—'}
                      </p>
                    </div>
                    <div>
                      <p className="text-gray-500 dark:text-gray-400">Roles</p>
                      <div className="flex flex-wrap gap-1 mt-0.5">
                        {managingUser.userRoles && managingUser.userRoles.length > 0
                          ? managingUser.userRoles.map((r) => <RoleTag key={r.id} role={r.name} />)
                          : <RoleTag role={managingUser.role} />}
                      </div>
                    </div>
                    <div>
                      <p className="text-gray-500 dark:text-gray-400">Created</p>
                      <p className="font-medium text-gray-900 dark:text-white mt-0.5">
                        {formatDate(managingUser.createdAt)}
                      </p>
                    </div>
                    <div>
                      <p className="text-gray-500 dark:text-gray-400">Last Active</p>
                      <p className="font-medium text-gray-900 dark:text-white mt-0.5">
                        {managingUser.lastActiveDate ? formatDate(managingUser.lastActiveDate) : '—'}
                      </p>
                    </div>
                  </div>
                  <div className="flex gap-3 pt-2">
                    <Button variant="outline" className="flex-1" onClick={() => setEditMode(true)}>
                      <Edit className="w-4 h-4 mr-2" />
                      Edit Details
                    </Button>
                    <Button
                      variant={managingUser.isActive ? 'outline' : 'primary'}
                      className={`flex-1 ${managingUser.isActive ? 'text-red-600 border-red-300 hover:bg-red-50 dark:hover:bg-red-900/20' : ''}`}
                      isLoading={actionLoading}
                      onClick={() => handleToggleActive(managingUser)}
                    >
                      {managingUser.isActive ? (
                        <><XCircle className="w-4 h-4 mr-2" />Deactivate</>
                      ) : (
                        <><CheckCircle className="w-4 h-4 mr-2" />Activate</>
                      )}
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
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Roles</label>
                    <div className="grid grid-cols-2 gap-2">
                      {ALL_ROLES.map((role) => {
                        const currentRoles = editForm.watch('roles') ?? [];
                        const checked = currentRoles.includes(role);
                        return (
                          <label key={role} className="flex items-center gap-2 cursor-pointer">
                            <input
                              type="checkbox"
                              checked={checked}
                              onChange={(e) => {
                                const next = e.target.checked
                                  ? [...currentRoles, role]
                                  : currentRoles.filter((r) => r !== role);
                                editForm.setValue('roles', next, { shouldValidate: true });
                              }}
                              className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                            />
                            <span className="text-sm text-gray-700 dark:text-gray-300">{role}</span>
                          </label>
                        );
                      })}
                    </div>
                    {editForm.formState.errors.roles && (
                      <p className="mt-1 text-xs text-red-500">{editForm.formState.errors.roles.message}</p>
                    )}
                  </div>
                  <div className="flex gap-3 pt-2">
                    <Button type="button" variant="outline" className="flex-1" onClick={() => setEditMode(false)}>
                      <X className="w-4 h-4 mr-2" />Cancel
                    </Button>
                    <Button type="submit" className="flex-1" isLoading={actionLoading}>
                      <Save className="w-4 h-4 mr-2" />Save Changes
                    </Button>
                  </div>
                </form>
              )}
            </div>
          )}

          {/* ── Password Tab ── */}
          {manageTab === 'password' && (
            <form onSubmit={passwordForm.handleSubmit(handleChangePassword)} className="space-y-4">
              <p className="text-sm text-gray-500 dark:text-gray-400">
                Set a new password for <strong className="text-gray-700 dark:text-gray-300">{managingUser.email}</strong>.
                The user will be able to log in immediately with this password.
              </p>
              <div className="relative">
                <Input
                  label="New Password"
                  type={showPassword ? 'text' : 'password'}
                  {...passwordForm.register('newPassword')}
                  error={passwordForm.formState.errors.newPassword?.message}
                />
                <button
                  type="button"
                  className="absolute right-3 top-8 text-gray-400 hover:text-gray-600"
                  onClick={() => setShowPassword((v) => !v)}
                >
                  {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
              <div className="relative">
                <Input
                  label="Confirm Password"
                  type={showConfirmPassword ? 'text' : 'password'}
                  {...passwordForm.register('confirmPassword')}
                  error={passwordForm.formState.errors.confirmPassword?.message}
                />
                <button
                  type="button"
                  className="absolute right-3 top-8 text-gray-400 hover:text-gray-600"
                  onClick={() => setShowConfirmPassword((v) => !v)}
                >
                  {showConfirmPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
              <Button type="submit" className="w-full" isLoading={actionLoading}>
                <Lock className="w-4 h-4 mr-2" />
                Change Password
              </Button>
            </form>
          )}

          {/* ── Shift Tab ── */}
          {manageTab === 'shift' && (
            <form onSubmit={shiftForm.handleSubmit(handleChangeShift)} className="space-y-4">
              <p className="text-sm text-gray-500 dark:text-gray-400">
                Assign a shift and department to this staff member.
              </p>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Shift</label>
                <select
                  {...shiftForm.register('shift')}
                  className="w-full px-3 py-2 border border-gray-200 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
                >
                  <option value="">None</option>
                  {SHIFT_OPTIONS.map((s) => <option key={s} value={s}>{s}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Department</label>
                <select
                  {...shiftForm.register('department')}
                  className="w-full px-3 py-2 border border-gray-200 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
                >
                  <option value="">None</option>
                  {DEPARTMENT_OPTIONS.map((d) => <option key={d} value={d}>{d}</option>)}
                </select>
              </div>
              <div className="p-3 bg-gray-50 dark:bg-gray-700/50 rounded-lg text-sm">
                <p className="text-gray-500 dark:text-gray-400">Current assignment</p>
                <p className="font-medium text-gray-800 dark:text-gray-200 mt-1">
                  {managingUser.shift
                    ? `${managingUser.shift} shift${managingUser.department ? ` · ${managingUser.department}` : ''}`
                    : 'No shift assigned'}
                </p>
              </div>
              <Button type="submit" className="w-full" isLoading={actionLoading}>
                <Clock className="w-4 h-4 mr-2" />
                Update Shift
              </Button>
            </form>
          )}

          {/* ── Danger Tab ── */}
          {manageTab === 'danger' && (
            <div className="space-y-4">
              <div className="p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
                <div className="flex gap-3">
                  <AlertTriangle className="w-5 h-5 text-red-600 dark:text-red-400 flex-shrink-0 mt-0.5" />
                  <div>
                    <p className="text-sm font-semibold text-red-800 dark:text-red-300">Delete User Account</p>
                    <p className="text-sm text-red-700 dark:text-red-400 mt-1">
                      This action permanently removes the user from the system. Their historical records
                      (reservations, payments, audit logs) will be preserved for compliance.
                    </p>
                  </div>
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Type <strong>{managingUser.email}</strong> to confirm
                </label>
                <input
                  value={deleteConfirmEmail}
                  onChange={(e) => setDeleteConfirmEmail(e.target.value)}
                  placeholder={managingUser.email}
                  className="w-full px-3 py-2 border border-gray-200 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
                />
              </div>
              <Button
                variant="outline"
                className="w-full text-red-600 border-red-300 hover:bg-red-50 dark:hover:bg-red-900/20 disabled:opacity-40"
                isLoading={actionLoading}
                disabled={deleteConfirmEmail !== managingUser.email}
                onClick={handleDelete}
              >
                <Trash2 className="w-4 h-4 mr-2" />
                Delete User Permanently
              </Button>
            </div>
          )}
        </Modal>
      )}
    </div>
  );
}

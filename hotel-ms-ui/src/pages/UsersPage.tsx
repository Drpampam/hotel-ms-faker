import { useState, useEffect, useCallback } from 'react';
import { Search, Plus, UserCog, Shield, X, Eye, EyeOff } from 'lucide-react';
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

const MOCK_USERS: User[] = [
  { id: '1', email: 'admin@hotelms.com', firstName: 'Admin', lastName: 'User', role: 'Admin', tenantId: 1, createdAt: new Date(Date.now() - 90 * 86400000).toISOString(), isActive: true },
  { id: '2', email: 'dev@hotelms.com', firstName: 'Tech', lastName: 'Developer', role: 'Developer', tenantId: 1, createdAt: new Date(Date.now() - 120 * 86400000).toISOString(), isActive: true },
  { id: '3', email: 'john.guest@example.com', firstName: 'John', lastName: 'Guest', role: 'Guest', tenantId: 1, createdAt: new Date(Date.now() - 10 * 86400000).toISOString(), isActive: true },
];

const ROLE_OPTIONS: { value: string; label: string }[] = [
  { value: '', label: 'All Roles' },
  { value: 'Admin', label: 'Admin' },
  { value: 'Guest', label: 'Guest' },
  { value: 'Developer', label: 'Developer' },
];

const createUserSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  email: z.string().email('Invalid email address'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
  confirmPassword: z.string(),
  role: z.enum(['Admin', 'Guest', 'Developer']),
  phoneNumber: z.string().optional(),
}).refine((d) => d.password === d.confirmPassword, {
  message: "Passwords don't match",
  path: ['confirmPassword'],
});

type CreateUserFormData = z.infer<typeof createUserSchema>;

export function UsersPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [viewUser, setViewUser] = useState<User | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const toast = useToast();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateUserFormData>({
    resolver: zodResolver(createUserSchema),
    defaultValues: { role: 'Admin' },
  });

  const fetchUsers = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await userService.getAll();
      setUsers(data.length > 0 ? data : MOCK_USERS);
    } catch {
      setUsers(MOCK_USERS);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  const filtered = users.filter((u) => {
    const matchSearch =
      !search ||
      u.firstName.toLowerCase().includes(search.toLowerCase()) ||
      u.lastName.toLowerCase().includes(search.toLowerCase()) ||
      u.email.toLowerCase().includes(search.toLowerCase());
    const matchRole = !roleFilter || u.role === roleFilter;
    return matchSearch && matchRole;
  });

  const onSubmit = async (data: CreateUserFormData) => {
    setIsSubmitting(true);
    try {
      const payload: CreateUserRequest = {
        fullName: `${data.firstName} ${data.lastName}`.trim(),
        email: data.email,
        password: data.password,
        role: data.role,
        phoneNumber: data.phoneNumber ?? '',
      };
      await userService.create(payload);
      toast.success('User created', `${data.firstName} ${data.lastName} has been added`);
      setIsCreateOpen(false);
      reset();
      await fetchUsers();
    } catch (err) {
      toast.error('Failed to create user', err instanceof Error ? err.message : 'Please check the details and try again');
    } finally {
      setIsSubmitting(false);
    }
  };

  const roleBadge = (role: string) => {
    const colorClass = ROLE_COLORS[role] ?? 'bg-slate-100 text-slate-700';
    return (
      <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium ${colorClass}`}>
        <Shield className="h-3 w-3" />
        {role}
      </span>
    );
  };

  const columns = [
    {
      key: 'name',
      header: 'User',
      render: (u: User) => (
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-full bg-indigo-600 flex items-center justify-center flex-shrink-0">
            <span className="text-sm font-semibold text-white">
              {getInitials(u.firstName, u.lastName)}
            </span>
          </div>
          <div>
            <p className="font-medium text-slate-900 dark:text-slate-100">
              {u.firstName} {u.lastName}
            </p>
            <p className="text-xs text-slate-500 dark:text-slate-400">{u.email}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'role',
      header: 'Role',
      render: (u: User) => roleBadge(u.role),
    },
    {
      key: 'isActive',
      header: 'Status',
      render: (u: User) => (
        <Badge variant={u.isActive !== false ? 'success' : 'default'} dot>
          {u.isActive !== false ? 'Active' : 'Inactive'}
        </Badge>
      ),
    },
    {
      key: 'createdAt',
      header: 'Joined',
      render: (u: User) => formatDate(u.createdAt),
    },
    {
      key: 'actions',
      header: '',
      render: (u: User) => (
        <Button
          variant="ghost"
          size="sm"
          leftIcon={<UserCog className="h-3.5 w-3.5" />}
          onClick={(e) => {
            e.stopPropagation();
            setViewUser(u);
          }}
        >
          Manage
        </Button>
      ),
    },
  ];

  const roleCounts = ROLE_OPTIONS.filter((r) => r.value).map((r) => ({
    role: r.label,
    count: users.filter((u) => u.role === r.value).length,
  }));

  return (
    <div className="page-container">
      <div className="page-header flex items-start justify-between flex-wrap gap-4">
        <div>
          <h2 className="page-title">Users</h2>
          <p className="page-subtitle">{users.length} system users</p>
        </div>
        <Button leftIcon={<Plus className="h-4 w-4" />} onClick={() => setIsCreateOpen(true)}>
          Add User
        </Button>
      </div>

      {/* Role breakdown */}
      <div className="grid grid-cols-2 sm:grid-cols-5 gap-3 mb-6">
        {roleCounts.map(({ role, count }) => (
          <div
            key={role}
            className="bg-white dark:bg-slate-800 rounded-xl border border-slate-100 dark:border-slate-700 p-3 text-center shadow-sm"
          >
            <p className="text-2xl font-bold text-slate-900 dark:text-slate-100">{count}</p>
            <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">{role}</p>
          </div>
        ))}
      </div>

      {/* Filters */}
      <Card className="mb-6" padding="sm">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              type="text"
              placeholder="Search users by name or email..."
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
          <div className="sm:w-44">
            <Select
              options={ROLE_OPTIONS}
              value={roleFilter}
              onChange={(e) => setRoleFilter(e.target.value)}
            />
          </div>
        </div>
      </Card>

      {/* Table */}
      <Card padding="none">
        <Table
          data={filtered}
          columns={columns}
          isLoading={isLoading}
          emptyMessage="No users found"
          emptyDescription="Add team members to get started"
        />
      </Card>

      {/* Create User Modal */}
      <Modal
        isOpen={isCreateOpen}
        onClose={() => {
          setIsCreateOpen(false);
          reset();
        }}
        title="Add New User"
        description="Create a new system user account"
        size="lg"
        footer={
          <>
            <Button
              variant="outline"
              onClick={() => {
                setIsCreateOpen(false);
                reset();
              }}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button isLoading={isSubmitting} onClick={handleSubmit(onSubmit)}>
              Create User
            </Button>
          </>
        }
      >
        <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Input
              label="First Name"
              required
              placeholder="Alice"
              {...register('firstName')}
              error={errors.firstName?.message}
            />
            <Input
              label="Last Name"
              required
              placeholder="Thompson"
              {...register('lastName')}
              error={errors.lastName?.message}
            />
            <div className="sm:col-span-2">
              <Input
                label="Email Address"
                required
                type="email"
                placeholder="alice@hotelms.com"
                {...register('email')}
                error={errors.email?.message}
              />
            </div>
            <div className="sm:col-span-2">
              <Select
                label="Role"
                required
                options={ROLE_OPTIONS.filter((r) => r.value).map((r) => ({
                  value: r.value,
                  label: r.label,
                }))}
                {...register('role')}
                error={errors.role?.message}
              />
            </div>
            {/* Password */}
            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">
                Password <span className="text-red-500">*</span>
              </label>
              <div className="relative">
                <input
                  {...register('password')}
                  type={showPassword ? 'text' : 'password'}
                  placeholder="Min. 6 characters"
                  className="w-full h-10 px-3 pr-10 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-slate-900 dark:text-slate-100 placeholder:text-slate-400"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                >
                  {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
              {errors.password && (
                <p className="mt-1 text-xs text-red-500">{errors.password.message}</p>
              )}
            </div>
            {/* Confirm Password */}
            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1.5">
                Confirm Password <span className="text-red-500">*</span>
              </label>
              <div className="relative">
                <input
                  {...register('confirmPassword')}
                  type={showConfirm ? 'text' : 'password'}
                  placeholder="Repeat password"
                  className="w-full h-10 px-3 pr-10 text-sm bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-slate-900 dark:text-slate-100 placeholder:text-slate-400"
                />
                <button
                  type="button"
                  onClick={() => setShowConfirm(!showConfirm)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                >
                  {showConfirm ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
              {errors.confirmPassword && (
                <p className="mt-1 text-xs text-red-500">{errors.confirmPassword.message}</p>
              )}
            </div>
            <div className="sm:col-span-2">
              <Input
                label="Phone Number"
                type="tel"
                placeholder="+1 555-0100"
                {...register('phoneNumber')}
              />
            </div>
          </div>
        </form>
      </Modal>

      {/* View User Modal */}
      <Modal
        isOpen={!!viewUser}
        onClose={() => setViewUser(null)}
        title="User Details"
        size="sm"
      >
        {viewUser && (
          <div className="space-y-4">
            <div className="flex items-center gap-4">
              <div className="w-14 h-14 rounded-2xl bg-indigo-600 flex items-center justify-center">
                <span className="text-xl font-bold text-white">
                  {getInitials(viewUser.firstName, viewUser.lastName)}
                </span>
              </div>
              <div>
                <h3 className="font-bold text-lg text-slate-900 dark:text-slate-100">
                  {viewUser.firstName} {viewUser.lastName}
                </h3>
                {roleBadge(viewUser.role)}
              </div>
            </div>
            <div className="space-y-2.5">
              {[
                { label: 'Email', value: viewUser.email },
                { label: 'Phone', value: viewUser.phoneNumber ?? '—' },
                { label: 'Status', value: viewUser.isActive !== false ? 'Active' : 'Inactive' },
                { label: 'Joined', value: formatDate(viewUser.createdAt) },
              ].map((item) => (
                <div
                  key={item.label}
                  className="flex justify-between items-center py-2 border-b border-slate-100 dark:border-slate-700 last:border-0"
                >
                  <span className="text-sm text-slate-500 dark:text-slate-400">{item.label}</span>
                  <span className="text-sm font-medium text-slate-900 dark:text-slate-100">
                    {item.value}
                  </span>
                </div>
              ))}
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}

export default UsersPage;

import api from '../lib/axios';
import type { User, CreateUserRequest, ApiResponse, PageApiResponse } from '../types';

// Map backend ApplicationUserDTO to UI User shape
function mapUser(r: {
  fullName?: string;
  status?: string;
  email?: string;
  creationDate?: string;
  lastActiveDate?: string;
  lastModifiedDate?: string;
  userRoles?: Array<{ id: number; name: string }>;
  isActive?: boolean;
  phoneNumber?: string;
  shift?: string;
  department?: string;
  picture?: string;
}): User {
  const parts = (r.fullName ?? '').trim().split(/\s+/);
  const firstName = parts[0] ?? r.email ?? '';
  const lastName = parts.slice(1).join(' ') || '';
  const primaryRole = r.userRoles?.[0]?.name ?? 'Admin';

  return {
    id: r.email ?? '',
    email: r.email ?? '',
    fullName: r.fullName,
    firstName,
    lastName,
    role: primaryRole as User['role'],
    userRoles: r.userRoles,
    status: r.status,
    tenantId: (r.tenantId as number | undefined) ?? 0,
    isActive: r.isActive ?? r.status !== 'Inactive',
    phoneNumber: r.phoneNumber,
    shift: r.shift,
    department: r.department,
    picture: r.picture,
    createdAt: r.creationDate ?? new Date().toISOString(),
    creationDate: r.creationDate,
    lastActiveDate: r.lastActiveDate,
    lastModifiedDate: r.lastModifiedDate,
  } as User;
}

export const userService = {
  async getAll(params?: { pageNumber?: number; pageSize?: number; tenantId?: number }): Promise<User[]> {
    const response = await api.post<PageApiResponse<unknown[]>>('/api/v1/User/get-users', {
      pageNumber: params?.pageNumber ?? 1,
      pageSize: params?.pageSize ?? 100,
      tenantId: params?.tenantId ?? null,
    });
    const raw = response.data?.data;
    if (!Array.isArray(raw)) return [];
    return raw.map((r) => mapUser(r as Parameters<typeof mapUser>[0]));
  },

  async getByEmail(email: string): Promise<User> {
    const response = await api.get<ApiResponse<unknown>>('/api/v1/User/get-user-by-email', {
      params: { email },
    });
    return mapUser(response.data.data as Parameters<typeof mapUser>[0]);
  },

  async create(user: CreateUserRequest): Promise<void> {
    await api.post('/api/v1/User/add-staff', user);
  },

  async update(user: { email: string; fullName: string; roles: string[] }): Promise<void> {
    await api.put('/api/v1/User/update-user', {
      email: user.email,
      fullName: user.fullName,
      roles: user.roles,
    });
  },

  async activate(email: string): Promise<void> {
    await api.put('/api/v1/User/activate-user', { email });
  },

  async deactivate(email: string): Promise<void> {
    await api.put('/api/v1/User/deactivate-user', { email });
  },

  async reassignRole(email: string, roles: string[]): Promise<void> {
    await api.put('/api/v1/User/reassign-role', { email, roles });
  },

  async adminChangePassword(email: string, newPassword: string): Promise<void> {
    await api.put('/api/v1/User/admin-change-password', { email, newPassword });
  },

  async deleteUser(email: string): Promise<void> {
    await api.delete('/api/v1/User/delete-user', { data: { email } });
  },

  async changeShift(email: string, shift: string | null, department: string | null): Promise<void> {
    await api.put('/api/v1/User/change-shift', { email, shift, department });
  },

  async getAssignedModules(email: string) {
    const response = await api.get('/api/v1/User/get-assigned-modules', { params: { email } });
    return response.data;
  },
};

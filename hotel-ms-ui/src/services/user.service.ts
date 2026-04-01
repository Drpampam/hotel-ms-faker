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
}): User {
  const parts = (r.fullName ?? '').trim().split(/\s+/);
  const firstName = parts[0] ?? r.email ?? '';
  const lastName = parts.slice(1).join(' ') || '';
  const primaryRole = r.userRoles?.[0]?.name ?? 'FrontDesk';

  return {
    id: r.email ?? '',   // backend uses email as identifier
    email: r.email ?? '',
    fullName: r.fullName,
    firstName,
    lastName,
    role: primaryRole as User['role'],
    userRoles: r.userRoles,
    status: r.status,
    tenantId: 1,
    isActive: r.status !== 'Inactive',
    createdAt: r.creationDate ?? new Date().toISOString(),
    creationDate: r.creationDate,
    lastActiveDate: r.lastActiveDate,
    lastModifiedDate: r.lastModifiedDate,
  } as User;
}

export const userService = {
  /**
   * POST /api/v1/User/get-users — body: { pageNumber, pageSize }
   */
  async getAll(params?: { pageNumber?: number; pageSize?: number }): Promise<User[]> {
    const response = await api.post<PageApiResponse<unknown[]>>('/api/v1/User/get-users', {
      pageNumber: params?.pageNumber ?? 1,
      pageSize: params?.pageSize ?? 100,
    });
    const raw = response.data?.data;
    if (!Array.isArray(raw)) return [];
    return raw.map((r) => mapUser(r as Parameters<typeof mapUser>[0]));
  },

  /**
   * GET /api/v1/User/get-user-by-email?email=...
   */
  async getByEmail(email: string): Promise<User> {
    const response = await api.get<ApiResponse<unknown>>('/api/v1/User/get-user-by-email', {
      params: { email },
    });
    return mapUser(response.data.data as Parameters<typeof mapUser>[0]);
  },

  /**
   * POST /api/v1/User/create-user — body: CreateUserRequestDTO
   * { email, fullName, phoneNumber, password, role, hotelName?, subscriptionPlanId? }
   */
  async create(user: CreateUserRequest): Promise<User> {
    const response = await api.post<ApiResponse<unknown>>('/api/v1/User/create-user', user);
    return mapUser(response.data.data as Parameters<typeof mapUser>[0]);
  },

  /**
   * PUT /api/v1/User/activate-user — body: { email }
   */
  async activate(email: string): Promise<void> {
    await api.put('/api/v1/User/activate-user', { email });
  },

  /**
   * PUT /api/v1/User/deactivate-user — body: { email }
   */
  async deactivate(email: string): Promise<void> {
    await api.put('/api/v1/User/deactivate-user', { email });
  },

  /**
   * PUT /api/v1/User/reassign-role — body: { email, roles: string[] }
   */
  async reassignRole(email: string, roles: string[]): Promise<void> {
    await api.put('/api/v1/User/reassign-role', { email, roles });
  },

  /**
   * GET /api/v1/User/get-assigned-modules?email=...
   */
  async getAssignedModules(email: string) {
    const response = await api.get('/api/v1/User/get-assigned-modules', { params: { email } });
    return response.data;
  },
};

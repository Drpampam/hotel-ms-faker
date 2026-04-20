import api from '../lib/api';

export type PlanType = 'Trial' | 'Monthly3' | 'Monthly6' | 'FiveYear' | 'Unlimited';

export interface LoginResponse {
  email: string;
  fullName: string;
  roles: string[];
  token: string;
}

export interface ProvisionRequest {
  email: string;
  fullName?: string;
  planType: PlanType;
}

export interface ProvisionResult {
  email: string;
  tempPassword: string;
  activationCode: string;
  planLabel: string;
  fullName: string;
}

export interface TenantSummary {
  id: number;
  name: string;
  adminEmail: string;
  planLabel: string;
  isActive: boolean;
  isExpired: boolean;
  isUnlimited: boolean;
  expiresAt: string | null;
  daysRemaining: number | null;
  createdAt: string;
}

const adminService = {
  async login(email: string, password: string): Promise<LoginResponse> {
    const res = await api.post<{ data: { email: string; fullName: string; roles: string[] } }>(
      '/api/v1/User/login',
      { email, password, rememberMe: true }
    );
    const token = res.headers['token'] as string;
    if (!token) throw new Error('Login failed: no token received');
    return {
      email: res.data.data?.email ?? email,
      fullName: res.data.data?.fullName ?? email,
      roles: res.data.data?.roles ?? [],
      token,
    };
  },

  async provisionTenant(data: ProvisionRequest): Promise<ProvisionResult> {
    const res = await api.post<{ data: ProvisionResult }>('/api/v1/activation/provision', data);
    return res.data.data;
  },

  async getAllTenants(): Promise<TenantSummary[]> {
    const res = await api.get<{ data: TenantSummary[] }>('/api/v1/activation/tenants');
    return res.data.data ?? [];
  },

  async renewSubscription(tenantId: number, code: string): Promise<void> {
    await api.post(`/api/v1/activation/renew/${tenantId}`, { code });
  },
};

export default adminService;

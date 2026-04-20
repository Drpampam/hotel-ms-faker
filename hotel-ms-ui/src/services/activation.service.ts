import api from '../lib/axios';

const BASE = '/api/v1/activation';

export type PlanType = 'Trial' | 'Monthly3' | 'Monthly6' | 'FiveYear' | 'Unlimited';

export interface ActivateTenantRequest {
  code: string;
  email: string;
  tenantName: string;
  adminPassword: string;
  adminFullName: string;
}

export interface ActivateTenantResponse {
  tenantId: number;
  tenantName: string;
  token: string;
  refreshToken: string;
  planType: PlanType;
  expiresAt: string | null;
  isUnlimited: boolean;
}

export interface SubscriptionStatus {
  tenantId: number;
  planType: PlanType;
  planLabel: string;
  isUnlimited: boolean;
  isExpired: boolean;
  isActive: boolean;
  expiresAt: string | null;
  daysRemaining: number | null;
}

export interface GenerateCodeRequest {
  email: string;
  planType: PlanType;
}

export interface GenerateCodeResponse {
  plaintextCode: string;
  boundToEmail: string;
  planType: PlanType;
  planLabel: string;
}

export interface SelfRegisterRequest {
  email: string;
  fullName: string;
  hotelName: string;
  password: string;
  planType: PlanType;
}

export interface SelfRegisterResponse {
  plaintextCode: string;
  boundToEmail: string;
  planLabel: string;
  hotelName: string;
}

export interface ActivateMyAccountResponse {
  tenantId: number;
  tenantName: string;
  token: string;
  refreshToken: string;
  planLabel: string;
  expiresAt: string | null;
  isUnlimited: boolean;
}

export interface TenantSummary {
  id: number;
  name: string;
  adminEmail: string;
  planType: number;
  planLabel: string;
  isActive: boolean;
  isExpired: boolean;
  isUnlimited: boolean;
  expiresAt: string | null;
  daysRemaining: number | null;
  createdAt: string;
}

const activationService = {
  async activate(data: ActivateTenantRequest): Promise<ActivateTenantResponse> {
    const res = await api.post<{ data: ActivateTenantResponse }>(`${BASE}/activate`, data);
    return res.data.data;
  },

  async getStatus(tenantId: number): Promise<SubscriptionStatus> {
    const res = await api.get<{ data: SubscriptionStatus }>(`${BASE}/status/${tenantId}`);
    return res.data.data;
  },

  async renew(tenantId: number, code: string): Promise<void> {
    await api.post(`${BASE}/renew/${tenantId}`, { code });
  },

  async generateCode(data: GenerateCodeRequest): Promise<GenerateCodeResponse> {
    const res = await api.post<{ data: GenerateCodeResponse }>(`${BASE}/generate`, data);
    return res.data.data;
  },

  async getAllTenants(): Promise<TenantSummary[]> {
    const res = await api.get<{ data: TenantSummary[] }>(`${BASE}/tenants`);
    return res.data.data ?? [];
  },

  async selfRegister(data: SelfRegisterRequest): Promise<SelfRegisterResponse> {
    const res = await api.post<{ data: SelfRegisterResponse }>(`${BASE}/self-register`, data);
    return res.data.data;
  },

  async activateMyAccount(code: string): Promise<ActivateMyAccountResponse & { tokenFromHeader?: string; refreshTokenFromHeader?: string; tenantIdFromHeader?: string }> {
    const res = await api.post<{ data: ActivateMyAccountResponse }>(`${BASE}/activate-my-account`, { code });
    return {
      ...res.data.data,
      tokenFromHeader: res.headers['token'] as string | undefined,
      refreshTokenFromHeader: res.headers['refreshtoken'] as string | undefined,
      tenantIdFromHeader: res.headers['x-tenant-id'] as string | undefined,
    };
  },
};

export default activationService;

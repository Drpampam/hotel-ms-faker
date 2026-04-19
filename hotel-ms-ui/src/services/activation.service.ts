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
};

export default activationService;

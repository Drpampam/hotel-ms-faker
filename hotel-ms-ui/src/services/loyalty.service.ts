import api from '../lib/axios';
import type { ApiResponse } from '../types';

export interface LoyaltySummary {
  userId: number;
  totalPoints: number;
  tier: string;
  records: LoyaltyRecord[];
}

export interface LoyaltyRecord {
  id: number;
  points: number;
  reason?: string;
  creationDate: string;
}

function mapLoyalty(raw: unknown): LoyaltySummary {
  const r = raw as Record<string, unknown>;
  return {
    userId: r.userId as number,
    totalPoints: (r.pointsBalance as number) ?? 0,
    tier: (r.tier as string) ?? 'Bronze',
    records: Array.isArray(r.records)
      ? r.records.map((rec: unknown) => {
          const lr = rec as Record<string, unknown>;
          return {
            id: lr.id as number,
            points: (lr.points as number) ?? 0,
            reason: lr.reason as string | undefined,
            creationDate: (lr.creationDate as string) ?? new Date().toISOString(),
          };
        })
      : [],
  };
}

export const loyaltyService = {
  async getByUserId(userId: number): Promise<LoyaltySummary> {
    const res = await api.get<ApiResponse<unknown>>(`/api/v1/loyalty/${userId}`);
    return mapLoyalty(res.data.data);
  },

  async accruePoints(userId: number, points: number, reason: string): Promise<LoyaltySummary> {
    const res = await api.post<ApiResponse<unknown>>(`/api/v1/loyalty/${userId}/accrue`, { points, reason });
    return mapLoyalty(res.data.data);
  },

  async redeemPoints(userId: number, points: number, reservationId?: number): Promise<LoyaltySummary> {
    const res = await api.post<ApiResponse<unknown>>(`/api/v1/loyalty/${userId}/redeem`, { points, reservationId });
    return mapLoyalty(res.data.data);
  },
};

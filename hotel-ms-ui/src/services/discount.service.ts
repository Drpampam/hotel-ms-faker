import api from '../lib/axios';
import type { ApiResponse, PageApiResponse, Discount, CreateDiscountRequest } from '../types';

function mapDiscount(raw: unknown): Discount {
  const r = raw as Record<string, unknown>;
  return {
    id: r.id as number,
    name: (r.name as string) ?? '',
    description: r.description as string | undefined,
    percentage: (r.percentage as number) ?? 0,
    fixedAmount: r.fixedAmount as number | undefined,
    isActive: (r.isActive as boolean) ?? false,
    startDate: r.startDate as string | undefined,
    endDate: r.endDate as string | undefined,
    creationDate: (r.creationDate as string) ?? new Date().toISOString(),
  };
}

export const discountService = {
  async getAll(): Promise<Discount[]> {
    const res = await api.get<PageApiResponse<unknown[]>>('/api/v1/discounts', {
      params: { pageSize: 100, pageNumber: 1 },
    });
    const raw = res.data?.data;
    return Array.isArray(raw) ? raw.map(mapDiscount) : [];
  },

  async getById(id: number): Promise<Discount> {
    const res = await api.get<ApiResponse<unknown>>(`/api/v1/discounts/${id}`);
    return mapDiscount(res.data.data);
  },

  async create(request: CreateDiscountRequest): Promise<Discount> {
    const res = await api.post<ApiResponse<unknown>>('/api/v1/discounts', request);
    return mapDiscount(res.data.data);
  },

  async update(id: number, request: CreateDiscountRequest): Promise<Discount> {
    const res = await api.put<ApiResponse<unknown>>('/api/v1/discounts', { id, ...request });
    return mapDiscount(res.data.data);
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/api/v1/discounts/${id}`);
  },
};

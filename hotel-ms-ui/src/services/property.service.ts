import api from '../lib/axios';
import type { Property, ApiResponse, PageApiResponse } from '../types';

function mapProperty(r: {
  id: number;
  name: string;
  description?: string;
  image?: string;
  creationDate?: string;
  lastModifiedDate?: string;
  address?: {
    street?: string;
    city?: string;
    state?: string;
    zipCode?: string;
    country?: string;
    latitude?: number;
    longitude?: number;
  };
}): Property {
  return {
    ...r,
    city: r.address?.city,
    country: r.address?.country,
    createdAt: r.creationDate ?? new Date().toISOString(),
  } as Property;
}

export const propertyService = {
  /**
   * GET /api/v1/Properties?tenantId=&name=&pageNumber=&pageSize=
   */
  async getAll(params?: {
    tenantId?: number;
    name?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<Property[]> {
    const response = await api.get<PageApiResponse<unknown[]>>('/api/v1/Properties', {
      params: { pageSize: 100, ...params },
    });
    const raw = response.data?.data;
    if (!Array.isArray(raw)) return [];
    return raw.map((r) => mapProperty(r as Parameters<typeof mapProperty>[0]));
  },

  /**
   * GET /api/v1/Properties/{id}
   */
  async getById(id: number): Promise<Property> {
    const response = await api.get<ApiResponse<unknown>>(`/api/v1/Properties/${id}`);
    return mapProperty(response.data.data as Parameters<typeof mapProperty>[0]);
  },

  /**
   * POST /api/v1/Properties
   * Body: { name, description, image, tenantId, address: { street, city, state, zipCode, country } }
   */
  /**
   * POST /api/v1/Properties
   * Backend returns BaseResponse (no data). Caller must reload after success.
   */
  async create(request: {
    name: string;
    description: string;
    image: string;
    tenantId: number;
    address: { street?: string; city?: string; state?: string; zipCode?: string; country?: string; latitude?: number; longitude?: number };
  }): Promise<void> {
    await api.post('/api/v1/Properties', request);
  },

  /**
   * PUT /api/v1/Properties
   * Backend returns BaseResponse (no data). Caller must reload after success.
   */
  async update(request: {
    id: number;
    name: string;
    description: string;
    image: string;
    address: { street?: string; city?: string; state?: string; zipCode?: string; country?: string; latitude?: number; longitude?: number };
  }): Promise<void> {
    await api.put('/api/v1/Properties', request);
  },
};

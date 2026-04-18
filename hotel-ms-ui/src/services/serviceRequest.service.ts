import api from '../lib/axios';
import type { ApiResponse, PageApiResponse, ServiceRequest } from '../types';

function mapState(raw: string | undefined): ServiceRequest['state'] {
  if (raw === 'Requested') return 'Pending';
  if (raw === 'InProgress') return 'InProgress';
  if (raw === 'Completed') return 'Completed';
  if (raw === 'Cancelled') return 'Cancelled';
  return 'Pending';
}

function mapServiceRequest(raw: unknown): ServiceRequest {
  const r = raw as Record<string, unknown>;
  return {
    id: r.id as number,
    reservationId: r.reservationId as number,
    roomId: undefined,
    roomNumber: undefined,
    requestType: (r.serviceType as string) ?? '',
    description: r.notes as string | undefined,
    priority: 'Normal',
    state: mapState(r.serviceRequestState as string | undefined),
    assignedTo: undefined,
    notes: r.notes as string | undefined,
    createdBy: r.createdBy as string | undefined,
    creationDate: (r.creationDate as string) ?? new Date().toISOString(),
    lastModifiedDate: r.completionDate as string | undefined,
  };
}

export const serviceRequestService = {
  async getAll(params?: { state?: string; reservationId?: number; pageNumber?: number; pageSize?: number }): Promise<ServiceRequest[]> {
    const res = await api.get<PageApiResponse<unknown[]>>('/api/v1/service-requests', {
      params: { pageSize: 100, pageNumber: 1, ...params },
    });
    const raw = res.data?.data;
    return Array.isArray(raw) ? raw.map(mapServiceRequest) : [];
  },

  async getById(id: number): Promise<ServiceRequest> {
    const res = await api.get<ApiResponse<unknown>>(`/api/v1/service-requests/${id}`);
    return mapServiceRequest(res.data.data);
  },

  async create(request: {
    reservationId: number;
    serviceType: string;
    notes?: string;
  }): Promise<ServiceRequest> {
    const res = await api.post<ApiResponse<unknown>>('/api/v1/service-requests', request);
    return mapServiceRequest(res.data.data);
  },

  async transition(id: number, trigger: string): Promise<ServiceRequest> {
    const res = await api.patch<ApiResponse<unknown>>(`/api/v1/service-requests/${id}/state`, JSON.stringify(trigger), {
      headers: { 'Content-Type': 'application/json' },
    });
    return mapServiceRequest(res.data.data);
  },
};

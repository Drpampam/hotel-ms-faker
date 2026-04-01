import api from '../lib/axios';
import type { Guest, ApiResponse, PageApiResponse } from '../types';

// Map backend GuestProfileResponseDTO to UI Guest shape
function mapGuest(r: {
  id: number;
  userId: number;
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  passportNumber?: string;
  nationality?: string;
  dateOfBirth?: string;
  preferredRoomType?: string;
  specialRequests?: string;
  loyaltyPoints: number;
  loyaltyTier?: string;
  tenantId?: number;
  creationDate?: string;
}): Guest {
  // Split fullName into first/last for UI convenience
  const parts = (r.fullName ?? '').trim().split(/\s+/);
  const firstName = parts[0] ?? '';
  const lastName = parts.slice(1).join(' ') || '';

  return {
    ...r,
    firstName,
    lastName,
    createdAt: r.creationDate ?? new Date().toISOString(),
    idType: undefined,
    idNumber: r.passportNumber,
  } as Guest;
}

export const guestService = {
  /**
   * GET /api/v1/guests
   * Query: tenantId, searchTerm, nationality, pageNumber, pageSize
   */
  async getAll(params?: {
    tenantId?: number;
    searchTerm?: string;
    nationality?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<Guest[]> {
    const response = await api.get<PageApiResponse<unknown[]>>('/api/v1/guests', {
      params: { pageSize: 100, ...params },
    });
    const raw = response.data?.data;
    if (!Array.isArray(raw)) return [];
    return raw.map((r) => mapGuest(r as Parameters<typeof mapGuest>[0]));
  },

  /**
   * GET /api/v1/guests/{id}
   */
  async getById(id: number): Promise<Guest> {
    const response = await api.get<ApiResponse<unknown>>(`/api/v1/guests/${id}`);
    return mapGuest(response.data.data as Parameters<typeof mapGuest>[0]);
  },

  /**
   * GET /api/v1/guests/by-user/{userId}
   */
  async getByUserId(userId: number): Promise<Guest> {
    const response = await api.get<ApiResponse<unknown>>(`/api/v1/guests/by-user/${userId}`);
    return mapGuest(response.data.data as Parameters<typeof mapGuest>[0]);
  },

  /**
   * POST /api/v1/guests — body: CreateGuestProfileRequestDTO
   * NOTE: The backend requires a userId (an existing application user).
   * The UI create flow should first create a user, then create the guest profile.
   * { userId: long, passportNumber?, nationality?, dateOfBirth?, preferredRoomType?, specialRequests?, tenantId? }
   */
  async create(request: {
    userId: number;
    passportNumber?: string;
    nationality?: string;
    dateOfBirth?: string;
    preferredRoomType?: string;
    specialRequests?: string;
    tenantId?: number;
  }): Promise<Guest> {
    const response = await api.post<ApiResponse<unknown>>('/api/v1/guests', request);
    return mapGuest(response.data.data as Parameters<typeof mapGuest>[0]);
  },

  /**
   * PUT /api/v1/guests — body: UpdateGuestProfileRequestDTO (includes id)
   */
  async update(request: { id: number; [key: string]: unknown }): Promise<Guest> {
    const response = await api.put<ApiResponse<unknown>>('/api/v1/guests', request);
    return mapGuest(response.data.data as Parameters<typeof mapGuest>[0]);
  },

  /**
   * GET /api/v1/guests/{id}/reservations
   */
  async getReservations(id: number, pageNumber = 1, pageSize = 20) {
    const response = await api.get(`/api/v1/guests/${id}/reservations`, {
      params: { pageNumber, pageSize },
    });
    return response.data;
  },
};

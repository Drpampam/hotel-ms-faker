import api from '../lib/axios';
import type { Reservation, CreateReservationRequest, ApiResponse, PageApiResponse } from '../types';

// Map backend ReservationResponseDTO to UI Reservation shape
function mapReservation(r: {
  id: number;
  roomId: number;
  roomNumber?: string;
  roomType?: string;
  guestId: number;
  guestName?: string;
  guestEmail?: string;
  checkInDate: string;
  checkOutDate: string;
  nightsCount: number;
  totalPrice: number;
  status?: string;
  specialRequests?: string;
  discountId?: number;
  createdBy?: string;
  creationDate?: string;
  lastModifiedDate?: string;
}): Reservation {
  return {
    ...r,
    status: (r.status as Reservation['status']) ?? 'Pending',
    totalAmount: r.totalPrice,
    createdAt: r.creationDate ?? new Date().toISOString(),
    reservationNumber: `RES-${r.id}`,
  } as Reservation;
}

export const reservationService = {
  /**
   * GET /api/v1/reservations
   * Query: guestId, roomId, propertyId, status (ReservationState enum), fromDate, toDate, pageNumber, pageSize
   */
  async getAll(params?: {
    guestId?: number;
    roomId?: number;
    propertyId?: number;
    status?: string;
    fromDate?: string;
    toDate?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<Reservation[]> {
    const response = await api.get<PageApiResponse<unknown[]>>('/api/v1/reservations', {
      params: { pageSize: 100, ...params },
    });
    const raw = response.data?.data;
    if (!Array.isArray(raw)) return [];
    return raw.map((r) => mapReservation(r as Parameters<typeof mapReservation>[0]));
  },

  /**
   * GET /api/v1/reservations/{id}
   */
  async getById(id: number): Promise<Reservation> {
    const response = await api.get<ApiResponse<unknown>>(`/api/v1/reservations/${id}`);
    return mapReservation(response.data.data as Parameters<typeof mapReservation>[0]);
  },

  /**
   * POST /api/v1/reservations — body: CreateReservationRequestDTO
   * { roomId: long, guestId: long, checkInDate, checkOutDate, discountId?, specialRequests? }
   */
  async create(reservation: CreateReservationRequest): Promise<Reservation> {
    const response = await api.post<ApiResponse<unknown>>('/api/v1/reservations', reservation);
    return mapReservation(response.data.data as Parameters<typeof mapReservation>[0]);
  },

  /**
   * PUT /api/v1/reservations — body: UpdateReservationRequestDTO (includes id)
   */
  async update(reservation: { id: number; [key: string]: unknown }): Promise<Reservation> {
    const response = await api.put<ApiResponse<unknown>>('/api/v1/reservations', reservation);
    return mapReservation(response.data.data as Parameters<typeof mapReservation>[0]);
  },

  /**
   * POST /api/v1/reservations/{id}/cancel
   */
  async cancel(id: number): Promise<Reservation> {
    const response = await api.post<ApiResponse<unknown>>(`/api/v1/reservations/${id}/cancel`);
    return mapReservation(response.data.data as Parameters<typeof mapReservation>[0]);
  },

  /**
   * POST /api/v1/reservations/{id}/checkin
   */
  async checkIn(id: number): Promise<Reservation> {
    const response = await api.post<ApiResponse<unknown>>(`/api/v1/reservations/${id}/checkin`);
    return mapReservation(response.data.data as Parameters<typeof mapReservation>[0]);
  },

  /**
   * POST /api/v1/reservations/{id}/checkout
   */
  async checkOut(id: number): Promise<Reservation> {
    const response = await api.post<ApiResponse<unknown>>(`/api/v1/reservations/${id}/checkout`);
    return mapReservation(response.data.data as Parameters<typeof mapReservation>[0]);
  },

  /**
   * PUT /api/v1/reservations/{id}/override-status — admin override, bypasses state machine
   */
  async updateStatus(id: number, status: string): Promise<Reservation> {
    const response = await api.put<ApiResponse<unknown>>(`/api/v1/reservations/${id}/override-status`, JSON.stringify(status), {
      headers: { 'Content-Type': 'application/json' },
    });
    return mapReservation(response.data.data as Parameters<typeof mapReservation>[0]);
  },
};

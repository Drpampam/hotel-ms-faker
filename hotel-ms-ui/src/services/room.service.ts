import api from '../lib/axios';
import type { Room, AddRoomRequest, UpdateRoomRequest, ApiResponse, PageApiResponse, RoomTrigger } from '../types';

// Map backend RoomResponseDTO to UI Room shape
function mapRoom(r: {
  id: number;
  number?: string;
  type?: string;
  capacity: number;
  pricePerNight: number;
  isAvailable: boolean;
  roomState?: string;
  propertyId: number;
  createdBy?: string;
  creationDate?: string;
  lastModifiedDate?: string;
}): Room {
  const stateToStatus = (state: string | undefined): Room['status'] => {
    switch (state) {
      case 'Available': return 'Available';
      case 'Occupied': return 'Occupied';
      case 'Maintenance': return 'Maintenance';
      case 'Cleaning': return 'Cleaning';
      default: return r.isAvailable ? 'Available' : 'Occupied';
    }
  };

  return {
    ...r,
    roomNumber: r.number ?? String(r.id),
    status: stateToStatus(r.roomState),
    roomState: (r.roomState as Room['roomState']) ?? 'Available',
    createdAt: r.creationDate,
  } as Room;
}

export const roomService = {
  /**
   * GET /api/v1/rooms
   * Query: propertyId, type, isAvailable, maxPrice, pageNumber, pageSize
   */
  async getAll(params?: {
    propertyId?: number;
    type?: string;
    isAvailable?: boolean;
    maxPrice?: number;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<Room[]> {
    const response = await api.get<PageApiResponse<unknown[]>>('/api/v1/rooms', { params: { pageSize: 100, ...params } });
    const raw = response.data?.data;
    if (!Array.isArray(raw)) return [];
    return raw.map((r) => mapRoom(r as Parameters<typeof mapRoom>[0]));
  },

  /**
   * GET /api/v1/rooms/{id}
   */
  async getById(id: number): Promise<Room> {
    const response = await api.get<ApiResponse<unknown>>(`/api/v1/rooms/${id}`);
    return mapRoom(response.data.data as Parameters<typeof mapRoom>[0]);
  },

  /**
   * POST /api/v1/rooms — body: AddRoomRequestDTO
   */
  async create(room: AddRoomRequest): Promise<Room> {
    const response = await api.post<ApiResponse<unknown>>('/api/v1/rooms', room);
    return mapRoom(response.data.data as Parameters<typeof mapRoom>[0]);
  },

  /**
   * PUT /api/v1/rooms — body: UpdateRoomRequestDTO (includes id)
   */
  async update(room: UpdateRoomRequest): Promise<Room> {
    const response = await api.put<ApiResponse<unknown>>('/api/v1/rooms', room);
    return mapRoom(response.data.data as Parameters<typeof mapRoom>[0]);
  },

  /**
   * DELETE /api/v1/rooms/{id}
   */
  async delete(id: number): Promise<void> {
    await api.delete(`/api/v1/rooms/${id}`);
  },

  /**
   * PATCH /api/v1/rooms/{id}/state — body: RoomTrigger (enum string)
   * e.g. "MakeAvailable", "StartMaintenance", "StartCleaning"
   */
  async changeState(id: number, trigger: RoomTrigger): Promise<Room> {
    const response = await api.patch<ApiResponse<unknown>>(
      `/api/v1/rooms/${id}/state`,
      trigger,
      { headers: { 'Content-Type': 'application/json' } }
    );
    return mapRoom(response.data.data as Parameters<typeof mapRoom>[0]);
  },

  /**
   * GET /api/v1/rooms/{id}/state
   */
  async getState(id: number) {
    const response = await api.get(`/api/v1/rooms/${id}/state`);
    return response.data;
  },

  /**
   * GET /api/v1/rooms/{id}/triggers
   */
  async getAvailableTriggers(id: number): Promise<RoomTrigger[]> {
    const response = await api.get<ApiResponse<RoomTrigger[]>>(`/api/v1/rooms/${id}/triggers`);
    return response.data?.data ?? [];
  },

  // Legacy compatibility shim used by pages
  async updateStatus(id: number, status: string): Promise<Room> {
    const statusToTrigger = (s: string): RoomTrigger => {
      switch (s) {
        case 'Occupied': return 'CheckIn';
        case 'Available': return 'CheckOut';
        case 'Maintenance': return 'SetMaintenance';
        case 'Cleaning': return 'SetCleaning';
        default: return 'CheckOut';
      }
    };
    return roomService.changeState(id, statusToTrigger(status));
  },
};

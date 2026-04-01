import api from '../lib/axios';
import type { HousekeepingTask, HousekeepingTaskTrigger, ApiResponse, PageApiResponse } from '../types';

function mapTask(r: {
  id: number;
  roomId: number;
  roomNumber?: string;
  assignedToUserId?: number;
  assignedToName?: string;
  taskType?: string;
  priority?: string;
  notes?: string;
  state?: string;
  availableTriggers?: HousekeepingTaskTrigger[];
  scheduledAt?: string;
  completedAt?: string;
  tenantId?: number;
  createdBy?: string;
  creationDate?: string;
}): HousekeepingTask {
  // Map backend state to UI status (backend: Done/Skipped, UI: Done/Skipped)
  const stateToStatus = (state: string | undefined): HousekeepingTask['status'] => {
    switch (state) {
      case 'Pending': return 'Pending';
      case 'InProgress': return 'InProgress';
      case 'Done': return 'Done';
      case 'Skipped': return 'Skipped';
      default: return 'Pending';
    }
  };

  return {
    ...r,
    status: stateToStatus(r.state),
    state: r.state as HousekeepingTask['state'],
    scheduledDate: r.scheduledAt,
    createdAt: r.creationDate ?? new Date().toISOString(),
  } as HousekeepingTask;
}

export const housekeepingService = {
  /**
   * GET /api/v1/housekeeping/tasks
   * Query: roomId, assignedToUserId, state, tenantId, fromDate, toDate, pageNumber, pageSize
   */
  async getTasks(params?: {
    roomId?: number;
    assignedToUserId?: number;
    state?: string;
    tenantId?: number;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<HousekeepingTask[]> {
    const response = await api.get<PageApiResponse<unknown[]>>('/api/v1/housekeeping/tasks', {
      params: { pageSize: 100, ...params },
    });
    const raw = response.data?.data;
    if (!Array.isArray(raw)) return [];
    return raw.map((r) => mapTask(r as Parameters<typeof mapTask>[0]));
  },

  /**
   * GET /api/v1/housekeeping/tasks/{id}
   */
  async getById(id: number): Promise<HousekeepingTask> {
    const response = await api.get<ApiResponse<unknown>>(`/api/v1/housekeeping/tasks/${id}`);
    return mapTask(response.data.data as Parameters<typeof mapTask>[0]);
  },

  /**
   * POST /api/v1/housekeeping/tasks
   * Body: { roomId, assignedToUserId?, taskType, priority, notes?, scheduledAt?, tenantId? }
   */
  async create(request: {
    roomId: number;
    assignedToUserId?: number;
    taskType: string;
    priority: string;
    notes?: string;
    scheduledAt?: string;
    tenantId?: number;
  }): Promise<HousekeepingTask> {
    const response = await api.post<ApiResponse<unknown>>('/api/v1/housekeeping/tasks', request);
    return mapTask(response.data.data as Parameters<typeof mapTask>[0]);
  },

  /**
   * PATCH /api/v1/housekeeping/tasks/{id}/state
   * Body: HousekeepingTaskTrigger (enum string: "Start" | "Complete" | "Skip")
   */
  async changeState(id: number, trigger: HousekeepingTaskTrigger): Promise<HousekeepingTask> {
    const response = await api.patch<ApiResponse<unknown>>(
      `/api/v1/housekeeping/tasks/${id}/state`,
      JSON.stringify(trigger),
      { headers: { 'Content-Type': 'application/json' } }
    );
    return mapTask(response.data.data as Parameters<typeof mapTask>[0]);
  },

  /**
   * GET /api/v1/housekeeping/schedule?tenantId=&date=
   */
  async getDailySchedule(tenantId: number, date?: string): Promise<HousekeepingTask[]> {
    const response = await api.get<ApiResponse<unknown[]>>('/api/v1/housekeeping/schedule', {
      params: { tenantId, date: date ?? new Date().toISOString() },
    });
    const raw = response.data?.data;
    if (!Array.isArray(raw)) return [];
    return raw.map((r) => mapTask(r as Parameters<typeof mapTask>[0]));
  },
};

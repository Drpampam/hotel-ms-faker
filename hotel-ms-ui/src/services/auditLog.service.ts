import api from '../lib/axios';
import type { PageApiResponse, AuditLog } from '../types';

function mapAuditLog(raw: unknown): AuditLog {
  const r = raw as Record<string, unknown>;
  return {
    id: r.id as number,
    action: (r.action as string) ?? '',
    performedBy: (r.performedBy as string) ?? '',
    performerEmail: (r.performerEmail as string) ?? '',
    performedAgainst: r.performedAgainst as string | undefined,
    ipAddress: r.ipAddress as string | undefined,
    datePerformed: (r.datePerformed as string) ?? new Date().toISOString(),
  };
}

export const auditLogService = {
  async getAll(params?: { pageNumber?: number; pageSize?: number }): Promise<AuditLog[]> {
    const res = await api.get<PageApiResponse<unknown[]>>('/api/v1/audit-logs', {
      params: { pageSize: 100, pageNumber: 1, ...params },
    });
    const raw = res.data?.data;
    return Array.isArray(raw) ? raw.map(mapAuditLog) : [];
  },
};

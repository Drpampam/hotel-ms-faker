import api from '../lib/axios';
import type { ApiResponse } from '../types';

export interface OccupancyReport {
  totalRooms: number;
  occupiedRooms: number;
  availableRooms: number;
  cleaningRooms: number;
  maintenanceRooms: number;
  occupancyRate: number;
  fromDate: string;
  toDate: string;
}

export interface RevenueSummary {
  totalRevenue: number;
  roomRevenue: number;
  taxCollected: number;
  totalDiscountsApplied: number;
  paidInvoicesCount: number;
  pendingInvoicesCount: number;
  fromDate: string;
  toDate: string;
}

export interface ReservationStats {
  totalReservations: number;
  pendingReservations: number;
  confirmedReservations: number;
  checkedInCount: number;
  checkedOutCount: number;
  cancelledCount: number;
  noShowCount: number;
  averageStayDays: number;
  fromDate: string;
  toDate: string;
}

export interface HousekeepingStats {
  totalTasks: number;
  pendingTasks: number;
  inProgressTasks: number;
  completedTasks: number;
  skippedTasks: number;
  completionRate: number;
  date: string;
}

export interface PaymentMethodSummary {
  method: string;
  count: number;
  amount: number;
}

export interface PaymentBreakdown {
  totalPayments: number;
  totalAmount: number;
  byMethod: PaymentMethodSummary[];
  fromDate: string;
  toDate: string;
}

export interface FrontDeskSummary {
  date: string;
  expectedArrivals: number;
  actualCheckIns: number;
  expectedDepartures: number;
  actualCheckOuts: number;
  currentlyOccupied: number;
  pendingServiceRequests: number;
  pendingHousekeepingTasks: number;
}

function toUtcParam(dateStr: string): string {
  return new Date(dateStr).toISOString();
}

export const reportService = {
  async getOccupancy(from: string, to: string, propertyId?: number): Promise<OccupancyReport> {
    const response = await api.get<ApiResponse<OccupancyReport>>('/api/v1/reports/occupancy', {
      params: { fromDate: toUtcParam(from), toDate: toUtcParam(to), propertyId },
    });
    return response.data.data;
  },

  async getRevenue(from: string, to: string, propertyId?: number): Promise<RevenueSummary> {
    const response = await api.get<ApiResponse<RevenueSummary>>('/api/v1/reports/revenue', {
      params: { fromDate: toUtcParam(from), toDate: toUtcParam(to), propertyId },
    });
    return response.data.data;
  },

  async getReservationStats(from: string, to: string, propertyId?: number): Promise<ReservationStats> {
    const response = await api.get<ApiResponse<ReservationStats>>('/api/v1/reports/reservations', {
      params: { fromDate: toUtcParam(from), toDate: toUtcParam(to), propertyId },
    });
    return response.data.data;
  },

  async getHousekeepingStats(date: string): Promise<HousekeepingStats> {
    const response = await api.get<ApiResponse<HousekeepingStats>>('/api/v1/reports/housekeeping', {
      params: { date: toUtcParam(date) },
    });
    return response.data.data;
  },

  async getPaymentBreakdown(from: string, to: string): Promise<PaymentBreakdown> {
    const response = await api.get<ApiResponse<PaymentBreakdown>>('/api/v1/reports/payments', {
      params: { fromDate: toUtcParam(from), toDate: toUtcParam(to) },
    });
    return response.data.data;
  },

  async getFrontDeskSummary(date?: string): Promise<FrontDeskSummary> {
    const response = await api.get<ApiResponse<FrontDeskSummary>>('/api/v1/reports/front-desk', {
      params: date ? { date: toUtcParam(date) } : {},
    });
    return response.data.data;
  },
};

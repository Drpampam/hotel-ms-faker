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

export interface ExpenseReportItem {
  id: number;
  reservationId: number;
  guestName: string | null;
  guestEmail: string | null;
  roomNumber: string | null;
  description: string;
  category: string | null;
  quantity: number;
  unitPrice: number;
  amount: number;
  createdBy: string | null;
  creationDate: string;
}

export interface ExpenseCategorySummary {
  category: string;
  count: number;
  amount: number;
}

export interface ExpenseReport {
  items: ExpenseReportItem[];
  totalItems: number;
  totalAmount: number;
  byCategory: ExpenseCategorySummary[];
  fromDate: string;
  toDate: string;
}

function toUtcStartOfDay(dateStr: string): string {
  const d = new Date(dateStr);
  d.setUTCHours(0, 0, 0, 0);
  return d.toISOString();
}

function toUtcEndOfDay(dateStr: string): string {
  const d = new Date(dateStr);
  d.setUTCHours(23, 59, 59, 999);
  return d.toISOString();
}

export const reportService = {
  async getOccupancy(from: string, to: string, propertyId?: number): Promise<OccupancyReport> {
    const response = await api.get<ApiResponse<OccupancyReport>>('/api/v1/reports/occupancy', {
      params: { fromDate: toUtcStartOfDay(from), toDate: toUtcEndOfDay(to), propertyId },
    });
    return response.data.data;
  },

  async getRevenue(from: string, to: string, propertyId?: number): Promise<RevenueSummary> {
    const response = await api.get<ApiResponse<RevenueSummary>>('/api/v1/reports/revenue', {
      params: { fromDate: toUtcStartOfDay(from), toDate: toUtcEndOfDay(to), propertyId },
    });
    return response.data.data;
  },

  async getReservationStats(from: string, to: string, propertyId?: number): Promise<ReservationStats> {
    const response = await api.get<ApiResponse<ReservationStats>>('/api/v1/reports/reservations', {
      params: { fromDate: toUtcStartOfDay(from), toDate: toUtcEndOfDay(to), propertyId },
    });
    return response.data.data;
  },

  async getHousekeepingStats(date: string): Promise<HousekeepingStats> {
    const response = await api.get<ApiResponse<HousekeepingStats>>('/api/v1/reports/housekeeping', {
      params: { date: toUtcStartOfDay(date) },
    });
    return response.data.data;
  },

  async getPaymentBreakdown(from: string, to: string): Promise<PaymentBreakdown> {
    const response = await api.get<ApiResponse<PaymentBreakdown>>('/api/v1/reports/payments', {
      params: { fromDate: toUtcStartOfDay(from), toDate: toUtcEndOfDay(to) },
    });
    return response.data.data;
  },

  async getFrontDeskSummary(date?: string): Promise<FrontDeskSummary> {
    const response = await api.get<ApiResponse<FrontDeskSummary>>('/api/v1/reports/front-desk', {
      params: date ? { date: toUtcStartOfDay(date) } : {},
    });
    return response.data.data;
  },

  async getExpenseReport(from: string, to: string, search?: string, reservationId?: number): Promise<ExpenseReport> {
    const response = await api.get<ApiResponse<ExpenseReport>>('/api/v1/reports/expenses', {
      params: {
        fromDate: toUtcStartOfDay(from),
        toDate: toUtcEndOfDay(to),
        ...(search ? { search } : {}),
        ...(reservationId ? { reservationId } : {}),
      },
    });
    return response.data.data;
  },
};

import api from '../lib/axios';
import type { ApiResponse, PageApiResponse, Invoice } from '../types';

function mapInvoice(raw: unknown): Invoice {
  const r = raw as Record<string, unknown>;
  return {
    id: r.id as number,
    invoiceNumber: (r.invoiceNumber as string) ?? '',
    reservationId: r.reservationId as number,
    guestId: r.guestId as number,
    guestName: r.guestName as string | undefined,
    guestEmail: r.guestEmail as string | undefined,
    issueDate: (r.issueDate as string) ?? new Date().toISOString(),
    dueDate: (r.dueDate as string) ?? new Date().toISOString(),
    subTotal: (r.subTotal as number) ?? 0,
    taxAmount: (r.taxAmount as number) ?? 0,
    discountAmount: (r.discountAmount as number) ?? 0,
    totalAmount: (r.totalAmount as number) ?? 0,
    status: (r.status as Invoice['status']) ?? 'Issued',
    lineItems: Array.isArray(r.lineItems) ? r.lineItems.map((l: unknown) => {
      const li = l as Record<string, unknown>;
      return {
        id: li.id as number,
        description: (li.description as string) ?? '',
        category: (li.category as string) ?? '',
        quantity: (li.quantity as number) ?? 1,
        unitPrice: (li.unitPrice as number) ?? 0,
        amount: (li.amount as number) ?? 0,
      };
    }) : [],
    creationDate: (r.creationDate as string) ?? new Date().toISOString(),
  };
}

export const billingService = {
  async generateInvoice(reservationId: number): Promise<Invoice> {
    const res = await api.post<ApiResponse<unknown>>(`/api/v1/billing/invoices/generate/${reservationId}`);
    return mapInvoice(res.data.data);
  },

  async getByReservation(reservationId: number): Promise<Invoice | null> {
    try {
      const res = await api.get<ApiResponse<unknown>>(`/api/v1/billing/invoices/by-reservation/${reservationId}`);
      return mapInvoice(res.data.data);
    } catch {
      return null;
    }
  },

  async getById(invoiceId: number): Promise<Invoice> {
    const res = await api.get<ApiResponse<unknown>>(`/api/v1/billing/invoices/${invoiceId}`);
    return mapInvoice(res.data.data);
  },

  async getAll(params?: { status?: string; guestId?: number; fromDate?: string; toDate?: string; pageNumber?: number; pageSize?: number }): Promise<{ data: Invoice[]; total: number }> {
    const res = await api.get<PageApiResponse<unknown[]>>('/api/v1/billing/invoices', { params: { pageSize: 50, pageNumber: 1, ...params } });
    const raw = res.data?.data;
    return {
      data: Array.isArray(raw) ? raw.map(mapInvoice) : [],
      total: res.data?.totalPageCount ?? 0,
    };
  },

  async markPaid(invoiceId: number): Promise<Invoice> {
    const res = await api.post<ApiResponse<unknown>>(`/api/v1/billing/invoices/${invoiceId}/mark-paid`);
    return mapInvoice(res.data.data);
  },

  async voidInvoice(invoiceId: number): Promise<Invoice> {
    const res = await api.post<ApiResponse<unknown>>(`/api/v1/billing/invoices/${invoiceId}/void`);
    return mapInvoice(res.data.data);
  },
};

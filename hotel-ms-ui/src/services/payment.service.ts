import api from '../lib/axios';
import type { ApiResponse, PageApiResponse, Payment, CapturePaymentRequest } from '../types';

function mapPayment(p: unknown): Payment {
  const r = p as Record<string, unknown>;
  return {
    id: r.id as number,
    reservationId: r.reservationId as number,
    paymentMethod: (r.paymentMethod as string) ?? '',
    amount: (r.amount as number) ?? 0,
    isSuccessful: (r.isSuccessful as boolean) ?? false,
    transactionId: r.transactionId as string | undefined,
    paymentDate: (r.paymentDate as string) ?? new Date().toISOString(),
    paymentState: (r.paymentState as Payment['paymentState']) ?? 'Pending',
  };
}

export const paymentService = {
  /**
   * POST /api/v1/payments/capture
   * Creates and immediately completes a payment (used at checkout).
   */
  async capture(request: CapturePaymentRequest): Promise<Payment> {
    const response = await api.post<ApiResponse<unknown>>('/api/v1/payments/capture', request);
    return mapPayment(response.data.data);
  },

  /**
   * GET /api/v1/payments?reservationId={id}
   * Returns all payments for a given reservation.
   */
  async getByReservation(reservationId: number): Promise<Payment[]> {
    const response = await api.get<PageApiResponse<unknown[]>>('/api/v1/payments', {
      params: { reservationId, pageSize: 50 },
    });
    const raw = response.data?.data;
    if (!Array.isArray(raw)) return [];
    return raw.map(mapPayment);
  },

  /**
   * PATCH /api/v1/payments/{id}/state with trigger "Refund"
   */
  async refund(paymentId: number): Promise<Payment> {
    const response = await api.patch<ApiResponse<unknown>>(
      `/api/v1/payments/${paymentId}/state`,
      JSON.stringify('Refund'),
      { headers: { 'Content-Type': 'application/json' } }
    );
    return mapPayment(response.data.data);
  },
};

using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface INotificationService : IAutoDependencyService
    {
        // ── Guest-facing ─────────────────────────────────────────────────────
        Task SendReservationConfirmedAsync(Reservation reservation, string guestEmail, string guestName);
        Task SendCheckInWelcomeAsync(Reservation reservation, string guestEmail, string guestName);
        Task SendCheckOutSummaryAsync(Reservation reservation, string guestEmail, string guestName);
        Task SendServiceRequestCompletedAsync(ServiceRequest serviceRequest, string guestEmail, string guestName);
        Task SendPasswordResetAsync(string toEmail, string fullName, string resetLink);

        // ── Hotel company-facing ──────────────────────────────────────────────
        Task SendNewBookingAlertAsync(Reservation reservation, string guestName, string guestEmail, string roomNumber);
        Task SendPaymentCompletedAlertAsync(Payment payment, Reservation reservation, string guestName, string guestEmail);
    }
}

using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface INotificationService : IAutoDependencyService
    {
        Task SendReservationConfirmedAsync(Reservation reservation, string guestEmail, string guestName);
        Task SendCheckInWelcomeAsync(Reservation reservation, string guestEmail, string guestName);
        Task SendCheckOutSummaryAsync(Reservation reservation, string guestEmail, string guestName);
        Task SendServiceRequestCompletedAsync(ServiceRequest serviceRequest, string guestEmail, string guestName);
    }
}

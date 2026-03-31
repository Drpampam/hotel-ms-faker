using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;

namespace hotelier_core_app.Service.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;

        public NotificationService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendReservationConfirmedAsync(Reservation reservation, string guestEmail, string guestName)
        {
            var subject = $"Reservation Confirmed — Booking #{reservation.Id}";
            var body = $@"
                <h2>Your reservation is confirmed!</h2>
                <p>Dear {guestEmail},</p>
                <p>We are pleased to confirm your reservation.</p>
                <table>
                    <tr><td><strong>Booking ID:</strong></td><td>{reservation.Id}</td></tr>
                    <tr><td><strong>Check-In:</strong></td><td>{reservation.CheckInDate:dd MMM yyyy}</td></tr>
                    <tr><td><strong>Check-Out:</strong></td><td>{reservation.CheckOutDate:dd MMM yyyy}</td></tr>
                    <tr><td><strong>Total Price:</strong></td><td>${reservation.TotalPrice:F2}</td></tr>
                </table>
                <p>We look forward to welcoming you. Please contact us if you have any special requests.</p>
                <p>Warm regards,<br/>Hotelier Team</p>";

            await _emailService.SendEmail(new SendEmailDTO(
                new List<string> { guestEmail },
                subject,
                body
            ));
        }

        public async Task SendCheckInWelcomeAsync(Reservation reservation, string guestEmail, string guestName)
        {
            var subject = "Welcome! You're Now Checked In";
            var body = $@"
                <h2>Welcome, {guestName}!</h2>
                <p>Your check-in has been completed successfully.</p>
                <table>
                    <tr><td><strong>Booking ID:</strong></td><td>{reservation.Id}</td></tr>
                    <tr><td><strong>Check-In Date:</strong></td><td>{reservation.CheckInDate:dd MMM yyyy}</td></tr>
                    <tr><td><strong>Check-Out Date:</strong></td><td>{reservation.CheckOutDate:dd MMM yyyy}</td></tr>
                </table>
                <p>If you need anything during your stay, please don't hesitate to contact our front desk.</p>
                <p>Enjoy your stay!<br/>Hotelier Team</p>";

            await _emailService.SendEmail(new SendEmailDTO(
                new List<string> { guestEmail },
                subject,
                body
            ));
        }

        public async Task SendCheckOutSummaryAsync(Reservation reservation, string guestEmail, string guestName)
        {
            var nights = (int)(reservation.CheckOutDate - reservation.CheckInDate).TotalDays;
            var subject = $"Thank You for Your Stay — Booking #{reservation.Id}";
            var body = $@"
                <h2>Thank You for Staying with Us, {guestName}!</h2>
                <p>We hope you enjoyed your {nights}-night stay.</p>
                <table>
                    <tr><td><strong>Booking ID:</strong></td><td>{reservation.Id}</td></tr>
                    <tr><td><strong>Check-In:</strong></td><td>{reservation.CheckInDate:dd MMM yyyy}</td></tr>
                    <tr><td><strong>Check-Out:</strong></td><td>{reservation.CheckOutDate:dd MMM yyyy}</td></tr>
                    <tr><td><strong>Total Charged:</strong></td><td>${reservation.TotalPrice:F2}</td></tr>
                </table>
                <p>We would love to see you again. Visit us soon!</p>
                <p>Warm regards,<br/>Hotelier Team</p>";

            await _emailService.SendEmail(new SendEmailDTO(
                new List<string> { guestEmail },
                subject,
                body
            ));
        }

        public async Task SendServiceRequestCompletedAsync(ServiceRequest serviceRequest, string guestEmail, string guestName)
        {
            var subject = "Your Service Request Has Been Completed";
            var body = $@"
                <h2>Service Request Completed</h2>
                <p>Dear {guestName},</p>
                <p>Your service request has been fulfilled.</p>
                <table>
                    <tr><td><strong>Request ID:</strong></td><td>{serviceRequest.Id}</td></tr>
                    <tr><td><strong>Service Type:</strong></td><td>{serviceRequest.ServiceType}</td></tr>
                </table>
                <p>Thank you for letting us serve you.</p>
                <p>Hotelier Team</p>";

            await _emailService.SendEmail(new SendEmailDTO(
                new List<string> { guestEmail },
                subject,
                body
            ));
        }
    }
}

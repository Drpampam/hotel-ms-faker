using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.Extensions.Configuration;

namespace hotelier_core_app.Service.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;
        private readonly string _companyEmail;
        private readonly string _companyName;

        public NotificationService(IEmailService emailService, IConfiguration configuration)
        {
            _emailService = emailService;
            _companyEmail = configuration["HotelSettings:CompanyEmail"] ?? "johnomotoye@gmail.com";
            _companyName = configuration["HotelSettings:CompanyName"] ?? "HotelMS";
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string EmailWrapper(string bodyContent) => $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset=""utf-8""/>
  <meta name=""viewport"" content=""width=device-width, initial-scale=1""/>
</head>
<body style=""margin:0;padding:0;background-color:#f4f6f9;font-family:Arial,Helvetica,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f4f6f9;padding:30px 0;"">
    <tr><td align=""center"">
      <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);"">
        <!-- Header -->
        <tr>
          <td style=""background:#4f46e5;padding:24px 32px;"">
            <h1 style=""margin:0;color:#ffffff;font-size:22px;font-weight:700;"">
              Hotel<span style=""color:#a5b4fc;"">MS</span>
            </h1>
          </td>
        </tr>
        <!-- Body -->
        <tr>
          <td style=""padding:32px;"">
            {bodyContent}
          </td>
        </tr>
        <!-- Footer -->
        <tr>
          <td style=""background:#f8fafc;border-top:1px solid #e2e8f0;padding:16px 32px;text-align:center;"">
            <p style=""margin:0;color:#94a3b8;font-size:12px;"">
              This is an automated message from HotelMS. Please do not reply to this email.
            </p>
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";

        private static string InfoRow(string label, string value) =>
            $@"<tr>
                <td style=""padding:8px 12px;color:#64748b;font-size:14px;border-bottom:1px solid #f1f5f9;width:40%;""><strong>{label}</strong></td>
                <td style=""padding:8px 12px;color:#1e293b;font-size:14px;border-bottom:1px solid #f1f5f9;"">{value}</td>
               </tr>";

        private static string InfoTable(string rows) =>
            $@"<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border:1px solid #e2e8f0;border-radius:6px;margin:16px 0;"">
                {rows}
               </table>";

        private Task Send(IEnumerable<string> recipients, string subject, string bodyContent) =>
            _emailService.SendEmail(new SendEmailDTO(recipients.ToList(), subject, EmailWrapper(bodyContent)));

        // ── Guest-facing ──────────────────────────────────────────────────────

        public Task SendReservationConfirmedAsync(Reservation reservation, string guestEmail, string guestName)
        {
            int nights = (int)(reservation.CheckOutDate.Date - reservation.CheckInDate.Date).TotalDays;
            var body = $@"
                <h2 style=""color:#1e293b;font-size:20px;margin:0 0 8px 0;"">Reservation Confirmed ✓</h2>
                <p style=""color:#64748b;margin:0 0 20px 0;"">Dear {guestName}, your reservation has been confirmed. We look forward to welcoming you!</p>
                {InfoTable(
                    InfoRow("Booking ID", $"#RES-{reservation.Id}") +
                    InfoRow("Check-In", reservation.CheckInDate.ToString("dddd, dd MMM yyyy")) +
                    InfoRow("Check-Out", reservation.CheckOutDate.ToString("dddd, dd MMM yyyy")) +
                    InfoRow("Duration", $"{nights} night{(nights != 1 ? "s" : "")}") +
                    InfoRow("Total Amount", $"${reservation.TotalPrice:F2}")
                )}
                {(reservation.SpecialRequests != null ? $@"<p style=""background:#f0f9ff;border-left:3px solid #4f46e5;padding:12px 16px;border-radius:0 6px 6px 0;color:#334155;font-size:14px;""><strong>Special Requests:</strong> {reservation.SpecialRequests}</p>" : "")}
                <p style=""color:#64748b;font-size:14px;margin-top:24px;"">If you have any questions or need to modify your booking, please contact our front desk.</p>
                <p style=""color:#64748b;font-size:14px;"">Warm regards,<br/><strong>The {_companyName} Team</strong></p>";

            return Send([guestEmail], $"Reservation Confirmed — Booking #RES-{reservation.Id}", body);
        }

        public Task SendCheckInWelcomeAsync(Reservation reservation, string guestEmail, string guestName)
        {
            var body = $@"
                <h2 style=""color:#1e293b;font-size:20px;margin:0 0 8px 0;"">Welcome, {guestName}! 🎉</h2>
                <p style=""color:#64748b;margin:0 0 20px 0;"">Your check-in is complete. We hope you have a wonderful stay with us.</p>
                {InfoTable(
                    InfoRow("Booking ID", $"#RES-{reservation.Id}") +
                    InfoRow("Check-In", reservation.CheckInDate.ToString("dddd, dd MMM yyyy")) +
                    InfoRow("Check-Out", reservation.CheckOutDate.ToString("dddd, dd MMM yyyy"))
                )}
                <div style=""background:#f0fdf4;border:1px solid #bbf7d0;border-radius:6px;padding:16px;margin:16px 0;"">
                  <p style=""margin:0;color:#166534;font-size:14px;"">
                    <strong>Need assistance?</strong> Our front desk team is available 24/7. Don't hesitate to reach out for any requests during your stay.
                  </p>
                </div>
                <p style=""color:#64748b;font-size:14px;"">Enjoy your stay!<br/><strong>The {_companyName} Team</strong></p>";

            return Send([guestEmail], "Welcome! You're Now Checked In", body);
        }

        public Task SendCheckOutSummaryAsync(Reservation reservation, string guestEmail, string guestName)
        {
            int nights = (int)(reservation.CheckOutDate.Date - reservation.CheckInDate.Date).TotalDays;
            var body = $@"
                <h2 style=""color:#1e293b;font-size:20px;margin:0 0 8px 0;"">Thank You for Staying with Us!</h2>
                <p style=""color:#64748b;margin:0 0 20px 0;"">Dear {guestName}, we hope you had a wonderful {nights}-night stay. It was a pleasure hosting you.</p>
                {InfoTable(
                    InfoRow("Booking ID", $"#RES-{reservation.Id}") +
                    InfoRow("Check-In", reservation.CheckInDate.ToString("dd MMM yyyy")) +
                    InfoRow("Check-Out", reservation.CheckOutDate.ToString("dd MMM yyyy")) +
                    InfoRow("Nights Stayed", nights.ToString()) +
                    InfoRow("Total Charged", $"${reservation.TotalPrice:F2}")
                )}
                <p style=""color:#64748b;font-size:14px;margin-top:16px;"">We would love to see you again. Visit us soon!</p>
                <p style=""color:#64748b;font-size:14px;"">Warm regards,<br/><strong>The {_companyName} Team</strong></p>";

            return Send([guestEmail], $"Thank You for Your Stay — Booking #RES-{reservation.Id}", body);
        }

        public Task SendServiceRequestCompletedAsync(ServiceRequest serviceRequest, string guestEmail, string guestName)
        {
            var body = $@"
                <h2 style=""color:#1e293b;font-size:20px;margin:0 0 8px 0;"">Service Request Completed ✓</h2>
                <p style=""color:#64748b;margin:0 0 20px 0;"">Dear {guestName}, your service request has been fulfilled by our team.</p>
                {InfoTable(
                    InfoRow("Request ID", $"#SR-{serviceRequest.Id}") +
                    InfoRow("Service Type", serviceRequest.ServiceType)
                )}
                <p style=""color:#64748b;font-size:14px;"">Thank you for letting us serve you.<br/><strong>The {_companyName} Team</strong></p>";

            return Send([guestEmail], "Your Service Request Has Been Completed", body);
        }

        public Task SendPasswordResetAsync(string toEmail, string fullName, string resetLink)
        {
            var body = $@"
                <h2 style=""color:#1e293b;font-size:20px;margin:0 0 8px 0;"">Reset Your Password</h2>
                <p style=""color:#64748b;margin:0 0 20px 0;"">Hi {fullName}, we received a request to reset the password for your account.</p>
                <div style=""text-align:center;margin:24px 0;"">
                  <a href=""{resetLink}"" style=""display:inline-block;background:#4f46e5;color:#ffffff;text-decoration:none;padding:12px 32px;border-radius:6px;font-size:15px;font-weight:600;"">
                    Reset Password
                  </a>
                </div>
                <p style=""color:#94a3b8;font-size:13px;"">This link expires in <strong>1 hour</strong>. If you did not request a password reset, you can safely ignore this email — your password will not change.</p>
                <p style=""color:#94a3b8;font-size:12px;word-break:break-all;"">Or copy this link into your browser:<br/>{resetLink}</p>";

            return Send([toEmail], "Reset Your Password — HotelMS", body);
        }

        // ── Hotel company-facing ──────────────────────────────────────────────

        public Task SendNewBookingAlertAsync(Reservation reservation, string guestName, string guestEmail, string roomNumber)
        {
            int nights = (int)(reservation.CheckOutDate.Date - reservation.CheckInDate.Date).TotalDays;
            var body = $@"
                <h2 style=""color:#1e293b;font-size:20px;margin:0 0 8px 0;"">New Booking Received 🏨</h2>
                <p style=""color:#64748b;margin:0 0 20px 0;"">A new reservation has been confirmed on the system.</p>
                {InfoTable(
                    InfoRow("Booking ID", $"#RES-{reservation.Id}") +
                    InfoRow("Guest", guestName) +
                    InfoRow("Guest Email", guestEmail) +
                    InfoRow("Room", roomNumber) +
                    InfoRow("Check-In", reservation.CheckInDate.ToString("dddd, dd MMM yyyy")) +
                    InfoRow("Check-Out", reservation.CheckOutDate.ToString("dddd, dd MMM yyyy")) +
                    InfoRow("Nights", nights.ToString()) +
                    InfoRow("Total Amount", $"${reservation.TotalPrice:F2}") +
                    (reservation.SpecialRequests != null ? InfoRow("Special Requests", reservation.SpecialRequests) : "") +
                    InfoRow("Booked By", reservation.CreatedBy ?? "System")
                )}
                <p style=""color:#64748b;font-size:14px;"">Log in to the management system to view full details.</p>";

            return Send([_companyEmail], $"New Booking — {guestName} / Room {roomNumber} / {reservation.CheckInDate:dd MMM yyyy}", body);
        }

        public Task SendPaymentCompletedAlertAsync(Payment payment, Reservation reservation, string guestName, string guestEmail)
        {
            int nights = (int)(reservation.CheckOutDate.Date - reservation.CheckInDate.Date).TotalDays;
            var body = $@"
                <h2 style=""color:#1e293b;font-size:20px;margin:0 0 8px 0;"">Payment Received ✓</h2>
                <p style=""color:#64748b;margin:0 0 20px 0;"">A payment has been successfully completed for the following reservation.</p>
                {InfoTable(
                    InfoRow("Payment ID", $"#PAY-{payment.Id}") +
                    InfoRow("Booking ID", $"#RES-{reservation.Id}") +
                    InfoRow("Guest", guestName) +
                    InfoRow("Guest Email", guestEmail) +
                    InfoRow("Check-In", reservation.CheckInDate.ToString("dd MMM yyyy")) +
                    InfoRow("Check-Out", reservation.CheckOutDate.ToString("dd MMM yyyy")) +
                    InfoRow("Nights", nights.ToString()) +
                    InfoRow("Payment Method", payment.PaymentMethod) +
                    InfoRow("Amount Paid", $"${payment.Amount:F2}") +
                    InfoRow("Transaction ID", payment.TransactionId ?? "N/A") +
                    InfoRow("Payment Date", payment.PaymentDate.ToString("dd MMM yyyy HH:mm") + " UTC")
                )}
                <p style=""color:#64748b;font-size:14px;"">This payment has been recorded in the billing system.</p>";

            return Send([_companyEmail], $"Payment Received — {guestName} / ${payment.Amount:F2} / {payment.PaymentMethod}", body);
        }
    }
}

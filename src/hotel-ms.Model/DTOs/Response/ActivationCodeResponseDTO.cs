using hotelier_core_app.Core.Enums;

namespace hotelier_core_app.Model.DTOs.Response
{
    public class ActivationCodeResponseDTO
    {
        public string PlaintextCode { get; set; } = string.Empty;
        public string BoundToEmail { get; set; } = string.Empty;
        public PlanType PlanType { get; set; }
        public string PlanLabel { get; set; } = string.Empty;
    }

    public class ActivateTenantResponseDTO
    {
        public long TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public PlanType PlanType { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsUnlimited { get; set; }
    }

    public class SubscriptionStatusResponseDTO
    {
        public long TenantId { get; set; }
        public PlanType PlanType { get; set; }
        public string PlanLabel { get; set; } = string.Empty;
        public bool IsUnlimited { get; set; }
        public bool IsExpired { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? DaysRemaining { get; set; }
    }

    public class ProvisionTenantResponseDTO
    {
        public string Email { get; set; } = string.Empty;
        public string TempPassword { get; set; } = string.Empty;
        public string ActivationCode { get; set; } = string.Empty;
        public string PlanLabel { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    public class SelfRegisterResponseDTO
    {
        public string PlaintextCode { get; set; } = string.Empty;
        public string BoundToEmail { get; set; } = string.Empty;
        public string PlanLabel { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
    }

    public class ActivateMyAccountResponseDTO
    {
        public long TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string PlanLabel { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public bool IsUnlimited { get; set; }
    }

    public class TenantSummaryDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string PlanLabel { get; set; } = string.Empty;
        public PlanType? PlanType { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
        public bool IsUnlimited { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? DaysRemaining { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

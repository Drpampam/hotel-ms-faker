using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.Enums;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Migrations;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace hotelier_core_app.Service.Implementation
{
    public class ActivationService : IActivationService
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IDBCommandRepository<AuditLog> _auditLogRepository;
        private readonly IDBQueryRepository<Tenant> _tenantQueryRepository;
        private readonly IDBCommandRepository<Tenant> _tenantCommandRepository;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<ActivationService> _logger;

        public ActivationService(
            AppDbContext context,
            ITenantProvider tenantProvider,
            IDBCommandRepository<AuditLog> auditLogRepository,
            IDBQueryRepository<Tenant> tenantQueryRepository,
            IDBCommandRepository<Tenant> tenantCommandRepository,
            ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<ActivationService> logger)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _auditLogRepository = auditLogRepository;
            _tenantQueryRepository = tenantQueryRepository;
            _tenantCommandRepository = tenantCommandRepository;
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<BaseResponse<ActivationCodeResponseDTO>> GenerateActivationCodeAsync(
            GenerateActivationCodeRequestDTO request, AuditLog auditLog)
        {
            var plaintext = GeneratePlaintextCode();
            var hash = HashCode(plaintext);

            var existing = await _context.ActivationCodes
                .FirstOrDefaultAsync(c => c.BoundToEmail == request.Email.ToLower() && !c.IsUsed);
            if (existing != null)
            {
                existing.IsUsed = true;
                _context.ActivationCodes.Update(existing);
            }

            _context.ActivationCodes.Add(new ActivationCode
            {
                CodeHash = hash,
                PlanType = request.PlanType,
                BoundToEmail = request.Email.ToLower(),
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = auditLog.PerformedBy ?? "System"
            });

            await _context.SaveChangesAsync();
            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveAsync();

            return BaseResponse<ActivationCodeResponseDTO>.Success(new ActivationCodeResponseDTO
            {
                PlaintextCode = FormatCode(plaintext),
                BoundToEmail = request.Email,
                PlanType = request.PlanType,
                PlanLabel = PlanLabel(request.PlanType)
            }, ResponseMessages.ActivationCodeGenerated);
        }

        public async Task<BaseResponse<ActivateTenantResponseDTO>> ActivateTenantAsync(
            ActivateTenantRequestDTO request, string ipAddress)
        {
            var normalized = NormalizeCode(request.Code);
            var hash = HashCode(normalized);

            var code = await _context.ActivationCodes
                .FirstOrDefaultAsync(c => c.CodeHash == hash && !c.IsUsed);

            if (code == null)
                return BaseResponse<ActivateTenantResponseDTO>.Failure(null, ResponseMessages.ActivationCodeInvalid);

            if (!code.BoundToEmail.Equals(request.Email.ToLower(), StringComparison.OrdinalIgnoreCase))
                return BaseResponse<ActivateTenantResponseDTO>.Failure(null, ResponseMessages.ActivationCodeEmailMismatch);

            // 1 — Create Tenant in public schema
            _tenantProvider.SetSchema("public");
            var (startDate, endDate) = PlanDates(code.PlanType);

            var tenant = new Tenant
            {
                Name = request.TenantName,
                PlanType = code.PlanType,
                SubscriptionStartDate = startDate,
                SubscriptionEndDate = endDate,
                CreatedBy = request.Email,
                CreationDate = DateTime.UtcNow
            };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // 2 — Provision tenant schema (migrations)
            var schema = $"tenant_{tenant.Id}";
            _tenantProvider.SetSchema(schema);
            try
            {
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex) when (ex.GetBaseException().Message.Contains("already exists"))
            {
                _logger.LogWarning("Schema {Schema} already exists — stamping pending migrations.", schema);
                foreach (var mig in _context.Database.GetPendingMigrations())
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ({0}, {1}) ON CONFLICT DO NOTHING",
                        mig, "8.0.0");
                }
            }

            // 3 — Seed idempotent schema guards
            await _context.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""Shift""      varchar(50);
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""Department"" varchar(100);
            ");

            // 4 — Seed roles in tenant schema
            var roleNames = new[] { "SuperAdmin", "Admin", "FrontDesk", "Housekeeping", "Guest", "Developer" };
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roleName,
                        TenantId = tenant.Id,
                        CreationDate = DateTime.UtcNow,
                        CreatedBy = request.Email
                    });
                }
            }

            // 5 — Create SuperAdmin user
            var adminUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.AdminFullName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                Status = "Active",
                TenantId = tenant.Id,
                CreatedBy = "System",
                CreationDate = DateTime.UtcNow
            };
            var createResult = await _userManager.CreateAsync(adminUser, request.AdminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return BaseResponse<ActivateTenantResponseDTO>.Failure(null, errors);
            }

            var superAdminRole = await _roleManager.FindByNameAsync("SuperAdmin");
            if (superAdminRole != null)
            {
                _context.UserRoles.Add(new ApplicationUserRole
                {
                    UserId = adminUser.Id,
                    RoleId = superAdminRole.Id,
                    TenantId = tenant.Id
                });
                await _context.SaveChangesAsync();
            }

            // 6 — Mark code used
            code.IsUsed = true;
            code.UsedAt = DateTime.UtcNow;
            code.UsedByTenantId = tenant.Id;
            _context.ActivationCodes.Update(code);
            await _context.SaveChangesAsync();

            // 7 — Generate token
            var token = _tokenService.GenerateJSONWebToken(adminUser.FullName!, adminUser.Email!, new List<string> { "SuperAdmin" });
            var refreshToken = GenerateRefreshToken();
            adminUser.RefreshToken = refreshToken;
            await _userManager.UpdateAsync(adminUser);

            await _auditLogRepository.AddAsync(new AuditLog
            {
                Action = UserAction.ActivateTenant,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = request.AdminFullName,
                PerformerEmail = request.Email,
                PerformedAgainst = request.TenantName,
                IpAddress = ipAddress,
                MacAddress = HashCode(request.Email)[..16]
            });
            await _auditLogRepository.SaveAsync();

            return BaseResponse<ActivateTenantResponseDTO>.Success(new ActivateTenantResponseDTO
            {
                TenantId = tenant.Id,
                TenantName = request.TenantName,
                Token = token,
                RefreshToken = refreshToken,
                PlanType = code.PlanType,
                ExpiresAt = endDate,
                IsUnlimited = code.PlanType == PlanType.Unlimited
            }, ResponseMessages.TenantActivated);
        }

        public async Task<BaseResponse<SubscriptionStatusResponseDTO>> GetSubscriptionStatusAsync(long tenantId)
        {
            _tenantProvider.SetSchema("public");
            var tenant = await _tenantQueryRepository.FindAsync(tenantId);
            if (tenant == null)
                return BaseResponse<SubscriptionStatusResponseDTO>.Failure(null, ResponseMessages.TenantNotExisting);

            var planType = tenant.PlanType ?? PlanType.Trial;
            var isUnlimited = planType == PlanType.Unlimited;
            var isExpired = !isUnlimited && tenant.SubscriptionEndDate.HasValue && tenant.SubscriptionEndDate < DateTime.UtcNow;
            var daysRemaining = isUnlimited ? (int?)null
                : tenant.SubscriptionEndDate.HasValue
                    ? Math.Max(0, (int)(tenant.SubscriptionEndDate.Value - DateTime.UtcNow).TotalDays)
                    : 0;

            return BaseResponse<SubscriptionStatusResponseDTO>.Success(new SubscriptionStatusResponseDTO
            {
                TenantId = tenantId,
                PlanType = planType,
                PlanLabel = PlanLabel(planType),
                IsUnlimited = isUnlimited,
                IsExpired = isExpired,
                IsActive = !isExpired,
                ExpiresAt = tenant.SubscriptionEndDate,
                DaysRemaining = daysRemaining
            }, ResponseMessages.SubscriptionStatusRetrieved);
        }

        public async Task<BaseResponse> RenewSubscriptionAsync(long tenantId, string code, string callerEmail, AuditLog auditLog)
        {
            _tenantProvider.SetSchema("public");
            var tenant = await _tenantQueryRepository.FindAsync(tenantId);
            if (tenant == null)
                return BaseResponse.Failure(ResponseMessages.TenantNotExisting);

            var normalized = NormalizeCode(code);
            var hash = HashCode(normalized);
            var activationCode = await _context.ActivationCodes
                .FirstOrDefaultAsync(c => c.CodeHash == hash && !c.IsUsed);

            if (activationCode == null)
                return BaseResponse.Failure(ResponseMessages.ActivationCodeInvalid);

            if (!activationCode.BoundToEmail.Equals(callerEmail.ToLower(), StringComparison.OrdinalIgnoreCase))
                return BaseResponse.Failure(ResponseMessages.ActivationCodeEmailMismatch);

            var (startDate, endDate) = PlanDates(activationCode.PlanType);
            tenant.PlanType = activationCode.PlanType;
            tenant.SubscriptionStartDate = startDate;
            tenant.SubscriptionEndDate = endDate;
            tenant.LastModifiedDate = DateTime.UtcNow;
            tenant.ModifiedBy = callerEmail;

            activationCode.IsUsed = true;
            activationCode.UsedAt = DateTime.UtcNow;
            activationCode.UsedByTenantId = tenantId;

            await _tenantCommandRepository.UpdateAsync(tenant);
            _context.ActivationCodes.Update(activationCode);
            await _context.SaveChangesAsync();

            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveAsync();

            return BaseResponse.Success($"Subscription renewed to {PlanLabel(activationCode.PlanType)} plan successfully.");
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static (DateTime start, DateTime? end) PlanDates(PlanType planType)
        {
            var now = DateTime.UtcNow;
            return planType switch
            {
                PlanType.Trial => (now, now.AddDays(30)),
                PlanType.Monthly3 => (now, now.AddMonths(3)),
                PlanType.Monthly6 => (now, now.AddMonths(6)),
                PlanType.FiveYear => (now, now.AddYears(5)),
                PlanType.Unlimited => (now, null),
                _ => (now, now.AddMonths(3))
            };
        }

        private static string PlanLabel(PlanType planType) => planType switch
        {
            PlanType.Trial => "30-Day Trial",
            PlanType.Monthly3 => "3-Month Plan",
            PlanType.Monthly6 => "6-Month Plan",
            PlanType.FiveYear => "5-Year Plan",
            PlanType.Unlimited => "Unlimited",
            _ => "Unknown"
        };

        private static string GeneratePlaintextCode()
        {
            var bytes = new byte[12];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToHexString(bytes);
        }

        private static string FormatCode(string raw)
        {
            raw = raw.ToUpper();
            return $"{raw[..4]}-{raw[4..8]}-{raw[8..12]}-{raw[12..16]}";
        }

        private static string NormalizeCode(string code) =>
            code.Replace("-", "").Replace(" ", "").ToUpper();

        private static string HashCode(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            RandomNumberGenerator.Fill(randomNumber);
            return Regex.Replace(Convert.ToBase64String(randomNumber), "[^a-zA-Z0-9]+", "", RegexOptions.Compiled);
        }
    }
}

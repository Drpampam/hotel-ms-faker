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
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""Shift""             varchar(50);
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""Department""         varchar(100);
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""MustChangePassword"" boolean NOT NULL DEFAULT false;
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

            // 5 — Create SuperAdmin user in public schema
            // All users live in public so login can always find them regardless of X-Tenant-Id.
            _tenantProvider.SetSchema("public");
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

        public async Task<BaseResponse> AdminRenewSubscriptionAsync(long tenantId, PlanType planType, string callerEmail)
        {
            _tenantProvider.SetSchema("public");
            var tenant = await _tenantQueryRepository.FindAsync(tenantId);
            if (tenant == null)
                return BaseResponse.Failure(ResponseMessages.TenantNotExisting);

            var (startDate, endDate) = PlanDates(planType);
            tenant.PlanType = planType;
            tenant.SubscriptionStartDate = startDate;
            tenant.SubscriptionEndDate = endDate;
            tenant.LastModifiedDate = DateTime.UtcNow;
            tenant.ModifiedBy = callerEmail;

            await _tenantCommandRepository.UpdateAsync(tenant);
            await _context.SaveChangesAsync();

            await _auditLogRepository.AddAsync(new AuditLog
            {
                Action = UserAction.RenewSubscription,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = callerEmail,
                PerformerEmail = callerEmail,
                PerformedAgainst = tenant.Name ?? tenantId.ToString(),
                IpAddress = "Admin-Portal",
                MacAddress = HashCode(callerEmail)[..16]
            });
            await _auditLogRepository.SaveAsync();

            return BaseResponse.Success($"Subscription renewed to {PlanLabel(planType)} plan successfully.");
        }

        public async Task<BaseResponse<ProvisionTenantResponseDTO>> ProvisionTenantAsync(ProvisionTenantRequestDTO request, string ipAddress)
        {
            _tenantProvider.SetSchema("public");

            var existing = await _userManager.FindByEmailAsync(request.Email);

            // If user already has a tenant, it is fully provisioned — refuse
            if (existing != null && existing.TenantId != null && existing.TenantId > 0)
                return BaseResponse<ProvisionTenantResponseDTO>.Failure(null,
                    "This email is already associated with an active tenant account.");

            var fullName = string.IsNullOrWhiteSpace(request.FullName)
                ? (existing?.FullName ?? request.Email)
                : request.FullName;
            var tempPassword = GenerateTempPassword();

            // 1 — Create tenant with 30-day trial in public schema
            var (trialStart, trialEnd) = PlanDates(PlanType.Trial);
            var tenant = new Tenant
            {
                Name = fullName,
                PlanType = PlanType.Trial,
                SubscriptionStartDate = trialStart,
                SubscriptionEndDate = trialEnd,
                CreatedBy = "Admin-Provision",
                CreationDate = DateTime.UtcNow
            };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // 2 — Provision tenant schema
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
            await _context.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""Shift""             varchar(50);
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""Department""         varchar(100);
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""MustChangePassword"" boolean NOT NULL DEFAULT false;
            ");

            // 3 — Seed roles in tenant schema
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
                        CreatedBy = "Admin-Provision"
                    });
                }
            }

            // 4 — Create or recover user in public schema
            _tenantProvider.SetSchema("public");
            ApplicationUser user;
            if (existing != null)
            {
                // Orphaned user (TenantId == null) — recover: reset password + link to new tenant
                user = existing;
                user.TenantId = tenant.Id;
                user.MustChangePassword = true;
                user.FullName = fullName;
                await _userManager.RemovePasswordAsync(user);
                var addPwResult = await _userManager.AddPasswordAsync(user, tempPassword);
                if (!addPwResult.Succeeded)
                {
                    var errs = string.Join(", ", addPwResult.Errors.Select(e => e.Description));
                    return BaseResponse<ProvisionTenantResponseDTO>.Failure(null, errs);
                }
                await _userManager.UpdateAsync(user);
            }
            else
            {
                user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = fullName,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true,
                    Status = "Active",
                    TenantId = tenant.Id,
                    MustChangePassword = true,
                    CreatedBy = "Admin-Provision",
                    CreationDate = DateTime.UtcNow
                };
                var createResult = await _userManager.CreateAsync(user, tempPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return BaseResponse<ProvisionTenantResponseDTO>.Failure(null, errors);
                }
            }

            // 5 — Assign SuperAdmin role (idempotent)
            var superAdminRole = await _roleManager.FindByNameAsync("SuperAdmin");
            if (superAdminRole != null)
            {
                var hasRole = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == superAdminRole.Id);
                if (!hasRole)
                {
                    _context.UserRoles.Add(new ApplicationUserRole
                    {
                        UserId = user.Id,
                        RoleId = superAdminRole.Id,
                        TenantId = tenant.Id
                    });
                    await _context.SaveChangesAsync();
                }
            }

            // 6 — Generate activation code for the paid plan (for later upgrade from trial)
            var plaintext = GeneratePlaintextCode();
            var hash = HashCode(plaintext);
            var prev = await _context.ActivationCodes
                .FirstOrDefaultAsync(c => c.BoundToEmail == request.Email.ToLower() && !c.IsUsed);
            if (prev != null) { prev.IsUsed = true; _context.ActivationCodes.Update(prev); }
            _context.ActivationCodes.Add(new ActivationCode
            {
                CodeHash = hash,
                PlanType = request.PlanType,
                BoundToEmail = request.Email.ToLower(),
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = "admin-provision"
            });
            await _context.SaveChangesAsync();

            await _auditLogRepository.AddAsync(new AuditLog
            {
                Action = UserAction.ActivateTenant,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = "Admin",
                PerformerEmail = "admin",
                PerformedAgainst = request.Email,
                IpAddress = ipAddress,
                MacAddress = HashCode(request.Email)[..16]
            });
            await _auditLogRepository.SaveAsync();

            return BaseResponse<ProvisionTenantResponseDTO>.Success(new ProvisionTenantResponseDTO
            {
                Email = request.Email,
                TempPassword = tempPassword,
                ActivationCode = FormatCode(plaintext),
                PlanLabel = PlanLabel(request.PlanType),
                FullName = fullName
            }, "Tenant provisioned. Share the credentials with the client.");
        }

        public async Task<BaseResponse<SelfRegisterResponseDTO>> SelfRegisterAsync(SelfRegisterRequestDTO request, string ipAddress)
        {
            _tenantProvider.SetSchema("public");

            // Reject if email already registered
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing != null)
                return BaseResponse<SelfRegisterResponseDTO>.Failure(null, "An account with this email already exists.");

            // Create user with no tenant yet (TenantId = null)
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                Status = "Active",
                TenantId = null,
                CreatedBy = "Self-Registration",
                CreationDate = DateTime.UtcNow
            };
            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return BaseResponse<SelfRegisterResponseDTO>.Failure(null, errors);
            }

            // Store hotel name for later activation (stash in the activation code record)
            var plaintext = GeneratePlaintextCode();
            var hash = HashCode(plaintext);

            // Invalidate any previous unused code for this email
            var prev = await _context.ActivationCodes
                .FirstOrDefaultAsync(c => c.BoundToEmail == request.Email.ToLower() && !c.IsUsed);
            if (prev != null) { prev.IsUsed = true; _context.ActivationCodes.Update(prev); }

            _context.ActivationCodes.Add(new ActivationCode
            {
                CodeHash = hash,
                PlanType = request.PlanType,
                BoundToEmail = request.Email.ToLower(),
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = request.Email,
                // Stash hotel name in GeneratedBy field using separator, or store as metadata
                // We re-use GeneratedBy as "email|hotelName" so we can retrieve it at activation
                // Actually use a dedicated approach — store hotel name in ActivationCode
            });

            // Remove the code just added and redo with hotel name embedded
            _context.ActivationCodes.Remove(_context.ActivationCodes.Local.Last());

            _context.ActivationCodes.Add(new ActivationCode
            {
                CodeHash = hash,
                PlanType = request.PlanType,
                BoundToEmail = request.Email.ToLower(),
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = $"self|{request.HotelName}"
            });

            await _context.SaveChangesAsync();

            await _auditLogRepository.AddAsync(new AuditLog
            {
                Action = UserAction.GenerateActivationCode,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = request.FullName,
                PerformerEmail = request.Email,
                PerformedAgainst = request.Email,
                IpAddress = ipAddress,
                MacAddress = HashCode(request.Email)[..16]
            });
            await _auditLogRepository.SaveAsync();

            return BaseResponse<SelfRegisterResponseDTO>.Success(new SelfRegisterResponseDTO
            {
                PlaintextCode = FormatCode(plaintext),
                BoundToEmail = request.Email,
                PlanLabel = PlanLabel(request.PlanType),
                HotelName = request.HotelName
            }, "Registration successful. Use the activation code below to activate your workspace after logging in.");
        }

        public async Task<BaseResponse<ActivateMyAccountResponseDTO>> ActivateMyAccountAsync(string callerEmail, ActivateMyAccountRequestDTO request)
        {
            _tenantProvider.SetSchema("public");

            var user = await _userManager.FindByEmailAsync(callerEmail);
            if (user == null)
                return BaseResponse<ActivateMyAccountResponseDTO>.Failure(null, "User not found.");

            if (user.TenantId != null && user.TenantId > 0)
                return BaseResponse<ActivateMyAccountResponseDTO>.Failure(null, "Your workspace is already activated.");

            var normalized = NormalizeCode(request.Code);
            var hash = HashCode(normalized);

            var code = await _context.ActivationCodes
                .FirstOrDefaultAsync(c => c.CodeHash == hash && !c.IsUsed);

            if (code == null)
                return BaseResponse<ActivateMyAccountResponseDTO>.Failure(null, ResponseMessages.ActivationCodeInvalid);

            if (!code.BoundToEmail.Equals(callerEmail.ToLower(), StringComparison.OrdinalIgnoreCase))
                return BaseResponse<ActivateMyAccountResponseDTO>.Failure(null, ResponseMessages.ActivationCodeEmailMismatch);

            // Extract hotel name stashed during self-register (format: "self|HotelName")
            var hotelName = code.GeneratedBy.StartsWith("self|")
                ? code.GeneratedBy[5..]
                : user.FullName ?? callerEmail;

            // 1 — Create Tenant in public schema
            var (startDate, endDate) = PlanDates(code.PlanType);
            var tenant = new Tenant
            {
                Name = hotelName,
                PlanType = code.PlanType,
                SubscriptionStartDate = startDate,
                SubscriptionEndDate = endDate,
                CreatedBy = callerEmail,
                CreationDate = DateTime.UtcNow
            };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // 2 — Provision tenant schema
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

            await _context.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""Shift""             varchar(50);
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""Department""         varchar(100);
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""MustChangePassword"" boolean NOT NULL DEFAULT false;
            ");

            // 3 — Seed roles in tenant schema
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
                        CreatedBy = callerEmail
                    });
                }
            }

            // 4 — Update existing user's TenantId in public schema
            _tenantProvider.SetSchema("public");
            user.TenantId = tenant.Id;
            await _userManager.UpdateAsync(user);

            // 5 — Assign SuperAdmin role
            var superAdminRole = await _roleManager.FindByNameAsync("SuperAdmin");
            if (superAdminRole != null)
            {
                var hasRole = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == superAdminRole.Id);
                if (!hasRole)
                {
                    _context.UserRoles.Add(new ApplicationUserRole
                    {
                        UserId = user.Id,
                        RoleId = superAdminRole.Id,
                        TenantId = tenant.Id
                    });
                    await _context.SaveChangesAsync();
                }
            }

            // 6 — Mark code used
            code.IsUsed = true;
            code.UsedAt = DateTime.UtcNow;
            code.UsedByTenantId = tenant.Id;
            _context.ActivationCodes.Update(code);
            await _context.SaveChangesAsync();

            // 7 — Issue fresh token with updated claims
            var token = _tokenService.GenerateJSONWebToken(user.FullName!, user.Email!, new List<string> { "SuperAdmin" });
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            await _userManager.UpdateAsync(user);

            await _auditLogRepository.AddAsync(new AuditLog
            {
                Action = UserAction.ActivateTenant,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = user.FullName,
                PerformerEmail = callerEmail,
                PerformedAgainst = hotelName,
                IpAddress = "Self-activation",
                MacAddress = HashCode(callerEmail)[..16]
            });
            await _auditLogRepository.SaveAsync();

            return BaseResponse<ActivateMyAccountResponseDTO>.Success(new ActivateMyAccountResponseDTO
            {
                TenantId = tenant.Id,
                TenantName = hotelName,
                Token = token,
                RefreshToken = refreshToken,
                PlanLabel = PlanLabel(code.PlanType),
                ExpiresAt = endDate,
                IsUnlimited = code.PlanType == PlanType.Unlimited
            }, "Workspace activated successfully! Welcome to HotelMS.");
        }

        public async Task<BaseResponse<List<TenantSummaryDTO>>> GetAllTenantsAsync()
        {
            _tenantProvider.SetSchema("public");
            var tenants = await _context.Tenants
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.Id)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var result = new List<TenantSummaryDTO>();

            foreach (var tenant in tenants)
            {
                var adminEmail = await _context.Users
                    .Where(u => u.TenantId == tenant.Id)
                    .OrderBy(u => u.Id)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync() ?? string.Empty;

                var planType = tenant.PlanType ?? PlanType.Trial;
                var isUnlimited = planType == PlanType.Unlimited;
                var isExpired = !isUnlimited && tenant.SubscriptionEndDate.HasValue && tenant.SubscriptionEndDate < now;
                var daysRemaining = isUnlimited ? (int?)null
                    : tenant.SubscriptionEndDate.HasValue
                        ? Math.Max(0, (int)(tenant.SubscriptionEndDate.Value - now).TotalDays)
                        : 0;

                result.Add(new TenantSummaryDTO
                {
                    Id = tenant.Id,
                    Name = tenant.Name ?? string.Empty,
                    AdminEmail = adminEmail,
                    PlanType = planType,
                    PlanLabel = PlanLabel(planType),
                    IsActive = !isExpired,
                    IsExpired = isExpired,
                    IsUnlimited = isUnlimited,
                    ExpiresAt = tenant.SubscriptionEndDate,
                    DaysRemaining = daysRemaining,
                    CreatedAt = tenant.CreationDate
                });
            }

            return BaseResponse<List<TenantSummaryDTO>>.Success(result, "Tenants retrieved.");
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

        private static string GenerateTempPassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghjkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string special = "@#$!";
            var all = upper + lower + digits + special;

            var bytes = new byte[12];
            RandomNumberGenerator.Fill(bytes);

            // Guarantee at least one of each required char class
            var chars = new char[12];
            chars[0] = upper[bytes[0] % upper.Length];
            chars[1] = lower[bytes[1] % lower.Length];
            chars[2] = digits[bytes[2] % digits.Length];
            chars[3] = special[bytes[3] % special.Length];
            for (int i = 4; i < 12; i++)
                chars[i] = all[bytes[i] % all.Length];

            // Shuffle
            RandomNumberGenerator.Fill(bytes);
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = bytes[i % bytes.Length] % (i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
            return new string(chars);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            RandomNumberGenerator.Fill(randomNumber);
            return Regex.Replace(Convert.ToBase64String(randomNumber), "[^a-zA-Z0-9]+", "", RegexOptions.Compiled);
        }
    }
}

using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.Enums;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Helpers;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Helpers;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace hotelier_core_app.Service.Implementation
{
    public class UserService : IUserService
    /// <summary>
    /// Initializes a new instance of the UserService class with required dependencies.
    /// </summary>
    /// <param name="userManager">User manager for identity operations.</param>
    /// <param name="roleManager">Role manager for identity operations.</param>
    /// <param name="signInManager">Sign-in manager for authentication.</param>
    /// <param name="auditLogCommandRepository">Audit log repository.</param>
    /// <param name="tenantCommandRepository">Tenant repository.</param>
    /// <param name="emailService">Email service for notifications.</param>
    /// <param name="mapper">AutoMapper instance.</param>
    /// <param name="config">Configuration provider.</param>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly IDBCommandRepository<Tenant> _tenantCommandRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly string _clientUrl;
        private const string HOTELIER_ADMIN = "Hotelier Admin";
        private const string HOTELIER_ADMIN_EMAIL = "admin@hotelier.com";
        private readonly IModuleService _moduleService;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            IDBCommandRepository<Tenant> tenantCommandRepository,
            IEmailService emailService,
            IMapper mapper,
            IConfiguration config,
            IModuleService moduleService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _auditLogCommandRepository = auditLogCommandRepository;
            _tenantCommandRepository = tenantCommandRepository;
            _emailService = emailService;
            _mapper = mapper;
            _clientUrl = config.GetSection("Client:ClientURI").Value ?? string.Empty;
            _moduleService = moduleService;
        }

        public async Task<BaseResponse> ActivateUser(ActivateUserRequestDTO model, AuditLog auditLog)
        /// <summary>
        /// Activates a user and assigns a role.
        /// </summary>
        /// <param name="model">Activation request details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if activated, otherwise failure.</returns>
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BaseResponse.Failure(ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);
            }

            user.IsActive = true;
            string userStatus = user.Status;
            user.Status = UserStatus.Active.ToString();
            user.IsDeleted = false;

            IdentityResult result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(model.Role.ToString()))
                {
                    ApplicationRole newRole = _mapper.Map<ApplicationRole>(model);
                    newRole.CreationDate = DateTime.UtcNow;
                    newRole.CreatedBy = auditLog.PerformedBy;
                    await _roleManager.CreateAsync(newRole);
                }

                try
                {
                    var roleAssignmentResult = await _userManager.AddToRoleAsync(user, model.Role.ToString());
                    if (!roleAssignmentResult.Succeeded)
                    {
                        throw new Exception(HandleIdentityErrors(roleAssignmentResult).Message);
                    }
                }
                catch (Exception ex)
                {
                    user.Status = userStatus;
                    user.IsDeleted = true;
                    user.IsActive = false;
                    await _userManager.UpdateAsync(user);
                    return BaseResponse.Failure(ex.Message, ResponseStatusCode.OperationFailed);
                }

                _auditLogCommandRepository.Add(auditLog);

                return BaseResponse.Success(ResponseMessages.UserActivated, ResponseStatusCode.UserActivated);
            }

            return BaseResponse.Failure(ResponseMessages.OperationFailed, ResponseStatusCode.OperationFailed);
        }

        public async Task<BaseResponse> CreateUser(CreateUserRequestDTO model, AuditLog auditLog)
        /// <summary>
        /// Creates a new user and assigns a role and tenant.
        /// </summary>
        /// <param name="model">User creation details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if created, otherwise failure.</returns>
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BaseResponse.Failure(ResponseMessages.UserExist, ResponseStatusCode.UserExist);
            }

            ApplicationUser newUser = _mapper.Map<ApplicationUser>(model);
            newUser.CreationDate = DateTime.UtcNow;
            newUser.CreatedBy = HOTELIER_ADMIN;

            IdentityResult newUserResult = await _userManager.CreateAsync(newUser, model.Password);

            if (!newUserResult.Succeeded)
            {
                return HandleIdentityErrors(newUserResult);
            }

            if (!await _roleManager.RoleExistsAsync(model.Role.ToString()))
            {
                ApplicationRole newRole = _mapper.Map<ApplicationRole>(model);
                newRole.CreationDate = DateTime.UtcNow;
                newRole.CreatedBy = HOTELIER_ADMIN;
                await _roleManager.CreateAsync(newRole);
            }

            try
            {
                var roleAssignmentResult = await _userManager.AddToRoleAsync(newUser, model.Role.ToString());
                if (!roleAssignmentResult.Succeeded)
                {
                    throw new Exception(HandleIdentityErrors(roleAssignmentResult).Message);
                }
            }
            catch (Exception ex)
            {
                await _userManager.DeleteAsync(newUser);
                return BaseResponse.Failure(ex.Message, ResponseStatusCode.SQlException);
            }

            auditLog.PerformedBy = HOTELIER_ADMIN;
            auditLog.PerformerEmail = HOTELIER_ADMIN_EMAIL;
            _auditLogCommandRepository.SwitchProvider(DBProvider.SQL_Dapper);
            await _auditLogCommandRepository.AddAsync(auditLog);

            await SendEmailConfirmationAsync(newUser, model.Email);

            await CreateTenantAsync(model, newUser);

            return BaseResponse.Success(ResponseMessages.UserCreated, ResponseStatusCode.UserCreated);
        }

        public async Task<BaseResponse> DeactivateUser(DeactivateUserRequestDTO model, AuditLog auditLog)
        /// <summary>
        /// Deactivates a user and updates their status.
        /// </summary>
        /// <param name="model">Deactivation request details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if deactivated, otherwise failure.</returns>
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BaseResponse.Failure(ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);
            }

            if (model.Status.Equals(UserStatus.Active))
            {
                return BaseResponse.Failure();
            }

            user.IsActive = false;
            user.Status = model.Status.ToString();
            user.LastModifiedDate = DateTime.UtcNow;
            if (model.Status == UserStatus.Sacked || model.Status == UserStatus.Resigned) user.IsDeleted = true;

            IdentityResult result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _auditLogCommandRepository.Add(auditLog);
                return BaseResponse.Success(ResponseMessages.UserDeactivated, ResponseStatusCode.UserDeactivated);
            }

            return BaseResponse.Failure(ResponseMessages.OperationFailed, ResponseStatusCode.OperationFailed);
        }

        public async Task<BaseResponse<List<ModuleGroupDTO>>> GetAssignedModules(string emailAddress)
        {
            var user = await _userManager.FindByEmailAsync(emailAddress);
            if (user == null)
            {
                return BaseResponse<List<ModuleGroupDTO>>.Failure(new List<ModuleGroupDTO>(), ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            
            var modules = _moduleService.GetAssignedModules(userRoles.ToList());
            return BaseResponse<List<ModuleGroupDTO>>.Success(modules.Data, ResponseMessages.ModulesRetrieved, ResponseStatusCode.ModulesRetrieved);
        }

        public async Task<BaseResponse<ApplicationUserDTO>> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BaseResponse<ApplicationUserDTO>.Failure( new ApplicationUserDTO(),
                    ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);
            }

            var userResponse = _mapper.Map<ApplicationUserDTO>(user);
            return BaseResponse<ApplicationUserDTO>.Success(userResponse);
        }

        public async Task<PageBaseResponse<List<ApplicationUserDTO>>> GetUsers(PageParamsDTO model)
        {
            var usersQuery =  _userManager.Users.Where(u => !u.IsDeleted);
            var totalUsers = await usersQuery.CountAsync();
            if (totalUsers == 0)
            {
                return PageBaseResponse<List<ApplicationUserDTO>>.Failure(new List<ApplicationUserDTO>(),
                    ResponseMessages.UsersFetchFailed, 0, ResponseStatusCode.UsersFetchFailed);
            }
            
            var users = await usersQuery
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();
            
            var usersResponse = _mapper.Map<List<ApplicationUserDTO>>(users);
            
            return PageBaseResponse<List<ApplicationUserDTO>>.Success(usersResponse, ResponseMessages.UsersRetrieved, totalUsers, ResponseStatusCode.UsersRetrieved);
        }

        public async Task<(BaseResponse<LoginResponseDTO>, string)> Login(UserLoginRequestDTO model, AuditLog auditLog)
        /// <summary>
        /// Authenticates a user and returns login response and refresh token.
        /// </summary>
        /// <param name="model">Login request details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a tuple of login response and refresh token.</returns>
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Email == model.Email);

            if (user == null)
            {
                return (BaseResponse<LoginResponseDTO>.Failure(new LoginResponseDTO(),
                    ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist), string.Empty);
            }

            if (!user.IsActive)
            {
                return (BaseResponse<LoginResponseDTO>.Failure(new LoginResponseDTO(),
                    ResponseMessages.UserInactive, ResponseStatusCode.UserInactive), string.Empty);
            }

            if (!user.EmailConfirmed)
            {
                return (BaseResponse<LoginResponseDTO>.Failure(new LoginResponseDTO(),
                    ResponseMessages.UserEmailNotConfirmed, ResponseStatusCode.UserEmailNotConfirmed), string.Empty);
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            if (!signInResult.Succeeded)
            {
                return (BaseResponse<LoginResponseDTO>.Failure(new LoginResponseDTO(),
                    ResponseMessages.InvalidCredential, ResponseStatusCode.InvalidCredential), string.Empty);
            }

            var userRole = await _userManager.GetRolesAsync(user);
            LoginResponseDTO data = new LoginResponseDTO
            {
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Picture = user.Picture ?? string.Empty,
                Roles = userRole.ToList()
            };

            string refreshToken = await GenerateRefreshTokenAndPersistData(user, auditLog);

            return (BaseResponse<LoginResponseDTO>.Success(data, ResponseMessages.LoginSuccessful,
                ResponseStatusCode.LoginSuccessful), refreshToken);
        }

        public async Task<BaseResponse> ReassignRole(EditUserRolesRequestDTO model, AuditLog auditLog)
        /// <summary>
        /// Reassigns roles to a user.
        /// </summary>
        /// <param name="model">Role reassignment details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if reassigned, otherwise failure.</returns>
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BaseResponse.Failure(ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);
            }

            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                if (user.IsActive)
                {
                    List<string> validRoles = new List<string>();

                    if (model.Roles != null)
                    {
                        foreach (var role in model.Roles)
                        {
                            if (!await _roleManager.RoleExistsAsync(role))
                            {
                                return BaseResponse.Failure(ResponseMessages.RoleNotExist, ResponseStatusCode.RoleNotExist); ;
                            }
                        }
                    }

                    var currentRoles = await _userManager.GetRolesAsync(user);
                    
                    var removeUserFromRole = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeUserFromRole.Succeeded)
                    {
                        return BaseResponse.Failure(ResponseMessages.RoleReassignmentError, ResponseStatusCode.GeneralError);
                    }
                    
                    var addUserToRole = await _userManager.AddToRolesAsync(user, model.Roles);
                    if (!addUserToRole.Succeeded)
                    {
                        return BaseResponse.Failure(ResponseMessages.RoleReassignmentError, ResponseStatusCode.GeneralError);
                    }
                    
                    await _auditLogCommandRepository.AddAsync(auditLog);
                    await _auditLogCommandRepository.SaveAsync();

                    // find matching roles
                    // if found, mark for exclusion from deletion
                    // check if any changes exists for the role reassignment

                    //if (currentRoles.Contains(newRole))
                    //{
                    //    return true;
                    //}

                    /*
                        ApplicationUserRole userRole = await _userRoleQueryRepository.GetByDefaultAsync(predicate => predicate.UserId == user.Id);
                        userRole.RoleId = model.RoleId;
                        _userRoleCommandRepository.Update(userRole);
                        _auditLogCommandRepository.Add(auditLog);

                        await _userRoleCommandRepository.SaveAsync();
                        await _auditLogCommandRepository.SaveAsync();

                        return BaseResponse.Success(ResponseMessages.UpdateSuccessful);
                    }

                            */


                    //var currentRoles = await _userManager.GetRolesAsync(user);

                    //// Remove old roles
                    //var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    //if (!removeResult.Succeeded)
                    //{
                    //    _logger.LogError($"Failed to remove existing roles for user {user.Email}.");
                    //    return false;
                    //}

                    //// Assign new role
                    //var addResult = await _userManager.AddToRoleAsync(user, newRole);
                    //if (!addResult.Succeeded)
                    //{
                    //    _logger.LogError($"Failed to add role {newRole} to user {user.Email}.");
                    //    return false;
                    //}

                    return BaseResponse.Success(ResponseMessages.RoleUpdated);
                }
                return BaseResponse.Failure(ResponseMessages.UserInactive);
            }
            return BaseResponse.Failure(ResponseMessages.UserDoesNotExist);
        }

        public async Task<(BaseResponse<RefreshTokenResponseDTO>, string)> RefreshToken(RefreshTokenRequestDTO model, AuditLog auditLog)
        /// <summary>
        /// Refreshes a user's authentication token.
        /// </summary>
        /// <param name="model">Refresh token request details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a tuple of refresh token response and new token.</returns>
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Email == model.Email);
            if (user == null)
            {
                return (BaseResponse<RefreshTokenResponseDTO>.Failure(new RefreshTokenResponseDTO(),
                    ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist), string.Empty);
            }

            if (user.RefreshToken == model.RefreshToken)
            {
                var userRole = await _userManager.GetRolesAsync(user);

                RefreshTokenResponseDTO data = new RefreshTokenResponseDTO()
                {
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Roles = userRole.ToList()
                };

                string refreshToken = await GenerateRefreshTokenAndPersistData(user, auditLog);

                return (BaseResponse<RefreshTokenResponseDTO>.Success(data, ResponseMessages.LoginSuccessful), refreshToken);
            }
            return (BaseResponse<RefreshTokenResponseDTO>.Failure(new RefreshTokenResponseDTO(),
                ResponseMessages.CantVerifyRefreshToken, ResponseStatusCode.CantVerifyRefreshToken), string.Empty);
        }

        public async Task<BaseResponse> UpdateUserDetail(EditUserDetailRequestDTO model, AuditLog auditLog)
        {
            if (string.IsNullOrEmpty(model.Email))
                return BaseResponse.Failure(ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BaseResponse.Failure(ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);

            if (!user.IsActive)
                return BaseResponse.Failure(ResponseMessages.UserInactive, ResponseStatusCode.UserInactive);

            user.FullName = model.FullName ?? user.FullName;
            user.LastModifiedDate = DateTime.UtcNow;
            user.ModifiedBy = auditLog.PerformedBy;
            await _userManager.UpdateAsync(user);

            if (model.Roles != null && model.Roles.Any())
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRolesAsync(user, model.Roles);
            }

            _auditLogCommandRepository.Add(auditLog);
            await _auditLogCommandRepository.SaveAsync();

            return BaseResponse.Success(ResponseMessages.UpdateSuccessful, ResponseStatusCode.UpdateSuccessful);
        }

        public async Task<BaseResponse> UpdateUserName(EditUserNameRequestDTO model, AuditLog auditLog)
        /// <summary>
        /// Updates a user's full name.
        /// </summary>
        /// <param name="model">User name update details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if updated, otherwise failure.</returns>
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BaseResponse.Failure(ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);
            }

            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (user.IsActive)
                {
                    user.FullName = model.Name;
                    user.LastModifiedDate = DateTime.UtcNow;
                    user.ModifiedBy = auditLog.PerformedBy;
                    await _userManager.UpdateAsync(user);

                    _auditLogCommandRepository.Add(auditLog);
                    await _auditLogCommandRepository.SaveAsync();

                    return BaseResponse.Success(ResponseMessages.UpdateSuccessful, ResponseStatusCode.UpdateSuccessful);
                }
                return BaseResponse.Failure(ResponseMessages.UserInactive, ResponseStatusCode.UserInactive);
            }
            return BaseResponse.Failure(ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);
        }

        #region private methods
        private async Task SendEmailConfirmationAsync(ApplicationUser user, string email)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var queryParams = new Dictionary<string, string?>
            {
                ["Email"] = email,
                ["Token"] = token
            };

            var confirmationLink = QueryHelpers.AddQueryString(_clientUrl + "emailconfirmation", queryParams);


            await _emailService.SendEmail(new SendEmailDTO(
                new List<string>() { email },
                "Email Confirmation",
                confirmationLink,
                null));
        }

        private async Task CreateTenantAsync(CreateUserRequestDTO model, ApplicationUser newUser)
        {
            var tenant = new Tenant
            {
                Name = model.HotelName,
                Description = $"Tenant for {model.HotelName}",
                Logo = string.Empty,
                SubscriptionPlanId = model.SubscriptionPlanId,
                CreatedBy = newUser.Id.ToString(),
                CreationDate = DateTime.UtcNow
            };

            tenant.Users.Add(newUser);

            await _tenantCommandRepository.AddAsync(tenant);
            await _tenantCommandRepository.SaveAsync();

            await CreateTenantSchemaAsync($"tenant_{tenant.Id}");
        }

        protected virtual async Task CreateTenantSchemaAsync(string schemaName)
        {
            var dbContext = _tenantCommandRepository as DbContext;
            if (dbContext != null)
            {
                var createSchemaSql = $"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\";";
                await dbContext.Database.ExecuteSqlRawAsync(createSchemaSql);
            }
        }

        private BaseResponse HandleIdentityErrors(IdentityResult result)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BaseResponse.Failure(errors, ResponseStatusCode.IdentityError);
        }

        private async Task<string> GenerateRefreshTokenAndPersistData(ApplicationUser user, AuditLog auditLog)
        {
            string refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.LastModifiedDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            auditLog.PerformedBy = user.FullName;
            auditLog.PerformerEmail = user.Email ?? string.Empty;
            auditLog.PerformedAgainst = user.Email ?? string.Empty;
            auditLog.MacAddress = HashHelper.GenerateSHA256Hash(user.Email ?? string.Empty);

            _auditLogCommandRepository.Add(auditLog);

            return refreshToken;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Regex.Replace(Convert.ToBase64String(randomNumber), "[^a-zA-Z0-9]+", "", RegexOptions.Compiled);
        }
        #endregion
    }
}

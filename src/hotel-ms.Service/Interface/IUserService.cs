using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IUserService : IAutoDependencyService
    {
        Task<BaseResponse> CreateUser(CreateUserRequestDTO model, AuditLog auditLog);

        Task<BaseResponse> DeactivateUser(DeactivateUserRequestDTO model, AuditLog auditLog, long? callerTenantId);

        Task<BaseResponse> ActivateUser(ActivateUserRequestDTO model, AuditLog auditLog, long? callerTenantId);

        Task<BaseResponse> UpdateUserName(EditUserNameRequestDTO model, AuditLog auditLog);

        Task<BaseResponse> UpdateUserDetail(EditUserDetailRequestDTO model, AuditLog auditLog, long? callerTenantId);

        Task<BaseResponse> ReassignRole(EditUserRolesRequestDTO model, AuditLog auditLog, long? callerTenantId);

        Task<(BaseResponse<LoginResponseDTO>, string)> Login(UserLoginRequestDTO model, AuditLog auditLog);

        Task<(BaseResponse<RefreshTokenResponseDTO>, string)> RefreshToken(RefreshTokenRequestDTO model, AuditLog auditLog);

        Task<PageBaseResponse<List<ApplicationUserDTO>>> GetUsers(PageParamsDTO model, long? callerTenantId, string callerEmail);

        Task<BaseResponse<ApplicationUserDTO>> GetUserByEmail(string email);

        Task<BaseResponse<List<ModuleGroupDTO>>> GetAssignedModules(string emailAddress);

        Task<BaseResponse> ForgotPasswordAsync(string email);
        Task<BaseResponse> ResetPasswordAsync(string email, string token, string newPassword);
        Task<BaseResponse> AdminChangePasswordAsync(AdminChangePasswordRequestDTO model, AuditLog auditLog, long? callerTenantId);
        Task<BaseResponse> ChangeTempPasswordAsync(string callerEmail, string currentPassword, string newPassword);
        Task<BaseResponse> DeleteUserAsync(DeleteUserRequestDTO model, AuditLog auditLog, long? callerTenantId);
        Task<BaseResponse> ChangeUserShiftAsync(ChangeUserShiftRequestDTO model, AuditLog auditLog, long? callerTenantId);
    }
}

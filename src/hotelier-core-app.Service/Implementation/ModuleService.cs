using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;

namespace hotelier_core_app.Service.Implementation
{
    /// <summary>
    /// Provides business logic for managing modules and module groups.
    /// </summary>
    public class ModuleService : IModuleService
    {
        private readonly IDBCommandRepository<ModuleGroup> _moduleGroupCommandRepository;
        private readonly IDBQueryRepository<ModuleGroup> _moduleGroupQueryRepository;
        private readonly IDBCommandRepository<Module> _moduleCommandRepository;
        private readonly IDBQueryRepository<Module> _moduleQueryRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly IMapper _mapper;

        public ModuleService(
            IDBCommandRepository<ModuleGroup> moduleGroupCommandRepository,
            IDBQueryRepository<ModuleGroup> moduleGroupQueryRepository,
            IDBCommandRepository<Module> moduleCommandRepository,
            IDBQueryRepository<Module> moduleQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            IMapper mapper)
        {
            _moduleGroupCommandRepository = moduleGroupCommandRepository;
            _moduleGroupQueryRepository = moduleGroupQueryRepository;
            _moduleCommandRepository = moduleCommandRepository;
            _moduleQueryRepository = moduleQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new module if it does not already exist.
        /// </summary>
        /// <param name="model">The module creation details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if created, otherwise failure if the module exists.</returns>
        public async Task<BaseResponse> CreateModule(CreateModuleDTO model, AuditLog auditLog)
        {
            var check = _moduleQueryRepository.GetBy(x => x.Name.ToLower().Equals(model.Name.ToLower()));
            if (!check.Any())
            {
                _moduleCommandRepository.Add(new Module
                {
                    ModuleGroupId = model.ModuleGroupId,
                    Name = model.Name,
                    Description = model.Description,
                    Url = model.Url,
                    CreatedBy = auditLog.PerformedBy,
                    ModifiedBy = auditLog.PerformedBy,
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                });
                _auditLogCommandRepository.Add(auditLog);

                await _moduleCommandRepository.SaveAsync();
                await _auditLogCommandRepository.SaveAsync();

                return BaseResponse.Success(ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
            }
            return BaseResponse.Failure(ResponseMessages.ModuleExist, ResponseStatusCode.ModuleExist);
        }

        public async Task<BaseResponse> CreateModuleGroup(CreateModuleGroupDTO model, AuditLog auditLog)
        /// <summary>
        /// Creates a new module group if it does not already exist.
        /// </summary>
        /// <param name="model">The module group creation details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if created, otherwise failure if the group exists.</returns>
        {
            var check = _moduleGroupQueryRepository.GetBy(x => x.Name.ToLower().Equals(model.Name.ToLower()));
            if (!check.Any())
            {
                _moduleGroupCommandRepository.Add(new ModuleGroup
                {
                    Name = model.Name,
                    Description = model.Description,
                    Url = model.Url,
                    CreatedBy = auditLog.PerformedBy,
                    ModifiedBy = auditLog.PerformedBy,
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                });
                _auditLogCommandRepository.Add(auditLog);

                await _moduleGroupCommandRepository.SaveAsync();
                await _auditLogCommandRepository.SaveAsync();

                return BaseResponse.Success(ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
            }
            return BaseResponse.Failure(ResponseMessages.ModuleGroupExist, ResponseStatusCode.ModuleGroupExist);
        }

        public async Task<BaseResponse> DeleteModule(long id, AuditLog auditLog)
        /// <summary>
        /// Deletes a module by its ID.
        /// </summary>
        /// <param name="id">The ID of the module to delete.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if deleted, otherwise failure if not found.</returns>
        {
            var module = _moduleQueryRepository.GetByDefault(x => x.Id == id);
            if (module != null)
            {
                _moduleCommandRepository.Delete(module);
                _auditLogCommandRepository.Add(auditLog);

                await _moduleGroupCommandRepository.SaveAsync();
                await _auditLogCommandRepository.SaveAsync();


                return BaseResponse.Success(ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
            }
            return BaseResponse.Failure(ResponseMessages.ModuleNotExist, ResponseStatusCode.ModuleNotExist);
        }

        public async Task<BaseResponse> DeleteModuleGroup(long id, AuditLog auditLog)
        /// <summary>
        /// Deletes a module group by its ID.
        /// </summary>
        /// <param name="id">The ID of the module group to delete.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if deleted, otherwise failure if not found.</returns>
        {
            var moduleGroup = _moduleGroupQueryRepository.GetByDefault(x => x.Id == id);
            if (moduleGroup != null)
            {
                _moduleGroupCommandRepository.Delete(moduleGroup);
                _auditLogCommandRepository.Add(auditLog);

                await _moduleGroupCommandRepository.SaveAsync();
                await _auditLogCommandRepository.SaveAsync();

                return BaseResponse.Success(ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
            }
            return BaseResponse.Failure(ResponseMessages.ModuleGroupNotExist, ResponseStatusCode.ModuleGroupNotExist);
        }

        public async Task<BaseResponse> EditModule(long id, EditModuleDTO model, AuditLog auditLog)
        /// <summary>
        /// Edits an existing module's details.
        /// </summary>
        /// <param name="id">The ID of the module to edit.</param>
        /// <param name="model">The new module details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if updated, otherwise failure if not found or validation fails.</returns>
        {
            if (string.IsNullOrEmpty(model.Name) &&
                string.IsNullOrEmpty(model.Description) &&
                string.IsNullOrEmpty(model.Url))
                return BaseResponse.Failure(ResponseMessages.ModuleUpdateValidation, ResponseStatusCode.ModuleUpdateValidation);

            var module = _moduleQueryRepository.GetByDefault(x => x.Id == model.Id);
            if (module != null)
            {
                if (!string.IsNullOrEmpty(model.Name)) module.Name = model.Name;
                if (!string.IsNullOrEmpty(model.Description)) module.Description = model.Description;
                if (!string.IsNullOrEmpty(model.Url)) module.Url = model.Url;
                module.ModifiedBy = auditLog.PerformedBy;
                module.LastModifiedDate = DateTime.UtcNow;

                _moduleCommandRepository.Update(module);
                _auditLogCommandRepository.Add(auditLog);
                await _moduleGroupCommandRepository.SaveAsync();
                await _auditLogCommandRepository.SaveAsync();

                return BaseResponse.Success(ResponseMessages.ModuleUpdated, ResponseStatusCode.ModuleUpdated);
            }
            return BaseResponse.Failure(ResponseMessages.ModuleNotExist, ResponseStatusCode.ModuleNotExist);
        }

        public async Task<BaseResponse> EditModuleGroup(long id, EditModuleGroupDTO model, AuditLog auditLog)
        /// <summary>
        /// Edits an existing module group's details.
        /// </summary>
        /// <param name="id">The ID of the module group to edit.</param>
        /// <param name="model">The new module group details.</param>
        /// <param name="auditLog">Audit log information for the operation.</param>
        /// <returns>Returns a success response if updated, otherwise failure if not found or validation fails.</returns>
        {
            if (string.IsNullOrEmpty(model.Name) &&
                string.IsNullOrEmpty(model.Description) &&
                string.IsNullOrEmpty(model.Url))
                return BaseResponse.Failure(ResponseMessages.ModuleGroupUpdateValidation, ResponseStatusCode.ModuleGroupUpdateValidation);

            var moduleGroup = _moduleGroupQueryRepository.GetByDefault(x => x.Id == model.Id);
            if (moduleGroup != null)
            {
                if (!string.IsNullOrEmpty(model.Name)) moduleGroup.Name = model.Name;
                if (!string.IsNullOrEmpty(model.Description)) moduleGroup.Description = model.Description;
                if (!string.IsNullOrEmpty(model.Url)) moduleGroup.Url = model.Url;
                moduleGroup.ModifiedBy = auditLog.PerformedBy;
                moduleGroup.LastModifiedDate = DateTime.UtcNow;

                _moduleGroupCommandRepository.Update(moduleGroup);
                _auditLogCommandRepository.Add(auditLog);
                await _moduleGroupCommandRepository.SaveAsync();
                await _auditLogCommandRepository.SaveAsync();

                return BaseResponse.Success(ResponseMessages.ModuleGroupUpdated, ResponseStatusCode.ModuleGroupUpdated);
            }
            return BaseResponse.Failure(ResponseMessages.ModuleGroupNotExist, ResponseStatusCode.ModuleGroupNotExist);
        }

        public async Task<BaseResponse<List<ModuleDTO>>> GetAllModule()
        /// <summary>
        /// Retrieves all modules in the system.
        /// </summary>
        /// <returns>Returns a list of all modules.</returns>
        {
            var modules = await _moduleQueryRepository.GetAllAsync();
            var modulesDTO = _mapper.Map<List<ModuleDTO>>(modules);

            return BaseResponse<List<ModuleDTO>>.Success(modulesDTO, ResponseMessages.OperationSuccessful);
        }

        public async Task<BaseResponse<List<ModuleGroupDTO>>> GetAllModuleGroup()
        /// <summary>
        /// Retrieves all module groups, including their modules.
        /// </summary>
        /// <returns>Returns a list of all module groups.</returns>
        {
            var modulesGroup = _moduleGroupQueryRepository.GetAllIncluding(include => include.Modules).ToList();
            List<ModuleGroupDTO> modulesGroupDTO = _mapper.Map<List<ModuleGroupDTO>>(modulesGroup);

            return BaseResponse<List<ModuleGroupDTO>>.Success(modulesGroupDTO, ResponseMessages.OperationSuccessful);
        }
        
        
        public BaseResponse<List<ModuleGroupDTO>> GetAssignedModules(List<string> roles)
        {
            if (roles == null || !roles.Any())
            {
                return BaseResponse<List<ModuleGroupDTO>>.Failure(null, "No roles provided.");
            }

            var allModuleGroups = _moduleGroupQueryRepository.GetAllIncluding(group => group.Modules).ToList();
            var filteredModuleGroups = allModuleGroups
                .Where(group => group.Modules.Any())
                .ToList();

            var result = _mapper.Map<List<ModuleGroupDTO>>(filteredModuleGroups);

            return BaseResponse<List<ModuleGroupDTO>>.Success(result, ResponseMessages.OperationSuccessful);
        }
    }
}
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.ExternalSystem;
using DPMS_WebAPI.ViewModels.Group;
using Microsoft.AspNetCore.Mvc;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IExternalSystemService : IService<ExternalSystem>
	{
		Task<ExternalSystem> AddExternalSystem(AddSystemVM model);
		Task<IEnumerable<User>> GetUsersFromExternalSystem(Guid systemId);
        Task<IEnumerable<ExternalSystemPurpose>> BulkAddPurposeToSystemAsync(Guid systemId, List<Guid> purposeIds);
        Task<ExternalSystemPurpose> AddPurposeToSystemAsync(Guid SystemId, Guid purposeId);
		// Get all purpose of a external system
        Task<IEnumerable<Purpose>> GetSystemPurposesAsync (Guid systemId);
        // remove a purpose from a external system - remove ExternalSystemPurpose
        Task<bool> RemoveSystemPurposeAsync(Guid systemId, Guid purposeId);
        Task<bool> BulkRemoveSystemPurposeAsync (Guid systemId, List<Guid> purposeIds);
        Task<ActionResult<List<SystemUserVM>>> GetAllUsersAsync(Guid systemId);
        Task<ExternalSystemDetailVM> GetExternalSystemDetailAsync(Guid systemId);
        Task UpdateSystemStatus(SystemStatusVM model, List<ExternalSystemStatus> allowedStatus);
        Task RemoveExternalSystem(Guid systemId);
        Task UpdateSystemUsers(Guid systemId, List<GroupUserVM> model);

        Task<bool> ValidateApiKeyAsync(string apikey);
        Task UpdateSystem(UpdateSystemVM model, Guid systemId);
    }
}

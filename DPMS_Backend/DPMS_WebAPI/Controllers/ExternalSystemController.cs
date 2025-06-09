using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.ExternalSystem;
using DPMS_WebAPI.ViewModels.Group;
using DPMS_WebAPI.ViewModels.Purpose;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for managing external systems.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.Authenticated)]
    public class ExternalSystemController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly IMapper _mapper;
        private readonly ILogger<ExternalSystemController> _logger;
        private readonly IExternalSystemService _systemService;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalSystemController"/> class.
        /// </summary>
        /// <param name="groupService">The group service.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="systemService">The external system service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="userService">The user service.</param>
        public ExternalSystemController(IGroupService groupService, IMapper mapper,
            IExternalSystemService systemService, ILogger<ExternalSystemController> logger, IUserService userService)
        {
            _groupService = groupService;
            _mapper = mapper;
            _systemService = systemService;
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// Gets all external systems.
        /// </summary>
        /// <returns>A list of external systems.</returns>
        [HttpGet]
        //[Authorize]
        public async Task<ActionResult<List<ExternalSystemVM>>> GetAll()
        {
            List<ExternalSystemVM> result = new List<ExternalSystemVM>();

            string email = User.FindFirst(ClaimTypes.Email)?.Value!;
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("User email not found.");
            }

            bool isAdmin = await _groupService.UserBelongToGroupAsync(email, PermissionGroup.ADMIN_DPMS);
            bool isDpo = await _groupService.UserBelongToGroupAsync(email, PermissionGroup.DPO);

            if (isAdmin || isDpo)
            {
                var externalSystems = await _systemService.GetAllAsync();
                result = _mapper.Map<List<ExternalSystemVM>>(externalSystems);
            }
            else
            {
                result = await _userService.GetManageSystems(email);
            }

            return result;
        }

        /// <summary>
        /// Adds a new external system.
        /// </summary>
        /// <param name="model">The model containing the details of the new system.</param>
        /// <returns>An action result indicating the result of the operation.</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> AddNewSystem(AddSystemVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                ExternalSystem result = await _systemService.AddExternalSystem(model);
                if (result != null)
                {
                    return CreatedAtAction("AddNewSystem", result.Id);
                }
                else
                {
                    return BadRequest("No system created");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Removes an external system.
        /// </summary>
        /// <param name="systemId">The ID of the system to remove.</param>
        /// <returns>An action result indicating the result of the operation.</returns>
        [HttpDelete]
        public async Task<ActionResult> RemoveSystem(Guid systemId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _systemService.RemoveExternalSystem(systemId);
                return Ok("System removed successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adds a purpose to an external system.
        /// </summary>
        /// <param name="model">The model containing the details of the purpose to add.</param>
        /// <returns>An action result indicating the result of the operation.</returns>
        [HttpPost("add-purpose")]
        public async Task<IActionResult> AddPurposeToSystem([FromBody] SystemPurposeVM model)
        {
            try
            {
                if (model.PurposeIds.Count != 1)
                    return BadRequest("Please provide exactly one purpose ID.");

                var result = await _systemService.AddPurposeToSystemAsync(model.ExternalSystemId, model.PurposeIds[0]);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error adding purpose to external system.");
                return Problem("An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Bulk adds purposes to an external system.
        /// </summary>
        /// <param name="model">The model containing the details of the purposes to add.</param>
        /// <returns>An action result indicating the result of the operation.</returns>
        [HttpPost("bulk-add-purposes")]
        public async Task<IActionResult> BulkAddPurposesToSystem([FromBody] SystemPurposeVM model)
        {
            try
            {
                if (!model.PurposeIds.Any())
                    return BadRequest("At least one purpose ID is required.");

                var result = await _systemService.BulkAddPurposeToSystemAsync(model.ExternalSystemId, model.PurposeIds);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error bulk adding purposes to external system.");
                return Problem("An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets all purposes of an external system.
        /// </summary>
        /// <param name="systemId">The ID of the system.</param>
        /// <returns>An action result containing the purposes of the system.</returns>
        [HttpGet("{systemId}/purposes")]
        public async Task<IActionResult> GetSystemPurposes(Guid systemId)
        {
            try
            {
                var purposes = await _systemService.GetSystemPurposesAsync(systemId);

                var result = _mapper.Map<IEnumerable<PurposeVM>>(purposes);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving purposes of external system.");
                return Problem("An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Removes a single purpose from an external system.
        /// </summary>
        /// <param name="model">The model containing the details of the purpose to remove.</param>
        /// <returns>An action result indicating the result of the operation.</returns>
        [HttpDelete("remove-purpose")]
        public async Task<IActionResult> RemoveSystemPurpose([FromBody] SystemPurposeVM model)
        {
            try
            {
                if (model.PurposeIds.Count != 1)
                    return BadRequest("Please provide exactly one purpose ID.");

                bool success = await _systemService.RemoveSystemPurposeAsync(model.ExternalSystemId, model.PurposeIds.FirstOrDefault());
                return success ? NoContent() : NotFound("Purpose not linked to this external system.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error removing purpose from external system.");
                return Problem("An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Bulk removes purposes from an external system.
        /// </summary>
        /// <param name="model">The model containing the details of the purposes to remove.</param>
        /// <returns>An action result indicating the result of the operation.</returns>
        [HttpDelete("bulk-remove-purposes")]
        public async Task<IActionResult> BulkRemoveSystemPurposes([FromBody] SystemPurposeVM model)
        {
            try
            {
                if (!model.PurposeIds.Any())
                    return BadRequest("At least one purpose ID is required.");

                bool success = await _systemService.BulkRemoveSystemPurposeAsync(model.ExternalSystemId, model.PurposeIds);
                return success ? NoContent() : NotFound("No linked purposes found to remove.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error bulk removing purposes from external system.");
                return Problem("An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets all users of an external system.
        /// </summary>
        /// <param name="systemId">The ID of the system.</param>
        /// <returns>An action result containing the users of the system.</returns>
        [HttpGet("{systemId}/get-users")]
        public async Task<ActionResult<List<SystemUserVM>>> GetAllUsers(Guid systemId)
        {
            var users = await _systemService.GetAllUsersAsync(systemId);
            return users;
        }

        /// <summary>
        /// Gets the details of an external system.
        /// </summary>
        /// <param name="systemId">The ID of the system.</param>
        /// <returns>An action result containing the details of the system.</returns>
        [HttpGet("{systemId}/get-system-details")]
        public async Task<ActionResult<ExternalSystemDetailVM>> GetSystemDetails(Guid systemId)
        {
            var system = await _systemService.GetExternalSystemDetailAsync(systemId);
            return system;
        }

        /// <summary>
        /// Updates the users of an external system.
        /// </summary>
        /// <param name="systemId">The ID of the system.</param>
        /// <param name="model">The model containing the details of the users to update.</param>
        /// <returns>An action result indicating the result of the operation.</returns>
        [HttpPut("update-system-users")]
        public async Task<ActionResult> UpdateSystemUsers(Guid systemId, List<GroupUserVM> model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _systemService.UpdateSystemUsers(systemId, model);
                return Ok("System users updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates the status of an external system.
        /// </summary>
        /// <param name="model">The model containing the details of the status to update.</param>
        /// <returns>An action result indicating the result of the operation.</returns>
        [HttpPut("update-active-status")]
        public async Task<ActionResult> UpdateSystemStatus(SystemStatusVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var allowedStatus = new List<ExternalSystemStatus> {
                    ExternalSystemStatus.Active,
                    ExternalSystemStatus.Inactive,
                };
                await _systemService.UpdateSystemStatus(model, allowedStatus);
                return Ok("System status updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an external system.
        /// </summary>
        /// <param name="model">The model containing the details of the system to update.</param>
        /// <param name="systemId">The ID of the system.</param>
        /// <returns>An action result indicating the result of the operation.</returns>
        [HttpPut("update-system")]
        public async Task<ActionResult> UpdateSystem(UpdateSystemVM model, Guid systemId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _systemService.UpdateSystem(model, systemId);
                return Ok("System updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Generates an API key for an external system.
        /// </summary>
        /// <param name="systemId">The ID of the system.</param>
        /// <returns>An action result containing the generated API key.</returns>
        [HttpGet("get-key/{systemId}")]
        public async Task<IActionResult> GenerateAPIKey(Guid systemId)
        {
            try
            {
                if (systemId == Guid.Empty) return BadRequest("SystemId cannot be empty");
                var apikey = ConsentUtils.GenerateApiKey();
                var system = await _systemService.GetByIdAsync(systemId);

                if (system == null) return NotFound("System not found");

                system.ApiKeyHash = ConsentUtils.HashApiKey(apikey);
                await _systemService.UpdateAsync(system);
                return Ok(apikey);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
    
    }
}

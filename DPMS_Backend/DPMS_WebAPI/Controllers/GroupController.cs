using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.DPIA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for Group-related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize (Policy = Policies.FeatureRequired)]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly IMapper _mapper;
        private readonly ILogger<GroupController> _logger;
        private readonly IUserService _userService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupService"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public GroupController(IGroupService groupService, IMapper mapper, ILogger<GroupController> logger, IUserService userService)
        {
            _groupService = groupService;
            _mapper = mapper;
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// Get groups
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetGroups([FromQuery] QueryParams queryParams)
        {
            var request = HttpContext.Request;
            foreach (var param in request.Query)
            {
                if (param.Key.Equals("pageNumber", StringComparison.OrdinalIgnoreCase) ||
                    param.Key.Equals("pageSize", StringComparison.OrdinalIgnoreCase) ||
                    param.Key.Equals("sortBy", StringComparison.OrdinalIgnoreCase) ||
                    param.Key.Equals("sortDirection", StringComparison.OrdinalIgnoreCase))
                    continue;
                queryParams.Filters[param.Key] = param.Value.ToString();
            }
            var groups = await _groupService.GetPagedAsync(queryParams);
            var groupVMs = _mapper.Map<PagedResponse<GroupVM>>(groups);
            return Ok(groupVMs);
            // return Ok(groups);
        }

        /// <summary>
        /// Get group by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/detail")]
        public async Task<ActionResult> GetGroupById(Guid id)
        {
            var group = await _groupService.GetGroupDetailAsync(id);
            return Ok(group);
        }

        /// <summary>
        /// Add global group (IsGlobal = 1)
        /// </summary>
        /// <param name="groupVM"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CreateGroup([FromBody] GroupVM groupVM)
        {
            try
            {
                Group group = _mapper.Map<Group>(groupVM);
                var result = await _groupService.AddAsync(group);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateGroup {msg}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update group
        /// </summary>
        /// <param name="id"></param>
        /// <param name="groupVM"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateGroup(Guid id, [FromBody] GroupVM groupVM)
        {
            try
            {
                await _groupService.UpdateAsync(id, _mapper.Map<CreateGroupVM>(groupVM));
                return Ok("Updated group successfully");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Delete group
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteGroup(Guid id)
        {
            try
            {
                bool result = await _groupService.DeleteAsync(id);
                if (result)
                {
                    return Ok("Deleted group successfully");
                }
                return BadRequest("Delete group failed");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Add user to group
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        [HttpPost("add-user-to-group")]
        public async Task<ActionResult> AddUsersToGroup(Guid groupId, List<Guid> userIds)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int updatedRecords = await _groupService.AddUserToGroup(groupId, userIds);
            if (updatedRecords >= 0)
            {
                return Ok("Added user to group sucessfully");
            }
            else
            {
                return BadRequest("Some error occured");
            }
        }

        /// <summary>
        /// Get users in group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        [HttpGet("get-users-in-group")]
        public async Task<ActionResult<List<UserVM>>> GetUsersInGroup(string groupName)
        {
            var users = await _groupService.GetUsersInGroup(groupName);
            return Ok(users);
        }

        /// <summary>
        ///	Fetch user in global groups
        /// </summary>
        /// <returns></returns>
        [HttpGet("fetch-all-global-users")]
        public async Task<List<UserVM>> FetchUserInGlobalGroups()
        {
            var users = await _groupService.FetchUserInGlobalGroups();
            return users;
        }

        [AllowAnonymous]
        [HttpGet("get-dpia-users")]
        public async Task<List<DPIAUserVM>> GetUserForDPIA()
        {
            var groups = new Dictionary<string, string>
    {
        { "QA Manager", PermissionGroup.QAManager },
        { "IT Manager", PermissionGroup.IT },
        { "CTO/CIO", PermissionGroup.CTO_CIO }
    };

            var userDict = new Dictionary<Guid, DPIAUserVM>();

            foreach (var group in groups)
            {
                var users = await _groupService.GetUsersInGroup(group.Value);
                foreach (var user in users)
                {
                   
                    if (!userDict.TryGetValue(user.Id, out var dpiaUser))
                    {
                        dpiaUser = new DPIAUserVM
                        {
                            Id = user.Id,
                            FullName = user.FullName,
                            Email = user.Email,
                            GroupNames = new List<string>()
                        };
                        userDict.Add(user.Id, dpiaUser);
                    }

                    dpiaUser.GroupNames.Add(group.Key);
                }
            }

            return userDict.Values.ToList();
        }
        // [HttpPost("update-users-in-group/{groupId}")]
        // public async Task<ActionResult> UpdateUserInGroup(Guid groupId, [FromBody] List<Guid> userIds)
        // {
        // 	if (!ModelState.IsValid)
        // 	{
        // 		return BadRequest(ModelState);
        // 	}

        /// <summary>
        /// Update users in group
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        [HttpPut("update-users-in-group")]
        public async Task<ActionResult> UpdateUserInGroup(Guid groupId, [FromBody] List<Guid> userIds)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _groupService.UpdateUserInGroup(groupId, userIds);
                return Ok("Updated users in group successfully");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Remove user from group
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        [HttpDelete("remove-user-from-group")]
        public async Task<ActionResult> RemoveUserFromGroup(Guid groupId, [FromBody] List<Guid> userIds)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _groupService.DeleteUserFromGroup(groupId, userIds);
                return Ok("Removed user from group sucessfully");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("users-not-in-group/{groupId:guid}")]
        public async Task<ActionResult<List<UserVM>>> GetUsersNotInGroup(Guid groupId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            List<User> users = (await _userService.FindAsync(u => !u.Groups.Any(g => g.Id == groupId))).ToList();
            List<UserVM> data = _mapper.Map<List<UserVM>>(users);
            return data;
        }
        
    }
}
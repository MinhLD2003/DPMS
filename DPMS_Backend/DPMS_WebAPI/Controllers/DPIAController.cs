using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using DPMS_WebAPI.Builders;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Comment;
using DPMS_WebAPI.ViewModels.DPIA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for DPIA-related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DPIAController : ControllerBase
    {
        private readonly IDPIAService _dpiaService;
        private readonly ILogger<DPIAController> _logger;
        private readonly IEventMessageBuilder _messageBuilder;
        private readonly IdentityService _identityService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dpiaService"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        /// <param name="messageBuilder"></param>
        /// <param name="identityService"></param>
        public DPIAController(IDPIAService dpiaService, IMapper mapper, ILogger<DPIAController> logger, IEventMessageBuilder messageBuilder, IdentityService identityService)
        {
            _dpiaService = dpiaService;
            _mapper = mapper;
            _logger = logger;
            _messageBuilder = messageBuilder;
            _identityService = identityService;
        }

        /// <summary>
        /// Get DPIAs
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> GetDPIAs([FromQuery] QueryParams queryParams)
        {
            try
            {
                var request = HttpContext.Request;
                foreach (var param in request.Query)
                {
                    if (param.Key.Equals("pageNumber", StringComparison.OrdinalIgnoreCase) ||
                        param.Key.Equals("pageSize", StringComparison.OrdinalIgnoreCase) ||
                        param.Key.Equals("sortBy", StringComparison.OrdinalIgnoreCase) ||
                        param.Key.Equals("sortDirection", StringComparison.OrdinalIgnoreCase))
                        continue;
                        
                    // Ensure we're adding filter values correctly
                    if (!string.IsNullOrEmpty(param.Value))
                    {
                        queryParams.Filters[param.Key] = param.Value.ToString();
                    }
                }
                
                var dpiaVMs = await _dpiaService.GetDPIAs(queryParams, User);
                return Ok(dpiaVMs);
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogWarning("Unauthorized access attempt to DPIA list: {Message}", e.Message);
                return Unauthorized(new { message = e.Message ?? "You are not authorized to access this resource" });
            }
            catch (ArgumentException e)
            {
                _logger.LogWarning("Invalid query parameters for DPIA list: {Message}", e.Message);
                return BadRequest(new { message = e.Message ?? "Invalid query parameters" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving DPIA list");
                return BadRequest(new { message = e.Message ?? "An error occurred while retrieving the DPIA list" });
            }
        }

        [HttpGet("dpia-detail/{id}")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> GetDPIADetail(Guid id)
        {
            try
            {
                var dpia = await _dpiaService.GetDPIAByIdAsync(id);
                return Ok(dpia);
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogWarning("Unauthorized access attempt to DPIA details: {Message}", e.Message);
                return Unauthorized(new { message = e.Message ?? "You are not authorized to access this resource" });
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogWarning("DPIA not found: {Message}", e.Message);
                return NotFound(new { message = e.Message ?? "DPIA not found" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving DPIA detail for ID {Id}", id);
                return BadRequest(new { message = e.Message ?? "An error occurred while retrieving the DPIA details" });
            }
        }
      
        /// <summary>
        /// Get DPIA comments
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/comments")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> GetComments(Guid id)
        {
            try
            {
                var comments = await _dpiaService.GetCommentsAsync(id, User);
                return Ok(comments);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Get DPIA history
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/history")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> GetHistory(Guid id)
        {
            try
            {
                var history = await _dpiaService.GetHistoryAsync(id, User);
                return Ok(history);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Add DPIA
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> AddDPIA([FromForm] DPIACreateVM model)

        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    // manually perform model binding for DPIAResponsibility (fucking ASP.NET Core Form)
                    // string responsibilitiesJson = Request.Form[nameof(DPIACreateVM.Responsibilities)];
                    // List<DPIAResponsibilityCreateVM> responsibilities = new List<DPIAResponsibilityCreateVM>();
                    // if (!string.IsNullOrEmpty(responsibilitiesJson))
                    // {
                    //    responsibilities = JsonSerializer.Deserialize<List<DPIAResponsibilityCreateVM>>(responsibilitiesJson, new JsonSerializerOptions
                    //    {
                    //        PropertyNameCaseInsensitive = true,
                    //    });
                    // }

                    // model.Responsibilities = responsibilities;

                    DPIA? result = await _dpiaService.AddAsync(model, User);
                    if (result == null) // add DPIA failed
                    {
                        _logger.LogError("Add DPIA failed");
                        return BadRequest("Add DPIA failed");
                    }

                    return Ok();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Update DPIA
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> UpdateDPIA(Guid id, DPIAUpdateVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    var existingDPIA = await _dpiaService.GetByIdAsync(id);
                    if (existingDPIA == null)
                    {
                        return NotFound($"DPIA with ID {id} not found");
                    }
                    var dpia = _mapper.Map<DPIA>(existingDPIA);

                    dpia.Title = model.Title;
                    dpia.Description = model.Description;
                    dpia.DueDate = model.DueDate.Value;
                    await _dpiaService.UpdateAsync(dpia);
                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Get DPIA members
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/members")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> GetMembers(Guid id)
        {
            try
            {
                var members = await _dpiaService.GetMembersAsync(id, User);
                return Ok(members);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet("members-for-dpia")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> GetMembersForDPIA()
        {
            try
            {
                var members = await _dpiaService.GetUsersForDPIA();
                return Ok(members);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    
        /// <summary>
        /// Update DPIA members
        /// </summary>
        /// <param name="id"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        [HttpPut("{id}/members")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> UpdateMembers(Guid id, List<DPIAMemberCreateVM> members)
        {
            try
            {
                await _dpiaService.UpdateMembersAsync(id, members);
                return Ok();
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogWarning("Unauthorized access attempt to update members: {Message}", e.Message);
                return Unauthorized(new { message = e.Message ?? "You are not authorized to perform this action" });
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogWarning("DPIA or member not found: {Message}", e.Message);
                return NotFound(new { message = e.Message ?? "DPIA or member not found" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating members for DPIA ID {Id}", id);
                return BadRequest(new { message = e.Message ?? "An error occurred while updating members" });
            }
        }

        /// <summary>
        /// Add DPIA members
        /// </summary>
        /// <param name="id"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        [HttpPost("{id}/members")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> AddMembers(Guid id, List<DPIAMemberCreateVM> members)
        {
            try
            {
                await _dpiaService.AddMembersAsync(id, members);
                return Ok();
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogWarning("Unauthorized access attempt to add members: {Message}", e.Message);
                return Unauthorized(new { message = e.Message ?? "You are not authorized to perform this action" });
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogWarning("DPIA not found: {Message}", e.Message);
                return NotFound(new { message = e.Message ?? "DPIA not found" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error adding members to DPIA ID {Id}", id);
                return BadRequest(new { message = e.Message ?? "An error occurred while adding members" });
            }
        }

        [HttpPut("{id}/update-members-responsibilities")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> UpdateMembersResponsibilities(Guid id, List<DPIAResponsibilityVM> responsibilityVMs)
        {
            try
            {
                await _dpiaService.UpdateMemberResponsibilitiesAsync(id, responsibilityVMs);
                return Ok();
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogWarning("Unauthorized access attempt to update responsibilities: {Message}", e.Message);
                return Unauthorized(new { message = e.Message ?? "You are not authorized to perform this action" });
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogWarning("DPIA or responsibility not found: {Message}", e.Message);
                return NotFound(new { message = e.Message ?? "DPIA or responsibility not found" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating member responsibilities for DPIA ID {Id}", id);
                return BadRequest(new { message = e.Message ?? "An error occurred while updating responsibilities" });
            }
        }

        /// <summary>
        /// Add DPIA comment
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// 
        [HttpPost("{id}/comments")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> AddComment(Guid id, [FromBody] string content)
        {
            try
            {
                var comment = new AddCommentVM
                {
                    ReferenceId = id,
                    UserId = _identityService.GetCurrentUserId(),
                    Type = CommentType.DPIA,
                    Content = content
                };
                await _dpiaService.AddCommentAsync(comment, User);
                return Ok();
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Update DPIA comment
        /// </summary>
        /// <param name="dpiaId"></param>
        /// <param name="commentId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        [HttpPut("{dpiaId}/comments/{commentId}")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> UpdateComment(Guid dpiaId, Guid commentId, string content)
        {
            try
            {
                var comment = new AddCommentVM
                {
                    ReferenceId = dpiaId,
                    UserId = _identityService.GetCurrentUserId(),
                    Type = CommentType.DPIA,
                    Content = content
                };
                await _dpiaService.UpdateCommentAsync(commentId, comment, User);
                return Ok();
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> DeleteDPIA(Guid id)
        {
            try
            {
                await _dpiaService.DeleteAsync(id, User);
                return Ok();
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{dpiaId}/{responsibilityId}")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> GetResponsibility(Guid dpiaId, Guid responsibilityId)
        {
            try
            {
                var responsibility = await _dpiaService.GetResponsibilityAsync(dpiaId, responsibilityId, User);
                return Ok(responsibility);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{dpiaId}/{responsibilityId}/update-responsibility-members")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> UpdateResponsibilityMembers(Guid dpiaId, Guid responsibilityId, DPIAResponsibilityMemberUpdateVM vm)
        {
            try
            {
                await _dpiaService.UpdateResponsibilityMembersAsync(dpiaId, responsibilityId, vm.UserIds, vm.Pic, User);
                return Ok();
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Update status for a member responsibility
        /// This is used to update the status of a member's responsibility in the DPIA process.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPut("{id}/update-member-responsibility-status")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> UpdateMemberResponsibilityStatus(Guid id, [FromBody] MemberTaskStatus status)
        {
            try
            {
                await _dpiaService.UpdateMemberResponsibilityStatusAsync(id, status, User);
                return Ok();
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{dpiaId}/{responsibilityId}")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> DeleteResponsibility(Guid dpiaId, Guid responsibilityId)
        {
            try
            {
                await _dpiaService.DeleteResponsibilityAsync(dpiaId, responsibilityId, User);
                return Ok();
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        /// <summary>
        /// Update status for a DPIA responsibility
        /// </summary>
        /// <param name="id"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPut("{id}/update-responsibility-status")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> UpdateResponsibilityStatus(Guid id, DPIAResponsibilityUpdateStatusVM vm)
        {
            try
            {
                await _dpiaService.UpdateResponsibilityStatusAsync(id, vm, User);
                return Ok();
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{id}/restart-responsibility/{responsibilityId}")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> RestartResponsibility(Guid id, Guid responsibilityId)
        {
            try
            {
                await _dpiaService.RestartResponsibilityAsync(id, responsibilityId, User);
                return Ok();
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Upload documents for a DPIA responsibility
        /// </summary>
        /// <param name="dpiaId"></param>
        /// <param name="responsibilityId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("{dpiaId}/{responsibilityId}/upload-documents")]
        [Consumes("multipart/form-data")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> UploadDocuments(Guid dpiaId, Guid responsibilityId, IFormFile file)
        {
            try
            {
                await _dpiaService.UploadDocumentAsync(dpiaId, responsibilityId, file, User);
                return Ok("Document uploaded successfully");
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}/documents/{documentId}")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> DeleteDocument(Guid id, Guid documentId)
        {
            try
            {
                await _dpiaService.DeleteDocumentAsync(id, documentId, User);
                return Ok();
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Upload documents for a DPIA
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("{id}/upload-document")]
        [Consumes("multipart/form-data")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> UploadDocument(Guid id, IFormFile file)
        {
            try
            {
                await _dpiaService.UploadDocumentAsync(id, file, User);
                return Ok("Document uploaded successfully");
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost("{id}/start-dpia")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> StartDPIA(Guid id)
        {
            try
            {
                await _dpiaService.StartDPIAAsync(id, User);
                return Ok();
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("{id}/request-approval")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> RequestApproval(Guid id)
        {
            try
            {
                await _dpiaService.RequestApprovalAsync(id, User);
                return Ok();
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("{id}/approve")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> Approve(Guid id)
        {
            try
            {
                await _dpiaService.ApproveAsync(id, User);
                return Ok();
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize(Policy = Policies.FeatureRequired)]
        [HttpPost("{id}/reject")]
        public async Task<ActionResult> Reject(Guid id)
        {
            try
            {
                await _dpiaService.RejectAsync(id, User);
                return Ok();
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("{id}/restart")]
        public async Task<ActionResult> Restart(Guid id)
        {
            try
            {
                await _dpiaService.RestartAsync(id, User);
                return Ok();
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }



    }
}
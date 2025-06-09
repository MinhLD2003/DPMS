using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.IssueTicket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for IssueTicket-related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.FeatureRequired)]
    public class IssueTicketController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IIssueTicketService _issueTicketService;
        private readonly IIssueTicketDocumentService _documentService;
        private readonly IUserService _userService;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="issueTicketService"></param>
        /// <param name="documentService"></param>
        public IssueTicketController(IMapper mapper, IIssueTicketService issueTicketService, IIssueTicketDocumentService documentService , IUserService userService)
        {
            _mapper = mapper;
            _issueTicketService = issueTicketService;
            _documentService = documentService;
            _userService = userService;
        }

        /// <summary>
        /// Create issue ticket
        /// </summary>
        /// <param name="issueTicketVM"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateIssueTicket([FromForm] IssueTicketVM issueTicketVM, List<IFormFile> files)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var issueTicket = _mapper.Map<IssueTicket>(issueTicketVM);
                issueTicket.Id = Guid.NewGuid();
                var result = await _issueTicketService.CreateIssueTicket(issueTicket, files);

                if (result == null)
                {
                    return BadRequest("Failed to create issue ticket.");
                }

                return CreatedAtAction(nameof(GetIssueTicketById), new { id = result.Id }, result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get issue tickets
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetIssueTickets([FromQuery] QueryParams queryParams)
        {
            // Get the current user's ID
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(currentUserId, out Guid userGuid))
            {
                return BadRequest("Invalid user ID format");
            }

            // Check if the user is in any of the admin permission groups
            var isAdmin = await _userService.CheckUserInGroup(userGuid, PermissionGroup.ADMIN_DPMS);
            var isBOD = await _userService.CheckUserInGroup(userGuid, PermissionGroup.CTO_CIO);
            var isDPO = await _userService.CheckUserInGroup(userGuid, PermissionGroup.DPO);

            if (queryParams.Filters == null)
            {
                queryParams.Filters = new Dictionary<string, string>();
            }

            bool hasAdminAccess = (isAdmin.IsSuccess && isAdmin.Value )|| (isBOD.IsSuccess && isBOD.Value )|| (isDPO.IsSuccess && isDPO.Value);

            // For regular users (non-admin), force filter by their own ID
            if (!hasAdminAccess)
            {
                queryParams.Filters["CreatedById"] = userGuid.ToString();
            }

            var request = HttpContext.Request;
            foreach (var param in request.Query)
            {
                if (param.Key.Equals("pageNumber", StringComparison.OrdinalIgnoreCase) ||
                    param.Key.Equals("pageSize", StringComparison.OrdinalIgnoreCase) ||
                    param.Key.Equals("sortBy", StringComparison.OrdinalIgnoreCase) ||
                    param.Key.Equals("sortDirection", StringComparison.OrdinalIgnoreCase) ||
                    (!hasAdminAccess && param.Key.Equals("CreatedById", StringComparison.OrdinalIgnoreCase)))
                    continue;
                queryParams.Filters[param.Key] = param.Value.ToString();
            }

            var result = await _issueTicketService.GetPagedAsync(queryParams, i => i.ExternalSystem!, i => i.Documents);

            var mappedResult = _mapper.Map<List<IssueTicketVM>>(result.Data);

            var response = new PagedResponse<IssueTicketVM>
            {
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages,
                TotalRecords = result.TotalRecords,
                Data = mappedResult
            };

            return Ok(response);
        }




        /// <summary>
        /// Get issue ticket by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIssueTicketById(Guid id)
        {
            try
            {
                var result = await _issueTicketService.GetDetailAsync(id, ticket => ticket.Documents, t => t.CreatedBy!, t => t.LastModifiedBy!);
                if (result == null)
                {
                    return NotFound($"Issue ticket with ID {id} not found.");
                }
                var issueTicketVM = _mapper.Map<IssueTicketVM>(result);
                return Ok(issueTicketVM);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update issue ticket
        /// </summary>
        /// <param name="id"></param>
        /// <param name="issueTicketVM"></param>
        /// <param name="newFiles"></param>
        /// <param name="removedFiles"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIssueTicket(Guid id, [FromForm] IssueTicketCreateVM issueTicketVM, [FromForm] List<IFormFile> newFiles, [FromForm] List<string> removedFiles)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var existingTicket = await _issueTicketService.GetByIdAsync(id);
                if (existingTicket == null)
                {
                    return NotFound($"Issue ticket with ID {id} not found.");
                }
                _mapper.Map(issueTicketVM, existingTicket, opts => opts.BeforeMap((src, dest) => src.Id = dest.Id));
                List<IssueTicketDocument> uploadedDocuments;
                try
                {
                    uploadedDocuments = await _issueTicketService.UpdateIssueTicketFilesOnS3(id, newFiles, removedFiles);
                }
                catch (Exception)
                {
                    return StatusCode(500, "Error uploading files.");
                }
                var result = await _issueTicketService.UpdateAsync(existingTicket);
                if (result == null)
                {
                    return BadRequest("Failed to update issue ticket.");
                }
                var updatedTicket = await _issueTicketService.GetByIdAsync(id);
                return Ok(updatedTicket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update issue ticket status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="issueTicketVM"></param>
        /// <returns></returns>
        [HttpPut("{id}/update-status")]
        public async Task<IActionResult> UpdateIssueTicketStatus(Guid id, [FromBody] IssueTicketStatus IssueTicketStatus)
        {
            try
            {
                var existingTicket = await _issueTicketService.GetByIdAsync(id);
                if (existingTicket == null)
                {
                    return NotFound($"Issue ticket with ID {id} not found.");
                }
                existingTicket.IssueTicketStatus = IssueTicketStatus;
                var isUpdated = await _issueTicketService.UpdateAsync(existingTicket);
                if (isUpdated == null)
                {
                    return BadRequest("Failed to update issue ticket status.");
                }

                var updatedTicket = await _issueTicketService.GetByIdAsync(id);

                // Return the full updated ticket details
                return Ok(updatedTicket);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete issue ticket
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssueTicket(Guid id)
        {
            try
            {
                var existingTicket = await _issueTicketService.GetDetailAsync(id, x => x.Documents);

                if (existingTicket == null)
                {
                    return NotFound($"Issue ticket with ID {id} not found.");
                }
                if (existingTicket.Documents != null && existingTicket.Documents.Any())
                {
                    await _documentService.BulkDeleteAsync(existingTicket.Documents);
                    await _documentService.DeleteIssueTicketFilesOnS3(existingTicket.Documents);
                }
                var isDeleted = await _issueTicketService.DeleteAsync(id);
                if (!isDeleted)
                {
                    return BadRequest("Failed to delete issue ticket.");
                }

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

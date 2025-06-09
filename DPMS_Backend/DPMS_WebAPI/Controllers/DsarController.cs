using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.DSAR;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DPMS_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.Authenticated)]
    public class DsarController : ControllerBase
    {
        private readonly IDsarService _dsarService;
        private readonly IConsentService _consentService;
        private readonly ILogger<DsarController> _logger;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dsarService"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        /// <param name="consentService"></param>
        public DsarController(IDsarService dsarService, IMapper mapper, ILogger<DsarController> logger, IConsentService consentService, IUserService userService)
        {
            _dsarService = dsarService;
            _mapper = mapper;
            _logger = logger;
            _consentService = consentService;
            _userService = userService;
        }

        /// <summary>
        /// Submit DSAR using token => and vm
        /// </summary>
        /// <param name="token"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost("{token}")]
        public async Task<IActionResult> SubmitDSAR(string token, SubmitDsarVM vm)
        {
            try
            {
                vm.RequiredResponse = DateTime.UtcNow.AddDays(3);
                vm.Status = DSARStatus.Submitted;
                vm.ExternalSystemId = await _consentService.GetSystemFromToken(token);
                var dsar = _mapper.Map<DSAR>(vm);
                var dsarCreated = await _dsarService.AddAsync(dsar);
                return CreatedAtAction(nameof(GetDsar), new { id = dsarCreated.Id }, dsarCreated);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error submitting dsar");
                return Problem("An error occurred while updating the purpose.");
            }
        }

        /// <summary>
        /// Submit DSAR using token => and vm
        /// </summary>
        /// <param name="token"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api-cjs/submitJS/{uString}")]
        public async Task<IActionResult> SubmitDSARJS(string uString,SubmitDsarVM vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                if(ConsentUtils.DecryptEmail(uString).ToLower() != vm.RequesterEmail.ToLower()) 
                {
                    return Ok();
                }
                vm.RequiredResponse = DateTime.UtcNow.AddDays(3);
                vm.Status = DSARStatus.Submitted;
                var dsar = _mapper.Map<DSAR>(vm);
                var dsarCreated = await _dsarService.AddAsync(dsar);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error submitting dsar");
                return Problem("An error occurred while updating the purpose.");
            }
        }

        /// <summary>
        /// Create DSAR 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateDsar(SubmitDsarVM vm)
        {
            try
            {
                vm.RequiredResponse = DateTime.UtcNow.AddDays(3);
                vm.Status = DSARStatus.Submitted;
                var dsar = _mapper.Map<DSAR>(vm);
                var dsarCreated = await _dsarService.AddAsync(dsar);
                return CreatedAtAction(nameof(GetDsar), new { id = dsarCreated.Id }, dsarCreated);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error submitting dsar");
                return Problem("An error occurred while updating the purpose.");
            }
        }
        /// <summary>
        /// Get list dsars by systemId
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        [HttpGet("get-list/{systemId:guid}")]
        public async Task<IActionResult> GetDsars(Guid systemId, [FromQuery] QueryParams queryParams)
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
                    queryParams.Filters[param.Key] = param.Value.ToString();
                }

                var dsars = await _dsarService.GetPagedAsync(queryParams);
                var dsarVMs = _mapper.Map<List<DsarVM>>(dsars.Data);

                var response = new PagedResponse<DsarVM>
                {
                    PageNumber = dsars.PageNumber,
                    PageSize = dsars.PageSize,
                    TotalPages = dsars.TotalPages,
                    TotalRecords = dsars.TotalRecords,
                    Data = dsarVMs.Where(d => d.ExternalSystemId == systemId).ToList()
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving data subject access request");
                return Problem("An error occurred while fetching data subject access request.");
            }
        }
        /// <summary>
        /// Get dsar by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetDsar(Guid id)
        {
            try
            {
                var dsar = await _dsarService.GetByIdAsync(id);

                var dsarVM = _mapper.Map<DsarVM>(dsar);
                return dsarVM != null ? Ok(dsarVM) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error retrieving data subject access request with ID {id}");
                return Problem("An error occurred while fetching the data subject access request.");
            }
        }
        /// <summary>
        /// Update status
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateStatus(UpdateStatusVM vm)
        {
            // Fetch the DSAR request by ID
            var dsar = await _dsarService.GetByIdAsync(vm.Id);
            if (dsar == null)
            {
                return NotFound($"DSAR request with ID {vm.Id} not found.");
            }

            // Update the status
            dsar.Status = vm.Status;

            // Set the completed date if the status is "Completed" or "Rejected"
            if (vm.Status == DSARStatus.Completed || vm.Status == DSARStatus.Rejected)
            {
                dsar.CompletedDate = DateTime.Now;
            }

            // Update the DSAR in the database
            await _dsarService.UpdateAsync(dsar);
            return Ok(new { Message = "Status updated successfully" });
        }
        
        /// <summary>
        /// Bulk update status
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPut("bulk-update-status")]
        public async Task<IActionResult> BulkUpdateStatus(List<UpdateStatusVM> vm)
        {
            try
            {
                // Update the DSAR in the database
                await _dsarService.BulkUpdatetStatus(vm);
                return Ok(new { Message = "Status updated successfully" });
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to bulk update status: {e}");
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("download-import-template")]
        public async Task<ActionResult> DownloadImportTemplate()
        {
            // get current logged in user
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User? user = await _userService.GetByIdAsync(userId);

            Stream templateData = await _dsarService.DownloadImportTemplate(user);

            return File(templateData, "application/octet-stream", $"DSAR_Import_Template.xlsx");
        }

        [HttpPost("import")]
        public async Task<ActionResult> DoImportDsar(IFormFile importFile)
        {
            // Validate file
            if (importFile == null || importFile.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            // Define allowed file types
            var allowedExtensions = new[] { ".xlsx" };
            var allowedMimeTypes = new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" };

            string fileExtension = Path.GetExtension(importFile.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file format. Allowed formats: Excel.");
            }

            if (!allowedMimeTypes.Contains(importFile.ContentType))
            {
                return BadRequest("Invalid MIME type.");
            }

            // File size limit (e.g., 10MB)
            const long maxFileSize = 10 * 1024 * 1024;
            if (importFile.Length > maxFileSize)
            {
                return BadRequest("File size exceeds the 10MB limit.");
            }

            Stream data = importFile.OpenReadStream();
            Result importResult = await _dsarService.DoImportDsarAsync(data);
            if (importResult.IsSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest(importResult.Errors);
            }
        }
    }
}

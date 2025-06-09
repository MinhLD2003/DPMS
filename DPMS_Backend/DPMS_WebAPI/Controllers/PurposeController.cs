using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Purpose;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for Purpose-related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.FeatureRequired)]
    public class PurposeController : ControllerBase
    {
        private readonly IPurposeService _purposeService;
        private readonly ILogger<PurposeController> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="purposeService"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public PurposeController(IPurposeService purposeService, IMapper mapper, ILogger<PurposeController> logger)
        {
            _purposeService = purposeService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get purposes
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetPurposes([FromQuery] QueryParams queryParams)
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

                var purposes = await _purposeService.GetPagedAsync(queryParams, p => p.CreatedBy);
                var purposesVM = _mapper.Map<List<ListPurposeVM>>(purposes.Data);

                var response = new PagedResponse<ListPurposeVM>
                {
                    PageNumber = purposes.PageNumber,
                    PageSize = purposes.PageSize,
                    TotalPages = purposes.TotalPages,
                    TotalRecords = purposes.TotalRecords,
                    Data = purposesVM
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving purposes");
                return Problem("An error occurred while fetching purposes.");
            }
        }

        /// <summary>
        /// Get purpose by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetPurpose(Guid id)
        {
            try
            {
                var purpose = await _purposeService.GetByIdAsync(id);

                var purposeVM = _mapper.Map<PurposeVM>(purpose);
                return purposeVM != null ? Ok(purposeVM) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error retrieving purpose with ID {id}");
                return Problem("An error occurred while fetching the purpose.");
            }
        }

        /// <summary>
        /// Create a new purpose
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreatePurpose([FromBody] PurposeVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var purpose = _mapper.Map<Purpose>(model);
                purpose.Status = PurposeStatus.Draft;
                await _purposeService.AddAsync(purpose);
                return CreatedAtAction(nameof(GetPurpose), new { id = purpose.Id }, purpose);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
           
        }

        /// <summary>
        /// Update a purpose
        /// Purpose can only be updated if it is in Draft status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePurpose(Guid id, [FromBody] PurposeVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // Find purpose
                var existingPurpose = await _purposeService.GetDetailAsync(id, p => p.ExternalSystems);
                if (existingPurpose == null) return NotFound();
                
                // Cannot update to Draft
                if (model.Status == PurposeStatus.Draft)
                    return BadRequest("Cannot update status back to Draft.");

                _mapper.Map(model, existingPurpose);

                // remove any externalsystem purpose link if inactive
                if (model.Status == PurposeStatus.Inactive)
                {
                    existingPurpose.ExternalSystems.Clear();
                }
                await _purposeService.UpdateAsync(existingPurpose);
                return Ok(existingPurpose);
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"Error updating purpose with ID {id}");
                return Problem("An error occurred while updating the purpose.");
            }
        }

        /// <summary>
        /// Update purpose status
        /// Purpose status can only be updated to Active or Inactive
        /// </summary>
        /// <param name="id"></param>
        /// <param name="statusUpdateVM"></param>
        /// <returns></returns>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] PurposeVM statusUpdateVM)
        {
            try
            {
                var purpose = await _purposeService.GetByIdAsync(id);
                if (purpose == null) return NotFound();
                if (statusUpdateVM.Status == PurposeStatus.Draft)
                    return BadRequest("Cannot update status back to Draft.");

                purpose.Status = statusUpdateVM.Status;
                await _purposeService.UpdateAsync(purpose);
                var purposeVM = _mapper.Map<PurposeVM>(purpose);
                return Ok(purpose);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error updating status for purpose ID {id}");
                return Problem("An error occurred while updating the status.");
            }
        }

        /// <summary>
        /// Delete purpose
        /// Purpose can only be deleted if it is in Draft status
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurpose(Guid id)
        {
            try
            {
                var purpose = await _purposeService.GetByIdAsync(id);
                if (purpose == null) return NotFound();
                if (purpose.Status != PurposeStatus.Draft)
                    return BadRequest("Cannot delete an active purpose.");

                await _purposeService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error deleting purpose with ID {id}");
                return Problem("An error occurred while deleting the purpose.");
            }
        }

    }
}

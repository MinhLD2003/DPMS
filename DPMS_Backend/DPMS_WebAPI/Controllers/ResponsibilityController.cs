using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.IssueTicket;
using DPMS_WebAPI.ViewModels.Responsibility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for Responsibility-related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.FeatureRequired)]
    public class ResponsibilityController : ControllerBase
    {
        private readonly IResponsibilityService _responsibilityService;
        private readonly ILogger<ResponsibilityController> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="responsibilityService"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public ResponsibilityController(IResponsibilityService responsibilityService, IMapper mapper, ILogger<ResponsibilityController> logger)
        {
            _responsibilityService = responsibilityService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get responsibilities
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetResponsibilitys()
        {
            try
            {
                var responsibilitys = await _responsibilityService.GetAllAsync();
                return Ok(responsibilitys);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving responsibilitys");
                return Problem("An error occurred while fetching responsibilitys.");
            }
        }

        /// <summary>
        /// Get responsibility by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetResponsibility(Guid id)
        {
            try
            {
                var responsibility = await _responsibilityService.GetByIdAsync(id);

                return responsibility != null ? Ok(responsibility) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error retrieving responsibility with ID {id}");
                return Problem("An error occurred while fetching the responsibility.");
            }
        }

        /// <summary>
        /// Create a new responsibility
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateResponsibility([FromBody] CreateResponsibilityVM model)
        {
           
            var responsibility = _mapper.Map<Responsibility>(model);
            await _responsibilityService.AddAsync(responsibility);

            return CreatedAtAction(nameof(GetResponsibility), new { id = responsibility.Id }, responsibility);
        }

        /// <summary>
        /// Update responsibility
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResponsibility(Guid id, [FromBody] CreateResponsibilityVM model)
        {
            try
            {
                var existingResponsibility = await _responsibilityService.GetByIdAsync(id);
                if (existingResponsibility == null) return NotFound();
                //if (existingResponsibility.Status != ResponsibilityStatus.Draft)
                //    return BadRequest("Responsibility is put into active cannot be update");

                _mapper.Map(model, existingResponsibility);

                await _responsibilityService.UpdateAsync(existingResponsibility);

                return Ok(existingResponsibility);
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"Error updating responsibility with ID {id}");
                return Problem("An error occurred while updating the responsibility.");
            }
        }

        /// <summary>
        /// Delete responsibility
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResponsibility(Guid id)
        {
            try
            {
                var responsibility = await _responsibilityService.GetByIdAsync(id);
                if (responsibility == null) return NotFound();
                
                await _responsibilityService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error deleting responsibility with ID {id}");
                return Problem("An error occurred while deleting the responsibility.");
            }
        }

    }
}

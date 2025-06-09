using AutoMapper;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Risk;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DPMS_WebAPI.Constants;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DPMS_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.FeatureRequired)]
    public class RiskController : ControllerBase
    {
        private readonly IRiskService _riskService;
        private readonly ILogger<RiskController> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="privacyPolicyService"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public RiskController(IRiskService riskService, IMapper mapper, ILogger<RiskController> logger)
        {
            _riskService = riskService;
            _mapper = mapper;
            _logger = logger;
        }
        /// <summary>
        /// Get risk register
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetRisks([FromQuery] QueryParams queryParams)
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
                var risks = await _riskService.GetPagedAsync(queryParams, p => p.CreatedBy);
                var riskListVMs = _mapper.Map<List<RiskListVM>>(risks.Data);
                var response = new PagedResponse<RiskListVM>
                {
                    PageNumber = risks.PageNumber,
                    PageSize = risks.PageSize,
                    TotalPages = risks.TotalPages,
                    TotalRecords = risks.TotalRecords,
                    Data = riskListVMs
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving risks");
                return Problem("An error occurred while fetching risks.");
            }
        }

        /// <summary>
        /// Update risk resolve status: after mitigation, etc. 
        /// </summary>
        /// <param name="id">RiskID</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("resolve-risk/{id:guid}")]
        public async Task<ActionResult> ResolveRisk(Guid id, RiskResolveVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existRisk = await _riskService.GetByIdAsync(id);
                if (existRisk == null) return NotFound();
                _mapper.Map(model, existRisk);
                await _riskService.UpdateAsync(existRisk);
                var riskVM = _mapper.Map<RiskVM>(existRisk);
                return Ok(existRisk);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error updating risk with ID {id}");
                return Problem("An error occurred while updating the risk.");
            }
        }

        /// <summary>
        /// Get Risk by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetRisk(Guid id)
        {
            try
            {
                var risk = await _riskService.GetByIdAsync(id);
                var riskVM = _mapper.Map<RiskVM>(risk);
                return riskVM != null ? Ok(riskVM) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error retrieving risk with ID {id}");
                return Problem("An error occurred while fetching the risk.");
            }
        }
        /// <summary>
        /// Register a risk
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateRisk([FromBody] RiskVM model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var risk = _mapper.Map<Risk>(model);
                await _riskService.AddAsync(risk);
                return CreatedAtAction(nameof(GetRisk), new { id = risk.Id }, risk);
            }catch(Exception e)
            {
                _logger.LogError(e, $"Error cretaing risk ");
                return Problem("An error occurred while fetching the risk.");
            }
            
        }

        /// <summary>
        /// Update a risk
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRisk(Guid id, [FromBody] RiskVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var existRisk = await _riskService.GetByIdAsync(id);
                if (existRisk == null) return NotFound();
                _mapper.Map(model, existRisk);
                await _riskService.UpdateAsync(existRisk);
                var riskVM = _mapper.Map<RiskVM>(existRisk);
                return Ok(existRisk);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error updating risk with ID {id}");
                return Problem("An error occurred while updating the risk.");
            }
        }

        /// <summary>
        /// Delete risk
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRisk(Guid id)
        {
            try
            {
                var risk = await _riskService.GetByIdAsync(id);
                if (risk == null) return NotFound();
                await _riskService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error deleting risk with ID {id}");
                return Problem("An error occurred while deleting the risk.");
            }
        }

        [HttpGet("export")]
        public async Task<ActionResult> Export()
        {
            Stream exportData = await _riskService.ExportAsync();
            if (exportData == null)
            {
                return NotFound();
            }

            return File(exportData, "application/octet-stream", $"Risk_Export_{DateTime.Now}.xlsx");
        }
    }
}

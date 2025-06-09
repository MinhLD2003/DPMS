using AutoMapper;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.ViewModels.Purpose;
using DPMS_WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DPMS_WebAPI.ViewModels.PrivacyPolicy;
using DPMS_WebAPI.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;
using DPMS_WebAPI.Constants;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for privacy policy-related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.Authenticated)]
    public class PrivacyPolicyController : ControllerBase
    {
        private readonly IPrivacyPolicyService _privacyPolicyService;
        private readonly ILogger<PrivacyPolicyController> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="privacyPolicyService"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public PrivacyPolicyController(IPrivacyPolicyService privacyPolicyService, IMapper mapper, ILogger<PrivacyPolicyController> logger)
        {
            _privacyPolicyService = privacyPolicyService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get list policies
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetPolicies([FromQuery] QueryParams queryParams)
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

                var policies = await _privacyPolicyService.GetPagedAsync(queryParams);
                var policiesVM = _mapper.Map<List<ListPolicyVM>>(policies.Data);

                var response = new PagedResponse<ListPolicyVM>
                {
                    PageNumber = policies.PageNumber,
                    PageSize = policies.PageSize,
                    TotalPages = policies.TotalPages,
                    TotalRecords = policies.TotalRecords,
                    Data = policiesVM
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving policies");
                return Problem("An error occurred while fetching policies.");
            }
        }

        /// <summary>
        /// Create privacy policy
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<IActionResult> CreatePolicy([FromBody] PolicyVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var existingPolicy = await _privacyPolicyService.FindAsync(p => p.PolicyCode == model.PolicyCode);
                if (existingPolicy.Any())
                {
                    return BadRequest(new { success = false, message = "Policy code already exists." });
                }
                var privacyPolicy = _mapper.Map<PrivacyPolicy>(model);
                privacyPolicy.Status = PolicyStatus.Draft;
                await _privacyPolicyService.AddAsync(privacyPolicy);

                return CreatedAtAction(nameof(GetPolicy), new { id = privacyPolicy.Id }, privacyPolicy);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error create policy");
                return Problem("An error occurred while creating the policy.");
            }

        }
        /// <summary>
        /// Get active policy
        /// Only 1 active policy at a time => active policy can be view
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("/api-cjs/get-policy")]
        public async Task<IActionResult> GetPolicyJS()
        {
            try
            {
                var privacyPolicies = await _privacyPolicyService.FindAsync(p => p.Status == PolicyStatus.Active);
                var privacyPolicy = privacyPolicies.FirstOrDefault();
                var policyVM = _mapper.Map<PolicyVM>(privacyPolicy);
                return policyVM != null ? Ok(policyVM) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error retrieving privacy policy");
                return Problem("An error occurred while fetching the privacy policy.");
            }
        }

        /// <summary>
        /// Get active policy
        /// Only 1 active policy at a time => active policy can be view
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-policy")]
        public async Task<IActionResult> GetPolicy()
        {
            try
            {
                var privacyPolicies = await _privacyPolicyService.FindAsync(p => p.Status == PolicyStatus.Active);
                var privacyPolicy = privacyPolicies.FirstOrDefault();
                var policyVM = _mapper.Map<PolicyVM>(privacyPolicy);
                return policyVM != null ? Ok(policyVM) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error retrieving privacy policy");
                return Problem("An error occurred while fetching the privacy policy.");
            }
        }

        /// <summary>
        /// Get policy by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetPolicy(Guid id)
        {
            try
            {
                var privacyPolicy = await _privacyPolicyService.GetByIdAsync(id);
                var policyVM = _mapper.Map<PolicyVM>(privacyPolicy);
                return policyVM != null ? Ok(policyVM) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error retrieving privacy policy with ID {id}");
                return Problem("An error occurred while fetching the privacy policy.");
            }
        }

        /// <summary>
        /// Update policy 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] PolicyVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // ip user trying to active a policy, we must ensure only one active policy exist
                if (model.Status == PolicyStatus.Active)
                {
                    await _privacyPolicyService.ActivePolicy(id);
                }

                var existingPolicy = await _privacyPolicyService.GetByIdAsync(id);
                if (existingPolicy == null) return NotFound();
                _mapper.Map(model, existingPolicy);

                await _privacyPolicyService.UpdateAsync(existingPolicy);
                var purposeVM = _mapper.Map<PolicyVM>(existingPolicy);
                return Ok(existingPolicy);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error updating privacy policy with ID {id}");
                return Problem("An error occurred while updating the privacy policy.");
            }
        }

        /// <summary>
        /// Only 1 active policy at a time => active policy can be view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("/get-active/{id:Guid}")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<IActionResult> ActivePolicy(Guid id)
        {
            try
            {
                //Find all active => deactive and active the poilcy with id
                await _privacyPolicyService.ActivePolicy(id);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error active privacy policy with ID {id}");
                return Problem("An error occurred while active the privacy policy.");
            }
        }

        /// <summary>
        /// Delete privacy policy, status must be false to delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<IActionResult> DeletePolicy(Guid id)
        {
            try
            {
                var privacyPolicy = await _privacyPolicyService.GetByIdAsync(id);
                if (privacyPolicy == null) return NotFound();
                if (privacyPolicy.Status == PolicyStatus.Active)
                    return BadRequest("Cannot delete an active policy.");
                await _privacyPolicyService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error deleting privacy policy with ID {id}");
                return Problem("An error occurred while deleting privacy policy.");
            }
        }
    }
}

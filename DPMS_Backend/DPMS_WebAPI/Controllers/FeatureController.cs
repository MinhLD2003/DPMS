using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Feature;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for Feature-related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.FeatureRequired)]
    public class FeatureController : ControllerBase
    {
        private readonly IFeatureService _featureService;
        private readonly IMapper _mapper;
        private readonly ILogger<FeatureController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="featureService"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public FeatureController(IFeatureService featureService, IMapper mapper, ILogger<FeatureController> logger)
        {
            _featureService = featureService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get list of nested features
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet("get-list-features/{groupId}")]
        public async Task<IActionResult> GetListNestedFeatures(Guid groupId)
        {
            try
            {
                var features = await _featureService.GetListNestedFeatures(groupId);
                return Ok(features);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Get features by group id
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFeatures([FromQuery] QueryParams queryParams)
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
                var FeatureList = await _featureService.GetPagedAsync(queryParams);


                return Ok(FeatureList);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Get feature detail
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeature(Guid id)
        {
            try
            {
                var feature = await _featureService.GetDetailAsync(id , x=> x.Children);
                return Ok(feature);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Create a new feature
        /// </summary>
        /// <param name="featureVM"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateFeature([FromBody] CreateFeatureVM featureVM)
        {
            try
            {
                Feature feature = _mapper.Map<Feature>(featureVM);
                await _featureService.AddAsync(feature);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Update a feature
        /// </summary>
        /// <param name="id"></param>
        /// <param name="featureVM"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeature(Guid id, [FromBody] FeatureVM featureVM)
        {
            try
            {
                Feature feature = _mapper.Map<Feature>(featureVM);
                await _featureService.UpdateAsync(feature);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Delete a feature
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeature(Guid id)
        {
            try
            {
                await _featureService.DeleteAsync(id);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Assign Features (permission) to a Group
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("add-feature-to-group")]
        public ActionResult AddFeatureToGroup([FromBody] FeatureGroupAssignmentModel model)
        {
            try
            {
                _featureService.AddFeaturesToGroup(model.FeatureIds, model.GroupId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured {message}", ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
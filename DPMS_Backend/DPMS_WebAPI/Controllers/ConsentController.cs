using AutoMapper;
using DPMS_WebAPI.AuthPolicies;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Consent;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Security.Claims;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for consent-related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ConsentController : ControllerBase
    {
        private readonly ILogger<ConsentController> _logger;
        private readonly IConsentService _consentService;
        private readonly IPrivacyPolicyService _privacyPolicyService;
        private readonly IExternalSystemService _externalSystemService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string DPMS_BASE_URL;
        private readonly string FE_BASE_URL;
        private readonly string DPMS_API_KEY;
        private const string DPMS_SYSTEM_ID = "1C9B5694-F2FF-4648-B50F-A93BBE30D941";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="externalSystemService"></param>
        /// <param name="privacyPolicyService"></param>
        /// <param name="consentService"></param>
        /// <param name="mapper"></param>
        public ConsentController(ILogger<ConsentController> logger,
            IExternalSystemService externalSystemService,
            IConsentService consentService,
            IPrivacyPolicyService privacyPolicyService,
            IMapper mapper,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _consentService = consentService;
            _externalSystemService = externalSystemService;
            _mapper = mapper;
            _privacyPolicyService = privacyPolicyService;
            _configuration = configuration;

            DPMS_BASE_URL = _configuration["Urls:BeBasePath"] ?? "http://localhost:5000";
            FE_BASE_URL = _configuration["Urls:FeBasePath"] ?? "http://localhost:5173";
            DPMS_API_KEY = _configuration["DpmsApiKey"] ?? "DPMS_API_KEY";

            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// System id should be encrypt currently not
        /// </summary>
        /// <param name="uniqueIdentifier"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("get-banner/{uniqueIdentifier}/{token}")]
        public async Task<IActionResult> GetConsentBanner(string uniqueIdentifier, string token)
        {
            try
            {
                var email = ConsentUtils.DecryptEmail(uniqueIdentifier);
                var systemId = await _consentService.GetSystemFromToken(token);

                // validate consent's token
                bool isTokenValid = await _consentService.ValidateConsentToken(token);
                if (!isTokenValid)
                {
                    return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid consent token", detail: "Invalid consent token");
                }

                // Lay ra purpose cua SystemId
                var systemPurpose = await _externalSystemService.GetSystemPurposesAsync(systemId);

                // Lấy consent 
                var consent = await _consentService.GetConsentByEmail(email);

                if (consent == null)
                    return Ok(systemPurpose.Select(sp => new
                    {
                        PurposeId = sp.Id,
                        PurposeName = sp.Name,
                        PurposeDescription = sp.Description,
                        Status = "null"
                    }).ToList());

                // Tạo dictionary từ consent.Purposes để tra cứu trạng thái
                var consentDict = consent.Purposes.ToDictionary(cp => cp.PurposeId, cp => cp.Status);

                // Trả kết quả => nếu id có trong consent Dict thì lấy status từ dict còn không thì trả null
                // => nếu có null thì FAP phải gọi API tạo link
                var result = systemPurpose.Select(sp => new
                {
                    PurposeId = sp.Id,
                    PurposeName = sp.Name,
                    PurposeDescription = sp.Description,
                    Status = consentDict.ContainsKey(sp.Id) ? consentDict[sp.Id].ToString() : null
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, "An error occurred while processing the consent request.");
            }
        }

        /// <summary>
        /// NOTE THAT this API is for DPMS internally use only
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("dpms-consent/{email}")]
        [Authorize(Policy = Policies.Authenticated)]
        public async Task<ActionResult> OnlyForDPMS(string email)
        {
            // validate email match with authenticated user
            string authenticatedUserEmail = User.FindFirstValue(ClaimTypes.Email);
            if (authenticatedUserEmail != email)
            {
                return Problem(statusCode: StatusCodes.Status403Forbidden,
                    detail: "You are not allowed to get consent of another person");
            }

            // Get Authorization header from incoming request
            string authorizationHeader = Request.Headers["Authorization"];

            HttpClient httpClient = _httpClientFactory.CreateClient();

            // Create a request for consent check
            var consentRequest = new HttpRequestMessage(HttpMethod.Get, $"{DPMS_BASE_URL}/api-consent/get-consent/{email}/{DPMS_SYSTEM_ID}");
            consentRequest.Headers.Add("X-API-KEY", DPMS_API_KEY);

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                consentRequest.Headers.Add("Authorization", authorizationHeader);
            }

            // using HttpClient to internally call [HttpGet("/api-consent/get-consent/{email}/{systemId}")]
            //string checkUserConsentUrl = $"{DPMS_BASE_URL}/api-consent/get-consent/{email}/{DPMS_SYSTEM_ID}";
            HttpResponseMessage consentResponse = await httpClient.SendAsync(consentRequest);
            if (!consentResponse.IsSuccessStatusCode)
            {
                // return error
                string errorMsg = await consentResponse.Content.ReadAsStringAsync();
                _logger.LogError("Error calling consent API: {StatusCode} {Reason}", consentResponse.StatusCode, errorMsg);
                return StatusCode((int)consentResponse.StatusCode, "Error retrieving user consent.");
            }

            string consentJson = await consentResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Consent response: {Response}", consentJson);

            List<DpmsConsentVM> purposes;
            try
            {
                purposes = JsonConvert.DeserializeObject<List<DpmsConsentVM>>(consentJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize consent response");
                return StatusCode(500, "Error parsing consent data.");
            }

            // Call internal API to generate a consent link
            // Request a consent link
            var linkRequest = new HttpRequestMessage(HttpMethod.Get, $"{DPMS_BASE_URL}/api-consent/get-link/{email}/{DPMS_SYSTEM_ID}");
            linkRequest.Headers.Add("X-API-KEY", DPMS_API_KEY);

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                linkRequest.Headers.Add("Authorization", authorizationHeader);
            }

            HttpResponseMessage linkResponse = await httpClient.SendAsync(linkRequest);

            if (!linkResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Error getting consent link: {StatusCode} {Reason}", linkResponse.StatusCode, linkResponse.ReasonPhrase);
                return StatusCode((int)linkResponse.StatusCode, "Could not generate consent link.");
            }

            string consentLink = await linkResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Consent link generated: {Link}", consentLink);

            bool hasMissingConsent = purposes.Any(p => string.Equals(p.Status, "null", StringComparison.OrdinalIgnoreCase));

            if (hasMissingConsent)
            {
                return Ok(new
                {
                    consented = false,
                    missingPurposes = purposes.Where(p => string.Equals(p.Status, "null", StringComparison.OrdinalIgnoreCase)).ToList(),
                    consentLink
                });
            }

            return Ok(new
            {
                consented = true,
                purposes,
                consentLink
            });

            // demo json response: gpt please deserialize it to C# object
            //[
            //    {
            //        "purposeId": "75601f27-a7ef-470e-9775-cf2d1cb4af35",
            //        "purposeName": "Truyền thông, quảng bá Dịch vụ và chương trình giáo dục",
            //        "status": "null"
            //    },
            //    {
            //        "purposeId": "d2d4e250-30f1-488e-8044-e34984a7e103",
            //        "purposeName": "Phân tích hiệu quả và cải thiện chất lượng Dịch vụ",
            //        "status": "True"
            //    },
            //    {
            //        "purposeId": "ea1f0f91-1eb1-44aa-a054-eacd5bd9edc1",
            //        "purposeName": "Đề xuất Dịch vụ cho Chủ thể dữ liệu",
            //        "status": "True"
            //    },
            //    {
            //        "purposeId": "3cda706c-8089-48d3-af23-fa472de69588",
            //        "purposeName": "Trao đổi và tương tác với Chủ thể dữ liệu",
            //        "status": "True"
            //    }
            //]

            // if any of the status is "null" string, using HttpClient to call [HttpGet("/api/get-link/{email}/{systemId}")]

            // return response depending on whether user consented or not
        }

        /// <summary>
        /// Need API Key
        /// </summary>
        /// <param name="email"></param>
        /// <param name="systemId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("/api-consent/get-link/{email}/{systemId}")]
        public async Task<IActionResult> GetConsentLink(string email, Guid systemId)
        {
            // Gen Create token
            var tokenString = await _consentService.CreateConsentToken(systemId);
            // Check mail
            var datasubject = await _consentService.FindAsync(c => c.Email == email);

            if (datasubject == null || !datasubject.Any())
            {
                // Gen uinque identify
                var uniqueIdentifier = ConsentUtils.EncryptEmail(email);
                return Ok($"{FE_BASE_URL}/public-banner/{uniqueIdentifier}/{tokenString}");
            }

            var dataSubjectId = datasubject.First().DataSubjectId;
            return Ok($"{FE_BASE_URL}/public-banner/{dataSubjectId}/{tokenString}");
        }

        /// <summary>
        /// Get consent currently using email without protection but the email should be encrypt / mask beforre transmit
        /// </summary>
        /// <param name="email"></param>
        /// <param name="systemId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("/api-consent/get-consent/{email}/{systemId}")]
        public async Task<IActionResult> GetConsent(string email, Guid systemId)
        {
            try
            {
                // Lay ra purpose cua SystemId
                var systemPurpose = await _externalSystemService.GetSystemPurposesAsync(systemId);

                // Lấy consent 
                var consent = await _consentService.GetConsentByEmail(email);

                if (consent == null)
                    return Ok(systemPurpose.Select(sp => new
                    {
                        PurposeId = sp.Id,
                        PurposeName = sp.Name,
                        Status = "null"
                    }).ToList());

                // Tạo dictionary từ consent.Purposes để tra cứu trạng thái
                var consentDict = consent.Purposes.ToDictionary(cp => cp.PurposeId, cp => cp.Status);

                // Trả kết quả => nếu id có trong consent Dict thì lấy status từ dict còn không thì trả null
                // => nếu có null thì FAP phải gọi API tạo link
                var result = systemPurpose.Select(sp => new
                {
                    PurposeId = sp.Id,
                    PurposeName = sp.Name,
                    Status = consentDict.ContainsKey(sp.Id) ? consentDict[sp.Id].ToString() : null
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, "An error occurred while processing the consent request.");
            }
        }

        /// <summary>
        /// Get list of purposes
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        [HttpGet("consent-log")]
        public async Task<IActionResult> GetConsentLog([FromQuery] QueryParams queryParams)
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

                var consentList = await _consentService.GetConsentLogWithPurpose(queryParams);
                var consents = _mapper.Map<List<ConsentLogVM>>(consentList.Data);

                var response = new PagedResponse<ConsentLogVM>
                {
                    PageNumber = consentList.PageNumber,
                    PageSize = consentList.PageSize,
                    TotalPages = consentList.TotalPages,
                    TotalRecords = consentList.TotalRecords,
                    Data = consents
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
        /// Get list of purposes get at that system
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        [HttpGet("consent-log/{systemId:guid}")]
        public async Task<IActionResult> GetConsentLog(Guid systemId, [FromQuery] QueryParams queryParams)
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

                var consentList = await _consentService.GetConsentLogWithPurpose(queryParams);
                var consents = consentList.Data.Where(c => c.ExternalSystemId == systemId).ToList();
                var data = _mapper.Map<List<ConsentLogVM>>(consents);

                var response = new PagedResponse<ConsentLogVM>
                {
                    PageNumber = consentList.PageNumber,
                    PageSize = consentList.PageSize,
                    TotalPages = consentList.TotalPages,
                    TotalRecords = consentList.TotalRecords,
                    Data = data
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
        /// Pollute SubmitConsentVM object with data from request (IP address, user-agent, etc. )
        /// </summary>
        /// <param name="consentVM"></param>
        private void PolluteRequestData(SubmitConsentVM consentVM)
        {
            consentVM.ConsentIp = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            consentVM.ConsentUserAgent = Request.Headers[HeaderNames.UserAgent].ToString();
        }

        /// <summary>
        /// Submit consent. E.g FAP has 4 purposes (in total of 6 purposes), and user only accept 2/4 purposes.
        /// Then it means we would INSERT 6 records into ConsentPurpose table, anf 1 record to Consent table.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> SubmitConsent(SubmitConsentRequestVM requestVM)
        {
            try
            {
                var privacyPolicy = (await _privacyPolicyService.FindAsync(p => p.Status == PolicyStatus.Active)).FirstOrDefault();
                if (privacyPolicy == null) return BadRequest("no active policy");
                SubmitConsentVM consentVM = new SubmitConsentVM
                {
                    PrivacyPolicyId = privacyPolicy.Id,
                    ConsentPurposes = requestVM.ConsentPurposes,
                    ConsentMethod = ConsentMethod.WebForm,
                    DataSubjectId = requestVM.UniqueIdentifier,
                    Email = ConsentUtils.DecryptEmail(requestVM.UniqueIdentifier ?? "Unknown"),
                    ExternalSystemId = await _consentService.GetSystemFromToken(requestVM.TokenString ?? "Unknown"),
                    IsWithdrawn = false
                };

                // pollute consentVM object with data from request (Ip, UserAgent, etc. )
                PolluteRequestData(consentVM);


                await _consentService.SubmitConsent(consentVM);
                await _consentService.UpdateToken(requestVM.TokenString ?? "Unknown");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Export consent logs to Excel
        /// </summary>
        /// <param name="systemId"></param>
        /// <returns></returns>
        //[Authorize(Policy = Policies.FeatureRequired)]
        [HttpGet("export-logs")]
        public async Task<ActionResult> ExportConsentLog(Guid? systemId)
        {
            Stream exportData = await _consentService.ExportConsentLog(systemId);
            if (exportData == null)
            {
                return NotFound();
            }

            return File(exportData, "application/octet-stream", $"Consent_Logs_{DateTime.Now}.xlsx");
        }

        [HttpGet("download-template/{systemId:guid}")]
        public async Task<ActionResult> DownloadImportTemplate(Guid systemId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                Stream templateData = await _consentService.DownloadImportTemplateAsync(systemId);
                return File(templateData, "application/octet-stream", $"Consent_Import_Template.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(ex.Message);
            }
        }

        [HttpPost("import-consent")]
        public async Task<ActionResult> ImportConsent(IFormFile importFile)
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

            try
            {
                Stream data = importFile.OpenReadStream();
                Result importResult = await _consentService.DoImportConsentAsync(data);
                if (importResult.IsSuccess)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(importResult.Errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Submit consent. E.g FAP has 4 purposes (in total of 6 purposes), and user only accept 2/4 purposes.
        /// Then it means we would INSERT 6 records into ConsentPurpose table, anf 1 record to Consent table.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api-cjs/consent-js")]
        public async Task<ActionResult> SubmitConsentJS(SubmitConsentVM requestVM)
        {
            try
            {
                requestVM.Email = ConsentUtils.DecryptEmail(requestVM.Email);
                // Get current policy
                var privacyPolicy = (await _privacyPolicyService.FindAsync(p => p.Status == PolicyStatus.Active)).FirstOrDefault();
                if (privacyPolicy == null) return BadRequest("no active policy");

                // Map current policy
                requestVM.PrivacyPolicyId = privacyPolicy.Id;

                // pollute consentVM object with data from request (Ip, UserAgent, etc. )
                PolluteRequestData(requestVM);

                // Save the consent request
                await _consentService.SubmitConsent(requestVM);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// System id should be encrypt currently not
        /// </summary>
        /// <param name="uniqueIdentifier"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("/api-cjs/banner/{uniqueIdentifier}/{systemID}")]
        public async Task<IActionResult> GetConsentBannerJS(string uniqueIdentifier, string systemID)
        {
            try
            {
                var email = ConsentUtils.DecryptEmail(uniqueIdentifier);
                var systemId = Guid.Parse(systemID);
                // Lay ra purpose cua SystemId
                var systemPurpose = await _externalSystemService.GetSystemPurposesAsync(systemId);

                // Lấy consent 
                var consent = await _consentService.GetConsentByEmail(email);
                

                if (consent == null || consent.Purposes.Any(p => p.Status == null) || consent.IsWithdrawn == true)
                    return Ok(systemPurpose.Select(sp => new
                    {
                        PurposeId = sp.Id,
                        PurposeName = sp.Name,
                        PurposeDescription = sp.Description,
                        Status = "null"
                    }).ToList());

                // Tạo dictionary từ consent.Purposes để tra cứu trạng thái
                var consentDict = consent.Purposes.ToDictionary(cp => cp.PurposeId, cp => cp.Status);

                // Trả kết quả => nếu id có trong consent Dict thì lấy status từ dict còn không thì trả null
                // => nếu có null thì FAP phải gọi API tạo link
                var result = systemPurpose.Select(sp => new
                {
                    PurposeId = sp.Id,
                    PurposeName = sp.Name,
                    PurposeDescription = sp.Description,
                    Status = consentDict.ContainsKey(sp.Id) ? consentDict[sp.Id].ToString() : null
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, "An error occurred while processing the consent request.");
            }
        }
        /// <summary>
        /// Get uString: unique identify for user
        /// </summary>
        /// <param name="uniqueIdentifier"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("/api-consent/get-ustring/{email}")]
        public async Task<IActionResult> GetUniqueString(string email)
        {
            // Check mail
            var datasubject = await _consentService.FindAsync(c => c.Email == email);
            if (datasubject == null || !datasubject.Any())
            {
                // Gen uinque identify
                var uniqueIdentifier = ConsentUtils.EncryptEmail(email);
                return Ok(uniqueIdentifier);
            }

            var dataSubjectId = datasubject.FirstOrDefault(d => d.IsWithdrawn == false).DataSubjectId;
            return Ok(dataSubjectId);
        }
    }
}

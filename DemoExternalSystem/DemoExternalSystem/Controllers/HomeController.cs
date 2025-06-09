using DemoExternalSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DemoExternalSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private const string API_KEY = "Oa1AiMCdAQfo73h8weY9P0yTx97rbd7Y";
        private const string EXTERNAL_SYSTEM_ID = "6b057ad0-4edd-41cc-93a2-81da5f5569c8";
        private const string DPMS_BASE_URL = "https://localhost:7226"; // Replace with actual DPMS API URL

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient();
        }

        private ConsentStatus ParseConsentStatus(string jsonResponse)
        {
            try
            {
                // Try to parse JSON
                List<PurposeDto> purposes = JsonSerializer.Deserialize<List<PurposeDto>>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                ConsentStatus consentStatus = new ConsentStatus();
                consentStatus.Purposes = purposes;

                Console.WriteLine("JSON is valid.");

                // Check for any "null" strings
                bool hasNullString = false;

                foreach (var purpose in purposes)
                {
                    if (purpose.PurposeId == "null" ||
                        purpose.PurposeName == "null" ||
                        purpose.Status == "null" ||
                        purpose.Status == null)
                    {
                        hasNullString = true;
                        break;
                    }
                }

                if (hasNullString)
                {
                    Console.WriteLine("Found at least one value with string 'null'.");
                    consentStatus.Status = false;
                }
                else
                {
                    Console.WriteLine("No value with string 'null' found.");
                    consentStatus.Status = true;
                }

                if(purposes.Count == 0)
                {
                    Console.WriteLine("Empty purpose");
                    consentStatus.Status = false;
                }

                return consentStatus;
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Invalid JSON: " + ex.Message);
                ConsentStatus consentStatus = new ConsentStatus
                {
                    Status = false,
                    Purposes = new List<PurposeDto>()
                };

                return consentStatus;
            }
        }

        public async Task<IActionResult> Index(string email)
        {
            _logger.LogInformation("Starting consent check for email: {Email}", email);

            if (string.IsNullOrEmpty(email))
            {
                //_logger.LogWarning("No email provided. Returning error view.");
                //ViewData["ErrorMessage"] = "Email is required.";
                //return View("Error");

                return View("Index");
            }

            try
            {
                // Step 1: Check consent status
                var consentUrl = $"{DPMS_BASE_URL}/api-consent/get-consent/{email}/{EXTERNAL_SYSTEM_ID}";
                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", API_KEY);

                _logger.LogInformation("Calling API: {ConsentUrl}, with X-API-KEY = {API_KEY}", consentUrl, API_KEY);
                var consentResponse = await _httpClient.GetAsync(consentUrl);

                if (!consentResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get consent status. HTTP Status: {StatusCode}, {message}", consentResponse.StatusCode, consentResponse.RequestMessage.ToString());
                    ViewData["ErrorMessage"] = "Error fetching consent status.";
                    return View("Error");
                }

                var consentStatus = await consentResponse.Content.ReadAsStringAsync();
                _logger.LogInformation("Consent API Response: {Response}", consentStatus);
                ConsentStatus consent = ParseConsentStatus(consentStatus);

                if (consent.Status)
                {
                    _logger.LogInformation("User {Email} has already consented. Showing UserConsented view.", email);
                    return View("UserConsented");
                }

                // Step 2: Get link if user has not consented
                var linkUrl = $"{DPMS_BASE_URL}/api-consent/get-link/{email}/{EXTERNAL_SYSTEM_ID}";
                _logger.LogInformation("User {Email} has NOT consented. Calling API: {LinkUrl}", email, linkUrl);

                var linkResponse = await _httpClient.GetAsync(linkUrl);
                if (!linkResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get consent link. HTTP Status: {StatusCode}", linkResponse.StatusCode);
                    ViewData["ErrorMessage"] = "Error fetching consent link.";
                    return View("Error");
                }

                var link = await linkResponse.Content.ReadAsStringAsync();
                _logger.LogInformation("Consent link received: {Link}", link);

                // Step 3: Redirect user to the consent link
                _logger.LogInformation("Redirecting user {Email} to {Link}", email, link);
                return Redirect(link);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Exception occurred while calling DPMS API.");
                ViewData["ErrorMessage"] = "An error occurred while connecting to the consent server.";
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Index method.");
                ViewData["ErrorMessage"] = "An unexpected error occurred.";
                return View("Error");
            }
        }
    }
}
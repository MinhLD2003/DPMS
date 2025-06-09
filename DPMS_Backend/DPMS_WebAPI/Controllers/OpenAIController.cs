using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI;
using System.Text.Json;
using System.Text;
namespace DPMS_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public OpenAIController(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [HttpPost("Ask")]
        public async Task<IActionResult> Ask([FromBody] string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                return BadRequest("Prompt cannot be empty.");
            }

            string apiKey = _configuration["OpenAI:ApiKey"]; // Read Google AI Studio API key from configuration
            string yourFineTunedModelId = "tunedModels/dpms-ai-assistant-20-73uuncgx9ze"; // <--- PUT YOUR MODEL ID HERE
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/{yourFineTunedModelId}:generateContent";

            try
            {
                // Prepare the request payload for Google's Gemini API
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                // Serialize the request body to JSON
                var jsonRequestBody = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                // Add API key to the request
                var request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}?key={apiKey}")
                {
                    Content = content
                };

                // Send the request to Google's Gemini API
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Error calling Google AI Studio API.");
                }

                // Parse the response
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                // Extract the generated text from the response
                string result = jsonResponse
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return Ok(new { response = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}

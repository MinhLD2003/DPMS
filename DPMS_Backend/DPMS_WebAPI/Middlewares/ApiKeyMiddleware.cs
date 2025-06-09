using DPMS_WebAPI.Interfaces.Services;

namespace DPMS_WebAPI.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeaderName = "X-API-KEY";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Resolve the scoped service from the current request's service provider
            var externalSystemService = context.RequestServices.GetRequiredService<IExternalSystemService>();          

            // Check if the API key header exists
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("API Key is missing.");
                return;
            }

            // Validate the API key
            bool isValid = await externalSystemService.ValidateApiKeyAsync(extractedApiKey);
            if (!isValid)
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Invalid API Key.");
                return;
            }

            // Continue processing if valid
            await _next(context);
        }
    }
}

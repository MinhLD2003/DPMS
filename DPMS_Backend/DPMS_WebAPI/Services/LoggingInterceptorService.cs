using DPMS_WebAPI.Models;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;

namespace DPMS_WebAPI.Services
{
    public class LoggingInterceptorService : IHttpLoggingInterceptor
    {
        private readonly DPMSLoggingContext _logContext;
        private readonly ILogger<LoggingInterceptorService> _logger;

        public LoggingInterceptorService(DPMSLoggingContext logContext, ILogger<LoggingInterceptorService> logger)
        {
            _logContext = logContext;
            _logger = logger;
        }

        public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
        {
            var user = logContext.HttpContext.User;
            HttpRequest request = logContext.HttpContext.Request;
            bool isRequestAuthenticated = user?.Identity?.IsAuthenticated ?? false;

            // only logs authenticated http requests (reducing database storage)
            if (isRequestAuthenticated)
            {
                HttpLog log = new HttpLog
                {
                    TraceId = logContext.HttpContext.TraceIdentifier,
                    Email = user.FindFirst(ClaimTypes.Email)?.Value,
                    HttpType = 1,
                    Method = request.Method,
                    IpAddress = request.HttpContext.Connection?.RemoteIpAddress?.ToString(),
                    Url = request.Path.ToString(),
                    UserAgent = request.Headers[HeaderNames.UserAgent].ToString()
                };

                _logContext.HttpLogs.AddAsync(log);
                _logContext.SaveChangesAsync();
            }

            return default;
        }

        public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
        {
            var user = logContext.HttpContext.User;
            bool isRequestAuthenticated = user?.Identity?.IsAuthenticated ?? false;
            HttpResponse response = logContext.HttpContext.Response;

            // only logs authenticated http requests (reducing database storage)
            if (isRequestAuthenticated)
            {
                HttpLog log = new HttpLog
                {
                    TraceId = logContext.HttpContext.TraceIdentifier,
                    ResponseStatus = response.StatusCode,
                    HttpType = 2
                };

                _logContext.HttpLogs.AddAsync(log);
                _logContext.SaveChangesAsync();
            }

            return default;
        }
    }
}

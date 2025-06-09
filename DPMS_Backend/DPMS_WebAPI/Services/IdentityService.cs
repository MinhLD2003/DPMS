using System.Security.Claims;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Services
{
    public class IdentityService
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityService(IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

		/// <summary>
		/// ThangHQ 21/02/2025 commented: HIGH ERROR POSSIBLITY on ClaimTypes.Email due to generated JWT does not has that claims
		/// </summary>
		/// <returns></returns>
		public async Task<User?> GetCurrentUserAsync()
        {
            var email = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null)
            {
                return null;
            }
            return await _userService.GetUserByEmailAsync(email);
        }
        public Guid GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                throw new UnauthorizedAccessException("Invalid user ID format.");
            }

            return parsedUserId;
        }   
    }
}
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for handling forgot password requests
    /// </summary>
    [Route("api/[controller]")]
	[ApiController]
	public class ForgotpasswordController : ControllerBase
	{
		private readonly IEmailService _emailService;
		private readonly AuthService _authService;
		private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="emailService"></param>
        /// <param name="authService"></param>
        /// <param name="userService"></param>
        public ForgotpasswordController(IEmailService emailService, AuthService authService, IUserService userService,  IConfiguration _configuration)
		{
			this._emailService = emailService;
			this._userService = userService;
			this._authService = authService;
			this._configuration = _configuration;
        }

		/// <summary>
		/// Send reset password email
		/// </summary>
		/// <param name="resetPasswordRequest"></param>
		/// <returns></returns>
		[HttpPost("forgot-password")]
		public async Task<ActionResult<bool>> ForgotPassword([FromBody] ForgotPasswordRequest? resetPasswordRequest)
		{
			if (resetPasswordRequest == null || string.IsNullOrWhiteSpace(resetPasswordRequest.Email))
			{
				return BadRequest("Invalid request. Email is required.");
			}

			User? user = await _userService.GetUserByEmailAsync(resetPasswordRequest.Email);

			if (user == null)
			{
				return NotFound("User not found.");
			}

            string token = _authService.GeneratePasswordResetToken(user.Email);
			string baseUrl = _configuration["Urls:FeBasePath"];
            bool emailSent = await _emailService.SendResetPasswordEmailAsync(user.Email, token, baseUrl);
			if (!emailSent)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Failed to send reset password email.");
			}
			return Ok(true);
		}

        /// <summary>
        /// Verify reset token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("verify")]
        public IActionResult VerifyResetToken([FromQuery] string token)
        {
            try
            {
                ClaimsPrincipal principal = _authService.GetPrincipalFromToken(token);
                string email = _authService.GetEmailFromResetToken(principal);
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest("Invalid reset token.");
                }

                return Ok(new { message = "Token is valid", email });
            }
            catch
            {
                return BadRequest("Invalid or expired token.");
            }
        }

        /// <summary>
        /// Set new password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("set-new-password")]
		public async Task<IActionResult> SetNewPassword([FromBody] ResetPasswordModel model)
		{
			if (model == null || string.IsNullOrWhiteSpace(model.Token) || string.IsNullOrWhiteSpace(model.NewPassword))
			{
				return BadRequest("Invalid request data.");
			}

			ClaimsPrincipal principal = _authService.GetPrincipalFromToken(model.Token);
			if (principal == null)
			{
				return BadRequest("Invalid or expired token.");
			}

			string email = principal.FindFirstValue(ClaimTypes.Email)!;
			if (string.IsNullOrEmpty(email))
			{
				return BadRequest("Invalid token payload.");
			}

			var user = await _userService.GetUserByEmailAsync(email);
			if (user == null)
			{
				return NotFound("User not found.");
			}
			byte[] salt = PasswordUtils.GenerateSalt();
			string password = PasswordUtils.HashPassword(model.NewPassword, Convert.ToBase64String(salt));

			User? resetResult = await _userService.UpdateUserPassword(password, Convert.ToBase64String(salt), email);
			if (resetResult == null)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Password reset failed.");
			}

			return Ok("Password reset successfully.");
		}

	}
}

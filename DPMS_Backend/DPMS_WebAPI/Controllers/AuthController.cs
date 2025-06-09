using AutoMapper;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DPMS_WebAPI.Controllers
{
	/// <summary>
	/// Controller responsible for authentication-related endpoints
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly AuthService _authService;
		private readonly IUserService _userService;
		private readonly ILogger<AuthController> _logger;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mapper"></param>
		/// <param name="authService"></param>
		/// <param name="logger"></param>
		/// <param name="userService"></param>
		public AuthController(IMapper mapper, AuthService authService, ILogger<AuthController> logger, IUserService userService)
		{
			_mapper = mapper;
			_authService = authService;
			_logger = logger;
			_userService = userService;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost("google")]
		public async Task<IActionResult> GoogleLogin([FromBody] AuthRequest request)
		{
			var user = await _authService.VerifyGoogleTokenAsync(request.Token);
			if (user == null)
			{
				// login failed
				_logger.LogWarning("Login google failed");
				return BadRequest(new { message = "Invalid token" });
			}

			// login success
			_logger.LogInformation("Google login for account {email} successfully", user.Email);
			var jwtToken = _authService.GenerateJwtToken(user);

			return Ok(new
			{
				message = "Login successful",
				token = jwtToken
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost("login")]
		public async Task<ActionResult> Login([FromBody] LoginVM request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			User? user = await _userService.GetUserByEmailAsync(request.Email);
			if (user == null)
			{
				_logger.LogInformation("Login failed for reason: \"Email {email} does not exists\"", request.Email);
				return BadRequest("Email does not exist");
			}

			if(user.Status == Enums.UserStatus.Deactivated)
			{
				_logger.LogInformation("Attempted to login {email} with deactivated status", request.Email);
				return BadRequest("Account is deactivated");
			}

			string salt = user.Salt!;
			string hashedPassword = user.Password!;

			if (string.IsNullOrEmpty(hashedPassword))
			{
				_logger.LogInformation("Login failed, account {email} does dot setup password yet", request.Email);
				return BadRequest("This account does not setup a password yet");
			}

			if (PasswordUtils.HashPassword(request.Password, salt) == hashedPassword) // login successfully
			{
				_logger.LogInformation("Login successfully {email}", request.Email);
				// update LastLogin timestamp
				await _userService.UpdateLastLoginTimeStamp(user);

				string jwtToken = _authService.GenerateJwtToken(user);

				return Ok(new
				{
					message = "Login successfully",
					token = jwtToken
				});
			}
			else
			{
				_logger.LogWarning("Login failed for account {email}", request.Email);
				return Unauthorized("Login failed");
			}
		}

		/// <summary>
		/// TODO: Email confirm
		/// 27/03/2025: Making this private this since this feature is no more active
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost("register")]
		private async Task<ActionResult> Register(RegisterVM model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState); // Return validation errors
			}

			User? user = await _userService.GetUserByEmailAsync(model.Email);
			if (user != null)
			{
				return Conflict("User already exists");
			}

			if (model.Password != model.ReTypePassword)
			{
				return BadRequest("Password does not match");
			}

			// hashing password
			byte[] salt = PasswordUtils.GenerateSalt();
			string password = PasswordUtils.HashPassword(model.Password, Convert.ToBase64String(salt));

			// set properties
			user = _mapper.Map<User>(model);
			user.Salt = Convert.ToBase64String(salt);
			user.Password = password;

			User addedUser = await _userService.AddAsync(user);

			return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
		}

		/// <summary>
		/// TODO: Implement email confirm
		/// </summary>
		/// <returns></returns>
		[HttpPost("logout")]
		public IActionResult Logout()
		{
			return Ok();
		}

		/// <summary>
		/// TODO: Implement refresh token
		/// </summary>
		/// <returns></returns>
		[HttpPost("refresh-token")]
		public IActionResult RefreshToken()
		{
			return Ok();
		}
	}
}

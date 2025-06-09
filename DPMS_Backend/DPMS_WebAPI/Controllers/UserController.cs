using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DPMS_WebAPI.Controllers
{
	/// <summary>
	/// Controller responsible for handling user-related requests
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class UserController : ControllerBase
	{
		private readonly ILogger<UserController> _logger;
		private readonly IUserService _userService;

		/// <summary>
		/// Manage users in the system, normally for admin or user manager
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="userService"></param>
		public UserController(ILogger<UserController> logger, IUserService userService)
		{
			_logger = logger;
			_userService = userService;
		}

		/// <summary>
		/// Change password for user
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[Authorize]
		[HttpPost("change-password")]
		public async Task<ActionResult> ChangePassword(ChangePasswordVM model)
		{
			string email = User.FindFirst(ClaimTypes.Email)!.Value;
			User? user = await _userService.GetUserByEmailAsync(email);

			try
			{
				await _userService.ChangePassword(email, model);
				_logger.LogWarning("Password for account {email} changed", email);

				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogWarning("Exception message {message}", ex.Message);
				return BadRequest(ex.Message);
			}
		}
	
		/// <summary>
		/// Get list of users
		/// </summary>
		/// <param name="queryParams"></param>
		/// <returns></returns>
		[Authorize(Policy = Policies.FeatureRequired)]
		[HttpGet]
		public async Task<ActionResult> GetUsers([FromQuery] QueryParams queryParams)
		{
			var users = await _userService.GetPagedAsync(queryParams);
			return Ok(users);
		}

		/// <summary>
		/// Get user by id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[Authorize(Policy = Policies.FeatureRequired)]
		[HttpGet("{id}")]
		public async Task<ActionResult> GetUserById(Guid id)
		{
			var user = await _userService.GetByIdAsync(id);
			return Ok(user);
		}

		/// <summary>
		/// Update user
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[Authorize(Policy = Policies.FeatureRequired)]
		[HttpPut("update-user-status")]
		public async Task<ActionResult> UpdateUserStatus(UpdateUserStatusVM model)
		{
			try
			{
				await _userService.UpdateUserStatus(model);
				return Ok("User status updated successfully");
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}
	}
}

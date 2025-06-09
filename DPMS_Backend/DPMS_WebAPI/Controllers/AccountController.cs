using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Auth;
using DPMS_WebAPI.ViewModels.Email;
using DPMS_WebAPI.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for account-related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.Authenticated)]
    public class AccountController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailTemplateService _emailTemplateService;

        /// <summary>
        /// Process personal information of user including change password, update profile, etc.
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="userService"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <param name="emailService"></param>
        /// <param name="emailTemplateService"></param>
        public AccountController(IMapper mapper,
            IUserService userService,
            IConfiguration configuration,
            ILogger<AccountController> logger,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService)
        {
            _mapper = mapper;
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        /// <summary>
        /// TODO: Validate
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult<CreateAccountVM>> CreateAccount(CreateAccountVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // check email exist
            if (await _userService.CheckUserExist(model.Email))
            {
                return Problem(statusCode: (int)HttpStatusCode.BadRequest, detail: "Email already exists");
            }

            byte[]? salt = PasswordUtils.GenerateSalt();
            string? rawPassword = model.Password;

            // if password is not set, generate random password
            if (rawPassword == null || rawPassword.Length == 0)
            {
                rawPassword = PasswordUtils.GeneratePassword(1, 6, 1);
            }

            // hashing password
            string hashedPassword = PasswordUtils.HashPassword(rawPassword, Convert.ToBase64String(salt));
            model.Password = hashedPassword;

            // create AccountCredentials object to send credentials using email
            AccountCredentials credentials = new AccountCredentials
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Password = rawPassword,
                Email = model.Email,
                DpmsLoginUrl = _configuration["Urls:FeBasePath"] ?? "https://localhost:5173"
            };

            try
            {
                User user = _mapper.Map<User>(model);
                user.Salt = model.Password == null ? null : Convert.ToBase64String(salt!); // store salt for account
                await _userService.AddAsync(user);

                // sending email contains credentials here
                await _emailTemplateService.SendAccountCredentialsAsync(credentials);

                return CreatedAtAction(nameof(CreateAccount), user); // TODO: Add Uri
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something wrong happen during creating new account");
                return BadRequest();
            }
        }


        /// <summary>
        /// Get list of accounts with pagination
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = Policies.FeatureRequired)]
        public async Task<ActionResult> GetAccountList([FromQuery] QueryParams queryParams)
        {
            var request = HttpContext.Request;
            foreach (var param in request.Query)
            {
                // Skip our reserved parameters
                if (param.Key.Equals("pageNumber", StringComparison.OrdinalIgnoreCase) ||
                    param.Key.Equals("pageSize", StringComparison.OrdinalIgnoreCase) ||
                    param.Key.Equals("sortBy", StringComparison.OrdinalIgnoreCase) ||
                    param.Key.Equals("sortDirection", StringComparison.OrdinalIgnoreCase))
                    continue;

                queryParams.Filters[param.Key] = param.Value.ToString();
            }

            var result = await _userService.GetPagedAsync(queryParams, u => u.Groups);
            foreach (var user in result.Data) // remove local groups from result (since Account List Page only display global group)
            {
                user.Groups = user.Groups.Where(g => g.IsGlobal).ToList();
            }
            PagedResponse<UserListVM> response = _mapper.Map<PagedResponse<UserListVM>>(result);
            return Ok(response);
        }

        [Authorize(Policy = Policies.Authenticated)]
        [HttpGet("profile/{id:guid}")]
        public async Task<ActionResult<ProfileVM>> GetProfile(Guid id)
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // user may view their own profile. Only Admin may view all profiles
            var isAdmin = await _userService.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS);
            if (isAdmin.IsFailed) // if validation wrong (group does not exist, userId does not exist, etc. )
            {
                return BadRequest(isAdmin);
            }

            if (isAdmin.Value || id == userId) // CheckUserInGroup run successfully, check user's admin permission
            {
                var result = await _userService.GetUserProfileAsync(id);
                return (result.IsSuccess ? result.Value : BadRequest(result.Errors));
            }

            return Forbid(); // user is not admin, but they view other profile --> forbid
        }

        /// <summary>
        /// Update user profile. Any user may update their own profile, but admin may update all
        /// account profile
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        [Authorize(Policy = Policies.Authenticated)]
        [HttpPut("profile")]
        public async Task<ActionResult> UpdateProfile(UpdateProfileVM profile)
        {
            // Retrieve the UserId claim
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validate if the UserId claim is a valid GUID
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest("Invalid UserId claim format.");
            }

            User? loggedIn = await _userService.GetByIdAsync(userId);

            // user may view their own profile. Only Admin may view all profiles
            var isAdmin = await _userService.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS);
            if (isAdmin.IsFailed) // if validation wrong (group does not exist, userId does not exist, etc. )
            {
                return BadRequest(isAdmin);
            }

            var updatedUser = await _userService.GetUserByEmailAsync(profile.Email);
            if (updatedUser == null)
            {
                return BadRequest("Updated user does not exist");
            }

            if (isAdmin.Value || updatedUser.Id == userId) // if logged in user is admin or they are updating their own profile
            {
                // update user
                updatedUser.UserName = profile.UserName;
                updatedUser.FullName = profile.FullName;
                updatedUser.Dob = profile.Dob;

                await _userService.UpdateAsync(updatedUser);
                return Ok();
            }
            else
            {
                return Forbid();
            }
        }

    }
}

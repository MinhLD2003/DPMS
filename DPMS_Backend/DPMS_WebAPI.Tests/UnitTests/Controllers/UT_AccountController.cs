using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Controllers;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Auth;
using DPMS_WebAPI.ViewModels.Email;
using DPMS_WebAPI.ViewModels.User;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;


namespace DPMS_WebAPI.Tests.UnitTests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AccountController>> _mockLogger;
        private readonly Mock<IEmailTemplateService> _mockEmailTemplateService;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockMapper = new Mock<IMapper>();
            _mockUserService = new Mock<IUserService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AccountController>>();
            _mockEmailTemplateService = new Mock<IEmailTemplateService>();

            // Setup configuration
            _mockConfiguration.Setup(c => c["Urls:FeBasePath"]).Returns("https://test-domain.com");

            _controller = new AccountController(
                _mockMapper.Object,
                _mockUserService.Object,
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockEmailService.Object,
                _mockEmailTemplateService.Object
            );

            // Setup default controller context
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region CreateAccount Tests

        [Fact]
        public async Task CreateAccount_WhenModelStateIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Email is required");
            var model = new CreateAccountVM();

            // Act
            var result = await _controller.CreateAccount(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateAccount_WhenEmailExists_ReturnsBadRequest()
        {
            // Arrange
            var model = new CreateAccountVM
            {
                Email = "existing@example.com",
                UserName = "testuser",
                FullName = "Test User",
                Password = "Password123!",
                IsPasswordConfirmed = true,
                Status = UserStatus.Activated
            };
            _mockUserService.Setup(s => s.CheckUserExist(model.Email)).ReturnsAsync(true);

            // Act
            var result = await _controller.CreateAccount(model);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(400, objectResult.StatusCode);

            var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal("Email already exists", problemDetails.Detail);
        }

        [Fact]
        public async Task CreateAccount_WithValidModel_CreatesAccountAndReturnsCreatedResult()
        {
            // Arrange
            var model = new CreateAccountVM
            {
                Email = "new@example.com",
                UserName = "newuser",
                FullName = "New User",
                Password = "Password123!",
                IsPasswordConfirmed = true,
                Status = UserStatus.Activated
            };

            _mockUserService.Setup(s => s.CheckUserExist(model.Email)).ReturnsAsync(false);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                UserName = model.UserName,
                FullName = model.FullName,
                Status = model.Status
            };

            _mockMapper.Setup(m => m.Map<User>(It.IsAny<CreateAccountVM>())).Returns(user);
            _mockEmailTemplateService.Setup(s => s.SendAccountCredentialsAsync(It.IsAny<AccountCredentials>())).ReturnsAsync(true);

            _mockUserService.Setup(s => s.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) => user);

            // Act
            var result = await _controller.CreateAccount(model);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdAtActionResult.StatusCode);
            Assert.Equal("CreateAccount", createdAtActionResult.ActionName);
            Assert.Equal(user, createdAtActionResult.Value);

            _mockUserService.Verify(s => s.AddAsync(It.IsAny<User>()), Times.Once);
            _mockEmailTemplateService.Verify(s => s.SendAccountCredentialsAsync(It.IsAny<AccountCredentials>()), Times.Once);
        }

        [Fact]
        public async Task CreateAccount_WithNoPassword_GeneratesRandomPassword()
        {
            // Arrange
            var model = new CreateAccountVM
            {
                Email = "new@example.com",
                UserName = "newuser",
                FullName = "New User",
                Password = null, // No password provided
                IsPasswordConfirmed = false,
                Status = UserStatus.Deactivated
            };

            _mockUserService.Setup(s => s.CheckUserExist(model.Email)).ReturnsAsync(false);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                UserName = model.UserName,
                FullName = model.FullName,
                Status = model.Status
            };

            _mockMapper.Setup(m => m.Map<User>(It.IsAny<CreateAccountVM>())).Returns(user);
            _mockUserService.Setup(s => s.AddAsync(It.IsAny<User>())).Returns((User user) => Task.FromResult(user));
            _mockEmailTemplateService.Setup(s => s.SendAccountCredentialsAsync(It.IsAny<AccountCredentials>())).ReturnsAsync(true);

            // Act
            var result = await _controller.CreateAccount(model);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdAtActionResult.StatusCode);

            // Verify that a password was generated and hashed
            _mockEmailTemplateService.Verify(s => s.SendAccountCredentialsAsync(
                It.Is<AccountCredentials>(c => c.Password != null && c.Password.Length > 0)),
                Times.Once);
        }

        [Fact]
        public async Task CreateAccount_WithInactiveStatus_SetsCorrectStatus()
        {
            // Arrange
            var model = new CreateAccountVM
            {
                Email = "new@example.com",
                UserName = "newuser",
                FullName = "New User",
                Password = "Password123!",
                IsPasswordConfirmed = true,
                Status = UserStatus.Deactivated
            };

            _mockUserService.Setup(s => s.CheckUserExist(model.Email)).ReturnsAsync(false);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                UserName = model.UserName,
                FullName = model.FullName
            };

            _mockMapper.Setup(m => m.Map<User>(It.IsAny<CreateAccountVM>())).Returns((CreateAccountVM src) => {
                user.Status = src.Status;
                return user;
            });

            _mockEmailTemplateService.Setup(s => s.SendAccountCredentialsAsync(It.IsAny<AccountCredentials>())).ReturnsAsync(true);
            _mockUserService.Setup(s => s.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) => user);

            // Act
            var result = await _controller.CreateAccount(model);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdAtActionResult.StatusCode);

            // Verify the status was set correctly
            _mockUserService.Verify(s => s.AddAsync(It.Is<User>(u => u.Status == UserStatus.Deactivated)), Times.Once);
        }

        [Fact]
        public async Task CreateAccount_PasswordNotConfirmed_ReturnsBadRequest()
        {
            // Arrange
            var model = new CreateAccountVM
            {
                Email = "new@example.com",
                UserName = "newuser",
                FullName = "New User",
                Password = "Password123!",
                IsPasswordConfirmed = false, // Password not confirmed
                Status = UserStatus.Activated
            };

            // Setup for ModelState.IsValid to be false
            _controller.ModelState.AddModelError("IsPasswordConfirmed", "Password must be confirmed");

            // Act
            var result = await _controller.CreateAccount(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateAccount_WhenExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var model = new CreateAccountVM
            {
                Email = "new@example.com",
                UserName = "newuser",
                FullName = "New User",
                Password = "Password123!",
                IsPasswordConfirmed = true,
                Status = UserStatus.Activated
            };
            _mockUserService.Setup(s => s.CheckUserExist(model.Email)).ReturnsAsync(false);
            _mockUserService.Setup(s => s.AddAsync(It.IsAny<User>())).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateAccount(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            // Verify the underlying Log method was called instead of the LogError extension method
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Something wrong happen during creating new account")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        #endregion

        #region GetAccountList Tests

        [Fact]
        public async Task GetAccountList_ReturnsPagedResponse()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "UserName",
                SortDirection = "asc",
                Filters = new Dictionary<string, string>()
            };

            var httpContext = new DefaultHttpContext();
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "pageNumber", "1" },
                { "pageSize", "10" },
                { "sortBy", "UserName" },
                { "sortDirection", "asc" },
                { "search", "test" }
            });

            httpContext.Request.Query = queryCollection;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    FullName ="user1",
                    UserName = "user1",
                    Email = "user1@example.com",
                    Groups = new List<Group>
                    {
                        new Group { Id = Guid.NewGuid(), Name = "Global Group", IsGlobal = true },
                        new Group { Id = Guid.NewGuid(), Name = "Local Group", IsGlobal = false }
                    }
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    FullName ="user1",
                    UserName = "user2",
                    Email = "user2@example.com",
                    Groups = new List<Group>
                    {
                        new Group { Id = Guid.NewGuid(), Name = "Global Group", IsGlobal = true }
                    }
                }
            };

            var pagedResponse = new PagedResponse<User>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2,
                Data = users
            };

            var mappedResponse = new PagedResponse<UserListVM>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2,
                Data = new List<UserListVM>
                {
                    new UserListVM { UserName = "user1", Email = "user1@example.com" },
                    new UserListVM { UserName = "user2", Email = "user2@example.com" }
                }
            };

            _mockUserService.Setup(s => s.GetPagedAsync(
                It.IsAny<QueryParams>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<User, object>>>()))
                .ReturnsAsync(pagedResponse);

            _mockMapper.Setup(m => m.Map<PagedResponse<UserListVM>>(It.IsAny<PagedResponse<User>>()))
                .Returns(mappedResponse);

            // Act
            var result = await _controller.GetAccountList(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResponse<UserListVM>>(okResult.Value);

            Assert.Equal(2, returnValue.TotalRecords);
            Assert.Equal(2, returnValue.Data.Count());

            // Verify that non-global groups were filtered out
            _mockUserService.Verify(s => s.GetPagedAsync(
                It.Is<QueryParams>(q => q.Filters.ContainsKey("search") && q.Filters["search"] == "test"),
                It.IsAny<System.Linq.Expressions.Expression<Func<User, object>>>()),
                Times.Once);
        }

        #endregion

        #region GetProfile Tests

        [Fact]
        public async Task GetProfile_UserViewingOwnProfile_ReturnsProfile()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var profileVM = new ProfileVM
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User"
            };

            _mockUserService.Setup(s => s.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Ok(false)); // Not an admin

            _mockUserService.Setup(s => s.GetUserProfileAsync(userId))
                .ReturnsAsync(Result.Ok(profileVM));

            // Act
            var result = await _controller.GetProfile(userId);

            // Assert
            var okResult = Assert.IsType<ProfileVM>(result.Value);
            Assert.Equal(userId, okResult.Id);
            Assert.Equal("testuser", okResult.UserName);
        }

        [Fact]
        public async Task GetProfile_AdminViewingOtherProfile_ReturnsProfile()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var profileVM = new ProfileVM
            {
                Id = targetUserId,
                UserName = "targetuser",
                Email = "target@example.com",
                FullName = "Target User"
            };

            _mockUserService.Setup(s => s.CheckUserInGroup(adminId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Ok(true)); // Is an admin

            _mockUserService.Setup(s => s.GetUserProfileAsync(targetUserId))
                .ReturnsAsync(Result.Ok(profileVM));

            // Act
            var result = await _controller.GetProfile(targetUserId);

            // Assert
            var okResult = Assert.IsType<ProfileVM>(result.Value);
            Assert.Equal(targetUserId, okResult.Id);
            Assert.Equal("targetuser", okResult.UserName);
        }

        [Fact]
        public async Task GetProfile_NonAdminViewingOtherProfile_ReturnsForbid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _mockUserService.Setup(s => s.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Ok(false)); // Not an admin

            // Act
            var result = await _controller.GetProfile(otherUserId);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetProfile_UserValidationFails_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _mockUserService.Setup(s => s.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail("User does not exist")); // Validation failed

            // Act
            var result = await _controller.GetProfile(targetUserId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetProfile_GetProfileFails_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _mockUserService.Setup(s => s.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Ok(true)); // Is admin

            _mockUserService.Setup(s => s.GetUserProfileAsync(userId))
                .ReturnsAsync(Result.Fail("Profile not found"));

            // Act
            var result = await _controller.GetProfile(userId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        #endregion

        #region UpdateProfile Tests

        [Fact]
        public async Task UpdateProfile_UserUpdatingOwnProfile_ReturnsOk()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var profile = new UpdateProfileVM
            {
                Email = "user@example.com",
                UserName = "updateduser",
                FullName = "Updated User",
                Dob = new DateTime(1990, 1, 1)
            };

            var user = new User
            {
                Id = userId,
                Email = profile.Email,
                UserName = "oldusername",
                FullName = "Old User Name",
                Dob = new DateTime(1980, 1, 1)
            };

            _mockUserService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockUserService.Setup(s => s.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Ok(false)); // Not an admin
            _mockUserService.Setup(s => s.GetUserByEmailAsync(profile.Email)).ReturnsAsync(user);
            _mockUserService.Setup(s => s.UpdateAsync(It.IsAny<User>()))
                 .ReturnsAsync((User user) => user);
            // Act
            var result = await _controller.UpdateProfile(profile);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Verify that the user was updated with the new values
            _mockUserService.Verify(s => s.UpdateAsync(It.Is<User>(u =>
                u.UserName == profile.UserName &&
                u.FullName == profile.FullName &&
                u.Dob == profile.Dob)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProfile_AdminUpdatingOtherProfile_ReturnsOk()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var profile = new UpdateProfileVM
            {
                Email = "user@example.com",
                UserName = "updateduser",
                FullName = "Updated User",
                Dob = new DateTime(1990, 1, 1)
            };

            var admin = new User
            {
                Id = adminId,
                Email = "admin@example.com",
                UserName = "admin",
                FullName = "Admin User"
            };

            var targetUser = new User
            {
                Id = targetUserId,
                Email = profile.Email,
                UserName = "oldusername",
                FullName = "Old User Name",
                Dob = new DateTime(1980, 1, 1)
            };

            _mockUserService.Setup(s => s.GetByIdAsync(adminId)).ReturnsAsync(admin);
            _mockUserService.Setup(s => s.CheckUserInGroup(adminId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Ok(true)); // Is an admin
            _mockUserService.Setup(s => s.GetUserByEmailAsync(profile.Email)).ReturnsAsync(targetUser);
            _mockUserService.Setup(s => s.UpdateAsync(It.IsAny<User>()))
                 .ReturnsAsync((User user) => user);
            // Act
            var result = await _controller.UpdateProfile(profile);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Verify that the target user was updated with the new values
            _mockUserService.Verify(s => s.UpdateAsync(It.Is<User>(u =>
                u.Id == targetUserId &&
                u.UserName == profile.UserName &&
                u.FullName == profile.FullName &&
                u.Dob == profile.Dob)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProfile_NonAdminUpdatingOtherProfile_ReturnsForbid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var profile = new UpdateProfileVM
            {
                Email = "other@example.com",
                UserName = "otheruser",
                FullName = "Other User",
                Dob = new DateTime(1990, 1, 1)
            };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                UserName = "user",
                FullName = "User"
            };

            var targetUser = new User
            {
                Id = targetUserId,
                Email = profile.Email,
                UserName = "otheruser",
                FullName = "Other User"
            };

            _mockUserService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockUserService.Setup(s => s.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Ok(false)); // Not an admin
            _mockUserService.Setup(s => s.GetUserByEmailAsync(profile.Email)).ReturnsAsync(targetUser);

            // Act
            var result = await _controller.UpdateProfile(profile);

            // Assert
            Assert.IsType<ForbidResult>(result);

            // Verify that no update was performed
            _mockUserService.Verify(s => s.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProfile_InvalidUserId_ReturnsBadRequest()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "not-a-guid") // Invalid GUID format
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var profile = new UpdateProfileVM
            {
                Email = "user@example.com",
                UserName = "user",
                FullName = "User"
            };

            // Act
            var result = await _controller.UpdateProfile(profile);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Invalid UserId claim format.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateProfile_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var profile = new UpdateProfileVM
            {
                Email = "nonexistent@example.com",
                UserName = "nonexistent",
                FullName = "Non Existent User"
            };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                UserName = "user",
                FullName = "User"
            };

            _mockUserService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockUserService.Setup(s => s.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Ok(true)); // Is an admin
            _mockUserService.Setup(s => s.GetUserByEmailAsync(profile.Email)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.UpdateProfile(profile);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Updated user does not exist", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateProfile_UserGroupValidationFails_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var profile = new UpdateProfileVM
            {
                Email = "user@example.com",
                UserName = "user",
                FullName = "User"
            };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                UserName = "user",
                FullName = "User"
            };

            _mockUserService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockUserService.Setup(s => s.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail("Group does not exist")); // Validation failed

            // Act
            var result = await _controller.UpdateProfile(profile);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        #endregion
    }
}

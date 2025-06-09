using AutoMapper;
using DPMS_WebAPI.Controllers;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPMS_WebAPI.Tests.UnitTests.Controllers
{
        public class UT_AuthController
        {
        private readonly Mock<IMapper> _mockMapper;
        private readonly IConfiguration _mockConfig;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly AuthService _authService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        public UT_AuthController()
        {
            _mockMapper = new Mock<IMapper>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<AuthController>>();

            // Use real configuration for JWT
            var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "thisisaverysecuretestjwtkey1234567890"},
            {"Jwt:Issuer", "unit-test-issuer"},
            {"Jwt:Audience", "unit-test-audience"}
        };
            _mockConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Use real AuthService
            _authService = new AuthService(_mockConfig, _mockUserRepository.Object);

            // Inject everything into the controller
            _controller = new AuthController(
                _mockMapper.Object,
                _authService,
                _mockLogger.Object,
                _mockUserService.Object
            );
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var password = "TestPassword123";
            var saltBase64 = "XZvzBS9QXpGvMf939REk3g==";
            var hashedPassword = PasswordUtils.HashPassword(password, saltBase64);

            var loginRequest = new LoginVM { Email = "test@example.com", Password = password };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = loginRequest.Email,
                FullName = "Test User",
                UserName = "testuser",
                Salt = saltBase64,
                Password = hashedPassword,
                Status = UserStatus.Activated
            };

            // ✅ Add this to avoid null error:
            _mockUserRepository.Setup(r => r.GetFeaturesByUserEmailAsync(user.Email!))
                .ReturnsAsync(new List<Feature>
                {
        new Feature { FeatureName= "feature1" ,Url = "example-url", HttpMethod = HttpMethodType.GET },
        new Feature {  FeatureName= "feature2",Url = "another-url", HttpMethod = HttpMethodType.POST }
                });

            _mockUserService.Setup(s => s.GetUserByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);
            _mockUserService.Setup(s => s.UpdateLastLoginTimeStamp(user))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseObj = okResult.Value;
            var dict = responseObj.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(responseObj));

            Assert.Equal("Login successfully", dict["message"]);
            Assert.NotNull(dict["token"]);
        }

        [Fact]
        public async Task Login_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _controller.Login(new LoginVM());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithNonExistentEmail_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginVM { Email = "nonexistent@example.com", Password = "password123" };

            _mockUserService.Setup(s => s.GetUserByEmailAsync(loginRequest.Email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email does not exist", badRequestResult.Value);
        }

        [Fact]
        public async Task Login_WithDeactivatedAccount_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginVM { Email = "deactivated@example.com", Password = "password123" };
            var user = new User
            {
                Email = "deactivated@example.com",
                Status = UserStatus.Deactivated,
                FullName = "Test User",
                UserName = "testuser"
            };

            _mockUserService.Setup(s => s.GetUserByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Account is deactivated", badRequestResult.Value);
        }

        [Fact]
        public async Task Login_WithNoPasswordSetup_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginVM { Email = "nopassword@example.com", Password = "password123" };
            var user = new User
            {
                Email = "nopassword@example.com",
                Salt = "XZvzBS9QXpGvMf939REk3g==",
                Password = null,
                Status = UserStatus.Activated,
                IsPasswordConfirmed = false,
                FullName = "Test User",
                UserName = "testuser"
            };

            _mockUserService.Setup(s => s.GetUserByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("This account does not setup a password yet", badRequestResult.Value);
        }

        [Fact]
        public async Task Login_WithIncorrectPassword_ReturnsUnauthorized()
        {
            // Arrange
            var correctPassword = "CorrectPassword123";
            var wrongPassword = "WrongPassword123";
            var saltBase64 = "XZvzBS9QXpGvMf939REk3g=="; // Test salt in Base64

            // Generate the hash using the correct password
            var hashedPassword = PasswordUtils.HashPassword(correctPassword, saltBase64);

            var loginRequest = new LoginVM { Email = "test@example.com", Password = wrongPassword };
            var user = new User
            {
                UserName = "testuser",
                FullName = "Test User",
                Email = "test@example.com",
                Salt = saltBase64,
                Password = hashedPassword,
                Status = UserStatus.Activated
            };

            _mockUserService.Setup(s => s.GetUserByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Login failed", unauthorizedResult.Value);
        }
    }
}

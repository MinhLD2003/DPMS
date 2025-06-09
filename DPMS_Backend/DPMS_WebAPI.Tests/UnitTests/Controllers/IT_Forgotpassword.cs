using DPMS_WebAPI.Controllers;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace DPMS_WebAPI.Tests.UnitTests.Controllers
{
    public class ForgotpasswordControllerTests
    {
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<AuthService> _mockAuthService;
        private readonly ForgotpasswordController _controller;

        public ForgotpasswordControllerTests()
        {
            _mockEmailService = new Mock<IEmailService>();
            _mockUserService = new Mock<IUserService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockAuthService = new Mock<AuthService>(null, null); // Pass null for dependencies not used in this test
            _controller = new ForgotpasswordController(
                _mockEmailService.Object,
                _mockAuthService.Object,
                _mockUserService.Object,
                _mockConfiguration.Object
            );
        }

        [Fact]
        public async Task ForgotPassword_InvalidRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.ForgotPassword(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid request. Email is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task ForgotPassword_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "nonexistent@example.com" };
            _mockUserService.Setup(s => s.GetUserByEmailAsync(request.Email)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.ForgotPassword(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task ForgotPassword_EmailSendingFails_ReturnsInternalServerError()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@example.com" };
            var user = new User { Email = request.Email, FullName = "", UserName = "" };
            _mockUserService.Setup(s => s.GetUserByEmailAsync(request.Email)).ReturnsAsync(user);
            _mockAuthService.Setup(a => a.GeneratePasswordResetToken(user.Email)).Returns("dummy-token");
            _mockConfiguration.Setup(c => c["Urls:FeBasePath"]).Returns("https://example.com");
            _mockEmailService.Setup(e => e.SendResetPasswordEmailAsync(user.Email, "dummy-token", "https://example.com"))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.ForgotPassword(request);

            // Assert
            var internalServerErrorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, internalServerErrorResult.StatusCode);
            Assert.Equal("Failed to send reset password email.", internalServerErrorResult.Value);
        }

        [Fact]
        public async Task ForgotPassword_Success_ReturnsOk()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@example.com" };
            var user = new User {Email = request.Email, FullName = "", UserName = "" };
            _mockUserService.Setup(s => s.GetUserByEmailAsync(request.Email)).ReturnsAsync(user);
            _mockAuthService.Setup(a => a.GeneratePasswordResetToken(user.Email)).Returns("dummy-token");
            _mockConfiguration.Setup(c => c["Urls:FeBasePath"]).Returns("https://example.com");
            _mockEmailService.Setup(e => e.SendResetPasswordEmailAsync(user.Email, "dummy-token", "https://example.com"))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ForgotPassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True((bool)okResult.Value);
        }
    }
}

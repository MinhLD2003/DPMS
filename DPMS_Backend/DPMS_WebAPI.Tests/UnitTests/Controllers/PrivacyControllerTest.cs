using System.Linq.Expressions;
using AutoMapper;
using DPMS_WebAPI.Controllers;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.MapperProfiles;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.PrivacyPolicy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;

namespace DPMS_WebAPI.Tests.UnitTests.Controllers
{
    public class PrivacyControllerTest
    {
        private readonly Mock<IPrivacyPolicyService> _mockPrivacyPolicyService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<PrivacyPolicyController>> _mockLogger;
        private readonly PrivacyPolicyController _controller;

        public PrivacyControllerTest()
        {
            _mockPrivacyPolicyService = new Mock<IPrivacyPolicyService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<PrivacyPolicyController>>();
            _controller = new PrivacyPolicyController(
                _mockPrivacyPolicyService.Object, 
                _mockMapper.Object, 
                _mockLogger.Object
            );

            // Setup default HttpContext for controller
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(
                new Dictionary<string, StringValues>());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region GetPolicies Tests

        [Fact]
        public async Task GetPolicies_ReturnsOkWithValidData()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "Title",
                SortDirection = "asc",
                Filters = new Dictionary<string, string>()
            };

            // Setup HttpContext with query parameters
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(
                new Dictionary<string, StringValues>
                {
                    { "Status", "Active" }
                });
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var pagedResponse = new PagedResponse<PrivacyPolicy>
            {
                Data = new List<PrivacyPolicy> 
                { 
                    new PrivacyPolicy { Id = Guid.NewGuid(), Title = "Test Policy" } 
                },
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1
            };

            var mappedPolicies = new List<ListPolicyVM>
            {
                new ListPolicyVM { Id = pagedResponse.Data[0].Id, Title = "Test Policy" }
            };

            _mockPrivacyPolicyService.Setup(s => s.GetPagedAsync(It.IsAny<QueryParams>()))
                .ReturnsAsync(pagedResponse);

            _mockMapper.Setup(m => m.Map<List<ListPolicyVM>>(It.IsAny<List<PrivacyPolicy>>()))
                .Returns(mappedPolicies);

            // Act
            var result = await _controller.GetPolicies(queryParams);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<PagedResponse<ListPolicyVM>>().Subject;
            
            response.Data.Should().HaveCount(1);
            response.PageNumber.Should().Be(1);
            response.TotalPages.Should().Be(1);
            response.TotalRecords.Should().Be(1);
        }

        [Fact]
        public async Task GetPolicies_ReturnsOkWithEmptyList()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            // Setup HttpContext with query parameters
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(
                new Dictionary<string, StringValues>
                {
                    { "Status", "Unknown" }
                });
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var emptyData = new PagedResponse<PrivacyPolicy>
            {
                Data = new List<PrivacyPolicy>(),
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0
            };
            
            _mockPrivacyPolicyService.Setup(s => s.GetPagedAsync(It.IsAny<QueryParams>()))
                .ReturnsAsync(emptyData);
                
            _mockMapper.Setup(m => m.Map<List<ListPolicyVM>>(It.IsAny<List<PrivacyPolicy>>()))
                .Returns(new List<ListPolicyVM>());

            // Act
            var result = await _controller.GetPolicies(queryParams);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<PagedResponse<ListPolicyVM>>().Subject;
            
            response.Data.Should().BeEmpty();
            response.PageNumber.Should().Be(1);
            response.TotalPages.Should().Be(0);
            response.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetPolicies_ReturnsProblem_WhenExceptionOccurs()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            _mockPrivacyPolicyService.Setup(s => s.GetPagedAsync(It.IsAny<QueryParams>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetPolicies(queryParams);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        #endregion

        #region CreatePolicy Tests

        [Fact]
        public async Task CreatePolicy_ReturnsCreatedAtAction_WithValidData()
        {
            // Arrange
            var policyVM = new PolicyVM 
            { 
                Title = "Test Policy",
                Content = "Policy Content"
            };
            
            var policy = new PrivacyPolicy 
            { 
                Id = Guid.NewGuid(),
                Title = "Test Policy",
                Content = "Policy Content",
                Status = PolicyStatus.Draft
            };
            
            _mockMapper.Setup(m => m.Map<PrivacyPolicy>(policyVM)).Returns(policy);
            _mockPrivacyPolicyService.Setup(s => s.AddAsync(It.IsAny<PrivacyPolicy>()))
                .ReturnsAsync(policy);

            // Act
            var result = await _controller.CreatePolicy(policyVM);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(_controller.GetPolicy));
            createdResult.RouteValues["id"].Should().Be(policy.Id);
            
            // Verify policy status is set to Draft
            var returnedPolicy = createdResult.Value.Should().BeOfType<PrivacyPolicy>().Subject;
            returnedPolicy.Status.Should().Be(PolicyStatus.Draft);
        }

        [Fact]
        public async Task CreatePolicy_ReturnsBadRequest_WithInvalidModel()
        {
            // Arrange
            var policyVM = new PolicyVM(); // Missing required fields
            _controller.ModelState.AddModelError("Title", "The Title field is required.");

            // Act
            var result = await _controller.CreatePolicy(policyVM);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region GetPolicy Tests

        [Fact]
        public async Task GetPolicy_ReturnsOk_WhenActivePolicyExists()
        {
            // Arrange
            var activePolicy = new PrivacyPolicy 
            { 
                Id = Guid.NewGuid(),
                Title = "Active Policy",
                Content = "This is active",
                Status = PolicyStatus.Active
            };
            
            var activePolicyVM = new PolicyVM
            {
                Title = "Active Policy",
                Content = "This is active"
            };
            
            _mockPrivacyPolicyService.Setup(s => s.FindAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>()))
                .ReturnsAsync(new List<PrivacyPolicy> { activePolicy });
                
            _mockMapper.Setup(m => m.Map<PolicyVM>(activePolicy)).Returns(activePolicyVM);

            // Act
            var result = await _controller.GetPolicy();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeOfType<PolicyVM>();
        }

        [Fact]
        public async Task GetPolicy_ReturnsNotFound_WhenNoActivePolicyExists()
        {
            // Arrange
            _mockPrivacyPolicyService.Setup(s => s.FindAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>()))
                .ReturnsAsync(new List<PrivacyPolicy>());

            // Act
            var result = await _controller.GetPolicy();

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetPolicy_ReturnsProblem_WhenExceptionOccurs()
        {
            // Arrange
            _mockPrivacyPolicyService.Setup(s => s.FindAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetPolicy();

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task GetPolicy_WithId_ReturnsOk_WhenPolicyExists()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy = new PrivacyPolicy 
            { 
                Id = policyId,
                Title = "Test Policy",
                Content = "Policy content"
            };
            
            var policyVM = new PolicyVM
            {
                Title = "Test Policy",
                Content = "Policy content"
            };
            
            _mockPrivacyPolicyService.Setup(s => s.GetByIdAsync(policyId))
                .ReturnsAsync(policy);
                
            _mockMapper.Setup(m => m.Map<PolicyVM>(policy))
                .Returns(policyVM);

            // Act
            var result = await _controller.GetPolicy(policyId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeOfType<PolicyVM>();
        }

        [Fact]
        public async Task GetPolicy_WithId_ReturnsNotFound_WhenPolicyDoesNotExist()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            _mockPrivacyPolicyService.Setup(s => s.GetByIdAsync(policyId))
                .ReturnsAsync((PrivacyPolicy)null);

            // Act
            var result = await _controller.GetPolicy(policyId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetPolicy_WithId_ReturnsProblem_WhenExceptionOccurs()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            _mockPrivacyPolicyService.Setup(s => s.GetByIdAsync(policyId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetPolicy(policyId);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        #endregion

        #region UpdatePolicy Tests

        [Fact]
        public async Task UpdatePolicy_ReturnsOk_WhenPolicyExists()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policyVM = new PolicyVM
            {
                Title = "Updated Policy",
                Content = "Updated content"
            };
            
            var existingPolicy = new PrivacyPolicy
            {
                Id = policyId,
                Title = "Original Policy",
                Content = "Original content",
                Status = PolicyStatus.Draft
            };
            
            var updatedPolicy = new PrivacyPolicy
            {
                Id = policyId,
                Title = "Updated Policy",
                Content = "Updated content",
                Status = PolicyStatus.Draft
            };
            
            _mockPrivacyPolicyService.Setup(s => s.GetByIdAsync(policyId))
                .ReturnsAsync(existingPolicy);
                
            _mockPrivacyPolicyService.Setup(s => s.UpdateAsync(It.IsAny<PrivacyPolicy>()))
                .ReturnsAsync(updatedPolicy);
                
            // Setup mapper to update the existing policy
            _mockMapper.Setup(m => m.Map(policyVM, existingPolicy))
                .Callback((PolicyVM src, PrivacyPolicy dest) => {
                    dest.Title = src.Title;
                    dest.Content = src.Content;
                });

            // Act
            var result = await _controller.UpdatePolicy(policyId, policyVM);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdatePolicy_MakesActive_WhenStatusIsActive()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policyVM = new PolicyVM
            {
                Title = "Updated Policy",
                Content = "Updated content",
                Status = PolicyStatus.Active
            };
            
            var existingPolicy = new PrivacyPolicy
            {
                Id = policyId,
                Title = "Original Policy",
                Content = "Original content",
                Status = PolicyStatus.Draft
            };
            
            _mockPrivacyPolicyService.Setup(s => s.GetByIdAsync(policyId))
                .ReturnsAsync(existingPolicy);
                
            _mockPrivacyPolicyService.Setup(s => s.ActivePolicy(policyId))
                .Returns(Task.CompletedTask);
                
            _mockPrivacyPolicyService.Setup(s => s.UpdateAsync(It.IsAny<PrivacyPolicy>()))
                .ReturnsAsync(existingPolicy);
                
            _mockMapper.Setup(m => m.Map(policyVM, existingPolicy))
                .Callback((PolicyVM src, PrivacyPolicy dest) => {
                    dest.Title = src.Title;
                    dest.Content = src.Content;
                    dest.Status = src.Status;
                });

            // Act
            var result = await _controller.UpdatePolicy(policyId, policyVM);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            
            // Verify ActivePolicy was called
            _mockPrivacyPolicyService.Verify(s => s.ActivePolicy(policyId), Times.Once);
        }

        [Fact]
        public async Task UpdatePolicy_ReturnsNotFound_WhenPolicyDoesNotExist()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policyVM = new PolicyVM
            {
                Title = "Updated Policy",
                Content = "Updated content"
            };
            
            _mockPrivacyPolicyService.Setup(s => s.GetByIdAsync(policyId))
                .ReturnsAsync((PrivacyPolicy)null);

            // Act
            var result = await _controller.UpdatePolicy(policyId, policyVM);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdatePolicy_ReturnsProblem_WhenExceptionOccurs()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policyVM = new PolicyVM
            {
                Title = "Updated Policy",
                Content = "Updated content"
            };
            
            _mockPrivacyPolicyService.Setup(s => s.GetByIdAsync(policyId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UpdatePolicy(policyId, policyVM);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
                
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        #endregion

        #region ActivePolicy Tests

        [Fact]
        public async Task ActivePolicy_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            _mockPrivacyPolicyService.Setup(s => s.ActivePolicy(policyId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ActivePolicy(policyId);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ActivePolicy_ReturnsProblem_WhenExceptionOccurs()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            _mockPrivacyPolicyService.Setup(s => s.ActivePolicy(policyId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.ActivePolicy(policyId);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
                
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        #endregion

        #region DeletePolicy Tests

        [Fact]
        public async Task DeletePolicy_ReturnsNoContent_WhenInactivePolicyDeleted()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy = new PrivacyPolicy
            {
                Id = policyId,
                Title = "Test Policy",
                Status = PolicyStatus.Draft // Not active
            };
            
            _mockPrivacyPolicyService.Setup(s => s.GetByIdAsync(policyId))
                .ReturnsAsync(policy);
                
            _mockPrivacyPolicyService.Setup(s => s.DeleteAsync(policyId))
                .ReturnsAsync(true); // Return successful deletion

            // Act
            var result = await _controller.DeletePolicy(policyId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeletePolicy_ReturnsBadRequest_WhenActivePolicyDeleted()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy = new PrivacyPolicy
            {
                Id = policyId,
                Title = "Test Policy",
                Status = PolicyStatus.Active // Active policy
            };
            
            _mockPrivacyPolicyService.Setup(s => s.GetByIdAsync(policyId))
                .ReturnsAsync(policy);

            // Act
            var result = await _controller.DeletePolicy(policyId);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Cannot delete an active policy.");
        }

        [Fact]
        public async Task DeletePolicy_ReturnsNotFound_WhenPolicyDoesNotExist()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            _mockPrivacyPolicyService.Setup(s => s.GetByIdAsync(policyId))
                .ReturnsAsync((PrivacyPolicy)null);

            // Act
            var result = await _controller.DeletePolicy(policyId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeletePolicy_ReturnsProblem_WhenExceptionOccurs()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            _mockPrivacyPolicyService.Setup(s => s.GetByIdAsync(policyId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.DeletePolicy(policyId);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
                
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        #endregion
    }
}

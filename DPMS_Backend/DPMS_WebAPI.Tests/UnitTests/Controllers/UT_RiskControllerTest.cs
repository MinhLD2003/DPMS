using AutoMapper;
using DPMS_WebAPI.Controllers;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Risk;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Linq.Expressions;


namespace DPMS_WebAPI.Tests.UnitTests.Controllers
{
    public class UT_RiskControllerTests
    {
        private readonly Mock<IRiskService> _mockRiskService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<RiskController>> _mockLogger;
        private readonly RiskController _controller;

        public UT_RiskControllerTests()
        {
            _mockRiskService = new Mock<IRiskService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<RiskController>>();
            _controller = new RiskController(
                _mockRiskService.Object,
                _mockMapper.Object,
                _mockLogger.Object
            );
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }
        private RiskVM GetValidRiskVM()
        {
            return new RiskVM
            {
                RiskName = "Test Risk",
                Mitigation = "Test Mitigation",
                Category = RiskCategory.Organizational,
                RiskContingency = "Test Contingency",
                Strategy = ResponseStrategy.Mitigate,
                RiskImpact = 3,
                RiskProbability = 2,
                Priority = 6,
                RiskImpactAfterMitigation = 2,
                RiskProbabilityAfterMitigation = 1,
                PriorityAfterMitigation = 2,
                RiskOwner = "Test Owner",
                RaisedAt = DateTime.Now
            };
        }
        #region GetRisks Tests

        [Fact]
        public async Task GetRisks_ShouldReturnPagedResults_WhenFiltersAreProvided()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string> { { "Title", "Testfilter" } }
            };

            var risks = new List<Risk> { new Risk { Id = Guid.NewGuid() } };
            var pagedResponse = new PagedResponse<Risk>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1,
                Data = risks
            };

            _mockRiskService.Setup(s => s.GetPagedAsync(queryParams, It.IsAny<Expression<Func<Risk, object>>>()))
                .ReturnsAsync(pagedResponse);

            // Act
            var result = await _controller.GetRisks(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<RiskListVM>>(okResult.Value);
            Assert.Equal(1, response.PageNumber);
            Assert.Equal(10, response.PageSize);
            Assert.Equal("Testfilter", queryParams.Filters["Title"]);
        }

        [Fact]
        public async Task GetRisks_ShouldCapPageSizeAutomatically_WhenValueExceedsLimit()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 1000 // Should be capped automatically to 50 (MaxPageSize)
            };

            var risks = new List<Risk> { new Risk { Id = Guid.NewGuid() } };
            var pagedResponse = new PagedResponse<Risk>
            {
                PageNumber = 1,
                PageSize = 50, // Expect this to be capped
                TotalPages = 1,
                TotalRecords = 1,
                Data = risks
            };

            _mockRiskService.Setup(s => s.GetPagedAsync(
                It.Is<QueryParams>(p => p.PageSize == 50), // Verify it was capped at 50
                It.IsAny<Expression<Func<Risk, object>>>()))
                .ReturnsAsync(pagedResponse);

            // Act
            var result = await _controller.GetRisks(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<RiskListVM>>(okResult.Value);
            Assert.Equal(50, response.PageSize); // Should be capped at MaxPageSize
        }

        [Fact]
        public async Task GetRisks_ShouldProcessValidSortDirection()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                SortDirection = "desc" // Valid sort direction
            };

            var risks = new List<Risk> { new Risk { Id = Guid.NewGuid() } };
            var pagedResponse = new PagedResponse<Risk>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1,
                Data = risks
            };

            _mockRiskService.Setup(s => s.GetPagedAsync(
                It.Is<QueryParams>(p => p.SortDirection == "desc"),
                It.IsAny<Expression<Func<Risk, object>>>()))
                .ReturnsAsync(pagedResponse);

            // Act
            var result = await _controller.GetRisks(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PagedResponse<RiskListVM>>(okResult.Value);
        }

        [Fact]
        public async Task GetRisks_ShouldHandleSpecialCharactersInFilters()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string> { { "Title", "Test@#$%^&*" } }
            };

            var risks = new List<Risk> { new Risk { Id = Guid.NewGuid() } };
            var pagedResponse = new PagedResponse<Risk>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1,
                Data = risks
            };

            _mockRiskService.Setup(s => s.GetPagedAsync(queryParams, It.IsAny<Expression<Func<Risk, object>>>()))
                .ReturnsAsync(pagedResponse);

            // Act
            var result = await _controller.GetRisks(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<RiskListVM>>(okResult.Value);
            Assert.Equal("Test@#$%^&*", queryParams.Filters["Title"]);
        }

        [Fact]
        public async Task GetRisks_ShouldHandleVeryLargePageNumber()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = int.MaxValue,
                PageSize = 10
            };

            var risks = new List<Risk>();
            var pagedResponse = new PagedResponse<Risk>
            {
                PageNumber = int.MaxValue,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0,
                Data = risks
            };

            _mockRiskService.Setup(s => s.GetPagedAsync(queryParams, It.IsAny<Expression<Func<Risk, object>>>()))
                .ReturnsAsync(pagedResponse);

            // Act
            var result = await _controller.GetRisks(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<RiskListVM>>(okResult.Value);
            Assert.Null(response.Data);
        }

        [Fact]
        public async Task GetRisks_ShouldReturnPagedResults_WhenValidParameters()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            var risks = new List<Risk> { new Risk { Id = Guid.NewGuid() } };
            var riskListVMs = new List<RiskListVM> { new RiskListVM { Id = risks[0].Id } };
            var pagedResponse = new PagedResponse<Risk>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1,
                Data = risks
            };

            // Use It.IsAny<Expression<Func<Risk, object>>> instead of direct function
            _mockRiskService.Setup(s => s.GetPagedAsync(
                It.IsAny<QueryParams>(),
                It.IsAny<Expression<Func<Risk, object>>>()))
                .ReturnsAsync(pagedResponse);

            _mockMapper.Setup(m => m.Map<List<RiskListVM>>(It.IsAny<List<Risk>>()))
                .Returns(riskListVMs);

            // Mock HttpContext
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
    {
        { "status", new StringValues("active") }
    });
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.GetRisks(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<RiskListVM>>(okResult.Value);
            Assert.Equal(1, response.PageNumber);
            Assert.Equal(10, response.PageSize);
            Assert.Equal(1, response.TotalPages);
            Assert.Equal(1, response.TotalRecords);
            Assert.Single(response.Data);
            Assert.Equal(risks[0].Id, response.Data[0].Id);
            Assert.Equal("active", queryParams.Filters["status"]);
        }

        [Fact]
        public async Task GetRisks_ShouldHandleException_AndReturnProblem()
        {
            // Arrange
            var queryParams = new QueryParams();

            _mockRiskService.Setup(s => s.GetPagedAsync(
                It.IsAny<QueryParams>(),
                It.IsAny<Expression<Func<Risk, object>>>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Mock HttpContext
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetRisks(queryParams);

            // Assert
            var problemResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, problemResult.StatusCode);
            var problem = Assert.IsType<ProblemDetails>(problemResult.Value);
            Assert.Equal("An error occurred while fetching risks.", problem.Detail);

            // Verify logger was called
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error retrieving risks")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region ResolveRisk Tests

        [Fact]
        public async Task ResolveRisk_ShouldReturnOk_WhenRiskExists()
        {
            // Arrange
            var riskId = Guid.NewGuid();
            var resolveModel = new RiskResolveVM
            {
                RiskImpactAfterMitigation = 2,
                RiskProbabilityAfterMitigation = 1,
                PriorityAfterMitigation = 3
            };
            var existingRisk = new Risk { Id = riskId };

            _mockRiskService.Setup(s => s.GetByIdAsync(riskId))
                .ReturnsAsync(existingRisk);

            _mockRiskService.Setup(s => s.UpdateAsync(It.IsAny<Risk>()))
                .ReturnsAsync(existingRisk);

            // Act
            var result = await _controller.ResolveRisk(riskId, resolveModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(existingRisk, okResult.Value);

            // Verify mapper was called
            _mockMapper.Verify(m => m.Map(resolveModel, existingRisk), Times.Once);

            // Verify service was called
            _mockRiskService.Verify(s => s.UpdateAsync(existingRisk), Times.Once);
        }
        [Fact]
        public async Task ResolveRisk_ShouldReturnNotFound_WhenRiskDoesNotExist()
        {
            // Arrange
            var riskId = Guid.NewGuid();
            var resolveModel = new RiskResolveVM
            {
                RiskImpactAfterMitigation = 2,
                RiskProbabilityAfterMitigation = 1,
                PriorityAfterMitigation = 3
            };

            _mockRiskService.Setup(s => s.GetByIdAsync(riskId))
                .ReturnsAsync((Risk)null);

            // Act
            var result = await _controller.ResolveRisk(riskId, resolveModel);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ResolveRisk_ShouldHandleException_AndReturnProblem()
        {
            // Arrange
            var riskId = Guid.NewGuid();
            var resolveModel = new RiskResolveVM
            {
                RiskImpactAfterMitigation = 2,
                RiskProbabilityAfterMitigation = 1,
                PriorityAfterMitigation = 3
            };
            var existingRisk = new Risk { Id = riskId };

            _mockRiskService.Setup(s => s.GetByIdAsync(riskId))
                .ReturnsAsync(existingRisk);

            _mockRiskService.Setup(s => s.UpdateAsync(It.IsAny<Risk>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.ResolveRisk(riskId, resolveModel);

            // Assert
            var problemResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, problemResult.StatusCode);
            var problem = Assert.IsType<ProblemDetails>(problemResult.Value);
            Assert.Equal("An error occurred while updating the risk.", problem.Detail);

        }

        #endregion

        #region GetRisk Tests

        [Fact]
        public async Task GetRisk_ShouldReturnOk_WhenRiskExists()
        {
            // Arrange
            var riskId = Guid.NewGuid();
            var risk = new Risk { Id = riskId };
            var riskVM = new RiskVM { Id = riskId };

            _mockRiskService.Setup(s => s.GetByIdAsync(riskId))
                .ReturnsAsync(risk);

            _mockMapper.Setup(m => m.Map<RiskVM>(risk))
                .Returns(riskVM);

            // Act
            var result = await _controller.GetRisk(riskId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(riskVM, okResult.Value);
        }

        [Fact]
        public async Task GetRisk_ShouldReturnNotFound_WhenRiskDoesNotExist()
        {
            // Arrange
            var riskId = Guid.NewGuid();

            _mockRiskService.Setup(s => s.GetByIdAsync(riskId))
                .ReturnsAsync((Risk)null);

            _mockMapper.Setup(m => m.Map<RiskVM>(null))
                .Returns((RiskVM)null);

            // Act
            var result = await _controller.GetRisk(riskId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetRisk_ShouldHandleException_AndReturnProblem()
        {
            // Arrange
            var riskId = Guid.NewGuid();

            _mockRiskService.Setup(s => s.GetByIdAsync(riskId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetRisk(riskId);

            // Assert
            var problemResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, problemResult.StatusCode);
            var problem = Assert.IsType<ProblemDetails>(problemResult.Value);
            Assert.Equal("An error occurred while fetching the risk.", problem.Detail);

            // Verify logger was called
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Error retrieving risk with ID {riskId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region CreateRisk Tests

        [Fact]
        public async Task CreateRisk_ValidModel_ReturnsCreatedAtAction()
        {
            // Arrange
            var riskId = Guid.NewGuid();
            var riskVM = new RiskVM { RiskName = "Test Risk" };
            var risk = new Risk { Id = riskId, RiskName = "Test Risk" };

            _mockMapper.Setup(m => m.Map<Risk>(riskVM))
                .Returns(risk);

            _mockRiskService.Setup(s => s.AddAsync(risk))
                .ReturnsAsync(risk);

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(RiskController.GetRisk), createdAtActionResult.ActionName);
            Assert.Equal(riskId, createdAtActionResult.RouteValues["id"]);
            Assert.Equal(risk, createdAtActionResult.Value);
        }
        [Fact]
        public async Task CreateRisk_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = new RiskVM();
            _controller.ModelState.AddModelError("RiskName", "Risk name is required.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            // The controller could return either SerializableError or ModelStateDictionary
            Assert.True(badRequestResult.Value is SerializableError || badRequestResult.Value is ModelStateDictionary);
        }

        [Fact]
        public async Task CreateRisk_MissingRiskName_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskName = null;

            SetupValidationError("RiskName", "Risk name is required.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            // Use a more flexible assertion that handles both types that could be returned
            Assert.True(
                (badRequestResult.Value is ModelStateDictionary && ((ModelStateDictionary)badRequestResult.Value).ContainsKey("RiskName")) ||
                (badRequestResult.Value is SerializableError && ((SerializableError)badRequestResult.Value).ContainsKey("RiskName"))
            );

            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_RiskNameTooLong_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskName = new string('A', 256); // Exceeds 255 character limit

            SetupValidationError("RiskName", "Risk name cannot exceed 200 characters.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_MissingMitigation_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.Mitigation = null;

            SetupValidationError("Mitigation", "Risk Mitigation is required.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_MissingCategory_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            // For enums, validation usually happens in the model binding, 
            // so we just need to add the error manually
            SetupValidationError("Category", "Risk category is required.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_MissingRiskContingency_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskContingency = null;

            SetupValidationError("RiskContingency", "Risk Contingency is required.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_MissingStrategy_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            SetupValidationError("Strategy", "Response strategy is required.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_RiskImpactTooLow_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskImpact = 0; // Lower than allowed

            SetupValidationError("RiskImpact", "Risk impact must be between 1 and 16.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_RiskImpactTooHigh_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskImpact = 17; // Higher than allowed

            SetupValidationError("RiskImpact", "Risk impact must be between 1 and 16.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_RiskProbabilityTooLow_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskProbability = 0; // Lower than allowed

            SetupValidationError("RiskProbability", "Risk probability must be between 1 and 5.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_RiskProbabilityTooHigh_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskProbability = 6; // Higher than allowed

            SetupValidationError("RiskProbability", "Risk probability must be between 1 and 5.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_PriorityTooLow_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.Priority = 0; // Lower than allowed

            SetupValidationError("Priority", "Priority must be between 1 and 80.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_PriorityTooHigh_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.Priority = 81; // Higher than allowed

            SetupValidationError("Priority", "Priority must be between 1 and 80.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_RiskImpactAfterMitigationTooLow_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskImpactAfterMitigation = 0; // Lower than allowed

            SetupValidationError("RiskImpactAfterMitigation", "Risk impact after mitigation must be between 1 and 16.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_RiskImpactAfterMitigationTooHigh_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskImpactAfterMitigation = 17; // Higher than allowed

            SetupValidationError("RiskImpactAfterMitigation", "Risk impact after mitigation must be between 1 and 16.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_RiskProbabilityAfterMitigationTooLow_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskProbabilityAfterMitigation = 0; // Lower than allowed

            SetupValidationError("RiskProbabilityAfterMitigation", "Risk probability after mitigation must be between 1 and 5.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_RiskProbabilityAfterMitigationTooHigh_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskProbabilityAfterMitigation = 6; // Higher than allowed

            SetupValidationError("RiskProbabilityAfterMitigation", "Risk probability after mitigation must be between 1 and 5.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_PriorityAfterMitigationTooLow_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.PriorityAfterMitigation = 0; // Lower than allowed

            SetupValidationError("PriorityAfterMitigation", "Priority after mitigation must be between 1 and 80.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_PriorityAfterMitigationTooHigh_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.PriorityAfterMitigation = 81; // Higher than allowed

            SetupValidationError("PriorityAfterMitigation", "Priority after mitigation must be between 1 and 80.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_MissingRiskOwner_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskOwner = null;

            SetupValidationError("RiskOwner", "Risk owner is required.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_RiskOwnerTooLong_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskOwner = new string('A', 201); // Exceeds 200 character limit

            SetupValidationError("RiskOwner", "Risk owner name cannot exceed 200 characters.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            VerifyServiceNotCalled();
        }

        [Fact]
        public async Task CreateRisk_MultipleValidationErrors_ReturnsBadRequest()
        {
            // Arrange
            var riskVM = GetValidRiskVM();
            riskVM.RiskName = null;
            riskVM.RiskOwner = null;
            riskVM.RiskImpact = 0;

            _controller.ModelState.AddModelError("RiskName", "Risk name is required.");
            _controller.ModelState.AddModelError("RiskOwner", "Risk owner is required.");
            _controller.ModelState.AddModelError("RiskImpact", "Risk impact must be between 1 and 16.");

            // Act
            var result = await _controller.CreateRisk(riskVM);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Check the error count, which should work regardless of the return type
            if (badRequestResult.Value is ModelStateDictionary modelState)
            {
                Assert.Equal(3, modelState.ErrorCount);
            }
            else if (badRequestResult.Value is SerializableError serializableError)
            {
                Assert.Equal(3, serializableError.Count);
            }

            VerifyServiceNotCalled();
        }

        private void SetupValidationError(string key, string errorMessage)
        {
            _controller.ModelState.AddModelError(key, errorMessage);
        }

        private void AssertModelStateError(IActionResult result, string key, string expectedError)
        {
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Handle both ModelStateDictionary and SerializableError
            if (badRequestResult.Value is ModelStateDictionary modelState)
            {
                Assert.True(modelState.ContainsKey(key));
            }
            else if (badRequestResult.Value is SerializableError serializableError)
            {
                Assert.True(serializableError.ContainsKey(key));
            }
            else
            {
                Assert.True(false, $"Expected ModelStateDictionary or SerializableError but got {badRequestResult.Value?.GetType().Name}");
            }

            VerifyServiceNotCalled();
        }

        private void VerifyServiceNotCalled()
        {
            _mockRiskService.Verify(s => s.AddAsync(It.IsAny<Risk>()), Times.Never);
        }
        #endregion

        #region UpdateRisk Tests

        [Fact]
        public async Task UpdateRisk_ShouldReturnOk_WhenRiskExists()
        {
            // Arrange
            var riskId = Guid.NewGuid();
            var riskVM = new RiskVM { RiskName = "Updated Risk" };
            var existingRisk = new Risk { Id = riskId, RiskName = "Original Risk" };

            _mockRiskService.Setup(s => s.GetByIdAsync(riskId))
                .ReturnsAsync(existingRisk);

            _mockRiskService.Setup(s => s.UpdateAsync(It.IsAny<Risk>()))
                .ReturnsAsync(existingRisk);

            // Act
            var result = await _controller.UpdateRisk(riskId, riskVM);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(existingRisk, okResult.Value);

            // Verify mapper was called
            _mockMapper.Verify(m => m.Map(riskVM, existingRisk), Times.Once);

            // Verify service was called
            _mockRiskService.Verify(s => s.UpdateAsync(existingRisk), Times.Once);
        }

        [Fact]
        public async Task UpdateRisk_ShouldReturnNotFound_WhenRiskDoesNotExist()
        {
            // Arrange
            var riskId = Guid.NewGuid();
            var riskVM = new RiskVM { RiskName = "Updated Risk" };

            _mockRiskService.Setup(s => s.GetByIdAsync(riskId))
                .ReturnsAsync((Risk)null);

            // Act
            var result = await _controller.UpdateRisk(riskId, riskVM);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateRisk_ShouldHandleException_AndReturnProblem()
        {
            // Arrange
            var riskId = Guid.NewGuid();
            var riskVM = new RiskVM { RiskName = "Updated Risk" };
            var existingRisk = new Risk { Id = riskId, RiskName = "Original Risk" };

            _mockRiskService.Setup(s => s.GetByIdAsync(riskId))
                .ReturnsAsync(existingRisk);

            _mockRiskService.Setup(s => s.UpdateAsync(It.IsAny<Risk>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UpdateRisk(riskId, riskVM);

            // Assert
            var problemResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, problemResult.StatusCode);
            var problem = Assert.IsType<ProblemDetails>(problemResult.Value);
            Assert.Equal("An error occurred while updating the risk.", problem.Detail);
        }
        [Fact]
        public async Task UpdateRisk_WithValidModel_ReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var riskVM = new RiskVM
            {
                RiskName = "Updated Risk Name",
                Mitigation = "Updated Mitigation Plan",
                Category = RiskCategory.Technical,
                RiskContingency = "Updated Contingency Plan",
                Strategy = ResponseStrategy.Mitigate,
                RiskImpact = 4,
                RiskProbability = 3,
                Priority = 12,
                RiskImpactAfterMitigation = 2,
                RiskProbabilityAfterMitigation = 1,
                PriorityAfterMitigation = 2,
                RiskOwner = "John Doe",
                RaisedAt = DateTime.Now
            };

            var existingRisk = new Risk
            {
                Id = id,
                RiskName = "Original Risk Name",
                // Initialize with original values
            };

            var mockRiskService = new Mock<IRiskService>();
            mockRiskService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existingRisk);
            mockRiskService.Setup(s => s.UpdateAsync(It.IsAny<Risk>())).ReturnsAsync(existingRisk);
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map(riskVM, existingRisk)).Returns(existingRisk);
            mockMapper.Setup(m => m.Map<RiskVM>(existingRisk)).Returns(riskVM);

            var mockLogger = new Mock<ILogger<RiskController>>();

            var controller = new RiskController(mockRiskService.Object, mockMapper.Object, mockLogger.Object);

            // Setup controller context with valid ModelState
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.UpdateRisk(id, riskVM);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult.Value.Should().Be(existingRisk);

            // Verify service was called correctly
            mockRiskService.Verify(s => s.GetByIdAsync(id), Times.Once);
            mockRiskService.Verify(s => s.UpdateAsync(existingRisk), Times.Once);
            mockMapper.Verify(m => m.Map(riskVM, existingRisk), Times.Once);
        }


        [Fact]
        public async Task UpdateRisk_WithValidModel_MapsAndUpdatesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var riskVM = new RiskVM
            {
                RiskName = "Updated Risk Name",
                Mitigation = "Updated Mitigation Plan",
                Category = RiskCategory.Technical,
                RiskContingency = "Updated Contingency Plan",
                Strategy = ResponseStrategy.Mitigate,
                RiskImpact = 4,
                RiskProbability = 3,
                Priority = 12,
                RiskImpactAfterMitigation = 2,
                RiskProbabilityAfterMitigation = 1,
                PriorityAfterMitigation = 2,
                RiskOwner = "John Doe",
                RaisedAt = DateTime.Now
            };

            // Create a real Risk object to capture mapping
            var existingRisk = new Risk { Id = id };

            var mockRiskService = new Mock<IRiskService>();
            mockRiskService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existingRisk);

            // Use callback to verify the Risk is updated correctly
            Risk capturedRisk = null;
            mockRiskService.Setup(s => s.UpdateAsync(It.IsAny<Risk>()))
     .Callback<Risk>(r => capturedRisk = r)
     .ReturnsAsync(existingRisk);  // Or use .Returns(Task.FromResult(existingRisk))

            // Setup mapper to actually map properties 
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map(riskVM, existingRisk))
                .Callback<RiskVM, Risk>((vm, risk) => {
                    risk.RiskName = vm.RiskName;
                    risk.Mitigation = vm.Mitigation;
                    risk.Category = vm.Category;
                    risk.RiskContingency = vm.RiskContingency;
                    risk.Strategy = vm.Strategy;
                    risk.RiskImpact = vm.RiskImpact;
                    // Map other properties...
                })
                .Returns(existingRisk);

            var mockLogger = new Mock<ILogger<RiskController>>();
            var controller = new RiskController(mockRiskService.Object, mockMapper.Object, mockLogger.Object);

            // Act
            var result = await controller.UpdateRisk(id, riskVM);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            capturedRisk.Should().NotBeNull();
            capturedRisk.RiskName.Should().Be("Updated Risk Name");
            capturedRisk.Mitigation.Should().Be("Updated Mitigation Plan");
            capturedRisk.Category.Should().Be(RiskCategory.Technical);
        }

        [Fact]
        public async Task UpdateRisk_RiskNotFound_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var riskVM = new RiskVM { RiskName = "Test Risk" };

            var mockRiskService = new Mock<IRiskService>();
            mockRiskService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Risk)null);

            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<RiskController>>();

            var controller = new RiskController(mockRiskService.Object, mockMapper.Object, mockLogger.Object);

            // Act
            var result = await controller.UpdateRisk(id, riskVM);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateRisk_ServiceThrowsException_ReturnsProblemResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var riskVM = new RiskVM { RiskName = "Test Risk" };
            var existingRisk = new Risk { Id = id };

            var mockRiskService = new Mock<IRiskService>();
            mockRiskService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(existingRisk);
            mockRiskService.Setup(s => s.UpdateAsync(It.IsAny<Risk>()))
                .ThrowsAsync(new Exception("Test exception"));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map(riskVM, existingRisk)).Returns(existingRisk);

            var mockLogger = new Mock<ILogger<RiskController>>();

            var controller = new RiskController(mockRiskService.Object, mockMapper.Object, mockLogger.Object);

            // Act
            var result = await controller.UpdateRisk(id, riskVM);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var problemResult = (ObjectResult)result;
            problemResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            // Verify logger was called
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateRisk_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var id = Guid.NewGuid();
            var riskVM = new RiskVM();
            var mockRiskService = new Mock<IRiskService>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<RiskController>>();
            var controller = new RiskController(mockRiskService.Object, mockMapper.Object, mockLogger.Object);
            // Add model error to make ModelState invalid
            controller.ModelState.AddModelError("RiskName", "Risk name is required");

            // Act
            var result = await controller.UpdateRisk(id, riskVM);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().BeOfType<SerializableError>();

            // If you want to verify the specific error message:
            var errors = (SerializableError)badRequestResult.Value;
            errors.Should().ContainKey("RiskName");
        }
        #endregion

        #region DeleteRisk Tests

        [Fact]
        public async Task DeleteRisk_ShouldReturnNoContent_WhenRiskExists()
        {
            // Arrange
            var riskId = Guid.NewGuid();
            var risk = new Risk { Id = riskId };

            _mockRiskService.Setup(s => s.GetByIdAsync(riskId))
                .ReturnsAsync(risk);

            _mockRiskService.Setup(s => s.DeleteAsync(riskId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteRisk(riskId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify service was called
            _mockRiskService.Verify(s => s.DeleteAsync(riskId), Times.Once);
        }

        [Fact]
        public async Task DeleteRisk_ShouldReturnNotFound_WhenRiskDoesNotExist()
        {
            // Arrange
            var riskId = Guid.NewGuid();

            _mockRiskService.Setup(s => s.GetByIdAsync(riskId))
                .ReturnsAsync((Risk)null);

            // Act
            var result = await _controller.DeleteRisk(riskId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteRisk_ShouldHandleException_AndReturnProblem()
        {
            // Arrange
            var riskId = Guid.NewGuid();
            var risk = new Risk { Id = riskId };

            _mockRiskService.Setup(s => s.GetByIdAsync(riskId))
                .ReturnsAsync(risk);

            _mockRiskService.Setup(s => s.DeleteAsync(riskId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.DeleteRisk(riskId);

            // Assert
            var problemResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, problemResult.StatusCode);
            var problem = Assert.IsType<ProblemDetails>(problemResult.Value);
            Assert.Equal("An error occurred while deleting the risk.", problem.Detail);
        }

        #endregion

        #region Export Tests

        [Fact]
        public async Task Export_ShouldReturnFile_WhenExportIsSuccessful()
        {
            // Arrange
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write("test data");
            writer.Flush();
            memoryStream.Position = 0;

            _mockRiskService.Setup(s => s.ExportAsync())
                .ReturnsAsync(memoryStream);

            // Act
            var result = await _controller.Export();

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/octet-stream", fileResult.ContentType);
            Assert.Contains("Risk_Export_", fileResult.FileDownloadName);
            Assert.Equal(memoryStream, fileResult.FileStream);
        }

        [Fact]
        public async Task Export_ShouldReturnNotFound_WhenExportFails()
        {
            // Arrange
            _mockRiskService.Setup(s => s.ExportAsync())
                .ReturnsAsync((Stream)null);

            // Act
            var result = await _controller.Export();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion
    }
}

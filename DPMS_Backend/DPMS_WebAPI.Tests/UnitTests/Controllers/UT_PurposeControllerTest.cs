using AutoMapper;
using DPMS_WebAPI.Controllers;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Purpose;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;


namespace DPMS_WebAPI.Tests.UnitTests.Controllers
{
    public class UT_PurposeControllerTest
    {
        private readonly Mock<IPurposeService> _mockPurposeService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<PurposeController>> _mockLogger;
        private readonly PurposeController _controller;

        public UT_PurposeControllerTest()
        {
            _mockPurposeService = new Mock<IPurposeService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<PurposeController>>();

            _controller = new PurposeController(
                _mockPurposeService.Object,
                _mockMapper.Object,
                _mockLogger.Object
            );

            // Setup default HttpContext for controller
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region GetPurposes Tests

        [Fact]
        public async Task GetPurposes_ReturnsOkWithPagedResponse_WhenDataExists()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "Name",
                SortDirection = "asc",
                Filters = new Dictionary<string, string>()
            };

            var pagedResponse = new PagedResponse<Purpose>
            {
                Data = new List<Purpose>
                {
                    new Purpose { Id = Guid.NewGuid(), Name = "Test Purpose" },
                    new Purpose { Id = Guid.NewGuid(), Name = "Another Purpose" }
                },
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var mappedPurposes = new List<ListPurposeVM>
            {
                new ListPurposeVM { Id = pagedResponse.Data[0].Id, Name = "Test Purpose" },
                new ListPurposeVM { Id = pagedResponse.Data[1].Id, Name = "Another Purpose" }
            };

            _mockPurposeService.Setup(s => s.GetPagedAsync(
                    It.IsAny<QueryParams>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<Purpose, object>>>()))
                .ReturnsAsync(pagedResponse);

            _mockMapper.Setup(m => m.Map<List<ListPurposeVM>>(pagedResponse.Data))
                .Returns(mappedPurposes);

            // Act
            var result = await _controller.GetPurposes(queryParams);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResponse = okResult.Value.Should().BeOfType<PagedResponse<ListPurposeVM>>().Subject;

            returnedResponse.Data.Should().HaveCount(2);
            returnedResponse.PageNumber.Should().Be(1);
            returnedResponse.TotalRecords.Should().Be(2);
        }

        [Fact]
        public async Task GetPurposes_ReturnsOkWithEmptyPagedResponse_WhenNoDataExists()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            var pagedResponse = new PagedResponse<Purpose>
            {
                Data = new List<Purpose>(),
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0
            };

            _mockPurposeService.Setup(s => s.GetPagedAsync(
                    It.IsAny<QueryParams>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<Purpose, object>>>()))
                .ReturnsAsync(pagedResponse);

            _mockMapper.Setup(m => m.Map<List<ListPurposeVM>>(pagedResponse.Data))
                .Returns(new List<ListPurposeVM>());

            // Act
            var result = await _controller.GetPurposes(queryParams);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResponse = okResult.Value.Should().BeOfType<PagedResponse<ListPurposeVM>>().Subject;

            returnedResponse.Data.Should().BeEmpty();
            returnedResponse.PageNumber.Should().Be(1);
            returnedResponse.TotalRecords.Should().Be(0);
        }
        [Fact]
        public async Task GetPurposes_ReturnsProblem_WhenExceptionOccurs()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            _mockPurposeService.Setup(s => s.GetPagedAsync(
                    It.IsAny<QueryParams>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<Purpose, object>>>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetPurposes(queryParams);

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
        public async Task GetPurposes_AddsQueryFilters_WhenAdditionalQueryParametersExist()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
                {
            { "Title", "test" }
                });
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var pagedResponse = new PagedResponse<Purpose>
            {
                Data = new List<Purpose>(),
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0
            };

            _mockPurposeService.Setup(s => s.GetPagedAsync(
                    It.IsAny<QueryParams>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<Purpose, object>>>()))
                .ReturnsAsync(pagedResponse);

            _mockMapper.Setup(m => m.Map<List<ListPurposeVM>>(pagedResponse.Data))
                .Returns(new List<ListPurposeVM>());

            // Act
            var result = await _controller.GetPurposes(queryParams);

            // Assert
            queryParams.Filters.Should().ContainKey("Title");
            queryParams.Filters["Title"].Should().Be("test");

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResponse = okResult.Value.Should().BeOfType<PagedResponse<ListPurposeVM>>().Subject;

            returnedResponse.Data.Should().BeEmpty();
        }
        #endregion

        #region GetPurpose Tests

        [Fact]
        public async Task GetPurpose_ReturnsOk_WhenPurposeExists()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var purpose = new Purpose { Id = purposeId, Name = "Test Purpose" };
            var purposeVM = new PurposeVM { Id = purposeId, Name = "Test Purpose" };

            _mockPurposeService.Setup(s => s.GetByIdAsync(purposeId))
                .ReturnsAsync(purpose);

            _mockMapper.Setup(m => m.Map<PurposeVM>(purpose))
                .Returns(purposeVM);

            // Act
            var result = await _controller.GetPurpose(purposeId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(purposeVM);
        }

        [Fact]
        public async Task GetPurpose_ReturnsNotFound_WhenPurposeDoesNotExist()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            _mockPurposeService.Setup(s => s.GetByIdAsync(purposeId))
                .ReturnsAsync((Purpose)null);

            _mockMapper.Setup(m => m.Map<PurposeVM>(null))
                .Returns((PurposeVM)null);

            // Act
            var result = await _controller.GetPurpose(purposeId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetPurpose_ReturnsProblem_WhenExceptionOccurs()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            _mockPurposeService.Setup(s => s.GetByIdAsync(purposeId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetPurpose(purposeId);

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

        #region CreatePurpose Tests

        [Fact]
        public async Task CreatePurpose_ReturnsCreatedAtAction_WhenPurposeCreatedSuccessfully()
        {
            // Arrange
            var purposeVM = new PurposeVM
            {
                Name = "Test Purpose",
                Description = "Test Description"
            };

            var purpose = new Purpose
            {
                Id = Guid.NewGuid(),
                Name = "Test Purpose",
                Description = "Test Description",
                Status = PurposeStatus.Draft
            };

            _mockMapper.Setup(m => m.Map<Purpose>(It.IsAny<PurposeVM>()))
                .Returns(purpose);

            _mockPurposeService.Setup(s => s.AddAsync(It.IsAny<Purpose>()))
                .ReturnsAsync(purpose);

            // Act
            var result = await _controller.CreatePurpose(purposeVM);

            // Assert
            var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(PurposeController.GetPurpose));
            createdAtActionResult.RouteValues["id"].Should().Be(purpose.Id);
            var returnedPurpose = createdAtActionResult.Value.Should().BeOfType<Purpose>().Subject;
            returnedPurpose.Should().BeEquivalentTo(purpose);
            returnedPurpose.Status.Should().Be(PurposeStatus.Draft);

            // Verify method calls
            _mockMapper.Verify(m => m.Map<Purpose>(It.IsAny<PurposeVM>()), Times.Once);
            _mockPurposeService.Verify(s => s.AddAsync(It.IsAny<Purpose>()), Times.Once);
        }

        [Fact]
        public async Task CreatePurpose_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var purposeVM = new PurposeVM(); // Empty model to trigger validation errors
            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            // Act
            var result = await _controller.CreatePurpose(purposeVM);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task CreatePurpose_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var purposeVM = new PurposeVM
            {
                Name = "Test Purpose",
                Description = "Test Description"
            };

            var purpose = new Purpose
            {
                Name = "Test Purpose",
                Description = "Test Description"
            };

            _mockMapper.Setup(m => m.Map<Purpose>(It.IsAny<PurposeVM>()))
                .Returns(purpose);

            var exceptionMessage = "Test exception";
            _mockPurposeService.Setup(s => s.AddAsync(It.IsAny<Purpose>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.CreatePurpose(purposeVM);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be(exceptionMessage);
        }

        [Fact]
        public async Task CreatePurpose_SetsPurposeStatusToDraft_RegardlessOfInputStatus()
        {
            // Arrange
            var purposeVM = new PurposeVM
            {
                Name = "Test Purpose",
                Description = "Test Description",
                Status = PurposeStatus.Active // Trying to set status to Active
            };

            Purpose capturedPurpose = null;

            _mockMapper.Setup(m => m.Map<Purpose>(It.IsAny<PurposeVM>()))
                .Returns(new Purpose
                {
                    Id = Guid.NewGuid(),
                    Name = purposeVM.Name,
                    Description = purposeVM.Description,
                    Status = purposeVM.Status // Initially set to the input status
                });

            _mockPurposeService.Setup(s => s.AddAsync(It.IsAny<Purpose>()))
                .Callback<Purpose>(p => capturedPurpose = p)
                .ReturnsAsync((Purpose p) => p);

            // Act
            var result = await _controller.CreatePurpose(purposeVM);

            // Assert
            capturedPurpose.Should().NotBeNull();
            capturedPurpose.Status.Should().Be(PurposeStatus.Draft); // Should be set to Draft
        }

        #endregion

        #region UpdatePurpose Tests

        [Fact]
        public async Task UpdatePurpose_ReturnsOk_WhenPurposeUpdatedSuccessfully()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var purposeVM = new PurposeVM
            {
                Id = purposeId,
                Name = "Updated Purpose",
                Description = "Updated Description",
                Status = PurposeStatus.Active // Set status to something other than Draft
            };

            var existingPurpose = new Purpose
            {
                Id = purposeId,
                Name = "Original Purpose",
                Description = "Original Description",
                Status = PurposeStatus.Draft,
                ExternalSystems = new List<ExternalSystemPurpose>() // Initialize this property
            };

            // Update the mock setup to use GetDetailAsync instead of GetByIdAsync
            _mockPurposeService.Setup(s => s.GetDetailAsync(
                It.Is<Guid>(id => id == purposeId),
                It.IsAny<Expression<Func<Purpose, object>>>()))
                    .ReturnsAsync(existingPurpose);

            _mockMapper.Setup(m => m.Map(It.IsAny<PurposeVM>(), It.IsAny<Purpose>()))
                .Callback<PurposeVM, Purpose>((src, dest) =>
                {
                    // Explicitly set all properties that should be mapped
                    dest.Name = src.Name;
                    dest.Description = src.Description;
                    dest.Status = src.Status;
                    // Make sure the ID remains unchanged
                    dest.Id = purposeId;
                });

            _mockPurposeService.Setup(s => s.UpdateAsync(It.Is<Purpose>(p => p.Id == purposeId)))
                .ReturnsAsync(existingPurpose);

            // Act
            var result = await _controller.UpdatePurpose(purposeId, purposeVM);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPurpose = okResult.Value.Should().BeOfType<Purpose>().Subject;
            returnedPurpose.Should().BeEquivalentTo(existingPurpose);

            // Verify method calls
            _mockPurposeService.Verify(s => s.GetDetailAsync(purposeId, It.IsAny<Expression<Func<Purpose, object>>>()), Times.Once);
            _mockPurposeService.Verify(s => s.UpdateAsync(It.Is<Purpose>(p => p.Id == purposeId)), Times.Once);
        }

        [Fact]
        public async Task UpdatePurpose_ReturnsNotFound_WhenPurposeDoesNotExist()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var purposeVM = new PurposeVM { Id = purposeId, Name = "Updated Purpose" };

            _mockPurposeService.Setup(s => s.GetByIdAsync(purposeId))
                .ReturnsAsync((Purpose)null);

            // Act
            var result = await _controller.UpdatePurpose(purposeId, purposeVM);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdatePurpose_ReturnsProblem_WhenExceptionOccurs()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var purposeVM = new PurposeVM { Id = purposeId, Name = "Updated Purpose" };

            // Update to use GetDetailAsync instead of GetByIdAsync
            _mockPurposeService.Setup(s => s.GetDetailAsync(purposeId, It.IsAny<Expression<Func<Purpose, object>>>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UpdatePurpose(purposeId, purposeVM);

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

        #region UpdateStatus Tests

        [Fact]
        public async Task UpdateStatus_ReturnsOk_WhenStatusUpdatedSuccessfully()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var statusUpdateVM = new PurposeVM { Status = PurposeStatus.Active };

            var existingPurpose = new Purpose
            {
                Id = purposeId,
                Name = "Test Purpose",
                Status = PurposeStatus.Draft
            };

            _mockPurposeService.Setup(s => s.GetByIdAsync(purposeId))
                .ReturnsAsync(existingPurpose);

            _mockPurposeService.Setup(s => s.UpdateAsync(existingPurpose))
                 .ReturnsAsync(existingPurpose);

            // Act
            var result = await _controller.UpdateStatus(purposeId, statusUpdateVM);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPurpose = okResult.Value.Should().BeOfType<Purpose>().Subject;

            returnedPurpose.Status.Should().Be(PurposeStatus.Active);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsNotFound_WhenPurposeDoesNotExist()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var statusUpdateVM = new PurposeVM { Status = PurposeStatus.Active };

            _mockPurposeService.Setup(s => s.GetByIdAsync(purposeId))
                .ReturnsAsync((Purpose)null);

            // Act
            var result = await _controller.UpdateStatus(purposeId, statusUpdateVM);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateStatus_ReturnsBadRequest_WhenTryingToSetStatusToDraft()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var statusUpdateVM = new PurposeVM { Status = PurposeStatus.Draft };

            var existingPurpose = new Purpose
            {
                Id = purposeId,
                Name = "Test Purpose",
                Status = PurposeStatus.Active
            };

            _mockPurposeService.Setup(s => s.GetByIdAsync(purposeId))
                .ReturnsAsync(existingPurpose);

            // Act
            var result = await _controller.UpdateStatus(purposeId, statusUpdateVM);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Cannot update status back to Draft.");
        }

        [Fact]
        public async Task UpdateStatus_ReturnsProblem_WhenExceptionOccurs()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var statusUpdateVM = new PurposeVM { Status = PurposeStatus.Active };

            _mockPurposeService.Setup(s => s.GetByIdAsync(purposeId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UpdateStatus(purposeId, statusUpdateVM);

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

        #region DeletePurpose Tests

        [Fact]
        public async Task DeletePurpose_ReturnsNoContent_WhenPurposeDeletedSuccessfully()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var existingPurpose = new Purpose
            {
                Id = purposeId,
                Name = "Test Purpose",
                Status = PurposeStatus.Draft
            };

            _mockPurposeService.Setup(s => s.GetByIdAsync(purposeId))
                .ReturnsAsync(existingPurpose);

            _mockPurposeService.Setup(s => s.DeleteAsync(purposeId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeletePurpose(purposeId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeletePurpose_ReturnsNotFound_WhenPurposeDoesNotExist()
        {
            // Arrange
            var purposeId = Guid.NewGuid();

            _mockPurposeService.Setup(s => s.GetByIdAsync(purposeId))
                .ReturnsAsync((Purpose)null);

            // Act
            var result = await _controller.DeletePurpose(purposeId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeletePurpose_ReturnsBadRequest_WhenPurposeIsNotInDraftStatus()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var existingPurpose = new Purpose
            {
                Id = purposeId,
                Name = "Test Purpose",
                Status = PurposeStatus.Active
            };

            _mockPurposeService.Setup(s => s.GetByIdAsync(purposeId))
                .ReturnsAsync(existingPurpose);

            // Act
            var result = await _controller.DeletePurpose(purposeId);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Cannot delete an active purpose.");
        }

        [Fact]
        public async Task DeletePurpose_ReturnsProblem_WhenExceptionOccurs()
        {
            // Arrange
            var purposeId = Guid.NewGuid();

            _mockPurposeService.Setup(s => s.GetByIdAsync(purposeId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.DeletePurpose(purposeId);

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

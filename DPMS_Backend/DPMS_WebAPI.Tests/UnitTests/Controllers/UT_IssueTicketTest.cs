using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Controllers;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.IssueTicket;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;

namespace DPMS_WebAPI.Tests.UnitTests.Controllers
{
    public class UT_IssueTicketTest
    {
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IIssueTicketService> _mockIssueTicketService;
        private readonly Mock<IIssueTicketDocumentService> _mockDocumentService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly IssueTicketController _controller;
        public UT_IssueTicketTest()
        {
            _mockMapper = new Mock<IMapper>();
            _mockIssueTicketService = new Mock<IIssueTicketService>();
            _mockDocumentService = new Mock<IIssueTicketDocumentService>();
            _mockUserService = new Mock<IUserService>();
            _controller = new IssueTicketController(
                _mockMapper.Object,
                _mockIssueTicketService.Object,
                _mockDocumentService.Object,
                _mockUserService.Object
            );

            // Setup HTTP context for testing query parameters
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        #region CreateIssueTicket Tests

        [Fact]
        public async Task CreateIssueTicket_ValidModel_ReturnsCreatedAtAction()
        {
            // Arrange
            var issueTicketVM = new IssueTicketVM
            {
                Title = "Test Ticket",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "Test Ticket",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var files = new List<IFormFile>();

            _mockMapper.Setup(m => m.Map<IssueTicket>(issueTicketVM))
                .Returns(issueTicket);

            _mockIssueTicketService.Setup(s => s.CreateIssueTicket(It.IsAny<IssueTicket>(), files))
                .ReturnsAsync(issueTicket);

            // Act
            var result = await _controller.CreateIssueTicket(issueTicketVM, files);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(IssueTicketController.GetIssueTicketById), createdAtActionResult.ActionName);
            Assert.Equal(issueTicket.Id, createdAtActionResult.RouteValues["id"]);
            Assert.Equal(issueTicket, createdAtActionResult.Value);
        }

        [Fact]
        public async Task CreateIssueTicket_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var issueTicketVM = new IssueTicketVM();
            var files = new List<IFormFile>();

            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.CreateIssueTicket(issueTicketVM, files);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateIssueTicket_ServiceReturnsNull_ReturnsBadRequest()
        {
            // Arrange
            var issueTicketVM = new IssueTicketVM
            {
                Title = "Test Ticket",
                TicketType = TicketType.Risk,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "Test Ticket",
                TicketType = TicketType.Risk,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var files = new List<IFormFile>();

            _mockMapper.Setup(m => m.Map<IssueTicket>(issueTicketVM))
                .Returns(issueTicket);

            _mockIssueTicketService.Setup(s => s.CreateIssueTicket(It.IsAny<IssueTicket>(), files))
                .ReturnsAsync((IssueTicket)null);

            // Act
            var result = await _controller.CreateIssueTicket(issueTicketVM, files);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to create issue ticket.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateIssueTicket_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var issueTicketVM = new IssueTicketVM
            {
                Title = "Test Ticket",
                TicketType = TicketType.Violation,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "Test Ticket",
                TicketType = TicketType.Violation,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var files = new List<IFormFile>();

            _mockMapper.Setup(m => m.Map<IssueTicket>(issueTicketVM))
                .Returns(issueTicket);

            _mockIssueTicketService.Setup(s => s.CreateIssueTicket(It.IsAny<IssueTicket>(), files))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.CreateIssueTicket(issueTicketVM, files);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error", statusCodeResult.Value);
        }

        #endregion

        #region GetIssueTickets Tests

        [Fact]
        public async Task GetIssueTickets_ReturnsOkWithPagedResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUserContext(userId, isAdmin: true);

            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            var tickets = new List<IssueTicket>
    {
        new IssueTicket {
            Id = Guid.NewGuid(),
            Title = "Ticket 1",
            TicketType = TicketType.DPIA,
            IssueTicketStatus = IssueTicketStatus.Pending
        },
        new IssueTicket {
            Id = Guid.NewGuid(),
            Title = "Ticket 2",
            TicketType = TicketType.Risk,
            IssueTicketStatus = IssueTicketStatus.Accept
        }
    };

            var pagedResponse = new PagedResponse<IssueTicket>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2,
                Data = tickets
            };

            var ticketVMs = new List<IssueTicketVM>
    {
        new IssueTicketVM {
            Id = tickets[0].Id,
            Title = "Ticket 1",
            TicketType = TicketType.DPIA,
            IssueTicketStatus = IssueTicketStatus.Pending
        },
        new IssueTicketVM {
            Id = tickets[1].Id,
            Title = "Ticket 2",
            TicketType = TicketType.Risk,
            IssueTicketStatus = IssueTicketStatus.Accept
        }
    };

            _mockIssueTicketService.Setup(s => s.GetPagedAsync(
                It.IsAny<QueryParams>(),
                It.IsAny<Expression<Func<IssueTicket, object>>>(),
                It.IsAny<Expression<Func<IssueTicket, object>>>()))
                .ReturnsAsync(pagedResponse);

            _mockMapper.Setup(m => m.Map<List<IssueTicketVM>>(tickets))
                .Returns(ticketVMs);

            // Act
            var result = await _controller.GetIssueTickets(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResponse<IssueTicketVM>>(okResult.Value);
            Assert.Equal(1, returnValue.PageNumber);
            Assert.Equal(10, returnValue.PageSize);
            Assert.Equal(1, returnValue.TotalPages);
            Assert.Equal(2, returnValue.TotalRecords);
            Assert.Equal(2, returnValue.Data.Count);
        }

        [Fact]
        public async Task GetIssueTickets_WithFilters_AppliesFiltersCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUserContext(userId, isAdmin: true);

            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            // Setup query parameters
            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
    {
        { "status", new StringValues("Pending") },
        { "pageNumber", new StringValues("1") },
        { "pageSize", new StringValues("10") }
    });

            _controller.ControllerContext.HttpContext.Request.Query = queryCollection;

            var tickets = new List<IssueTicket>
    {
        new IssueTicket {
            Id = Guid.NewGuid(),
            Title = "Ticket 1",
            IssueTicketStatus = IssueTicketStatus.Pending,
            TicketType = TicketType.DPIA
        }
    };

            var pagedResponse = new PagedResponse<IssueTicket>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1,
                Data = tickets
            };

            var ticketVMs = new List<IssueTicketVM>
    {
        new IssueTicketVM {
            Id = tickets[0].Id,
            Title = "Ticket 1",
            IssueTicketStatus = IssueTicketStatus.Pending,
            TicketType = TicketType.DPIA
        }
    };

            _mockIssueTicketService.Setup(s => s.GetPagedAsync(
                It.Is<QueryParams>(q => q.Filters.ContainsKey("status") && q.Filters["status"] == "Pending"),
                It.IsAny<Expression<Func<IssueTicket, object>>>(),
                It.IsAny<Expression<Func<IssueTicket, object>>>()))
                .ReturnsAsync(pagedResponse);

            _mockMapper.Setup(m => m.Map<List<IssueTicketVM>>(tickets))
                .Returns(ticketVMs);

            // Act
            var result = await _controller.GetIssueTickets(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResponse<IssueTicketVM>>(okResult.Value);
            Assert.Equal(1, returnValue.TotalRecords);
            Assert.Single(returnValue.Data);
        }

        [Fact]
        public async Task GetIssueTickets_WithSorting_AppliesSortingCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUserContext(userId, isAdmin: true);

            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "Title",
                SortDirection = "desc"
            };

            // Setup query collection to simulate query parameters from request
            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
    {
        { "sortBy", new StringValues("Title") },
        { "sortDirection", new StringValues("desc") },
        { "pageNumber", new StringValues("1") },
        { "pageSize", new StringValues("10") }
    });

            _controller.ControllerContext.HttpContext.Request.Query = queryCollection;

            var tickets = new List<IssueTicket>
    {
        new IssueTicket {
            Id = Guid.NewGuid(),
            Title = "Z Ticket",
            IssueTicketStatus = IssueTicketStatus.Pending,
            TicketType = TicketType.DPIA
        },
        new IssueTicket {
            Id = Guid.NewGuid(),
            Title = "A Ticket",
            IssueTicketStatus = IssueTicketStatus.Accept,
            TicketType = TicketType.Risk
        }
    };

            var pagedResponse = new PagedResponse<IssueTicket>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2,
                Data = tickets
            };

            var ticketVMs = new List<IssueTicketVM>
    {
        new IssueTicketVM {
            Id = tickets[0].Id,
            Title = "Z Ticket",
            IssueTicketStatus = IssueTicketStatus.Pending,
            TicketType = TicketType.DPIA
        },
        new IssueTicketVM {
            Id = tickets[1].Id,
            Title = "A Ticket",
            IssueTicketStatus = IssueTicketStatus.Accept,
            TicketType = TicketType.Risk
        }
    };

            // Set up the service to verify the correct sort parameters are passed
            _mockIssueTicketService.Setup(s => s.GetPagedAsync(
                It.Is<QueryParams>(q =>
                    q.SortBy == "Title" &&
                    q.SortDirection == "desc"),
                It.IsAny<Expression<Func<IssueTicket, object>>>(),
                It.IsAny<Expression<Func<IssueTicket, object>>>()))
                .ReturnsAsync(pagedResponse);

            _mockMapper.Setup(m => m.Map<List<IssueTicketVM>>(tickets))
                .Returns(ticketVMs);

            // Act
            var result = await _controller.GetIssueTickets(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResponse<IssueTicketVM>>(okResult.Value);

            // Verify the correct params were passed to the service
            _mockIssueTicketService.Verify(s => s.GetPagedAsync(
                It.Is<QueryParams>(q =>
                    q.SortBy == "Title" &&
                    q.SortDirection == "desc"),
                It.IsAny<Expression<Func<IssueTicket, object>>>(),
                It.IsAny<Expression<Func<IssueTicket, object>>>()),
                Times.Once);

            // Basic verification that response is correct
            Assert.Equal(2, returnValue.TotalRecords);
            Assert.Equal(2, returnValue.Data.Count);
        }

        [Fact]
        public async Task GetIssueTickets_NonAdminUser_FiltersToUserCreatedTickets()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUserContext(userId, isAdmin: false); // Non-admin user

            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            // Setup query parameters
            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
    {
        { "status", new StringValues("Pending") },
        { "pageNumber", new StringValues("1") },
        { "pageSize", new StringValues("10") }
    });

            _controller.ControllerContext.HttpContext.Request.Query = queryCollection;

            var tickets = new List<IssueTicket>
    {
        new IssueTicket {
            Id = Guid.NewGuid(),
            Title = "User's Ticket",
            IssueTicketStatus = IssueTicketStatus.Pending,
            TicketType = TicketType.DPIA,
            CreatedById = userId
        }
    };

            var pagedResponse = new PagedResponse<IssueTicket>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1,
                Data = tickets
            };

            var ticketVMs = new List<IssueTicketVM>
    {
        new IssueTicketVM {
            Id = tickets[0].Id,
            Title = "User's Ticket",
            IssueTicketStatus = IssueTicketStatus.Pending,
            TicketType = TicketType.DPIA
        }
    };

            // Verify that the CreatedById filter is applied for non-admin users
            _mockIssueTicketService.Setup(s => s.GetPagedAsync(
                It.Is<QueryParams>(q =>
                    q.Filters.ContainsKey("CreatedById") &&
                    q.Filters["CreatedById"] == userId.ToString()),
                It.IsAny<Expression<Func<IssueTicket, object>>>(),
                It.IsAny<Expression<Func<IssueTicket, object>>>()))
                .ReturnsAsync(pagedResponse);

            _mockMapper.Setup(m => m.Map<List<IssueTicketVM>>(tickets))
                .Returns(ticketVMs);

            // Act
            var result = await _controller.GetIssueTickets(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResponse<IssueTicketVM>>(okResult.Value);

            // Verify CreatedById filter was applied
            _mockIssueTicketService.Verify(s => s.GetPagedAsync(
                It.Is<QueryParams>(q =>
                    q.Filters.ContainsKey("CreatedById") &&
                    q.Filters["CreatedById"] == userId.ToString()),
                It.IsAny<Expression<Func<IssueTicket, object>>>(),
                It.IsAny<Expression<Func<IssueTicket, object>>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetIssueTickets_WithSortingAndFilters_AppliesBothCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUserContext(userId, isAdmin: true);

            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "Title",
                SortDirection = "asc",
                Filters = new Dictionary<string, string>()
            };

            // Setup query collection to simulate query parameters from request
            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
    {
        { "sortBy", new StringValues("Title") },
        { "sortDirection", new StringValues("asc") },
        { "status", new StringValues("Pending") },
        { "pageNumber", new StringValues("1") },
        { "pageSize", new StringValues("10") }
    });

            _controller.ControllerContext.HttpContext.Request.Query = queryCollection;

            var tickets = new List<IssueTicket>
    {
        new IssueTicket {
            Id = Guid.NewGuid(),
            Title = "A Ticket",
            IssueTicketStatus = IssueTicketStatus.Pending,
            TicketType = TicketType.DPIA
        }
    };

            var pagedResponse = new PagedResponse<IssueTicket>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1,
                Data = tickets
            };

            var ticketVMs = new List<IssueTicketVM>
    {
        new IssueTicketVM {
            Id = tickets[0].Id,
            Title = "A Ticket",
            IssueTicketStatus = IssueTicketStatus.Pending,
            TicketType = TicketType.DPIA
        }
    };

            // Set up the service to verify the correct parameters are passed
            _mockIssueTicketService.Setup(s => s.GetPagedAsync(
                It.Is<QueryParams>(q =>
                    q.SortBy == "Title" &&
                    q.SortDirection == "asc" &&
                    q.Filters.ContainsKey("status") &&
                    q.Filters["status"] == "Pending"),
                It.IsAny<Expression<Func<IssueTicket, object>>>(),
                It.IsAny<Expression<Func<IssueTicket, object>>>()))
                .ReturnsAsync(pagedResponse);

            _mockMapper.Setup(m => m.Map<List<IssueTicketVM>>(tickets))
                .Returns(ticketVMs);

            // Act
            var result = await _controller.GetIssueTickets(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResponse<IssueTicketVM>>(okResult.Value);

            // Verify the correct params were passed to the service
            _mockIssueTicketService.Verify(s => s.GetPagedAsync(
                It.Is<QueryParams>(q =>
                    q.SortBy == "Title" &&
                    q.SortDirection == "asc" &&
                    q.Filters.ContainsKey("status") &&
                    q.Filters["status"] == "Pending"),
                It.IsAny<Expression<Func<IssueTicket, object>>>(),
                It.IsAny<Expression<Func<IssueTicket, object>>>()),
                Times.Once);

            Assert.Equal(1, returnValue.TotalRecords);
            Assert.Single(returnValue.Data);
        }

        [Fact]
        public async Task GetIssueTickets_ReturnsOkWithEmptyResponse_WhenNoDataExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUserContext(userId, isAdmin: true);

            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            var pagedResponse = new PagedResponse<IssueTicket>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0,
                Data = new List<IssueTicket>()
            };

            _mockIssueTicketService.Setup(s => s.GetPagedAsync(
                It.IsAny<QueryParams>(),
                It.IsAny<Expression<Func<IssueTicket, object>>>(),
                It.IsAny<Expression<Func<IssueTicket, object>>>()))
                .ReturnsAsync(pagedResponse);

            _mockMapper.Setup(m => m.Map<List<IssueTicketVM>>(pagedResponse.Data))
                .Returns(new List<IssueTicketVM>());

            // Act
            var result = await _controller.GetIssueTickets(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResponse<IssueTicketVM>>(okResult.Value);
            Assert.Empty(returnValue.Data);
            Assert.Equal(0, returnValue.TotalRecords);
        }

        [Fact]
        public async Task GetIssueTickets_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            // Setup user context without ID claim
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            // Act
            var result = await _controller.GetIssueTickets(queryParams);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetIssueTickets_ReturnsBadRequest_WhenUserIdIsInvalidFormat()
        {
            // Arrange
            // Setup user with invalid GUID format
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
    };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            // Act
            var result = await _controller.GetIssueTickets(queryParams);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid user ID format", badRequestResult.Value);
        }

        // Helper method to set up user context and permissions
        private void SetupUserContext(Guid userId, bool isAdmin)
        {
            // Setup user identity
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
    };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Initialize empty query collection if not already set
            _controller.ControllerContext.HttpContext.Request.Query =
                _controller.ControllerContext.HttpContext.Request.Query ??
                new QueryCollection(new Dictionary<string, StringValues>());

            // Mock permission checks
            _mockUserService.Setup(s => s.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(isAdmin);
            _mockUserService.Setup(s => s.CheckUserInGroup(userId, PermissionGroup.CTO_CIO))
                .ReturnsAsync(false);
            _mockUserService.Setup(s => s.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(false);
        }
        #endregion

        #region GetIssueTicketById Tests

        [Fact]
        public async Task GetIssueTicketById_ExistingId_ReturnsOkWithTicket()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var ticket = new IssueTicket
            {
                Id = ticketId,
                Title = "Test Ticket",
                TicketType = TicketType.System,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var ticketVM = new IssueTicketVM
            {
                Id = ticketId,
                Title = "Test Ticket",
                TicketType = TicketType.System,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            _mockIssueTicketService.Setup(s => s.GetDetailAsync(
                ticketId,
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>()))
                .ReturnsAsync(ticket);

            _mockMapper.Setup(m => m.Map<IssueTicketVM>(ticket))
                .Returns(ticketVM);

            // Act
            var result = await _controller.GetIssueTicketById(ticketId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTicket = Assert.IsType<IssueTicketVM>(okResult.Value);
            Assert.Equal(ticketId, returnedTicket.Id);
            Assert.Equal("Test Ticket", returnedTicket.Title);
            Assert.Equal(TicketType.System, returnedTicket.TicketType);
            Assert.Equal(IssueTicketStatus.Pending, returnedTicket.IssueTicketStatus);
        }

        [Fact]
        public async Task GetIssueTicketById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            _mockIssueTicketService.Setup(s => s.GetDetailAsync(
                nonExistingId,
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>()))
                .ReturnsAsync((IssueTicket)null);

            // Act
            var result = await _controller.GetIssueTicketById(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Issue ticket with ID {nonExistingId} not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetIssueTicketById_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var ticketId = Guid.NewGuid();

            _mockIssueTicketService.Setup(s => s.GetDetailAsync(
                ticketId,
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetIssueTicketById(ticketId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error", statusCodeResult.Value);
        }

        #endregion

        #region UpdateIssueTicket Tests

        [Fact]
        public async Task UpdateIssueTicket_ValidModel_ReturnsOkWithUpdatedTicket()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var existingTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Original Title",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var updatedTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Updated Title",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicketVM = new IssueTicketCreateVM
            {
                Title = "Updated Title",
                TicketType = TicketType.DPIA
            };

            var newFiles = new List<IFormFile>();
            var removedFiles = new List<string>();

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(existingTicket);

            _mockMapper.Setup(m => m.Map(issueTicketVM, existingTicket, It.IsAny<Action<IMappingOperationOptions<IssueTicketCreateVM, IssueTicket>>>()))
                .Callback((IssueTicketCreateVM src, IssueTicket dest, Action<IMappingOperationOptions<IssueTicketCreateVM, IssueTicket>> opts) =>
                {
                    dest.Title = src.Title;
                    dest.TicketType = src.TicketType;
                });

            _mockIssueTicketService.Setup(s => s.UpdateIssueTicketFilesOnS3(ticketId, newFiles, removedFiles))
                .ReturnsAsync(new List<IssueTicketDocument>());

            _mockIssueTicketService.Setup(s => s.UpdateAsync(It.IsAny<IssueTicket>()))
                .ReturnsAsync(updatedTicket);

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(updatedTicket);

            // Act
            var result = await _controller.UpdateIssueTicket(ticketId, issueTicketVM, newFiles, removedFiles);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTicket = Assert.IsType<IssueTicket>(okResult.Value);
            Assert.Equal(ticketId, returnedTicket.Id);
            Assert.Equal("Updated Title", returnedTicket.Title);
            Assert.Equal(TicketType.DPIA, returnedTicket.TicketType);
        }

        [Fact]
        public async Task UpdateIssueTicket_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            var issueTicketVM = new IssueTicketCreateVM
            {
                Title = "Updated Title",
                TicketType = TicketType.Risk
            };

            var newFiles = new List<IFormFile>();
            var removedFiles = new List<string>();

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(nonExistingId))
                .ReturnsAsync((IssueTicket)null);

            // Act
            var result = await _controller.UpdateIssueTicket(nonExistingId, issueTicketVM, newFiles, removedFiles);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Issue ticket with ID {nonExistingId} not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateIssueTicket_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var issueTicketVM = new IssueTicketCreateVM();
            var newFiles = new List<IFormFile>();
            var removedFiles = new List<string>();

            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.UpdateIssueTicket(ticketId, issueTicketVM, newFiles, removedFiles);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateIssueTicket_FileUploadError_ReturnsInternalServerError()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var existingTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Original Title",
                TicketType = TicketType.Violation,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicketVM = new IssueTicketCreateVM
            {
                Title = "Updated Title",
                TicketType = TicketType.Violation
            };

            var newFiles = new List<IFormFile> { CreateMockFile("test.pdf", "test content") };
            var removedFiles = new List<string>();

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(existingTicket);

            _mockMapper.Setup(m => m.Map(issueTicketVM, existingTicket, It.IsAny<Action<IMappingOperationOptions<IssueTicketCreateVM, IssueTicket>>>()));

            _mockIssueTicketService.Setup(s => s.UpdateIssueTicketFilesOnS3(ticketId, newFiles, removedFiles))
                .ThrowsAsync(new Exception("Error uploading files"));

            // Act
            var result = await _controller.UpdateIssueTicket(ticketId, issueTicketVM, newFiles, removedFiles);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Error uploading files.", statusCodeResult.Value);
        }

        [Fact]
        public async Task UpdateIssueTicket_ServiceUpdateFails_ReturnsBadRequest()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var existingTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Original Title",
                TicketType = TicketType.System,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicketVM = new IssueTicketCreateVM
            {
                Title = "Updated Title",
                TicketType = TicketType.System
            };

            var newFiles = new List<IFormFile>();
            var removedFiles = new List<string>();

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(existingTicket);

            _mockMapper.Setup(m => m.Map(issueTicketVM, existingTicket, It.IsAny<Action<IMappingOperationOptions<IssueTicketCreateVM, IssueTicket>>>()));

            _mockIssueTicketService.Setup(s => s.UpdateIssueTicketFilesOnS3(ticketId, newFiles, removedFiles))
                .ReturnsAsync(new List<IssueTicketDocument>());

            _mockIssueTicketService.Setup(s => s.UpdateAsync(It.IsAny<IssueTicket>()))
                .ReturnsAsync((IssueTicket)null);

            // Act
            var result = await _controller.UpdateIssueTicket(ticketId, issueTicketVM, newFiles, removedFiles);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to update issue ticket.", badRequestResult.Value);
        }

        #endregion

        #region UpdateIssueTicketStatus Tests

        [Fact]
        public async Task UpdateIssueTicketStatus_ExistingId_ReturnsOkWithUpdatedTicket()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var existingTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Test Ticket",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var updatedTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Test Ticket",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Accept
            };

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(existingTicket);

            _mockIssueTicketService.Setup(s => s.UpdateAsync(It.Is<IssueTicket>(t =>
                t.Id == ticketId && t.IssueTicketStatus == IssueTicketStatus.Accept)))
                .ReturnsAsync(updatedTicket);

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(updatedTicket);

            // Act
            var result = await _controller.UpdateIssueTicketStatus(ticketId, IssueTicketStatus.Accept);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTicket = Assert.IsType<IssueTicket>(okResult.Value);
            Assert.Equal(ticketId, returnedTicket.Id);
            Assert.Equal(IssueTicketStatus.Accept, returnedTicket.IssueTicketStatus);
        }

        [Fact]
        public async Task UpdateIssueTicketStatus_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(nonExistingId))
                .ReturnsAsync((IssueTicket)null);

            // Act
            var result = await _controller.UpdateIssueTicketStatus(nonExistingId, IssueTicketStatus.Done);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Issue ticket with ID {nonExistingId} not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateIssueTicketStatus_ServiceUpdateFails_ReturnsBadRequest()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var existingTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Test Ticket",
                TicketType = TicketType.Risk,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(existingTicket);

            _mockIssueTicketService.Setup(s => s.UpdateAsync(It.IsAny<IssueTicket>()))
                .ReturnsAsync((IssueTicket)null);

            // Act
            var result = await _controller.UpdateIssueTicketStatus(ticketId, IssueTicketStatus.Reject);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to update issue ticket status.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateIssueTicketStatus_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var ticketId = Guid.NewGuid();

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UpdateIssueTicketStatus(ticketId, IssueTicketStatus.Done);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error", statusCodeResult.Value);
        }

        #endregion

        #region DeleteIssueTicket Tests

        [Fact]
        public async Task DeleteIssueTicket_ExistingId_ReturnsNoContent()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var existingTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Test Ticket",
                TicketType = TicketType.System,
                IssueTicketStatus = IssueTicketStatus.Pending,
                Documents = new List<IssueTicketDocument> {
        new IssueTicketDocument {
            Id = Guid.NewGuid(),
            Title = "test.pdf",
            FileUrl = "https://example-bucket.s3.amazonaws.com/tickets/test.pdf",
            FileFormat = "application/pdf"
        }
    }
            };

            _mockIssueTicketService.Setup(s => s.GetDetailAsync(
                ticketId,
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>()))
                .ReturnsAsync(existingTicket);

            _mockDocumentService.Setup(s => s.BulkDeleteAsync(existingTicket.Documents))
                .ReturnsAsync(true);

            // Change this line
            _mockDocumentService.Setup(s => s.DeleteIssueTicketFilesOnS3(existingTicket.Documents))
                .Returns(Task.FromResult(true));

            _mockIssueTicketService.Setup(s => s.DeleteAsync(ticketId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteIssueTicket(ticketId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteIssueTicket_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            _mockIssueTicketService.Setup(s => s.GetDetailAsync(
                nonExistingId,
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>()))
                .ReturnsAsync((IssueTicket)null);

            // Act
            var result = await _controller.DeleteIssueTicket(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Issue ticket with ID {nonExistingId} not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteIssueTicket_ServiceDeleteFails_ReturnsBadRequest()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var existingTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Test Ticket",
                TicketType = TicketType.System,
                IssueTicketStatus = IssueTicketStatus.Pending,
                Documents = new List<IssueTicketDocument>()
            };

            _mockIssueTicketService.Setup(s => s.GetDetailAsync(
                ticketId,
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>()))
                .ReturnsAsync(existingTicket);

            _mockIssueTicketService.Setup(s => s.DeleteAsync(ticketId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteIssueTicket(ticketId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to delete issue ticket.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteIssueTicket_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var ticketId = Guid.NewGuid();

            _mockIssueTicketService.Setup(s => s.GetDetailAsync(
                ticketId,
                It.IsAny<System.Linq.Expressions.Expression<Func<IssueTicket, object>>>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.DeleteIssueTicket(ticketId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error", statusCodeResult.Value);
        }

        #endregion

        #region Helper Methods

        private IFormFile CreateMockFile(string fileName, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(bytes));
            return file.Object;
        }

        #endregion

        #region CreateIssueTicket File Validation Tests

        [Fact]
        public async Task CreateIssueTicket_WithOversizedFile_ReturnsInternalServerError()
        {
            // Arrange
            var issueTicketVM = new IssueTicketVM
            {
                Title = "Test Ticket",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "Test Ticket",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            // Create a file that exceeds the 25MB limit
            var oversizedFile = CreateMockFile("large.pdf", new string('x', 100)); // Just using small content but mocking large size
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("large.pdf");
            mockFile.Setup(f => f.Length).Returns(26 * 1024 * 1024); // 26MB
            var files = new List<IFormFile> { mockFile.Object };

            _mockMapper.Setup(m => m.Map<IssueTicket>(issueTicketVM))
                .Returns(issueTicket);

            _mockIssueTicketService.Setup(s => s.CreateIssueTicket(It.IsAny<IssueTicket>(), files))
                .ThrowsAsync(new ArgumentException("File 'large.pdf' exceeds the maximum allowed size of 25MB."));

            // Act
            var result = await _controller.CreateIssueTicket(issueTicketVM, files);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error", statusCodeResult.Value);
        }
        [Fact]
        public async Task CreateIssueTicket_WithInvalidFileType_ReturnsInternalServerError()
        {
            // Arrange
            var issueTicketVM = new IssueTicketVM
            {
                Title = "Test Ticket",
                TicketType = TicketType.Risk,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "Test Ticket",
                TicketType = TicketType.Risk,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            // Create a file with invalid extension
            var invalidFile = CreateMockFile("image.jpg", "test content");
            var files = new List<IFormFile> { invalidFile };

            _mockMapper.Setup(m => m.Map<IssueTicket>(issueTicketVM))
                .Returns(issueTicket);

            _mockIssueTicketService.Setup(s => s.CreateIssueTicket(It.IsAny<IssueTicket>(), files))
                .ThrowsAsync(new ArgumentException("File 'image.jpg' has an invalid extension. Only .xlsx, .docx, and .pdf files are allowed."));

            // Act
            var result = await _controller.CreateIssueTicket(issueTicketVM, files);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error", statusCodeResult.Value);
        }

        [Fact]
        public async Task CreateIssueTicket_WithMultipleValidFiles_ReturnsCreatedAtAction()
        {
            // Arrange
            var issueTicketVM = new IssueTicketVM
            {
                Title = "Test Ticket",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "Test Ticket",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending,
                Documents = new List<IssueTicketDocument>
        {
            new IssueTicketDocument { Id = Guid.NewGuid(), Title = "report.pdf" ,FileUrl="tickets/report.pdf",  FileFormat="pdf"},
            new IssueTicketDocument { Id = Guid.NewGuid(), Title = "data.xlsx",FileUrl="tickets/data.xlsx",FileFormat="xlsx" },
            new IssueTicketDocument { Id = Guid.NewGuid(), Title = "document.docx" ,FileUrl="tickets/document.docx",FileFormat="docx" }
        }
            };

            // Create multiple files with valid extensions and sizes
            var files = new List<IFormFile>
    {
        CreateMockFile("report.pdf", "pdf content"),
        CreateMockFile("data.xlsx", "xlsx content"),
        CreateMockFile("document.docx", "docx content")
    };

            _mockMapper.Setup(m => m.Map<IssueTicket>(issueTicketVM))
                .Returns(issueTicket);

            _mockIssueTicketService.Setup(s => s.CreateIssueTicket(It.IsAny<IssueTicket>(), files))
                .ReturnsAsync(issueTicket);

            // Act
            var result = await _controller.CreateIssueTicket(issueTicketVM, files);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(IssueTicketController.GetIssueTicketById), createdAtActionResult.ActionName);
            Assert.Equal(issueTicket.Id, createdAtActionResult.RouteValues["id"]);
            Assert.Equal(issueTicket, createdAtActionResult.Value);
        }

        [Fact]
        public async Task CreateIssueTicket_WithMixedValidAndInvalidFiles_ReturnsInternalServerError()
        {
            // Arrange
            var issueTicketVM = new IssueTicketVM
            {
                Title = "Test Ticket",
                TicketType = TicketType.Violation,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "Test Ticket",
                TicketType = TicketType.Violation,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            // Create a mix of valid and invalid files
            var files = new List<IFormFile>
    {
        CreateMockFile("valid.pdf", "pdf content"),
        CreateMockFile("invalid.exe", "exe content")
    };

            _mockMapper.Setup(m => m.Map<IssueTicket>(issueTicketVM))
                .Returns(issueTicket);

            _mockIssueTicketService.Setup(s => s.CreateIssueTicket(It.IsAny<IssueTicket>(), files))
                .ThrowsAsync(new ArgumentException("File 'invalid.exe' has an invalid extension. Only .xlsx, .docx, and .pdf files are allowed."));

            // Act
            var result = await _controller.CreateIssueTicket(issueTicketVM, files);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error", statusCodeResult.Value);
        }

        #endregion

        #region UpdateIssueTicket File Validation Tests

        [Fact]
        public async Task UpdateIssueTicket_WithOversizedFile_ReturnsBadRequest()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var existingTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Original Title",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicketVM = new IssueTicketCreateVM
            {
                Title = "Updated Title",
                TicketType = TicketType.DPIA
            };

            // Create a file that exceeds the 25MB limit
            var oversizedFile = CreateMockFile("large.xlsx", new string('x', 30 * 1024 * 1024));
            var newFiles = new List<IFormFile> { oversizedFile };
            var removedFiles = new List<string>();

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(existingTicket);

            _mockMapper.Setup(m => m.Map(issueTicketVM, existingTicket, It.IsAny<Action<IMappingOperationOptions<IssueTicketCreateVM, IssueTicket>>>()));

            _mockIssueTicketService.Setup(s => s.UpdateIssueTicketFilesOnS3(ticketId, newFiles, removedFiles))
                .ThrowsAsync(new ArgumentException("File 'large.xlsx' exceeds the maximum allowed size of 25MB."));

            // Act
            var result = await _controller.UpdateIssueTicket(ticketId, issueTicketVM, newFiles, removedFiles);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal("Error uploading files.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateIssueTicket_WithInvalidFileType_ReturnsBadRequest()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var existingTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Original Title",
                TicketType = TicketType.Risk,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicketVM = new IssueTicketCreateVM
            {
                Title = "Updated Title",
                TicketType = TicketType.Risk
            };

            // Create a file with invalid extension
            var invalidFile = CreateMockFile("script.js", "javascript code");
            var newFiles = new List<IFormFile> { invalidFile };
            var removedFiles = new List<string>();

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(existingTicket);

            _mockMapper.Setup(m => m.Map(issueTicketVM, existingTicket, It.IsAny<Action<IMappingOperationOptions<IssueTicketCreateVM, IssueTicket>>>()));

            _mockIssueTicketService.Setup(s => s.UpdateIssueTicketFilesOnS3(ticketId, newFiles, removedFiles))
                .ThrowsAsync(new ArgumentException("File 'script.js' has an invalid extension. Only .xlsx, .docx, and .pdf files are allowed."));

            // Act
            var result = await _controller.UpdateIssueTicket(ticketId, issueTicketVM, newFiles, removedFiles);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal("Error uploading files.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateIssueTicket_WithValidFilesAndRemovedFiles_ReturnsOk()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var existingTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Original Title",
                TicketType = TicketType.System,
                IssueTicketStatus = IssueTicketStatus.Pending,
                Documents = new List<IssueTicketDocument>
        {
            new IssueTicketDocument { Id = Guid.NewGuid(), Title = "old.pdf", FileUrl = "tickets/old.pdf",  FileFormat="pdf" }
        }
            };

            var updatedTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Updated Title",
                TicketType = TicketType.System,
                IssueTicketStatus = IssueTicketStatus.Pending,
                Documents = new List<IssueTicketDocument>
        {
            new IssueTicketDocument { Id = Guid.NewGuid(), Title = "new.pdf", FileUrl = "tickets/new.pdf" , FileFormat="pdf" }
        }
            };

            var issueTicketVM = new IssueTicketCreateVM
            {
                Title = "Updated Title",
                TicketType = TicketType.System
            };

            var newFiles = new List<IFormFile> { CreateMockFile("new.pdf", "new content") };
            var removedFiles = new List<string> { "tickets/old.pdf" };

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(existingTicket);

            _mockMapper.Setup(m => m.Map(issueTicketVM, existingTicket, It.IsAny<Action<IMappingOperationOptions<IssueTicketCreateVM, IssueTicket>>>()))
                .Callback((IssueTicketCreateVM src, IssueTicket dest, Action<IMappingOperationOptions<IssueTicketCreateVM, IssueTicket>> opts) =>
                {
                    dest.Title = src.Title;
                    dest.TicketType = src.TicketType;
                });

            _mockIssueTicketService.Setup(s => s.UpdateIssueTicketFilesOnS3(ticketId, newFiles, removedFiles))
                .ReturnsAsync(updatedTicket.Documents);

            _mockIssueTicketService.Setup(s => s.UpdateAsync(It.IsAny<IssueTicket>()))
                .ReturnsAsync(updatedTicket);

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(updatedTicket);

            // Act
            var result = await _controller.UpdateIssueTicket(ticketId, issueTicketVM, newFiles, removedFiles);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTicket = Assert.IsType<IssueTicket>(okResult.Value);
            Assert.Equal(ticketId, returnedTicket.Id);
            Assert.Equal("Updated Title", returnedTicket.Title);
            Assert.Equal(1, returnedTicket.Documents.Count);
            Assert.Equal("new.pdf", returnedTicket.Documents[0].Title);
        }

        [Fact]
        public async Task UpdateIssueTicket_WithBorderlineSizeFile_ReturnsOk()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var existingTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Original Title",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var updatedTicket = new IssueTicket
            {
                Id = ticketId,
                Title = "Updated Title",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            var issueTicketVM = new IssueTicketCreateVM
            {
                Title = "Updated Title",
                TicketType = TicketType.DPIA
            };

            // Create a file that's exactly at the 25MB limit
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("borderline.pdf");
            mockFile.Setup(f => f.Length).Returns(25 * 1024 * 1024); // Exactly 25MB

            var newFiles = new List<IFormFile> { mockFile.Object };
            var removedFiles = new List<string>();

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(existingTicket);

            _mockMapper.Setup(m => m.Map(issueTicketVM, existingTicket, It.IsAny<Action<IMappingOperationOptions<IssueTicketCreateVM, IssueTicket>>>()));

            _mockIssueTicketService.Setup(s => s.UpdateIssueTicketFilesOnS3(ticketId, newFiles, removedFiles))
                .ReturnsAsync(new List<IssueTicketDocument>());

            _mockIssueTicketService.Setup(s => s.UpdateAsync(It.IsAny<IssueTicket>()))
                .ReturnsAsync(updatedTicket);

            _mockIssueTicketService.Setup(s => s.GetByIdAsync(ticketId))
                .ReturnsAsync(updatedTicket);

            // Act
            var result = await _controller.UpdateIssueTicket(ticketId, issueTicketVM, newFiles, removedFiles);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedTicket, okResult.Value);
        }

        #endregion

    }
}

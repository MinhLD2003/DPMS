using AutoMapper;
using DPMS_WebAPI.Controllers;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace DPMS_WebAPI.Tests.UnitTests.Controllers
{
    public class GroupControllerTest
    {
        private readonly Mock<IGroupService> _groupServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<GroupController>> _loggerMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly GroupController _controller;

        public GroupControllerTest()
        {
            _groupServiceMock = new Mock<IGroupService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<GroupController>>();
            _userServiceMock = new Mock<IUserService>();

            _controller = new GroupController(
                _groupServiceMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _userServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateGroup_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var groupVM = new GroupVM { Name = "Test Group" };
            var group = new Group { Name = "Test Group" };
            var createdGroup = new Group { Id = Guid.NewGuid(), Name = "Test Group" };

            _mapperMock.Setup(m => m.Map<Group>(groupVM)).Returns(group);
            _groupServiceMock.Setup(s => s.AddAsync(group)).ReturnsAsync(createdGroup);

            // Act
            var result = await _controller.CreateGroup(groupVM);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(createdGroup);
        }

        [Fact]
        public async Task CreateGroup_ShouldReturnBadRequest_WhenExceptionThrown()
        {
            // Arrange
            var groupVM = new GroupVM { Name = "Test Group" };

            _mapperMock.Setup(m => m.Map<Group>(groupVM)).Throws(new Exception("Something went wrong"));

            // Act
            var result = await _controller.CreateGroup(groupVM);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("Something went wrong");
        }

        [Fact]
        public async Task GetGroupById_ShouldReturnOkResult_WithGroupData()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var mockResult = new GroupDetailVM { Name = "MockGroup" };

            _groupServiceMock.Setup(s => s.GetGroupDetailAsync(groupId)).ReturnsAsync(mockResult);

            // Act
            var result = await _controller.GetGroupById(groupId);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(mockResult);
        }

        [Fact]
        public async Task AddUsersToGroup_ShouldReturnBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("userIds", "Required");

            // Act
            var result = await _controller.AddUsersToGroup(Guid.NewGuid(), new List<Guid>());

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddUsersToGroup_ShouldReturnOk_WhenUpdateSuccess()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var userIds = new List<Guid> { Guid.NewGuid() };

            _groupServiceMock.Setup(s => s.AddUserToGroup(groupId, userIds)).ReturnsAsync(1);

            // Act
            var result = await _controller.AddUsersToGroup(groupId, userIds);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be("Added user to group sucessfully");
        }
    }

}

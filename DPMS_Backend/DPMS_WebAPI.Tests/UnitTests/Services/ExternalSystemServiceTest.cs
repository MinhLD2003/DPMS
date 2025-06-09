using System.Linq.Expressions;
using AutoMapper;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Events.EventModels;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.MapperProfiles;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.ViewModels.ExternalSystem;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace DPMS_WebAPI.Tests.UnitTests.Services
{
    public class ExternalSystemServiceTest
    {
        private readonly Mock<IExternalSystemRepository> _mockExternalSystemRepository;
        private readonly Mock<IExternalSystemPurposeRepository> _mockExternalSystemPurposeRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<ExternalSystemService>> _mockLogger;
        private readonly ExternalSystemService _externalSystemService;
        private readonly Mock<IGroupRepository> _mockGroupRepository;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IUserService> _mockUserService;
        
        public ExternalSystemServiceTest()
        {
            _mockExternalSystemRepository = new Mock<IExternalSystemRepository>();
            _mockExternalSystemPurposeRepository = new Mock<IExternalSystemPurposeRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<ExternalSystemService>>();
            _mockGroupRepository = new Mock<IGroupRepository>();
            _mockMediator = new Mock<IMediator>();
            _mockUserService = new Mock<IUserService>();

            // Setup default mocks for UnitOfWork properties
            _mockUnitOfWork.Setup(x => x.ExternalSystems).Returns(_mockExternalSystemRepository.Object);
            _mockUnitOfWork.Setup(x => x.ExternalSystemPurposes).Returns(_mockExternalSystemPurposeRepository.Object);
            _mockUnitOfWork.Setup(x => x.Groups).Returns(_mockGroupRepository.Object);

            // Create real mapper instance
            var mapperConfig = new MapperConfiguration(cfg => {
                cfg.AddProfile<AutoMapperProfiles>();
            });
            var mapper = mapperConfig.CreateMapper();

            _externalSystemService = new ExternalSystemService(
                _mockUnitOfWork.Object,
                _mockExternalSystemPurposeRepository.Object,
                _mockExternalSystemRepository.Object,
                mapper,
                _mockGroupRepository.Object,
                _mockMediator.Object,
                _mockUserService.Object
            );
        }

        [Fact]
        public async Task AddExternalSystem_Should_Succeed_When_Valid_Input()
        {
            // Arrange
            var externalSystem = new ExternalSystem
            {
                Id = Guid.NewGuid(),
                Name = "Test External System",
                Description = "Test Description",
                Status = ExternalSystemStatus.WaitingForFIC
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                UserName = "testuser",
                FullName = "Test User"
            };

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { user });

            _mockUnitOfWork
                .Setup(x => x.Users)
                .Returns(mockUserRepository.Object);

            _mockExternalSystemRepository
                .Setup(x => x.AddAsync(It.IsAny<ExternalSystem>()))
                .ReturnsAsync(externalSystem);

            _mockGroupRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Group, bool>>>()))
                .ReturnsAsync(new List<Group>());

            // Act
            var result = await _externalSystemService.AddExternalSystem(new AddSystemVM
            {
                Name = "Test External System",
                Description = "Test Description",
                ProductDevEmails = new List<string> { "test@test.com" },
                BusinessOwnerEmails = new List<string> { "test@test.com" },
                Domain = "test.com"
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(externalSystem.Name, result.Name);
            Assert.Equal(externalSystem.Description, result.Description);
            Assert.Equal(externalSystem.Status, result.Status);

            _mockExternalSystemRepository.Verify(x => x.AddAsync(It.IsAny<ExternalSystem>()), Times.Once);
            _mockGroupRepository.Verify(x => x.AddAsync(It.IsAny<Group>()), Times.Exactly(2));
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.AtLeast(2));
            _mockMediator.Verify(x => x.Publish(It.IsAny<UserAddedToSystemNotification>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task AddExternalSystem_Should_Throw_When_Email_Not_Exists()
        {
            // Arrange
            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User>());

            _mockUnitOfWork
                .Setup(x => x.Users)
                .Returns(mockUserRepository.Object);

            // Setup the external system repository mock
            _mockExternalSystemRepository
                .Setup(x => x.AddAsync(It.IsAny<ExternalSystem>()))
                .ReturnsAsync((ExternalSystem)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _externalSystemService.AddExternalSystem(new AddSystemVM
            {
                Name = "Test External System",
                Description = "Test Description",
                ProductDevEmails = new List<string> { "nonexistent@test.com" },
                BusinessOwnerEmails = new List<string> { "test@test.com" },
                Domain = "test.com"
            }));

            // Verify that the external system was never added
            _mockExternalSystemRepository.Verify(x => x.AddAsync(It.IsAny<ExternalSystem>()), Times.Never);
            _mockGroupRepository.Verify(x => x.AddAsync(It.IsAny<Group>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
            _mockMediator.Verify(x => x.Publish(It.IsAny<UserAddedToSystemNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AddExternalSystem_Should_Add_User_To_Existing_Group()
        {
            // Arrange
            var existingGroup = new Group { Id = Guid.NewGuid(), Name = "Test Group" };
            var user = new User 
            { 
                Id = Guid.NewGuid(), 
                Email = "test@test.com",
                UserName = "testuser",
                FullName = "Test User"
            };

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { user });

            _mockUnitOfWork
                .Setup(x => x.Users)
                .Returns(mockUserRepository.Object);

            _mockGroupRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Group, bool>>>()))
                .ReturnsAsync(new List<Group> { existingGroup });

            // Act
            await _externalSystemService.AddExternalSystem(new AddSystemVM
            {
                Name = "Test External System",
                Description = "Test Description",
                ProductDevEmails = new List<string> { "test@test.com" },
                BusinessOwnerEmails = new List<string> { "test@test.com" },
                Domain = "test.com"
            });

            // Assert
            _mockGroupRepository.Verify(x => x.AddAsync(It.IsAny<Group>()), Times.Exactly(2));
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RemoveExternalSystem_Should_Succeed_When_Status_Is_WaitingForFIC()
        {
            // Arrange
            var system = new ExternalSystem
            {
                Id = Guid.NewGuid(),
                Name = "Test System",
                Status = ExternalSystemStatus.WaitingForFIC
            };

            var groups = new List<Group>
            {
                new Group { Id = Guid.NewGuid(), SystemId = system.Id, Name = "ProductDev" },
                new Group { Id = Guid.NewGuid(), SystemId = system.Id, Name = "BusinessOwner" }
            };

            _mockExternalSystemRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(system);

            _mockGroupRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Group, bool>>>()))
                .ReturnsAsync(groups);

            // Act
            await _externalSystemService.RemoveExternalSystem(system.Id);

            // Assert
            _mockExternalSystemRepository.Verify(x => x.DeleteAsync(system.Id), Times.Once);
            _mockGroupRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Exactly(2));
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RemoveExternalSystem_Should_Throw_When_Status_Is_Active()
        {
            // Arrange
            var system = new ExternalSystem
            {
                Id = Guid.NewGuid(),
                Name = "Test System",
                Status = ExternalSystemStatus.Active
            };

            _mockExternalSystemRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(system);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _externalSystemService.RemoveExternalSystem(system.Id));

            // Assert
            _mockExternalSystemRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _mockGroupRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task AddPurposeToSystemAsync_Should_Succeed_When_Valid_Input()
        {
            // Arrange
            var system = new ExternalSystem 
            { 
                Id = Guid.NewGuid(),
                Name = "Test System"
            };
            var purpose = new Purpose { Id = Guid.NewGuid() };

            _mockUnitOfWork
                .Setup(x => x.ExternalSystems.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(system);

            _mockUnitOfWork
                .Setup(x => x.Purposes.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(purpose);

            _mockUnitOfWork
                .Setup(x => x.ExternalSystemPurposes.FindAsync(It.IsAny<Expression<Func<ExternalSystemPurpose, bool>>>()))
                .ReturnsAsync(new List<ExternalSystemPurpose>());

            // Act
            await _externalSystemService.AddPurposeToSystemAsync(system.Id, purpose.Id);

            // Assert
            _mockUnitOfWork.Verify(x => x.ExternalSystemPurposes.AddAsync(It.IsAny<ExternalSystemPurpose>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddPurposeToSystemAsync_Should_Throw_When_Purpose_Already_Exists()
        {
            // Arrange
            var system = new ExternalSystem
            {
                Id = Guid.NewGuid(),
                Name = "Test System"
            };
            var purpose = new Purpose { Id = Guid.NewGuid() };
            var existingPurpose = new ExternalSystemPurpose { ExternalSystemId = system.Id, PurposeId = purpose.Id };

            _mockExternalSystemRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(system);

            _mockExternalSystemPurposeRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<ExternalSystemPurpose, bool>>>()))
                .ReturnsAsync(new List<ExternalSystemPurpose> { existingPurpose });

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => 
                _externalSystemService.AddPurposeToSystemAsync(system.Id, purpose.Id));

            // Assert
            _mockExternalSystemPurposeRepository.Verify(x => x.AddAsync(It.IsAny<ExternalSystemPurpose>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateSystemStatus_Should_Succeed_When_Valid_Status()
        {
            // Arrange
            var system = new ExternalSystem
            {
                Id = Guid.NewGuid(),
                Name = "Test System",
                Status = ExternalSystemStatus.Active
            };
            var statusVM = new SystemStatusVM 
            { 
                SystemId = system.Id, 
                Status = ExternalSystemStatus.Inactive 
            };
            var allowedStatuses = new List<ExternalSystemStatus> { ExternalSystemStatus.Active, ExternalSystemStatus.Inactive };

            _mockUnitOfWork
                .Setup(x => x.ExternalSystems.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(system);

            // Act
            await _externalSystemService.UpdateSystemStatus(statusVM, allowedStatuses);

            // Assert
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
            Assert.Equal(ExternalSystemStatus.Inactive, system.Status);
        }

        [Fact]
        public async Task UpdateSystemStatus_Should_Throw_When_Invalid_Status_Transition()
        {
            // Arrange
            var system = new ExternalSystem
            {
                Id = Guid.NewGuid(),
                Name = "Test System",
                Status = ExternalSystemStatus.Active
            };
            var statusVM = new SystemStatusVM 
            { 
                SystemId = system.Id, 
                Status = ExternalSystemStatus.WaitingForFIC 
            };
            var allowedStatuses = new List<ExternalSystemStatus> { ExternalSystemStatus.Active, ExternalSystemStatus.Inactive };

            _mockExternalSystemRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(system);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _externalSystemService.UpdateSystemStatus(statusVM, allowedStatuses));

            // Assert
            _mockExternalSystemRepository.Verify(x => x.Update(It.IsAny<ExternalSystem>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateSystemStatus_Should_Throw_When_System_Not_Found()
        {
            // Arrange
            var statusVM = new SystemStatusVM 
            { 
                SystemId = Guid.NewGuid(),
                Status = ExternalSystemStatus.Active 
            };
            var allowedStatuses = new List<ExternalSystemStatus> { ExternalSystemStatus.Active };

            _mockExternalSystemRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ExternalSystem)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _externalSystemService.UpdateSystemStatus(statusVM, allowedStatuses));

            // Assert
            _mockExternalSystemRepository.Verify(x => x.Update(It.IsAny<ExternalSystem>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllUsersAsync_Should_Return_Users_When_System_Has_Users()
        {
            // Arrange
            var system = new ExternalSystem
            {
                Id = Guid.NewGuid(),
                Name = "Test System"
            };
            var users = new List<User>
            {
                new User { 
                    Id = Guid.NewGuid(), 
                    Email = "test1@test.com", 
                    UserName = "test1", 
                    FullName = "Test User 1",
                    Status = UserStatus.Activated
                },
                new User { 
                    Id = Guid.NewGuid(), 
                    Email = "test2@test.com", 
                    UserName = "test2", 
                    FullName = "Test User 2",
                    Status = UserStatus.Activated
                }
            };

            _mockUnitOfWork
                .Setup(x => x.ExternalSystems.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(system);

            _mockUnitOfWork
                .Setup(x => x.ExternalSystems.GetUsersAsync(It.IsAny<Guid>()))
                .ReturnsAsync(users);

            // Create real mapper instance
            var mapperConfig = new MapperConfiguration(cfg => {
                cfg.AddProfile<AutoMapperProfiles>();
            });
            var mapper = mapperConfig.CreateMapper();

            // Create service with real mapper
            var service = new ExternalSystemService(
                _mockUnitOfWork.Object,
                _mockExternalSystemPurposeRepository.Object,
                _mockExternalSystemRepository.Object,
                mapper,
                _mockGroupRepository.Object,
                _mockMediator.Object,
                _mockUserService.Object
            );

            // Act
            var result = await service.GetAllUsersAsync(system.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(users.Count, result.Value.Count);
            
            // Verify the first user's properties
            var firstUser = result.Value[0];
            Assert.Equal(users[0].Id, firstUser.Id);
            Assert.Equal(users[0].Email, firstUser.Email);
            Assert.Equal(users[0].FullName, firstUser.FullName);
            Assert.Equal(users[0].Status, firstUser.Status);

            // Verify the second user's properties
            var secondUser = result.Value[1];
            Assert.Equal(users[1].Id, secondUser.Id);
            Assert.Equal(users[1].Email, secondUser.Email);
            Assert.Equal(users[1].FullName, secondUser.FullName);
            Assert.Equal(users[1].Status, secondUser.Status);
        }

        [Fact]
        public async Task GetAllUsersAsync_Should_Return_Empty_When_System_Has_No_Users()
        {
            // Arrange
            var system = new ExternalSystem
            {
                Id = Guid.NewGuid(),
                Name = "Test System"
            };

            _mockExternalSystemRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(system);

            _mockExternalSystemRepository
                .Setup(x => x.GetUsersAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<User>());

            // Act
            var result = await _externalSystemService.GetAllUsersAsync(system.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task AddPurposeToSystemAsync_Should_Throw_When_System_Not_Found()
        {
            // Arrange
            var system = new ExternalSystem
            {
                Id = Guid.NewGuid(),
                Name = "Test System"
            };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                UserName = "testuser",
                FullName = "Test User"
            };
            var purpose = new Purpose { Id = Guid.NewGuid() };

            _mockExternalSystemRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ExternalSystem)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _externalSystemService.AddPurposeToSystemAsync(system.Id, purpose.Id));

            // Assert
            _mockExternalSystemPurposeRepository.Verify(x => x.AddAsync(It.IsAny<ExternalSystemPurpose>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }
    }
}

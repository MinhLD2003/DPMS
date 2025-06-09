using AutoMapper;
using DPMS_WebAPI.Builders;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.ViewModels.DPIA;
using DPMS_WebAPI.ViewModels.Comment;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Xunit;

namespace DPMS_WebAPI.Tests.UnitTests.Services
{
    public class DPIAServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IDPIARepository> _dpiaRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEventMessageBuilder> _messageBuilderMock;
        private readonly Mock<IFileRepository> _fileRepositoryMock;
        private readonly Mock<IMemberResponsibilityRepository> _memberResponsibilityRepositoryMock;
        private readonly Mock<IDPIAMemberRepository> _dpiaMemberRepositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IExternalSystemService> _externalSystemServiceMock;
        private readonly DPIAService _dpiaService;
        private readonly string _testTemplatePath;
        
        public DPIAServiceTest()    
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _dpiaRepositoryMock = new Mock<IDPIARepository>();
            _mapperMock = new Mock<IMapper>();
            _messageBuilderMock = new Mock<IEventMessageBuilder>();
            _fileRepositoryMock = new Mock<IFileRepository>();
            _memberResponsibilityRepositoryMock = new Mock<IMemberResponsibilityRepository>();
            _dpiaMemberRepositoryMock = new Mock<IDPIAMemberRepository>();
            _mediatorMock = new Mock<IMediator>();
            _externalSystemServiceMock = new Mock<IExternalSystemService>();
            _dpiaService = new DPIAService(_unitOfWorkMock.Object,
                _mapperMock.Object,
                _messageBuilderMock.Object,
                _dpiaRepositoryMock.Object,
                _fileRepositoryMock.Object,
                _memberResponsibilityRepositoryMock.Object,
                _dpiaMemberRepositoryMock.Object,
                _mediatorMock.Object,
                _externalSystemServiceMock.Object
            );
            
        }

        [Fact]
        public async Task GetCommentsAsync_UserIsMember_ReturnsComments()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA"
            };
            
            var comments = new List<Comment>
            {
                new Comment { Id = Guid.NewGuid(), Content = "Test comment" }
            };
            
            _unitOfWorkMock.Setup(u => u.DPIAs.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember> { new DPIAMember { Id = Guid.NewGuid(), DPIAId = dpiaId, UserId = userId } });
                
            _dpiaRepositoryMock.Setup(r => r.GetCommentsAsync(dpiaId))
                .ReturnsAsync(comments);
                
            // Act
            var result = await _dpiaService.GetCommentsAsync(dpiaId, principal);
            
            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test comment", result[0].Content);
        }
        
        [Fact]
        public async Task GetCommentsAsync_UserIsNotMemberNorAdmin_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA"
            };
            
            _unitOfWorkMock.Setup(u => u.DPIAs.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember>()); // Empty list - user is not a member
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _dpiaService.GetCommentsAsync(dpiaId, principal));
                
            Assert.Equal("User is not a member of this DPIA", exception.Message);
        }
        
        [Fact]
        public async Task GetCommentsAsync_DPIANotFound_ThrowsException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            _unitOfWorkMock.Setup(u => u.DPIAs.GetByIdAsync(dpiaId))
                .ReturnsAsync((DPIA)null);

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Ok(true));

            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember>()); // Empty list - user is not a member
                

                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.GetCommentsAsync(dpiaId, principal));
                
            Assert.Equal("DPIA not found", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_ValidRequest_DeletesSuccessfully()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Draft
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));
                
            _dpiaRepositoryMock.Setup(r => r.DeleteAsync(dpiaId))
                .Returns(Task.FromResult(true));
                
            // Act
            await _dpiaService.DeleteAsync(dpiaId, principal);
            
            // Assert
            _dpiaRepositoryMock.Verify(r => r.DeleteAsync(dpiaId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        
        [Fact]
        public async Task DeleteAsync_DPIANotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync((DPIA)null);
                
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _dpiaService.DeleteAsync(dpiaId, principal));
        }
        
        [Fact]
        public async Task DeleteAsync_UserIDMissingFromClaims_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var principal = new ClaimsPrincipal(new ClaimsIdentity()); // Empty claims
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Draft
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _dpiaService.DeleteAsync(dpiaId, principal));
                
            Assert.Equal("User ID not found in claims", exception.Message);
        }
        
        [Fact]
        public async Task DeleteAsync_UserNotInDPO_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Draft
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(false));

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Ok(false));
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _dpiaService.DeleteAsync(dpiaId, principal));
                
            Assert.Equal("Only DPO or admin can perform this action", exception.Message);
        }
        
        [Fact]
        public async Task DeleteAsync_DPIANotInDraftStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Started // Not in Draft status
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));
                
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _dpiaService.DeleteAsync(dpiaId, principal));
        }

        [Fact]
        public async Task GetDPIAByIdAsync_ValidInput_ReturnsPopulatedDPIADetailVM()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Description = "Test Description",
                Status = DPIAStatus.Draft,
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = Guid.NewGuid(),
                ExternalSystem = new ExternalSystem { Name = "Test System" },
                CreatedById = Guid.NewGuid(),
                CreatedBy = new User { 
                    FullName = "Creator", 
                    UserName = "creator",
                    Email = "creator@example.com"
                },
                LastModifiedById = Guid.NewGuid(),
                LastModifiedBy = new User { 
                    FullName = "Modifier",
                    UserName = "modifier",
                    Email = "modifier@example.com"
                },
                Documents = new List<DPIADocument>(),
                DPIAMembers = new List<DPIAMember>(),
                Responsibilities = new List<DPIAResponsibility>()
            };
            
            var dpiaMembers = new List<DPIAMember> 
            { 
                new DPIAMember 
                { 
                    Id = Guid.NewGuid(),
                    DPIAId = dpiaId,
                    User = new User 
                    {
                        UserName = "testuser", 
                        FullName = "Test User", 
                        Email = "test@example.com"
                    }
                }
            };

            var responsibilities = new List<DPIAResponsibility>
            {
                new DPIAResponsibility
                {
                    Id = Guid.NewGuid(),
                    DPIAId = dpiaId,
                    Responsibility = new Responsibility { Title = "Test Responsibility" }
                }
            };
            
            var responsibilityVMs = new List<DPIAResponsibilityListVM>
            {
                new DPIAResponsibilityListVM 
                { 
                    Id = responsibilities[0].Id, 
                    Title = "Test Responsibility" 
                }
            };
            
            var expectedDpiaDetailVM = new DPIADetailVM
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Description = "Test Description",
                Status = DPIAStatus.Draft,
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = dpia.ExternalSystemId.Value,
                ExternalSystemName = "Test System",
                CreatedBy = "Creator",
                CreatedById = dpia.CreatedById,
                UpdatedBy = "Modifier",
                UpdatedById = dpia.LastModifiedById,
                Responsibilities = responsibilityVMs
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetDPIADetailAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _dpiaRepositoryMock.Setup(r => r.GetDPIAMembersAsync(dpiaId))
                .ReturnsAsync(dpiaMembers);
                
            _dpiaRepositoryMock.Setup(r => r.GetDPIAResponsibilitiesAsync(dpiaId))
                .ReturnsAsync(responsibilities);
                
            _mapperMock.Setup(m => m.Map<List<DPIAResponsibilityListVM>>(It.IsAny<List<DPIAResponsibility>>()))
                .Returns(responsibilityVMs);
                
            _mapperMock.Setup(m => m.Map<DPIADetailVM>(It.IsAny<DPIA>()))
                .Returns(expectedDpiaDetailVM);
            
            // Act
            var result = await _dpiaService.GetDPIAByIdAsync(dpiaId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(dpiaId, result.Id);
            Assert.Equal("Test DPIA", result.Title);
            Assert.Equal("Test Description", result.Description);
            Assert.Equal(DPIAStatus.Draft, result.Status);
            Assert.Equal(DPIAType.NewOrUpdatedSystem, result.Type);
            Assert.Equal(dpia.ExternalSystemId, result.ExternalSystemId);
            Assert.Equal("Test System", result.ExternalSystemName);
            Assert.Equal(dpia.CreatedById, result.CreatedById);
            Assert.Equal(dpia.LastModifiedById, result.UpdatedById);
            Assert.NotEmpty(result.Responsibilities);
        }

        [Fact]
        public async Task GetDPIAByIdAsync_DPIANotFound_ThrowsException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            
            _dpiaRepositoryMock.Setup(r => r.GetDPIADetailAsync(dpiaId))
                .ReturnsAsync((DPIA)null);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.GetDPIAByIdAsync(dpiaId));
                
            Assert.Equal("DPIA not found", exception.Message);
        }

        [Fact]
        public async Task GetDPIADetailByMemberId_ValidInput_ReturnsDPIADetailVM()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Description = "Test Description",
                ExternalSystem = new ExternalSystem { Name = "Test System" },
                Documents = new List<DPIADocument>(),
                DPIAMembers = new List<DPIAMember>(),
                Responsibilities = new List<DPIAResponsibility>()
            };
            
            var dpiaMembers = new List<DPIAMember> 
            { 
                new DPIAMember 
                { 
                    Id = memberId,
                    DPIAId = dpiaId,
                    User = new User 
                    {
                        UserName = "testuser", 
                        FullName = "Test User", 
                        Email = "test@example.com"
                    }
                }
            };

            var responsibilityId = Guid.NewGuid();
            var dpiaResponsibilities = new List<DPIAResponsibility>
            {
                new DPIAResponsibility
                {
                    Id = responsibilityId,
                    DPIAId = dpiaId,
                    Responsibility = new Responsibility { Title = "Test Responsibility" },
                    MemberResponsibilities = new List<MemberResponsibility>
                    {
                        new MemberResponsibility { MemberId = memberId }
                    }
                }
            };
            
            var expectedDpiaDetailVM = new DPIADetailVM
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Description = "Test Description",
                Responsibilities = new List<DPIAResponsibilityListVM>
                {
                    new DPIAResponsibilityListVM { Id = responsibilityId }
                }
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetDetailAsync(
                    dpiaId,
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>()
                ))
                .ReturnsAsync(dpia);
                
            _dpiaRepositoryMock.Setup(r => r.GetDPIAMembersAsync(dpiaId))
                .ReturnsAsync(dpiaMembers);
                
            _dpiaRepositoryMock.Setup(r => r.GetDPIAResponsibilitiesAsync(dpiaId))
                .ReturnsAsync(dpiaResponsibilities);
                
            _mapperMock.Setup(m => m.Map<List<DPIAResponsibilityListVM>>(It.IsAny<List<DPIAResponsibility>>()))
                .Returns(expectedDpiaDetailVM.Responsibilities);
                
            _mapperMock.Setup(m => m.Map<DPIADetailVM>(It.IsAny<DPIA>()))
                .Returns(expectedDpiaDetailVM);
            
            // Act
            var result = await _dpiaService.GetDPIADetailByMemberId(dpiaId, memberId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(dpiaId, result.Id);
            Assert.Equal("Test DPIA", result.Title);
            Assert.Equal("Test Description", result.Description);
            Assert.NotEmpty(result.Responsibilities);
        }

        [Fact]
        public async Task GetDPIADetailByMemberId_DPIANotFound_ThrowsException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            
            _dpiaRepositoryMock.Setup(r => r.GetDetailAsync(
                    dpiaId,
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>()
                ))
                .ReturnsAsync((DPIA)null);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.GetDPIADetailByMemberId(dpiaId, memberId));
                
            Assert.Equal("DPIA not found", exception.Message);
        }
        
        [Fact]
        public async Task AddAsync_ValidInput_ReturnsDPIA()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            var externalSystemId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            
            var entity = new DPIACreateVM
            {
                Title = "Test DPIA",
                ExternalSystemId = externalSystemId,
                Description = "Test Description",
                DueDate = DateTime.UtcNow.AddDays(30),
                Type = DPIAType.NewOrUpdatedSystem,
                Responsibilities = new List<DPIAResponsibilityCreateVM>
                {
                    new DPIAResponsibilityCreateVM
                    {
                        DueDate = DateTime.UtcNow.AddDays(20),
                        ResponsibilityId = responsibilityId,
                        UserIds = new List<Guid> { userId },
                        Pic = userId
                    }
                }
            };

            var dpia = new DPIA
            {
                Title = entity.Title,
                Description = entity.Description,
                ExternalSystemId = entity.ExternalSystemId,
                Type = entity.Type,
                Events = new List<DPIAEvent>(),
                Documents = new List<DPIADocument>(),
                DPIAMembers = new List<DPIAMember>()
            };

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));

            _externalSystemServiceMock.Setup(s => s.GetByIdAsync(externalSystemId))
                .ReturnsAsync(new ExternalSystem { 
                    Name = "Test System",
                    Status = ExternalSystemStatus.Active
                });

            _mapperMock.Setup(m => m.Map<DPIA>(It.IsAny<DPIACreateVM>()))
                .Returns(dpia);

            _unitOfWorkMock.Setup(u => u.Users.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { 
                    new User { 
                        Id = userId,
                        UserName = "testuser",
                        FullName = "Test User",
                        Email = "test@example.com"
                    } 
                });

            _messageBuilderMock.Setup(m => m.BuildDPIACreatedEvent(It.IsAny<string>()))
                .Returns("DPIA Created");

            _unitOfWorkMock.Setup(u => u.DPIAs.AddAsync(It.IsAny<DPIA>()))
                .ReturnsAsync(dpia);

            // Act
            var result = await _dpiaService.AddAsync(entity, principal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Title, result.Title);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddAsync_UserNotInDPOGroup_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            var entity = new DPIACreateVM
            {
                Title = "Test DPIA",
                ExternalSystemId = Guid.NewGuid()
            };

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(false));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _dpiaService.AddAsync(entity, principal));
        }

        [Fact]
        public async Task AddAsync_ExternalSystemNotFound_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            var entity = new DPIACreateVM
            {
                Title = "Test DPIA",
                ExternalSystemId = Guid.NewGuid()
            };

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));

            _externalSystemServiceMock.Setup(s => s.GetByIdAsync(entity.ExternalSystemId))
                .ReturnsAsync((ExternalSystem)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.AddAsync(entity, principal));
            
            Assert.Equal("System not found", exception.Message);
        }

        [Fact]
        public async Task AddAsync_ExternalSystemInactive_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            var entity = new DPIACreateVM
            {
                Title = "Test DPIA",
                ExternalSystemId = Guid.NewGuid()
            };

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));

            _externalSystemServiceMock.Setup(s => s.GetByIdAsync(entity.ExternalSystemId))
                .ReturnsAsync(new ExternalSystem { 
                    Name = "Test System",
                    Status = ExternalSystemStatus.Inactive 
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.AddAsync(entity, principal));
            
            Assert.Contains("Cannot create DPIA when system is in", exception.Message);
        }

        [Fact]
        public async Task AddAsync_DPIADueDateInPast_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            var entity = new DPIACreateVM
            {
                Title = "Test DPIA",
                ExternalSystemId = Guid.NewGuid(),
                DueDate = DateTime.UtcNow.AddDays(-1)
            };

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));

            _externalSystemServiceMock.Setup(s => s.GetByIdAsync(entity.ExternalSystemId))
                .ReturnsAsync(new ExternalSystem { 
                    Name = "Test System",
                    Status = ExternalSystemStatus.Active 
                });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _dpiaService.AddAsync(entity, principal));
        }

        [Fact]
        public async Task AddAsync_ResponsibilityDueDateInPast_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            var entity = new DPIACreateVM
            {
                Title = "Test DPIA",
                ExternalSystemId = Guid.NewGuid(),
                DueDate = DateTime.UtcNow.AddDays(30),
                Responsibilities = new List<DPIAResponsibilityCreateVM>
                {
                    new DPIAResponsibilityCreateVM
                    {
                        DueDate = DateTime.UtcNow.AddDays(-1),
                        ResponsibilityId = Guid.NewGuid(),
                        UserIds = new List<Guid> { userId }
                    }
                }
            };

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));

            _externalSystemServiceMock.Setup(s => s.GetByIdAsync(entity.ExternalSystemId))
                .ReturnsAsync(new ExternalSystem { 
                    Name = "Test System",
                    Status = ExternalSystemStatus.Active 
                });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _dpiaService.AddAsync(entity, principal));
        }

        [Fact]
        public async Task AddAsync_ResponsibilityDueDateAfterDPIADueDate_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            var entity = new DPIACreateVM
            {
                Title = "Test DPIA",
                ExternalSystemId = Guid.NewGuid(),
                DueDate = DateTime.UtcNow.AddDays(10),
                Responsibilities = new List<DPIAResponsibilityCreateVM>
                {
                    new DPIAResponsibilityCreateVM
                    {
                        DueDate = DateTime.UtcNow.AddDays(20),
                        ResponsibilityId = Guid.NewGuid(),
                        UserIds = new List<Guid> { userId },
                        Pic = userId
                    }
                }
            };

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));

            _externalSystemServiceMock.Setup(s => s.GetByIdAsync(entity.ExternalSystemId))
                .ReturnsAsync(new ExternalSystem { 
                    Name = "Test System",
                    Status = ExternalSystemStatus.Active 
                });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _dpiaService.AddAsync(entity, principal));
        }

        [Fact]
        public async Task AddAsync_PICNotInUserIds_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            var entity = new DPIACreateVM
            {
                Title = "Test DPIA",
                ExternalSystemId = Guid.NewGuid(),
                DueDate = DateTime.UtcNow.AddDays(30),
                Responsibilities = new List<DPIAResponsibilityCreateVM>
                {
                    new DPIAResponsibilityCreateVM
                    {
                        DueDate = DateTime.UtcNow.AddDays(20),
                        ResponsibilityId = Guid.NewGuid(),
                        UserIds = new List<Guid> { userId },
                        Pic = differentUserId
                    }
                }
            };

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));

            _externalSystemServiceMock.Setup(s => s.GetByIdAsync(entity.ExternalSystemId))
                .ReturnsAsync(new ExternalSystem { 
                    Name = "Test System",
                    Status = ExternalSystemStatus.Active 
                });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _dpiaService.AddAsync(entity, principal));
        }

        [Fact]
        public async Task AddAsync_UserIdsHasValuesButPicIsNull_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            var entity = new DPIACreateVM
            {
                Title = "Test DPIA",
                ExternalSystemId = Guid.NewGuid(),
                DueDate = DateTime.UtcNow.AddDays(30),
                Responsibilities = new List<DPIAResponsibilityCreateVM>
                {
                    new DPIAResponsibilityCreateVM
                    {
                        DueDate = DateTime.UtcNow.AddDays(20),
                        ResponsibilityId = Guid.NewGuid(),
                        UserIds = new List<Guid> { userId },
                        Pic = null
                    }
                }
            };

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));

            _externalSystemServiceMock.Setup(s => s.GetByIdAsync(entity.ExternalSystemId))
                .ReturnsAsync(new ExternalSystem { 
                    Name = "Test System",
                    Status = ExternalSystemStatus.Active 
                });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _dpiaService.AddAsync(entity, principal));
        }

        [Fact]
        public async Task AddAsync_SomeUserIDsDoNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var nonExistentUserId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            var entity = new DPIACreateVM
            {
                Title = "Test DPIA",
                ExternalSystemId = Guid.NewGuid(),
                DueDate = DateTime.UtcNow.AddDays(30),
                Responsibilities = new List<DPIAResponsibilityCreateVM>
                {
                    new DPIAResponsibilityCreateVM
                    {
                        DueDate = DateTime.UtcNow.AddDays(20),
                        ResponsibilityId = Guid.NewGuid(),
                        UserIds = new List<Guid> { userId, nonExistentUserId },
                        Pic = userId
                    }
                }
            };

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));

            _externalSystemServiceMock.Setup(s => s.GetByIdAsync(entity.ExternalSystemId))
                .ReturnsAsync(new ExternalSystem { 
                    Name = "Test System",
                    Status = ExternalSystemStatus.Active 
                });

            var dpia = new DPIA
            {
                Title = entity.Title,
                Description = entity.Description,
                ExternalSystemId = entity.ExternalSystemId,
                Type = entity.Type,
                Events = new List<DPIAEvent>(),
                Documents = new List<DPIADocument>()
            };

            _mapperMock.Setup(m => m.Map<DPIA>(It.IsAny<DPIACreateVM>()))
                .Returns(dpia);

            _unitOfWorkMock.Setup(u => u.Users.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { 
                    new User { 
                        Id = userId,
                        UserName = "testuser",
                        FullName = "Test User",
                        Email = "test@example.com"
                    } 
                }); // Only one user exists

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _dpiaService.AddAsync(entity, principal));
        }

        [Fact]
        public async Task GetHistoryAsync_UserIsMember_ReturnsEvents()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA"
            };
            
            var events = new List<DPIAEvent>
            {
                new DPIAEvent { Id = Guid.NewGuid(), Event = "Test event" }
            };
            
            var eventVMs = new List<EventDetailVM>
            {
                new EventDetailVM { Id = events[0].Id, Text = "Test event" }
            };
            
            _unitOfWorkMock.Setup(u => u.DPIAs.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember> { new DPIAMember { Id = Guid.NewGuid(), DPIAId = dpiaId, UserId = userId } });
                
            _unitOfWorkMock.Setup(u => u.DPIAs.GetEventsAsync(dpiaId))
                .ReturnsAsync(events);
                
            _mapperMock.Setup(m => m.Map<List<EventDetailVM>>(events))
                .Returns(eventVMs);
                
            // Act
            var result = await _dpiaService.GetHistoryAsync(dpiaId, principal);
            
            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test event", result[0].Text);
        }

        [Fact]
        public async Task GetHistoryAsync_UserIsNotMemberNorAdmin_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA"
            };
            
            _unitOfWorkMock.Setup(u => u.DPIAs.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember>()); // Empty list - user is not a member
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _dpiaService.GetHistoryAsync(dpiaId, principal));
                
            Assert.Equal("User is not a member of this DPIA", exception.Message);
        }

        [Fact]
        public async Task GetHistoryAsync_DPIANotFound_ThrowsException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            _unitOfWorkMock.Setup(u => u.DPIAs.GetByIdAsync(dpiaId))
                .ReturnsAsync((DPIA)null);

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));

            // User is not in Admin group
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));

            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember>()); // Empty list - user is not a member
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.GetHistoryAsync(dpiaId, principal));
                
            Assert.Equal("DPIA not found", exception.Message);
        }

        [Fact]
        public async Task GetMembersAsync_UserIsMember_ReturnsMembers()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpiaMembers = new List<DPIAMember>
            {
                new DPIAMember 
                { 
                    Id = Guid.NewGuid(), 
                    DPIAId = dpiaId, 
                    UserId = userId,
                    User = new User
                    {
                        UserName = "testuser",
                        FullName = "Test User",
                        Email = "test@example.com"
                    }
                }
            };
            
            var memberVMs = new List<DPIAMemberVM>
            {
                new DPIAMemberVM 
                { 
                    Id = dpiaMembers[0].Id,
                    UserId = userId,
                    FullName = "Test User",
                    Email = "test@example.com"
                }
            };
            
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(dpiaMembers);
                
            _dpiaRepositoryMock.Setup(r => r.GetDPIAMembersAsync(dpiaId))
                .ReturnsAsync(dpiaMembers);
                
            _mapperMock.Setup(m => m.Map<List<DPIAMemberVM>>(dpiaMembers))
                .Returns(memberVMs);
                
            // Act
            var result = await _dpiaService.GetMembersAsync(dpiaId, principal);
            
            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(userId, result[0].UserId);
            Assert.Equal("Test User", result[0].FullName);
        }

        [Fact]
        public async Task GetMembersAsync_UserIsNotMemberNorAdmin_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember>()); // Empty list - user is not a member
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _dpiaService.GetMembersAsync(dpiaId, principal));
                
            Assert.Equal("User is not a member of this DPIA", exception.Message);
        }

        [Fact]
        public async Task GetMembersAsync_DPIANotFound_ThrowsException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true)); // User is DPO, so membership check passes
            
            _dpiaRepositoryMock.Setup(r => r.GetDPIAMembersAsync(dpiaId))
                .ThrowsAsync(new Exception("DPIA not found"));
                
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.GetMembersAsync(dpiaId, principal));
        }

        [Fact]
        public async Task AddCommentAsync_ValidUserIsMember_Success()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA"
            };
            
            var addCommentVM = new AddCommentVM
            {
                Content = "Test comment content",
                ReferenceId = dpiaId,
                Type = CommentType.DPIA
            };
            
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                ReferenceId = dpiaId,
                UserId = userId,
                Content = addCommentVM.Content,
                Type = CommentType.DPIA,
                CreatedAt = DateTime.UtcNow
            };
            
            _unitOfWorkMock.Setup(u => u.DPIAs.GetByIdAsync(comment.ReferenceId))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember> { new DPIAMember { Id = Guid.NewGuid(), DPIAId = dpiaId, UserId = userId } });
                
            _dpiaRepositoryMock.Setup(r => r.SaveCommentAsync(It.IsAny<Comment>()))
                .Returns(Task.CompletedTask);
            
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(comment.ReferenceId))
                .ReturnsAsync(dpia);

            _mapperMock.Setup(m => m.Map<Comment>(It.IsAny<AddCommentVM>()))
                .Returns(comment);
                
            // Act
            await _dpiaService.AddCommentAsync(addCommentVM, principal);
            
            // Assert
            _dpiaRepositoryMock.Verify(r => r.SaveCommentAsync(It.IsAny<Comment>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        
        [Fact]
        public async Task UpdateCommentAsync_ValidUserIsOwner_Success()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var commentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var comment = new Comment
            {
                Id = commentId,
                ReferenceId = dpiaId,
                UserId = userId,
                Content = "Original content",
                Type = CommentType.DPIA
            };
            
            var addCommentVM = new AddCommentVM
            {
                ReferenceId = dpiaId,
                Content = "Updated content",
                Type = CommentType.DPIA,
                UserId = userId
            };
            
            _unitOfWorkMock.Setup(u => u.Comments.GetByIdAsync(commentId))
                .ReturnsAsync(comment);
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember> { new DPIAMember { Id = Guid.NewGuid(), DPIAId = dpiaId, UserId = userId } });
            
            _unitOfWorkMock.Setup(u => u.Comments.Update(It.IsAny<Comment>()))
                .Verifiable();

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));


            _mapperMock.Setup(m => m.Map<Comment>(It.IsAny<AddCommentVM>()))
                .Returns(new Comment { 
                    Id = commentId,
                    ReferenceId = dpiaId,
                    UserId = userId,
                    Content = "Updated content",
                    Type = CommentType.DPIA
                });
                
            // Act
            await _dpiaService.UpdateCommentAsync(commentId, addCommentVM, principal);
            
            // Assert
            _unitOfWorkMock.Verify(u => u.Comments.Update(It.IsAny<Comment>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        
        [Fact]
        public async Task UpdateCommentAsync_CommentNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var addCommentVM = new AddCommentVM
            {
                ReferenceId = dpiaId,
                Content = "Updated content",
                Type = CommentType.DPIA,
                UserId = userId
            };
            
            _unitOfWorkMock.Setup(u => u.Comments.GetByIdAsync(commentId))
                .ReturnsAsync((Comment)null);

            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember> { new DPIAMember { Id = Guid.NewGuid(), DPIAId = dpiaId, UserId = userId } });
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));
                
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _dpiaService.UpdateCommentAsync(commentId, addCommentVM, principal));
        }
        
        [Fact]
        public async Task UpdateCommentAsync_UserNotOwner_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var commentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var comment = new Comment
            {
                Id = commentId,
                ReferenceId = dpiaId,
                UserId = differentUserId, // Different user is the owner
                Content = "Original content",
                Type = CommentType.DPIA
            };
            
            var addCommentVM = new AddCommentVM
            {
                ReferenceId = dpiaId,
                Content = "Updated content",
                Type = CommentType.DPIA,
                UserId = userId
            };
            
            _unitOfWorkMock.Setup(u => u.Comments.GetByIdAsync(commentId))
                .ReturnsAsync(comment);

            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember> { new DPIAMember { Id = Guid.NewGuid(), DPIAId = dpiaId, UserId = userId } });

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));

            _mapperMock.Setup(m => m.Map<Comment>(It.IsAny<AddCommentVM>()))
                .Returns(new Comment { 
                    Id = commentId,
                    ReferenceId = dpiaId,
                    UserId = userId,
                    Content = "Updated content",
                    Type = CommentType.DPIA
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _dpiaService.UpdateCommentAsync(commentId, addCommentVM, principal));
                
            Assert.Equal("User is not authorized to update this comment", exception.Message);
        }

        [Fact]
        public async Task AddMembersAsync_ValidInput_MembersAddedSuccessfully()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Draft
            };
            
            var members = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM
                {
                    UserId = Guid.NewGuid(),
                    Responsibilities = new List<Guid>()
                }
            };
            
            var dpiaMembers = new List<DPIAMember>
            {
                new DPIAMember
                {
                    UserId = members[0].UserId,
                    DPIAId = dpiaId,
                    Responsibilities = new List<MemberResponsibility>()
                }
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _mapperMock.Setup(m => m.Map<List<DPIAMember>>(members))
                .Returns(dpiaMembers);
                
            _dpiaRepositoryMock.Setup(r => r.GetDPIAMembersAsync(dpiaId))
                .ReturnsAsync(new List<DPIAMember>());
                
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(members[0].UserId))
                .ReturnsAsync(new User 
                { 
                    Id = members[0].UserId,
                    UserName = "testuser",
                    FullName = "Test User",
                    Email = "test@example.com"
                });
                
            _dpiaRepositoryMock.Setup(r => r.BulkAddMembersAsync(It.IsAny<List<DPIAMember>>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _dpiaService.AddMembersAsync(dpiaId, members);
            
            // Assert
            _dpiaRepositoryMock.Verify(r => r.BulkAddMembersAsync(It.IsAny<List<DPIAMember>>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        
        [Fact]
        public async Task AddMembersAsync_DPIANotFound_ThrowsException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            
            var members = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM
                {
                    UserId = Guid.NewGuid(),
                    Responsibilities = new List<Guid>()
                }
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync((DPIA)null);
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.AddMembersAsync(dpiaId, members));
                
            Assert.Equal("DPIA not found", exception.Message);
        }
        
        [Fact]
        public async Task AddMembersAsync_SomeUsersNotFound_ThrowsException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Draft
            };
            
            var members = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM
                {
                    UserId = userId,
                    Responsibilities = new List<Guid>()
                }
            };
            
            var dpiaMembers = new List<DPIAMember>
            {
                new DPIAMember
                {
                    UserId = userId,
                    DPIAId = dpiaId,
                    Responsibilities = new List<MemberResponsibility>()
                }
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _mapperMock.Setup(m => m.Map<List<DPIAMember>>(members))
                .Returns(dpiaMembers);
                
            _dpiaRepositoryMock.Setup(r => r.GetDPIAMembersAsync(dpiaId))
                .ReturnsAsync(new List<DPIAMember>());
                
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId))
                .ReturnsAsync((User)null); // User not found
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _dpiaService.AddMembersAsync(dpiaId, members));
                
            Assert.Contains($"User with ID {userId} not found", exception.Message);
        }
        
        [Fact]
        public async Task AddMembersAsync_AllMembersAlreadyExist_ThrowsException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Draft
            };
            
            var members = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM
                {
                    UserId = userId,
                    Responsibilities = new List<Guid>()
                }
            };
            
            var dpiaMembers = new List<DPIAMember>
            {
                new DPIAMember
                {
                    UserId = userId,
                    DPIAId = dpiaId,
                    Responsibilities = new List<MemberResponsibility>()
                }
            };
            
            var existingMembers = new List<DPIAMember>
            {
                new DPIAMember
                {
                    UserId = userId,
                    DPIAId = dpiaId
                }
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _mapperMock.Setup(m => m.Map<List<DPIAMember>>(members))
                .Returns(dpiaMembers);
                
            _dpiaRepositoryMock.Setup(r => r.GetDPIAMembersAsync(dpiaId))
                .ReturnsAsync(existingMembers);
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.AddMembersAsync(dpiaId, members));
                
            Assert.Contains("Members already exist", exception.Message);
        }

        [Fact]
        public async Task UpdateMembersAsync_ValidInput_MembersUpdatedSuccessfully()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var existingUserId = Guid.NewGuid();
            var newUserId = Guid.NewGuid();
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Draft,
                ExternalSystem = new ExternalSystem
                {
                    Name = "Test External System"
                }
            };
            
            var members = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM
                {
                    UserId = newUserId,
                    Responsibilities = new List<Guid>()
                }
            };
            
            var dpiaMembers = new List<DPIAMember>
            {
                new DPIAMember
                {
                    UserId = newUserId,
                    DPIAId = dpiaId,
                    Responsibilities = new List<MemberResponsibility>()
                }
            };
            
            var existingMembers = new List<DPIAMember>
            {
                new DPIAMember
                {
                    Id = Guid.NewGuid(),
                    UserId = existingUserId,
                    DPIAId = dpiaId
                }
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetDetailAsync(
                    dpiaId,
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>()
                ))
                .ReturnsAsync(dpia);
                
            _mapperMock.Setup(m => m.Map<List<DPIAMember>>(members))
                .Returns(dpiaMembers);
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(existingMembers);
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.BulkAddAsync(It.IsAny<List<DPIAMember>>()))
                .Returns(Task.CompletedTask);
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.BulkDeleteAsync(It.IsAny<List<DPIAMember>>()))
                .Returns(Task.CompletedTask);
                
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(newUserId))
                .ReturnsAsync(new User
                {
                    Id = newUserId,
                    UserName = "newuser",
                    FullName = "New User",
                    Email = "newuser@example.com"
                });
                
            // Act
            await _dpiaService.UpdateMembersAsync(dpiaId, members);
            
            // Assert
            _unitOfWorkMock.Verify(u => u.DPIAMembers.BulkAddAsync(It.IsAny<List<DPIAMember>>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.DPIAMembers.BulkDeleteAsync(It.IsAny<List<DPIAMember>>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
        }
        
        [Fact]
        public async Task UpdateMembersAsync_DPIANotFound_ThrowsException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            
            var members = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM
                {
                    UserId = Guid.NewGuid(),
                    Responsibilities = new List<Guid>()
                }
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetDetailAsync(
                    dpiaId,
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>()
                ))
                .ReturnsAsync((DPIA)null);
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.UpdateMembersAsync(dpiaId, members));
                
            Assert.Equal("DPIA not found", exception.Message);
        }
        
        [Fact]
        public async Task UpdateMembersAsync_SomeUsersNotFound_ThrowsException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Draft,
                ExternalSystem = new ExternalSystem
                {
                    Name = "Test External System"
                }
            };
            
            var members = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM
                {
                    UserId = userId,
                    Responsibilities = new List<Guid>()
                }
            };
            
            var dpiaMembers = new List<DPIAMember>
            {
                new DPIAMember
                {
                    UserId = userId,
                    DPIAId = dpiaId,
                    Responsibilities = new List<MemberResponsibility>()
                }
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetDetailAsync(
                    dpiaId,
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>()
                ))
                .ReturnsAsync(dpia);
                
            _mapperMock.Setup(m => m.Map<List<DPIAMember>>(members))
                .Returns(dpiaMembers);
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember>());
                
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId))
                .ReturnsAsync((User)null); // User not found

            _unitOfWorkMock.Setup(u => u.DPIAMembers.UpdateAsync(It.IsAny<List<DPIAMember>>()))
                .Throws(new Exception("Failed to update members"));
                
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _dpiaService.UpdateMembersAsync(dpiaId, members));
        }

        [Fact]
        public async Task UpdateMemberResponsibilitiesAsync_ValidInput_ResponsibilitiesUpdated()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var dpiaResponsibilityId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Draft,
                ExternalSystem = new ExternalSystem
                {
                    Name = "Test External System"
                },
                Responsibilities = new List<DPIAResponsibility>
                {
                    new DPIAResponsibility
                    {
                        Id = dpiaResponsibilityId,
                        DPIAId = dpiaId,
                        ResponsibilityId = responsibilityId
                    }
                }
            };
            
            var responsibilityVMs = new List<DPIAResponsibilityVM>
            {
                new DPIAResponsibilityVM
                {
                    ResponsibilityId = responsibilityId,
                    UserId = new List<Guid> { userId },
                    Pic = userId
                }
            };
            
            var existingDpiaResponsibilities = new List<DPIAResponsibility>
            {
                new DPIAResponsibility
                {
                    Id = dpiaResponsibilityId,
                    DPIAId = dpiaId,
                    ResponsibilityId = responsibilityId
                }
            };
            
            var dpiaMembers = new List<DPIAMember>
            {
                new DPIAMember
                {
                    Id = memberId,
                    UserId = userId,
                    DPIAId = dpiaId
                }
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetDetailAsync(
                    dpiaId,
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>()
                ))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, bool>>>()))
                .ReturnsAsync(existingDpiaResponsibilities);
                
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(dpiaMembers);
                
            _unitOfWorkMock.Setup(u => u.MemberResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MemberResponsibility, bool>>>()))
                .ReturnsAsync(new List<MemberResponsibility>());
                
            // Set up the BulkAddAsync method
            _unitOfWorkMock.Setup(u => u.MemberResponsibilities.BulkAddAsync(It.IsAny<List<MemberResponsibility>>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _dpiaService.UpdateMemberResponsibilitiesAsync(dpiaId, responsibilityVMs);
            
            // Assert
            _unitOfWorkMock.Verify(u => u.MemberResponsibilities.BulkAddAsync(It.IsAny<List<MemberResponsibility>>()), Times.AtLeastOnce);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
        }
        
        [Fact]
        public async Task UpdateMemberResponsibilitiesAsync_DPIANotFound_ThrowsException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            
            var responsibilityVMs = new List<DPIAResponsibilityVM>
            {
                new DPIAResponsibilityVM
                {
                    ResponsibilityId = responsibilityId,
                    UserId = new List<Guid> { Guid.NewGuid() }
                }
            };
            
            _dpiaRepositoryMock.Setup(r => r.GetDetailAsync(
                    dpiaId,
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>()
                ))
                .ReturnsAsync((DPIA)null);
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.UpdateMemberResponsibilitiesAsync(dpiaId, responsibilityVMs));
                
            Assert.Equal("DPIA not found", exception.Message);
        }
        
        [Fact]
        public async Task DeleteResponsibilityAsync_UserIsDPO_DeletesResponsibility()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Draft
            };
            
            var dpiaResponsibility = new DPIAResponsibility
            {
                Id = Guid.NewGuid(),
                DPIAId = dpiaId,
                ResponsibilityId = responsibilityId
            };
            
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));
                
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, bool>>>()))
                .ReturnsAsync(new List<DPIAResponsibility> { dpiaResponsibility });
                
            _unitOfWorkMock.Setup(u => u.MemberResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MemberResponsibility, bool>>>()))
                .ReturnsAsync(new List<MemberResponsibility>());
                
            _unitOfWorkMock.Setup(u => u.MemberResponsibilities.BulkDeleteAsync(It.IsAny<List<MemberResponsibility>>()))
                .Returns(Task.CompletedTask);
                
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.DeleteAsync(dpiaResponsibility.Id))
                .Returns(Task.CompletedTask);
                
            // Act
            await _dpiaService.DeleteResponsibilityAsync(dpiaId, responsibilityId, principal);
            
            // Assert
            _unitOfWorkMock.Verify(u => u.DPIAResponsibilities.DeleteAsync(dpiaResponsibility.Id), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
        }
        
        [Fact]
        public async Task DeleteResponsibilityAsync_UserIsAdmin_DeletesResponsibility()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Draft
            };
            
            var dpiaResponsibility = new DPIAResponsibility
            {
                Id = Guid.NewGuid(),
                DPIAId = dpiaId,
                ResponsibilityId = responsibilityId
            };
            
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Ok(true));
                
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, bool>>>()))
                .ReturnsAsync(new List<DPIAResponsibility> { dpiaResponsibility });
                
            _unitOfWorkMock.Setup(u => u.MemberResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MemberResponsibility, bool>>>()))
                .ReturnsAsync(new List<MemberResponsibility>());
                
            _unitOfWorkMock.Setup(u => u.MemberResponsibilities.BulkDeleteAsync(It.IsAny<List<MemberResponsibility>>()))
                .Returns(Task.CompletedTask);
                
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.DeleteAsync(dpiaResponsibility.Id))
                .Returns(Task.CompletedTask);
                
            // Act
            await _dpiaService.DeleteResponsibilityAsync(dpiaId, responsibilityId, principal);
            
            // Assert
            _unitOfWorkMock.Verify(u => u.DPIAResponsibilities.DeleteAsync(dpiaResponsibility.Id), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
        }
        
        [Fact]
        public async Task DeleteResponsibilityAsync_DPIANotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));
                
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync((DPIA)null);
                
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _dpiaService.DeleteResponsibilityAsync(dpiaId, responsibilityId, principal));
        }
        
        [Fact]
        public async Task DeleteResponsibilityAsync_ResponsibilityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Draft
            };
            
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));
                
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, bool>>>()))
                .ReturnsAsync(new List<DPIAResponsibility>()); // Empty list - responsibility not found
                
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _dpiaService.DeleteResponsibilityAsync(dpiaId, responsibilityId, principal));
        }
        
        [Fact]
        public async Task DeleteResponsibilityAsync_DPIANotInDraft_ThrowsInvalidOperationException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Status = DPIAStatus.Started // Not in Draft status
            };
            
            var dpiaResponsibility = new DPIAResponsibility
            {
                Id = Guid.NewGuid(),
                DPIAId = dpiaId,
                ResponsibilityId = responsibilityId
            };
            
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));
                
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, bool>>>()))
                .ReturnsAsync(new List<DPIAResponsibility> { dpiaResponsibility });
                
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _dpiaService.DeleteResponsibilityAsync(dpiaId, responsibilityId, principal));
        }

        [Fact]
        public async Task GetResponsibilityAsync_ValidInput_ReturnsDetail()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);

            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA",
                Documents = new List<DPIADocument>()
            };

            var dpiaResponsibilityId = Guid.NewGuid();
            var dpiaResponsibility = new DPIAResponsibility
            {
                Id = dpiaResponsibilityId,
                DPIAId = dpiaId,
                ResponsibilityId = responsibilityId,
                Responsibility = new Responsibility 
                { 
                    Id = responsibilityId,
                    Title = "Test Responsibility" 
                },
                DPIA = dpia,
                Status = ResponsibilityStatus.NotStarted
            };

            var expectedVM = new DPIAResponsibilityDetailVM
            {
                Id = dpiaResponsibilityId,
                ResponsibilityName = "Test Responsibility",
                Status = ResponsibilityStatus.NotStarted,
                Members = new List<MemberResponsibilityVM>(),
                Comments = new List<CommentVM>()
            };

            // Setup DPIAMembers.FindAsync for CheckDPIAMembershipAsync
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember> { new DPIAMember { Id = Guid.NewGuid(), DPIAId = dpiaId, UserId = userId } });

            // Setup Users.CheckUserInGroup for CheckDPIAMembershipAsync
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));

            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));

            // Setup DPIAResponsibilities.FindAsync
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, bool>>>()))
                .ReturnsAsync(new List<DPIAResponsibility> { dpiaResponsibility });

            // Setup DPIAResponsibilities.GetDetailAsync
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.GetDetailAsync(
                dpiaResponsibilityId,
                It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, object>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, object>>>()))
                .ReturnsAsync(dpiaResponsibility);

            // Setup Comments.FindAsync
            _unitOfWorkMock.Setup(u => u.Comments.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, bool>>>()))
                .ReturnsAsync(new List<Comment>());

            // Setup DPIAResponsibilities.GetMembersAsync
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.GetMembersAsync(dpiaResponsibilityId))
                .ReturnsAsync(new List<MemberResponsibility>());

            // Setup dpiaRepository.GetDetailAsync
            _dpiaRepositoryMock.Setup(r => r.GetDetailAsync(
                dpiaId,
                It.IsAny<System.Linq.Expressions.Expression<Func<DPIA, object>>>()))
                .ReturnsAsync(dpia);

            // Setup mapper
            _mapperMock.Setup(m => m.Map<DPIAResponsibilityDetailVM>(It.IsAny<DPIAResponsibility>()))
                .Returns(expectedVM);

            _mapperMock.Setup(m => m.Map<List<CommentVM>>(It.IsAny<List<Comment>>()))
                .Returns(new List<CommentVM>());

            _mapperMock.Setup(m => m.Map<List<MemberResponsibilityVM>>(It.IsAny<List<MemberResponsibility>>()))
                .Returns(new List<MemberResponsibilityVM>());

            // Act
            var result = await _dpiaService.GetResponsibilityAsync(dpiaId, responsibilityId, principal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dpiaResponsibilityId, result.Id);
            Assert.Equal("Test Responsibility", result.ResponsibilityName);
            Assert.Equal(ResponsibilityStatus.NotStarted, result.Status);
        }
        
        [Fact]
        public async Task GetResponsibilityAsync_ResponsibilityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA"
            };
            
            // Setup DPIAMembers.FindAsync for CheckDPIAMembershipAsync
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember> { new DPIAMember { Id = Guid.NewGuid(), DPIAId = dpiaId, UserId = userId } });

            // Setup Users.CheckUserInGroup for CheckDPIAMembershipAsync
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));
                
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));
                
            // Return empty list - responsibility not found
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, bool>>>()))
                .ReturnsAsync(new List<DPIAResponsibility>()); 
                
            _dpiaRepositoryMock.Setup(r => r.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _dpiaService.GetResponsibilityAsync(dpiaId, responsibilityId, principal));
        }
        
        [Fact]
        public async Task RestartResponsibilityAsync_ValidPIC_UpdatesStatus()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA"
            };
            
            var dpiaResponsibility = new DPIAResponsibility
            {
                Id = responsibilityId,
                DPIAId = dpiaId,
                Status = ResponsibilityStatus.Completed,
                MemberResponsibilities = new List<MemberResponsibility>
                {
                    new MemberResponsibility
                    {
                        Id = Guid.NewGuid(),
                        DPIAResponsibilityId = responsibilityId,
                        MemberId = memberId,
                        IsPic = true,
                        Member = new DPIAMember
                        {
                            Id = memberId,
                            UserId = userId,
                            DPIAId = dpiaId
                        }
                    }
                }
            };
            
            var dpiaMembers = new List<DPIAMember>
            {
                new DPIAMember
                {
                    Id = memberId,
                    UserId = userId,
                    DPIAId = dpiaId
                }
            };
            
            // Setup for finding the responsibility
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, bool>>>()))
                .ReturnsAsync(new List<DPIAResponsibility> { dpiaResponsibility });
            
            // Setup for DPIAs.GetByIdAsync (used in CheckResponsibilityPICAsync)
            _unitOfWorkMock.Setup(u => u.DPIAs.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            // Setup for DPIAResponsibilities.GetByIdAsync (used in CheckResponsibilityPICAsync)
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.GetByIdAsync(responsibilityId))
                .ReturnsAsync(dpiaResponsibility);
            
            // Setup for finding the DPIA members
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(dpiaMembers);
            
            // Setup for member responsibilities with IsPic=true (used in CheckResponsibilityPICAsync)
            _unitOfWorkMock.Setup(u => u.MemberResponsibilities.FindAsync(
                It.Is<System.Linq.Expressions.Expression<Func<MemberResponsibility, bool>>>(
                    expr => expr.ToString().Contains("IsPic"))))
                .ReturnsAsync(dpiaResponsibility.MemberResponsibilities);
            
            // Setup for finding all member responsibilities
            _unitOfWorkMock.Setup(u => u.MemberResponsibilities.FindAsync(
                It.Is<System.Linq.Expressions.Expression<Func<MemberResponsibility, bool>>>(
                    expr => !expr.ToString().Contains("IsPic"))))
                .ReturnsAsync(dpiaResponsibility.MemberResponsibilities);
            
            _messageBuilderMock.Setup(m => m.BuildDPIAResponsibilityStatusChangeEvent(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("Responsibility restarted");

            // User is in DPO group
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));
            
            // Act
            await _dpiaService.RestartResponsibilityAsync(dpiaId, responsibilityId, principal);
            
            // Assert
            Assert.Equal(ResponsibilityStatus.NotStarted, dpiaResponsibility.Status);
            // _unitOfWorkMock.Verify(u => u.DPIAResponsibilities.Update(dpiaResponsibility), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        
        [Fact]
        public async Task RestartResponsibilityAsync_ValidDPO_UpdatesStatus()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA"
            };
            
            var dpiaResponsibility = new DPIAResponsibility
            {
                Id = responsibilityId,
                DPIAId = dpiaId,
                Status = ResponsibilityStatus.Completed,
                MemberResponsibilities = new List<MemberResponsibility>()
            };
            
            // Setup for finding the responsibility
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, bool>>>()))
                .ReturnsAsync(new List<DPIAResponsibility> { dpiaResponsibility });
            
            // Setup for DPIAs.GetByIdAsync (used in CheckResponsibilityPICAsync)
            _unitOfWorkMock.Setup(u => u.DPIAs.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            // Setup for DPIAResponsibilities.GetByIdAsync (used in CheckResponsibilityPICAsync)
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.GetByIdAsync(responsibilityId))
                .ReturnsAsync(dpiaResponsibility);
                
            // Setup for member responsibilities (empty, not PIC)
            _unitOfWorkMock.Setup(u => u.MemberResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MemberResponsibility, bool>>>()))
                .ReturnsAsync(new List<MemberResponsibility>());
                
            // User is in DPO group
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));
                
            _messageBuilderMock.Setup(m => m.BuildDPIAResponsibilityStatusChangeEvent(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("Responsibility restarted");
                
            // Act
            await _dpiaService.RestartResponsibilityAsync(dpiaId, responsibilityId, principal);
            
            // Assert
            Assert.Equal(ResponsibilityStatus.NotStarted, dpiaResponsibility.Status);
            // _unitOfWorkMock.Verify(u => u.DPIAResponsibilities.Update(dpiaResponsibility), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        
        [Fact]
        public async Task RestartResponsibilityAsync_ValidAdmin_UpdatesStatus()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA"
            };
            
            var dpiaResponsibility = new DPIAResponsibility
            {
                Id = responsibilityId,
                DPIAId = dpiaId,
                Status = ResponsibilityStatus.Completed,
                MemberResponsibilities = new List<MemberResponsibility>()
            };
            
            // Setup for finding the responsibility
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, bool>>>()))
                .ReturnsAsync(new List<DPIAResponsibility> { dpiaResponsibility });
            
            // Setup for DPIAs.GetByIdAsync (used in CheckResponsibilityPICAsync)
            _unitOfWorkMock.Setup(u => u.DPIAs.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            // Setup for DPIAResponsibilities.GetByIdAsync (used in CheckResponsibilityPICAsync)
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.GetByIdAsync(responsibilityId))
                .ReturnsAsync(dpiaResponsibility);
                
            // Setup for member responsibilities (empty, not PIC)
            _unitOfWorkMock.Setup(u => u.MemberResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MemberResponsibility, bool>>>()))
                .ReturnsAsync(new List<MemberResponsibility>());
                
            // User is NOT in DPO group
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
            
            // User is in Admin group
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Ok(true));
                
            _messageBuilderMock.Setup(m => m.BuildDPIAResponsibilityStatusChangeEvent(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("Responsibility restarted");
                
            // Act
            await _dpiaService.RestartResponsibilityAsync(dpiaId, responsibilityId, principal);
            
            // Assert
            Assert.Equal(ResponsibilityStatus.NotStarted, dpiaResponsibility.Status);
            // _unitOfWorkMock.Verify(u => u.DPIAResponsibilities.Update(dpiaResponsibility), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        
        [Fact]
        public async Task RestartResponsibilityAsync_NotCompletedStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA"
            };
            
            var dpiaResponsibility = new DPIAResponsibility
            {
                Id = responsibilityId,
                DPIAId = dpiaId,
                Status = ResponsibilityStatus.InProgress, // Not completed
                MemberResponsibilities = new List<MemberResponsibility>()
            };
            
            // Setup for finding the responsibility
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, bool>>>()))
                .ReturnsAsync(new List<DPIAResponsibility> { dpiaResponsibility });
                
            // Setup for DPIAs.GetByIdAsync (used in CheckResponsibilityPICAsync)
            _unitOfWorkMock.Setup(u => u.DPIAs.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            // Setup for DPIAResponsibilities.GetByIdAsync (used in CheckResponsibilityPICAsync)
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.GetByIdAsync(responsibilityId))
                .ReturnsAsync(dpiaResponsibility);
                
            // Setup for member responsibilities (empty)
            _unitOfWorkMock.Setup(u => u.MemberResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MemberResponsibility, bool>>>()))
                .ReturnsAsync(new List<MemberResponsibility>());
                
            // User is in DPO group
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Ok(true));
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _dpiaService.RestartResponsibilityAsync(dpiaId, responsibilityId, principal));
                
            Assert.Contains("Responsibility must be completed", exception.Message);
        }
        
        [Fact]
        public async Task RestartResponsibilityAsync_UserNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            var responsibilityId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var principal = CreateClaimsPrincipal(userId);
            
            var dpia = new DPIA
            {
                Id = dpiaId,
                Title = "Test DPIA"
            };
            
            var dpiaResponsibility = new DPIAResponsibility
            {
                Id = responsibilityId,
                DPIAId = dpiaId,
                Status = ResponsibilityStatus.Completed,
                MemberResponsibilities = new List<MemberResponsibility>()
            };
            
            // Setup for finding the responsibility
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAResponsibility, bool>>>()))
                .ReturnsAsync(new List<DPIAResponsibility> { dpiaResponsibility });
            
            // Setup for DPIAs.GetByIdAsync (used in CheckResponsibilityPICAsync)
            _unitOfWorkMock.Setup(u => u.DPIAs.GetByIdAsync(dpiaId))
                .ReturnsAsync(dpia);
                
            // Setup for DPIAResponsibilities.GetByIdAsync (used in CheckResponsibilityPICAsync)
            _unitOfWorkMock.Setup(u => u.DPIAResponsibilities.GetByIdAsync(responsibilityId))
                .ReturnsAsync(dpiaResponsibility);
                
            // User is NOT in DPO group
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.DPO))
                .ReturnsAsync(Result.Fail<bool>("Not in DPO group"));
                
            // User is NOT in Admin group
            _unitOfWorkMock.Setup(u => u.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS))
                .ReturnsAsync(Result.Fail<bool>("Not in Admin group"));
                
            // Setup DPIAMembers.FindAsync - user is a member of the DPIA, but not a PIC
            _unitOfWorkMock.Setup(u => u.DPIAMembers.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DPIAMember, bool>>>()))
                .ReturnsAsync(new List<DPIAMember> { new DPIAMember { Id = Guid.NewGuid(), DPIAId = dpiaId, UserId = userId } });
                
            // Setup MemberResponsibilities.FindAsync - return empty list as user is not a PIC
            _unitOfWorkMock.Setup(u => u.MemberResponsibilities.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MemberResponsibility, bool>>>()))
                .ReturnsAsync(new List<MemberResponsibility>());
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _dpiaService.RestartResponsibilityAsync(dpiaId, responsibilityId, principal));
                
            Assert.Equal("Only the PIC, DPO, or admin can perform this action", exception.Message);
        }

        private ClaimsPrincipal CreateClaimsPrincipal(Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("name", "Test User")
            };
            var identity = new ClaimsIdentity(claims);
            return new ClaimsPrincipal(identity);
        }
    }
}
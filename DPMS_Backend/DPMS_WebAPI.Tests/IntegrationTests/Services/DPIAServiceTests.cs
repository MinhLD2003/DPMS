using DPMS_WebAPI.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.DPIA;
using DPMS_WebAPI.ViewModels.Comment;
using DPMS_WebAPI.Enums;
using System.Security.Claims;
using DPMS_WebAPI.Constants;

namespace DPMS_WebAPI.Tests.IntegrationTests.Services
{
    public class DPIAServiceTests : ServiceTestEnvironment
    {
        private readonly DPIAService _dpiaService;
        public DPIAServiceTests()
        {
            _dpiaService = new DPIAService(
                _unitOfWork,
                _mapper,
                _messageBuilder,
                _dpiaRepository,
                _fileRepository,
                _mRes,
                _dpiaMember,
                _mediatorMock.Object,
                _externalSystemService
            );
        }

        [Fact]
        public async Task AddAsync_ShouldReturnDPIA_WhenDPIAIsCreated()
        {
            // Arrange
            var externalSystem = await CreateExternalSystem();
            await _unitOfWork.SaveChangesAsync();

            var dpiaVM = new DPIACreateVM
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = externalSystem.Id,
            };

            await _unitOfWork.SaveChangesAsync();

            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _dpiaService.AddAsync(dpiaVM, principal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dpiaVM.Title, result.Title);
            Assert.Equal(dpiaVM.Description, result.Description);
            Assert.Equal(dpiaVM.Type, result.Type);
            Assert.Equal(dpiaVM.ExternalSystemId, result.ExternalSystemId);
        }
        
        [Fact]
        public async Task AddAsync_ShouldAddResponsibilities_WhenResponsibilitiesAreCreated()
        {
            // Arrange
            var externalSystem = await CreateExternalSystem();
            await _unitOfWork.SaveChangesAsync();

            var member = await CreateUser();

            var  dpiaVM = new DPIACreateVM
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = externalSystem.Id,
                Responsibilities = new List<DPIAResponsibilityCreateVM>
                {
                    new DPIAResponsibilityCreateVM
                    {
                        DueDate = DateTime.Now.AddDays(1),
                        UserIds = new List<Guid> { member.Id },
                        Pic = member.Id,
                    },
                },
            };

            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _dpiaService.AddAsync(dpiaVM, principal);

            var responsibilities = await _unitOfWork.DPIAResponsibilities.FindAsync(r => r.DPIAId == result.Id);
            var members = await _unitOfWork.DPIAMembers.FindAsync(m => m.DPIAId == result.Id);

            // Assert
            Assert.NotNull(responsibilities);
            Assert.Equal(dpiaVM.Responsibilities.Count, responsibilities.Count());
            Assert.Equal(dpiaVM.Responsibilities.Count, members.Count());
        }

        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            // Arrange
            var externalSystem = await CreateExternalSystem();
            await _unitOfWork.SaveChangesAsync();

            var invalidUserId = Guid.NewGuid();

            var dpiaVM = new DPIACreateVM
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = externalSystem.Id,
                Responsibilities = new List<DPIAResponsibilityCreateVM>
                {
                    new DPIAResponsibilityCreateVM
                    {
                        UserIds = new List<Guid> { invalidUserId },
                        Pic = invalidUserId,
                    },
                
                },
            };

            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _dpiaService.AddAsync(dpiaVM, principal));

            // Assert
            Assert.Equal("User not found", exception.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenUserIsNotInDPOGroup()
        {
            // Arrange
            var externalSystem = await CreateExternalSystem();
            await _unitOfWork.SaveChangesAsync();

            var user = await CreateUser();

            var dpiaVM = new DPIACreateVM
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = externalSystem.Id,
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _dpiaService.AddAsync(dpiaVM, principal));

            // Assert
            Assert.Equal("User is not in DPO group", exception.Message);
            
        }

        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenExternalSystemIsNotFound()
        {
            // Arrange
            var externalSystem = await CreateExternalSystem();
            externalSystem.Status = ExternalSystemStatus.Inactive;
            await _unitOfWork.SaveChangesAsync();

            var dpiaVM = new DPIACreateVM
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = Guid.NewGuid(),
            };

            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await _dpiaService.AddAsync(dpiaVM, principal));

            // Assert
            Assert.Equal("System not found", exception.Message);
        }

        [Theory]
        [InlineData(ExternalSystemStatus.Inactive)]
        [InlineData(ExternalSystemStatus.WaitingForFIC)]
        public async Task AddAsync_ShouldThrowException_WhenExternalSystemIsNotActive(ExternalSystemStatus systemStatus)
        {
            // Arrange
            var externalSystem = await CreateExternalSystem();
            externalSystem.Status = systemStatus;
            await _unitOfWork.SaveChangesAsync();
            
            var dpiaVM = new DPIACreateVM
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = externalSystem.Id,
            };

            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };

            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await _dpiaService.AddAsync(dpiaVM, principal));

            // Assert
            Assert.Equal($"Cannot create DPIA when system is in {systemStatus}", exception.Message);
        }
        
        [Theory]
        [InlineData(0, 3, true)]
        [InlineData(1, 3, true)]
        [InlineData(2, 3, true)]
        [InlineData(null, 3, false)]
        public async Task AddAsync_ShouldThrowException_WhenResponsibilityPicIsNotInUserIds(int? picIndex, int userCount, bool isValid)
        {
            // Arrange
            var externalSystem = await CreateExternalSystem();
            await _unitOfWork.SaveChangesAsync();

            var userList = new List<User>();
            for (int i = 0; i < userCount; i++)
            {
                var u = await CreateUser();
                userList.Add(u);
            }

            var dpiaVM = new DPIACreateVM
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = externalSystem.Id,
                Responsibilities = new List<DPIAResponsibilityCreateVM>
                {
                    new DPIAResponsibilityCreateVM
                    {
                        Pic = picIndex.HasValue ? userList[picIndex.Value].Id : null,
                        UserIds = userList.Select(u => u.Id).ToList(),
                    },
                },
            };

            var user = await CreateUserWithGroup(PermissionGroup.DPO);  

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            if (isValid)
            {
                var result = await _dpiaService.AddAsync(dpiaVM, principal);
                Assert.NotNull(result);
            }
            else
            {
                var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _dpiaService.AddAsync(dpiaVM, principal));
                Assert.Equal("Person in charge must be included in the user list for the responsibility.", exception.Message);
            }

        }

        [Fact]
        public async Task AddAsync_ShouldSucceed_WhenExternalSystemIsActive()
        {
            // Arrange
            var externalSystem = await CreateExternalSystem();
            externalSystem.Status = ExternalSystemStatus.Active;
            await _unitOfWork.SaveChangesAsync();

            var dpiaVM = new DPIACreateVM
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = externalSystem.Id,
            };

            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _dpiaService.AddAsync(dpiaVM, principal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dpiaVM.Title, result.Title);
            Assert.Equal(dpiaVM.Description, result.Description);
            Assert.Equal(dpiaVM.Type, result.Type);
            Assert.Equal(dpiaVM.ExternalSystemId, result.ExternalSystemId);
        }
        
        [Theory]
        [InlineData(10, 5, null)] // Valid case: Both dates in future, responsibility before DPIA
        [InlineData(-1, 5, "Due date cannot be in the past.")] // Invalid case: DPIA date in past
        [InlineData(10, -1, "Responsibility due date cannot be in the past.")] // Invalid case: Responsibility date in past
        [InlineData(10, 15, "Responsibility due date cannot be later than the DPIA due date.")] // Invalid case: Responsibility date after DPIA date
        public async Task AddAsync_Test_DueDate_And_ResponsibilityDueDate(int dpiaDaysFromNow, int responsibilityDaysFromNow, string expectedErrorMessage)
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dueDate = now.AddDays(dpiaDaysFromNow);
            var responsibilityDueDate = now.AddDays(responsibilityDaysFromNow);

            var externalSystem = await CreateExternalSystem();
            await _unitOfWork.SaveChangesAsync();

            var member = await CreateUserWithGroup(PermissionGroup.Auditor);

            var dpiaVM = new DPIACreateVM
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = externalSystem.Id,
                DueDate = dueDate,
                Responsibilities = new List<DPIAResponsibilityCreateVM>
                {
                    new DPIAResponsibilityCreateVM
                    {
                        DueDate = responsibilityDueDate,
                        UserIds = new List<Guid> { member.Id },
                        Pic = member.Id,
                    },
                },
            };

            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, PermissionGroup.DPO)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act & Assert
            if (expectedErrorMessage == null)
            {
                var result = await _dpiaService.AddAsync(dpiaVM, principal);
                Assert.NotNull(result);
                Assert.Equal(dueDate, result.DueDate);
                var responsibilities = await _unitOfWork.DPIAResponsibilities.FindAsync(r => r.DPIAId == result.Id);
                Assert.NotNull(responsibilities);
                Assert.Equal(responsibilityDueDate, responsibilities.First().DueDate);
            }
            else
            {
                var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _dpiaService.AddAsync(dpiaVM, principal));
                Assert.Equal(expectedErrorMessage, exception.Message);
            }
        }

        [Fact]
        public async Task GetHistoryAsync_ShouldReturnHistory_WhenDPIAExists()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);
            await _unitOfWork.SaveChangesAsync();

            var user = await CreateUserWithGroup(PermissionGroup.DPO);
        
            var event1 = new DPIAEvent
            {
                DPIAId = dpia.Id,
                Event = "DPIA was created",
                EventType = DPIAEventType.Initiated,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.DPIAs.SaveEventsAsync(event1);
            await _unitOfWork.SaveChangesAsync();

            var event2 = new DPIAEvent
            {
                DPIAId = dpia.Id,
                Event = "DPIA was updated",
                EventType = DPIAEventType.Updated,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow.AddSeconds(1)
            };

            await _unitOfWork.DPIAs.SaveEventsAsync(event2);
            await _unitOfWork.SaveChangesAsync();

            // Set up claims principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _dpiaService.GetHistoryAsync(dpia.Id, principal);
            result.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));

            // Assert
            Assert.NotNull(result);
            
            Assert.Equal(2, result.Count);
            Assert.Equal(event1.Event, result[0].Text);
            Assert.Equal(event1.EventType, result[0].Type);
            Assert.Equal(event2.Event, result[1].Text);
            Assert.Equal(event2.EventType, result[1].Type);
        }

        [Fact]
        public async Task GetHistoryAsync_ShouldThrowException_WhenDPIADoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            
            // Create a user for the principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            
            // Set up claims principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _dpiaService.GetHistoryAsync(nonExistentId, principal));
        }

        [Fact]
        public async Task GetResponsibilityAsync_ShouldReturnResponsibility_WhenResponsibilityExists()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var member = await CreateDPIAMember(dpia);

            var dpiaResponsibility = await CreateDPIAResponsibility(dpia, responsibility);

            var memberResponsibility = await CreateMemberResponsibility(dpiaResponsibility, member);

            await _unitOfWork.SaveChangesAsync();
            
            // Set up claims principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, member.UserId.ToString()),
                new Claim(ClaimTypes.Email, member.User.Email),
                new Claim(ClaimTypes.Name, member.User.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _dpiaService.GetResponsibilityAsync(dpia.Id, responsibility.Id, principal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responsibility.Id, result.ResponsibilityId);
            Assert.Equal(responsibility.Title, result.ResponsibilityName);
            Assert.Equal(dpiaResponsibility.DueDate, result.DueDate);
            Assert.Equal(dpiaResponsibility.Status, result.Status);
            Assert.Equal(member.UserId, result.Members.First().UserId);
            Assert.Equal(member.User!.FullName, result.Members.First().FullName);
            Assert.Equal(member.User.Email, result.Members.First().Email);
            Assert.Equal(memberResponsibility.CompletionStatus, result.Members.First().CompletionStatus);
            Assert.Equal(memberResponsibility.IsPic, result.Members.First().IsPic);

        }  

        [Fact]
        public async Task GetResponsibilityAsync_ShouldThrowException_WhenResponsibilityDoesNotExist()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);
            
            // Create user and set up principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            await _unitOfWork.SaveChangesAsync();

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => 
                await _dpiaService.GetResponsibilityAsync(dpia.Id, Guid.NewGuid(), principal));

            // Assert
            Assert.Equal("Responsibility not found", exception.Message);
        }

        [Theory]
        [InlineData(CompletionStatus.NotStarted)]
        [InlineData(CompletionStatus.InProgress)]
        [InlineData(CompletionStatus.Completed)]
        public async Task UpdateMemberResponsibilityStatusAsync_ShouldUpdateMemberResponsibilityStatus_WhenMemberResponsibilityExists(CompletionStatus completionStatus)
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var member = await CreateDPIAMember(dpia);

            var dpiaResponsibility = await CreateDPIAResponsibility(dpia, responsibility);

            var memberResponsibility = await CreateMemberResponsibility(dpiaResponsibility, member);

            await _unitOfWork.SaveChangesAsync();   
            
            // Set up claims principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, member.UserId.ToString()),
                new Claim(ClaimTypes.Email, member.User.Email),
                new Claim(ClaimTypes.Name, member.User.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            await _dpiaService.UpdateMemberResponsibilityStatusAsync(dpia.Id, new MemberTaskStatus
            {
                MemberResponsibilityId = memberResponsibility.Id,
                CompletionStatus = completionStatus
            }, principal);

            // Assert
            Assert.Equal(completionStatus, memberResponsibility.CompletionStatus);    
        }

        [Fact]
        public async Task UpdateMemberResponsibilityStatusAsync_ShouldThrowException_WhenMemberResponsibilityDoesNotExist()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var member = await CreateDPIAMember(dpia);

            var dpiaResponsibility = await CreateDPIAResponsibility(dpia, responsibility);

            await _unitOfWork.SaveChangesAsync();
            
            // Set up claims principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, member.UserId.ToString()),
                new Claim(ClaimTypes.Email, member.User.Email),
                new Claim(ClaimTypes.Name, member.User.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => 
                await _dpiaService.UpdateMemberResponsibilityStatusAsync(dpia.Id, new MemberTaskStatus
                {
                    MemberResponsibilityId = Guid.NewGuid(),
                    CompletionStatus = CompletionStatus.NotStarted
                }, principal));

            // Assert
            Assert.Equal("Member responsibility not found", exception.Message);
        }

        [Fact]
        public async Task UpdateMemberResponsibilityStatusAsync_ShouldThrowException_WhenDPIADoesNotExist()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var member = await CreateDPIAMember(dpia);

            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            var dpiaResponsibility = await CreateDPIAResponsibility(dpia, responsibility);

            var memberResponsibility = await CreateMemberResponsibility(dpiaResponsibility, member);

            await _unitOfWork.SaveChangesAsync();
            
            // Set up claims principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => 
                await _dpiaService.UpdateMemberResponsibilityStatusAsync(Guid.NewGuid(), new MemberTaskStatus
                {
                    MemberResponsibilityId = memberResponsibility.Id,
                    CompletionStatus = CompletionStatus.NotStarted
                }, principal));  

            // Assert
            Assert.Equal("DPIA not found", exception.Message);
        }
            
        [Fact]
        public async Task UpdateMemberResponsibilityStatusAsync_ShouldThrowException_WhenResponsibilityDoesNotExist()
        {
            // Arrange
            var dpia_1 = await CreateDPIA(DPIAType.NewOrUpdatedSystem);
            var dpia_2 = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var member = await CreateDPIAMember(dpia_1);

            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            var dpiaResponsibility = await CreateDPIAResponsibility(dpia_1, responsibility);

            var memberResponsibility = await CreateMemberResponsibility(dpiaResponsibility, member);

            await _unitOfWork.SaveChangesAsync();
            
            // Set up claims principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => 
                await _dpiaService.UpdateMemberResponsibilityStatusAsync(dpia_2.Id, new MemberTaskStatus
                {
                    MemberResponsibilityId = memberResponsibility.Id,
                    CompletionStatus = CompletionStatus.NotStarted
                }, principal));

            // Assert
            Assert.Equal("Responsibility not found for this DPIA", exception.Message);
        }

        [Fact]
        public async Task StartDPIAAsync_ShouldThrowException_WhenDPIAHasNoResponsibilities()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            await _unitOfWork.SaveChangesAsync();

            var claims = new List<Claim>    
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await _dpiaService.StartDPIAAsync(dpia.Id, principal));

            // Assert
            Assert.Equal("DPIA has no responsibilities", exception.Message);
        }
        
        [Fact]
        public async Task StartDPIAAsync_ShouldThrowException_WhenDPIAIsNotDraft()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            dpia.Status = DPIAStatus.Approved;

            await _unitOfWork.SaveChangesAsync();

            var claims = new List<Claim>    
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);  

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await _dpiaService.StartDPIAAsync(dpia.Id, principal));

            // Assert
            Assert.Equal("DPIA can only be started from Draft status", exception.Message);
        }

        [Fact]
        public async Task StartDPIAAsync_ShouldThrowException_WhenDPIAIsNotReady()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var dpiaResponsibility = await CreateDPIAResponsibility(dpia, responsibility);

            dpiaResponsibility.Status = ResponsibilityStatus.NotStarted;

            await _unitOfWork.SaveChangesAsync();

            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            var claims = new List<Claim>    
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await _dpiaService.StartDPIAAsync(dpia.Id, principal));

            // Assert
            Assert.Equal("All responsibilities must be ready before starting DPIA", exception.Message);
        }

        [Fact]
        public async Task StartDPIAAsync_ShouldThrowException_WhenUserIsNotInDPOGroup()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var user = await CreateUser();

            await _unitOfWork.SaveChangesAsync();

            var claims = new List<Claim>        
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act  
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _dpiaService.StartDPIAAsync(dpia.Id, principal));

            // Assert
            Assert.Equal("Only DPO or admin can perform this action", exception.Message);
        }

        [Fact]
        public async Task StartDPIAAsync_ShouldThrowException_WhenDPIAIsNotFound()
        {
            // Arrange
            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            await _unitOfWork.SaveChangesAsync();

            var claims = new List<Claim>    
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await _dpiaService.StartDPIAAsync(Guid.NewGuid(), principal));

            // Assert
            Assert.Equal("DPIA not found", exception.Message);
        }

        [Fact]
        public async Task StartDPIAAsync_ShouldUpdateResponsibilitiesToInProgress()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var dpiaResponsibility = await CreateDPIAResponsibility(dpia, responsibility);
            dpiaResponsibility.Status = ResponsibilityStatus.Ready;
            
            await _unitOfWork.SaveChangesAsync();

            var user = await CreateUserWithGroup(PermissionGroup.DPO);  

            var claims = new List<Claim>    
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            await _dpiaService.StartDPIAAsync(dpia.Id, principal);  

            var updatedDpiaResponsibility = await _unitOfWork.DPIAResponsibilities.GetByIdAsync(dpiaResponsibility.Id);
        
            // Assert
            Assert.Equal(ResponsibilityStatus.InProgress, updatedDpiaResponsibility.Status);
        }

        [Fact]
        public async Task DeleteResponsibilityAsync_ShouldDeleteResponsibility()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var dpiaResponsibility = await CreateDPIAResponsibility(dpia, responsibility);

            var member = await CreateDPIAMember(dpia);

            var memberResponsibility = await CreateMemberResponsibility(dpiaResponsibility, member);

            await _unitOfWork.SaveChangesAsync();

            // Set up claims principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            await _dpiaService.DeleteResponsibilityAsync(dpia.Id, responsibility.Id, principal);

            // Assert
            Assert.Equal(0, (await _unitOfWork.DPIAResponsibilities.GetAllAsync()).Count());
        }

        [Fact]
        public async Task DeleteResponsibilityAsync_ShouldThrowException_WhenResponsibilityDoesNotExist()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);
            
            // Set up claims principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            await _unitOfWork.SaveChangesAsync();

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => 
                await _dpiaService.DeleteResponsibilityAsync(dpia.Id, Guid.NewGuid(), principal));

            // Assert
            Assert.Equal("Responsibility not found", exception.Message);
        }

        [Fact]
        public async Task DeleteResponsibilityAsync_ShouldThrowException_WhenDPIADoesNotExist()
        {
            // Arrange
            var responsibility = await CreateResponsibility();
            
            // Set up claims principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            await _unitOfWork.SaveChangesAsync();

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => 
                await _dpiaService.DeleteResponsibilityAsync(Guid.NewGuid(), responsibility.Id, principal));

            // Assert
            Assert.Equal("DPIA not found", exception.Message);
        }

        [Fact]
        public async Task DeleteResponsibilityAsync_ShouldThrowException_WhenDPIAIsNotDraft()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var dpiaResponsibility = await CreateDPIAResponsibility(dpia, responsibility);

            dpia.Status = DPIAStatus.Started;

            await _unitOfWork.SaveChangesAsync();
            
            // Set up claims principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _dpiaService.DeleteResponsibilityAsync(dpia.Id, responsibility.Id, principal));

            // Assert
            Assert.Equal("DPIA is not in Draft status", exception.Message);  
            
        }  
        
        [Fact]
        public async Task DeleteDocumentAsync_ShouldDeleteDocument()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var document = await CreateDocument(dpia, responsibility);

            await _unitOfWork.SaveChangesAsync();
            
            // Set up claims principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            await _dpiaService.DeleteDocumentAsync(dpia.Id, document.Id, principal);

            // Assert
            Assert.Equal(0, (await _unitOfWork.DPIADocuments.GetAllAsync()).Count());
        }

        [Fact]
        public async Task DeleteDocumentAsync_ShouldThrowException_WhenDocumentDoesNotExist()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);
            
            // Set up claims principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            await _unitOfWork.SaveChangesAsync();

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => 
                await _dpiaService.DeleteDocumentAsync(dpia.Id, Guid.NewGuid(), principal));

            // Assert
            Assert.Equal("Document not found", exception.Message);
        }

        [Fact]
        public async Task DeleteDocumentAsync_ShouldThrowException_WhenFileDoesNotExist()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var document = await CreateDocument(dpia, responsibility);
            // Set the file url to a non-existent file
            document.FileUrl = "https://mock-storage.com/test_not_found.pdf";

            await _unitOfWork.SaveChangesAsync();   
            
            // Set up claims principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => 
                await _dpiaService.DeleteDocumentAsync(dpia.Id, document.Id, principal));

            // Assert
            Assert.Equal("File not found in storage", exception.Message);

        }

        [Fact]
        public async Task DPIADetailVM_ShouldReturnDPIADetailVM()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);
            
            var responsibility = await CreateResponsibility();

            var document = await CreateDocument(dpia, responsibility);

            var member = await CreateDPIAMember(dpia);

            var dpiaResponsibility = await CreateDPIAResponsibility(dpia, responsibility);

            var memberResponsibility = await CreateMemberResponsibility(dpiaResponsibility, member);

            await _unitOfWork.SaveChangesAsync();

            // Act
            var result = await _dpiaService.GetDPIADetailByMemberId(dpia.Id, member.Id);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(dpia.Id, result.Id);
            Assert.Equal(dpia.Title, result.Title);
            Assert.Equal(dpia.Description, result.Description);
            Assert.Equal(dpia.Status, result.Status);
            Assert.Equal(dpia.Type, result.Type);
            Assert.Equal(dpia.CreatedAt, result.CreatedAt);
            Assert.Equal(dpia.LastModifiedAt, result.LastModifiedAt);

            Assert.Equal(responsibility.Id, result.Responsibilities.First().ResponsibilityId);
            Assert.Equal(responsibility.Title, result.Responsibilities.First().Title);
            Assert.Equal(dpiaResponsibility.DueDate, result.Responsibilities.First().DueDate);
            Assert.Equal(dpiaResponsibility.Status, result.Responsibilities.First().Status);

            Assert.Equal(member.Id, result.Members.First().Id);
            Assert.Equal(member.User!.FullName, result.Members.First().FullName);
            Assert.Equal(member.User.Email, result.Members.First().Email);
            Assert.Equal(memberResponsibility.CreatedAt, result.Members.First().JoinedAt);

            Assert.Equal(document.Id, result.Documents.First().Id);
            Assert.Equal(document.Title, result.Documents.First().Title);
            Assert.Equal(document.FileUrl, result.Documents.First().FileUrl);
            Assert.Equal(document.FileFormat, result.Documents.First().FileFormat);
            Assert.Equal(document.CreatedAt, result.Documents.First().CreatedAt);
            Assert.Equal(document.ResponsibleId, result.Documents.First().ResponsibilityId);

        }

        [Fact]
        public async Task RestartResponsibilityAsync_ShouldRestartResponsibility()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var dpiaResponsibility = await CreateDPIAResponsibility(dpia, responsibility);

            var member = await CreateDPIAMember(dpia);

            var memberResponsibility = await CreateMemberResponsibility(dpiaResponsibility, member);    

            dpia.Status = DPIAStatus.Started;
            memberResponsibility.CompletionStatus = CompletionStatus.Completed;
            dpiaResponsibility.Status = ResponsibilityStatus.Completed;

            await _unitOfWork.SaveChangesAsync();
            
            // Set up claims principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            await _dpiaService.RestartResponsibilityAsync(dpia.Id, responsibility.Id, principal);  

            var updatedMemberResponsibility = await _unitOfWork.MemberResponsibilities.GetByIdAsync(memberResponsibility.Id);
            var updatedDpiaResponsibility = await _unitOfWork.DPIAResponsibilities.GetByIdAsync(dpiaResponsibility.Id);

            // Assert
            Assert.Equal(CompletionStatus.InProgress, updatedMemberResponsibility.CompletionStatus);
            Assert.Equal(ResponsibilityStatus.InProgress, updatedDpiaResponsibility.Status);
            
        }

        [Fact]
        public async Task RestartResponsibilityAsync_ShouldThrowException_WhenResponsibilityDoesNotExist()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);
            
            // Set up claims principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            await _unitOfWork.SaveChangesAsync();

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => 
                await _dpiaService.RestartResponsibilityAsync(dpia.Id, Guid.NewGuid(), principal));

            // Assert
            Assert.Equal("DPIA responsibility not found", exception.Message);
        }

        [Fact]
        public async Task RestartResponsibilityAsync_ShouldThrowException_WhenDPIADoesNotExist()
        {
            // Arrange
            var responsibility = await CreateResponsibility();
            
            // Set up claims principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            await _unitOfWork.SaveChangesAsync();

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => 
                await _dpiaService.RestartResponsibilityAsync(Guid.NewGuid(), responsibility.Id, principal));

            // Assert
            Assert.Equal("DPIA responsibility not found", exception.Message);
        }

        [Fact]
        public async Task RestartResponsibilityAsync_ShouldThrowException_WhenResponsibilityIsNotCompleted()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var responsibility = await CreateResponsibility();

            var dpiaResponsibility = await CreateDPIAResponsibility(dpia, responsibility);
            
            // Set up claims principal
            var user = await CreateUserWithGroup(PermissionGroup.DPO);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            await _unitOfWork.SaveChangesAsync();

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _dpiaService.RestartResponsibilityAsync(dpia.Id, responsibility.Id, principal));

            // Assert
            Assert.Equal("Responsibility must be completed before restarting", exception.Message);
        }

        [Fact]
        public async Task DeleteDPIAAsync_ShouldThrowException_WhenDPIAIsNotFound()
        {
            // Arrange
            var dpiaId = Guid.NewGuid();
            
            var user = await CreateUserWithGroup(PermissionGroup.DPO);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _dpiaService.DeleteAsync(dpiaId, principal));
                
            Assert.Equal("DPIA not found", exception.Message);
        }

        [Fact]
        public async Task AddCommentAsync_ShouldAddComment_WhenUserIsMember()
        {
            // Arrange
            var externalSystem = await CreateExternalSystem();
            await _unitOfWork.SaveChangesAsync();

            // Create DPO user
            var dpoUser = await CreateUserWithGroup(PermissionGroup.DPO);
            
            // Create regular member user
            var memberUser = await CreateUser();

            // Create DPIA
            var dpia = new DPIA
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = externalSystem.Id,
                Status = DPIAStatus.Draft
            };
            await _unitOfWork.DPIAs.AddAsync(dpia);
            await _unitOfWork.SaveChangesAsync();

            // Add user as member
            var member = new DPIAMember
            {
                DPIAId = dpia.Id,
                UserId = memberUser.Id
            };
            await _unitOfWork.DPIAMembers.AddAsync(member);
            await _unitOfWork.SaveChangesAsync();

            // Create comment view model
            var commentVM = new AddCommentVM
            {
                ReferenceId = dpia.Id,
                Content = "This is a test comment",
                Type = CommentType.DPIA,
                UserId = memberUser.Id
            };

            // Set up claims for member user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, memberUser.Id.ToString()),
                new Claim(ClaimTypes.Email, memberUser.Email),
                new Claim(ClaimTypes.Name, memberUser.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            await _dpiaService.AddCommentAsync(commentVM, principal);

            // Assert
            var comments = await _unitOfWork.Comments.FindAsync(c => c.ReferenceId == dpia.Id && c.Type == CommentType.DPIA);
            Assert.NotNull(comments);
            Assert.Single(comments);
            Assert.Equal("This is a test comment", comments.First().Content);
            Assert.Equal(memberUser.Id, comments.First().UserId);
        }

        [Fact]
        public async Task AddCommentAsync_ShouldThrowException_WhenUserIsNotMember()
        {
            // Arrange
            var externalSystem = await CreateExternalSystem();
            await _unitOfWork.SaveChangesAsync();

            // Create DPO user
            var dpoUser = await CreateUserWithGroup(PermissionGroup.DPO);
            
            // Create non-member user
            var nonMemberUser = await CreateUser();

            // Create DPIA
            var dpia = new DPIA
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = externalSystem.Id,
                Status = DPIAStatus.Draft
            };
            await _unitOfWork.DPIAs.AddAsync(dpia);
            await _unitOfWork.SaveChangesAsync();

            // Create comment view model
            var commentVM = new AddCommentVM
            {
                ReferenceId = dpia.Id,
                Content = "This is a test comment",
                Type = CommentType.DPIA,
                UserId = nonMemberUser.Id
            };

            // Set up claims for non-member user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nonMemberUser.Id.ToString()),
                new Claim(ClaimTypes.Email, nonMemberUser.Email),
                new Claim(ClaimTypes.Name, nonMemberUser.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _dpiaService.AddCommentAsync(commentVM, principal));
                
            Assert.Equal("User is not a member of this DPIA", exception.Message);
        }

        [Fact]
        public async Task AddCommentAsync_ShouldAddComment_WhenUserIsDPO()
        {
            // Arrange
            var externalSystem = await CreateExternalSystem();
            await _unitOfWork.SaveChangesAsync();

            // Create DPO user
            var dpoUser = await CreateUserWithGroup(PermissionGroup.DPO);

            // Create DPIA
            var dpia = new DPIA
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = externalSystem.Id,
                Status = DPIAStatus.Draft
            };
            await _unitOfWork.DPIAs.AddAsync(dpia);
            await _unitOfWork.SaveChangesAsync();

            // Create comment view model
            var commentVM = new AddCommentVM
            {
                ReferenceId = dpia.Id,
                Content = "This is a DPO comment",
                Type = CommentType.DPIA,
                UserId = dpoUser.Id
            };

            // Set up claims for DPO user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, dpoUser.Id.ToString()),
                new Claim(ClaimTypes.Email, dpoUser.Email),
                new Claim(ClaimTypes.Name, dpoUser.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            await _dpiaService.AddCommentAsync(commentVM, principal);

            // Assert
            var comments = await _unitOfWork.Comments.FindAsync(c => c.ReferenceId == dpia.Id && c.Type == CommentType.DPIA);
            Assert.NotNull(comments);
            Assert.Single(comments);
            Assert.Equal("This is a DPO comment", comments.First().Content);
            Assert.Equal(dpoUser.Id, comments.First().UserId);
        }

        [Fact]
        public async Task AddCommentAsync_ShouldThrowException_WhenDPIANotFound()
        {
            // Arrange
            // Create user
            var user = await CreateUser();
            
            // Non-existent DPIA ID
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);

            var member = await CreateDPIAMember(dpia);

            await _unitOfWork.SaveChangesAsync();

            // Create comment view model
            var commentVM = new AddCommentVM
            {
                ReferenceId = Guid.NewGuid(),
                Content = "This is a test comment",
                Type = CommentType.DPIA,
                UserId = member.UserId
            };

            // Set up claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, member.UserId.ToString()),
                new Claim(ClaimTypes.Email, member.User.Email),
                new Claim(ClaimTypes.Name, member.User.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.AddCommentAsync(commentVM, principal));
                
            Assert.Equal("DPIA not found", exception.Message);
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldUpdateComment_WhenUserIsOwner()
        {
            // Arrange
            var externalSystem = await CreateExternalSystem();
            await _unitOfWork.SaveChangesAsync();

            // Create user
            var user = await CreateUser();

            // Create DPIA
            var dpia = new DPIA
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = DPIAType.NewOrUpdatedSystem,
                ExternalSystemId = externalSystem.Id,
                Status = DPIAStatus.Draft
            };
            await _unitOfWork.DPIAs.AddAsync(dpia);
            await _unitOfWork.SaveChangesAsync();

            // Add user as member
            var member = new DPIAMember
            {
                DPIAId = dpia.Id,
                UserId = user.Id
            };
            await _unitOfWork.DPIAMembers.AddAsync(member);
            await _unitOfWork.SaveChangesAsync();

            // Add original comment
            var comment = new Comment
            {
                ReferenceId = dpia.Id,
                UserId = user.Id,
                Content = "Original comment",
                Type = CommentType.DPIA
            };
            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            // Create updated comment view model
            var updateCommentVM = new AddCommentVM
            {
                ReferenceId = dpia.Id,
                Content = "Updated comment",
                Type = CommentType.DPIA,
                UserId = user.Id
            };

            // Set up claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            await _dpiaService.UpdateCommentAsync(comment.Id, updateCommentVM, principal);

            // Assert
            var updatedComment = await _unitOfWork.Comments.GetByIdAsync(comment.Id);
            Assert.NotNull(updatedComment);
            Assert.Equal("Updated comment", updatedComment.Content);
        }

        [Fact]
        public async Task AddMembersAsync_ShouldAddMembers_WhenInputIsValid()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);
            await _unitOfWork.SaveChangesAsync();

            // Create users to add as members
            var user1 = await CreateUser();
            var user2 = await CreateUser();
            await _unitOfWork.SaveChangesAsync();

            // Create member view models
            var membersToAdd = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM { 
                    UserId = user1.Id,
                    Responsibilities = new List<Guid>() 
                },
                new DPIAMemberCreateVM { 
                    UserId = user2.Id,
                    Responsibilities = new List<Guid>() 
                }
            };

            // Act
            await _dpiaService.AddMembersAsync(dpia.Id, membersToAdd);

            // Assert
            var members = await _unitOfWork.DPIAMembers.FindAsync(m => m.DPIAId == dpia.Id);
            Assert.NotNull(members);
            Assert.Equal(2, members.Count());
            Assert.Contains(members, m => m.UserId == user1.Id);
            Assert.Contains(members, m => m.UserId == user2.Id);
        }

        [Fact]
        public async Task AddMembersAsync_ShouldThrowException_WhenDPIANotFound()
        {
            // Arrange
            var nonExistentDpiaId = Guid.NewGuid();
            
            // Create a user
            var user = await CreateUser();
            await _unitOfWork.SaveChangesAsync();

            // Create member view model
            var membersToAdd = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM { 
                    UserId = user.Id,
                    Responsibilities = new List<Guid>() 
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.AddMembersAsync(nonExistentDpiaId, membersToAdd));
                
            Assert.Equal("DPIA not found", exception.Message);
        }

        [Fact]
        public async Task AddMembersAsync_ShouldThrowException_WhenSomeUsersNotFound()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);
            await _unitOfWork.SaveChangesAsync();


            // Create one valid user
            var validUser = await CreateUser();
            await _unitOfWork.SaveChangesAsync();
            
            // Create non-existent user ID
            var nonExistentUserId = Guid.NewGuid();
            
            // Create member view models with both valid and invalid users
            var membersToAdd = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM { 
                    UserId = validUser.Id,
                    Responsibilities = new List<Guid>() 
                },
                new DPIAMemberCreateVM { 
                    UserId = nonExistentUserId,
                    Responsibilities = new List<Guid>() 
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _dpiaService.AddMembersAsync(dpia.Id, membersToAdd));
                
            Assert.Equal($"User with ID {nonExistentUserId} not found.", exception.Message);
        }

        [Fact]
        public async Task AddMembersAsync_ShouldThrowException_WhenAllMembersAlreadyExist()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);
            
            // Create a user
            var user = await CreateUser();
            
            // Add the user as a member already
            var existingMember = new DPIAMember
            {
                DPIAId = dpia.Id,
                UserId = user.Id
            };
            await _unitOfWork.DPIAMembers.AddAsync(existingMember);
            await _unitOfWork.SaveChangesAsync();

            // Create member view model with the same user
            var membersToAdd = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM { 
                    UserId = user.Id,
                    Responsibilities = new List<Guid>() 
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.AddMembersAsync(dpia.Id, membersToAdd));
                
            Assert.Equal("Members already exist in the DPIA.", exception.Message);
        }

        [Fact]
        public async Task UpdateMembersAsync_ShouldUpdateMembers_WhenInputIsValid()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);
            
            // Create users for members
            var user1 = await CreateUser();
            var user2 = await CreateUser();
            
            // Add existing members
            var existingMember1 = new DPIAMember
            {
                DPIAId = dpia.Id,
                UserId = user1.Id
            };
            await _unitOfWork.DPIAMembers.AddAsync(existingMember1);
            
            var existingMember2 = new DPIAMember
            {
                DPIAId = dpia.Id,
                UserId = user2.Id
            };
            await _unitOfWork.DPIAMembers.AddAsync(existingMember2);
            await _unitOfWork.SaveChangesAsync();

            // Create updated member view models
            var updatedMembers = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM { 
                    UserId = user1.Id,
                    Responsibilities = new List<Guid>() 
                }
                // user2 is omitted, which means they should be removed
            };

            // Act
            await _dpiaService.UpdateMembersAsync(dpia.Id, updatedMembers);

            // Assert
            var remainingMembers = await _unitOfWork.DPIAMembers.FindAsync(m => m.DPIAId == dpia.Id);
            Assert.NotNull(remainingMembers);
            Assert.Single(remainingMembers);
            Assert.Equal(user1.Id, remainingMembers.First().UserId);
        }

        [Fact]
        public async Task UpdateMembersAsync_ShouldThrowException_WhenDPIANotFound()
        {
            // Arrange
            var nonExistentDpiaId = Guid.NewGuid();
            
            // Create a user
            var user = await CreateUser();
            await _unitOfWork.SaveChangesAsync();

            // Create member view model
            var membersToUpdate = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM { 
                    UserId = user.Id,
                    Responsibilities = new List<Guid>() 
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _dpiaService.UpdateMembersAsync(nonExistentDpiaId, membersToUpdate));
                
            Assert.Equal("DPIA not found", exception.Message);
        }

        [Fact]
        public async Task UpdateMembersAsync_ShouldThrowException_WhenSomeUsersNotFound()
        {
            // Arrange
            var dpia = await CreateDPIA(DPIAType.NewOrUpdatedSystem);
            
            // Create one valid user
            var validUser = await CreateUser();
            
            // Add existing member
            var existingMember = new DPIAMember
            {
                DPIAId = dpia.Id,
                UserId = validUser.Id
            };
            await _unitOfWork.DPIAMembers.AddAsync(existingMember);
            await _unitOfWork.SaveChangesAsync();
            
            // Create non-existent user ID
            var nonExistentUserId = Guid.NewGuid();
            
            // Create member view models with both valid and invalid users
            var membersToUpdate = new List<DPIAMemberCreateVM>
            {
                new DPIAMemberCreateVM { 
                    UserId = validUser.Id,
                    Responsibilities = new List<Guid>() 
                },
                new DPIAMemberCreateVM { 
                    UserId = nonExistentUserId,
                    Responsibilities = new List<Guid>() 
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _dpiaService.UpdateMembersAsync(dpia.Id, membersToUpdate));
                
            Assert.Equal($"User with ID {nonExistentUserId} not found.", exception.Message);
        }

        private async Task<ExternalSystem> CreateExternalSystem()
        {
            var externalSystem = new ExternalSystem
            {
                Name = "Test External System",
            };
            externalSystem = await _unitOfWork.ExternalSystems.AddAsync(externalSystem);
            return externalSystem;
        }

        private async Task<DPIADocument> CreateDocument(DPIA dpia, Responsibility responsibility)
        {
            var fileUrl = await _fileRepository.UploadFileAsync(new MemoryStream(), "test.pdf", "application/pdf");


            var document = new DPIADocument
            {
                DPIAId = dpia.Id,
                ResponsibleId = responsibility.Id,
                Title = "Test Document",
                FileUrl = fileUrl,
                FileFormat = "Test File Format",
            };
            document = await _unitOfWork.DPIADocuments.AddAsync(document);
            return document;
        }

        private async Task<User> CreateUserWithGroup(string groupName)
        {
            var user = await CreateUser();

            var group = await CreateGroup(groupName);

            user.Groups.Add(group);

            await _unitOfWork.SaveChangesAsync();

            return user;
        }

        private async Task<Group> CreateGroup(string name)
        {
            var group = new Group { Id = Guid.NewGuid(), Name = name, Description = name };
            return group;
        }
        
        private async Task<DPIA> CreateDPIA(DPIAType type)
        {
            var dpia = new DPIA
            {
                Title = "Test DPIA",
                Description = "Test Description",
                Type = type,
                Status = DPIAStatus.Draft,
            };
            dpia = await _dpiaRepository.AddAsync(dpia);
            return dpia;
        }

        private async Task<Responsibility> CreateResponsibility()
        {
            var responsibility = new Responsibility
            {
                Title = "Test Responsibility",
                Description = "Test Description",   
            };
            responsibility = await _unitOfWork.Responsibilities.AddAsync(responsibility);
            return responsibility;
        }

        private async Task<User> CreateUser()
        {
            var user = new User
            {
                FullName = "Test User",
                Email = "test@test.com",
                Password = "TestPassword",
                UserName = "test",
            };
            user = await _unitOfWork.Users.AddAsync(user);
            return user;
        }

        private async Task<DPIAMember> CreateDPIAMember(DPIA dpia)
        {
            var user = await CreateUser();

            var member = new DPIAMember
            {
                DPIAId = dpia.Id,
                UserId = user.Id,
            };
            member = await _unitOfWork.DPIAMembers.AddAsync(member);
            return member;
        }
        
        private async Task<DPIAResponsibility> CreateDPIAResponsibility(DPIA dpia, Responsibility responsibility)
        {
            var dpiaResponsibility = new DPIAResponsibility
            {
                DPIAId = dpia.Id,
                ResponsibilityId = responsibility.Id,
                DueDate = DateTime.Now.AddDays(1),
            };
            dpiaResponsibility = await _unitOfWork.DPIAResponsibilities.AddAsync(dpiaResponsibility);
            return dpiaResponsibility;
        }   
    
        private async Task<MemberResponsibility> CreateMemberResponsibility(DPIAResponsibility dpiaResponsibility, DPIAMember member)
        {
            var memberResponsibility = new MemberResponsibility
            {
                DPIAResponsibilityId = dpiaResponsibility.Id,
                MemberId = member.Id,
            };
            memberResponsibility = await _unitOfWork.MemberResponsibilities.AddAsync(memberResponsibility);
            return memberResponsibility;
        }
    }


}


using AutoMapper;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Consent;
using DPMS_WebAPI.ViewModels.Purpose;
using FluentResults;
using Moq;
using System.Text;
using System.Linq.Expressions;
using FlexCel.XlsAdapter;
using System.IO;
using FlexCel.Core;

namespace DPMS_WebAPI.Tests.UnitTests.Services
{
    public class ConsentServiceTest : IDisposable
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IConsentRepository> _consentRepositoryMock;
        private readonly Mock<IConsentTokenRepository> _consentTokenRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IExternalSystemService> _externalSystemServiceMock;
        private readonly Mock<IExternalSystemRepository> _externalSystemRepositoryMock;
        private readonly Mock<IPurposeService> _purposeServiceMock;
        private readonly Mock<IExternalSystemPurposeRepository> _externalSystemPurposeRepositoryMock;
        private readonly Mock<IConsentPurposeService> _consentPurposeServiceMock;
        private readonly Mock<IPrivacyPolicyService> _policyServiceMock;
        private readonly Mock<IConsentPurposeRepository> _consentPurposeRepositoryMock;
        private readonly ConsentService _consentService;
        private readonly string _testTemplatePath;

        public ConsentServiceTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _consentRepositoryMock = new Mock<IConsentRepository>();
            _consentTokenRepositoryMock = new Mock<IConsentTokenRepository>();
            _mapperMock = new Mock<IMapper>();
            _externalSystemServiceMock = new Mock<IExternalSystemService>();
            _externalSystemRepositoryMock = new Mock<IExternalSystemRepository>();
            _purposeServiceMock = new Mock<IPurposeService>();
            _externalSystemPurposeRepositoryMock = new Mock<IExternalSystemPurposeRepository>();
            _consentPurposeServiceMock = new Mock<IConsentPurposeService>();
            _policyServiceMock = new Mock<IPrivacyPolicyService>();
            _consentPurposeRepositoryMock = new Mock<IConsentPurposeRepository>();

            // Create test template directory if it doesn't exist
            var templateDir = Path.Combine(Directory.GetCurrentDirectory(), "ExcelTemplates");
            if (!Directory.Exists(templateDir))
            {
                Directory.CreateDirectory(templateDir);
            }

            // Create test template file
            _testTemplatePath = Path.Combine(templateDir, "ConsentTemplate.xlsx");
            CreateTestTemplate(_testTemplatePath);

            _consentService = new ConsentService(
                _unitOfWorkMock.Object,
                _consentRepositoryMock.Object,
                _consentTokenRepositoryMock.Object,
                _mapperMock.Object,
                _externalSystemServiceMock.Object,
                _externalSystemRepositoryMock.Object,
                _purposeServiceMock.Object,
                _externalSystemPurposeRepositoryMock.Object,
                _consentPurposeServiceMock.Object,
                _consentPurposeRepositoryMock.Object,
                _policyServiceMock.Object
            );
        }

        private void CreateTestTemplate(string path)
        {
            var xls = new XlsFile(1, TExcelFileFormat.v2019, true);
            xls.NewFile(1);

            // Add sheets and basic structure
            xls.SetCellValue(1, 1, "ConsentSubmission");
            xls.SetCellValue(1, 2, "Purposes");
            xls.SetCellValue(1, 3, "ConsentPVM");

            xls.Save(path);
        }

        public void Dispose()
        {
            // Clean up test template file
            if (File.Exists(_testTemplatePath))
            {
                File.Delete(_testTemplatePath);
            }
        }

        [Fact]
        public async Task SubmitConsent_WithValidData_ShouldSucceed()
        {
            // Arrange
            var submitConsentVM = new SubmitConsentVM
            {
                Email = "test@example.com",
                ExternalSystemId = Guid.NewGuid(),
                ConsentMethod = ConsentMethod.WebForm,
                ConsentIp = "127.0.0.1",
                ConsentUserAgent = "Test Browser",
                PrivacyPolicyId = Guid.NewGuid(),
                ConsentPurposes = new List<ConsentPurposeVM>
                {
                    new ConsentPurposeVM { PurposeId = Guid.NewGuid() }
                }
            };

            var systemPurpose = new List<ExternalSystemPurpose>
            {
                new ExternalSystemPurpose { PurposeId = submitConsentVM.ConsentPurposes[0].PurposeId }
            };

            _externalSystemPurposeRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<ExternalSystemPurpose, bool>>>()))
                .ReturnsAsync(systemPurpose);

            var consent = new Consent();
            _mapperMock.Setup(x => x.Map<Consent>(submitConsentVM)).Returns(consent);

            // Act
            await _consentService.SubmitConsent(submitConsentVM);

            // Assert
            _consentRepositoryMock.Verify(x => x.AddAsync(consent), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SubmitConsent_WithNoPurposes_ShouldThrowException()
        {
            // Arrange
            var submitConsentVM = new SubmitConsentVM
            {
                Email = "test@example.com",
                ExternalSystemId = Guid.NewGuid(),
                ConsentMethod = ConsentMethod.WebForm,
                ConsentIp = "127.0.0.1",
                ConsentUserAgent = "Test Browser",
                PrivacyPolicyId = Guid.NewGuid(),
                ConsentPurposes = new List<ConsentPurposeVM>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _consentService.SubmitConsent(submitConsentVM));
        }

        [Fact]
        public async Task GetConsentByEmail_WithValidEmail_ShouldReturnConsent()
        {
            // Arrange
            var email = "test@example.com";
            var expectedConsent = new Consent
            {
                Email = email,
                ExternalSystemId = Guid.NewGuid(),
                ConsentDate = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _consentRepositoryMock
                .Setup(x => x.GetConsentByEmailAsync(email))
                .ReturnsAsync(expectedConsent);

            // Act
            var result = await _consentService.GetConsentByEmail(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedConsent, result);
            _consentRepositoryMock.Verify(x => x.GetConsentByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task GetConsentByEmail_WithUnknownEmail_ShouldReturnNull()
        {
            // Arrange
            var unknownEmail = "unknown@example.com";
            _consentRepositoryMock
                .Setup(x => x.GetConsentByEmailAsync(unknownEmail))
                .ReturnsAsync((Consent)null);

            // Act
            var result = await _consentService.GetConsentByEmail(unknownEmail);

            // Assert
            Assert.Null(result);
            _consentRepositoryMock.Verify(x => x.GetConsentByEmailAsync(unknownEmail), Times.Once);
        }

        [Fact]
        public async Task GetConsentByEmail_WithEmptyEmail_ShouldReturnNull()
        {
            // Arrange
            var emptyEmail = "";
            _consentRepositoryMock
                .Setup(x => x.GetConsentByEmailAsync(emptyEmail))
                .ReturnsAsync((Consent)null);

            // Act
            var result = await _consentService.GetConsentByEmail(emptyEmail);

            // Assert
            Assert.Null(result);
            _consentRepositoryMock.Verify(x => x.GetConsentByEmailAsync(emptyEmail), Times.Once);
        }

        [Fact]
        public async Task GetConsentByEmail_WithNullEmail_ShouldReturnNull()
        {
            // Arrange
            string nullEmail = null;
            _consentRepositoryMock
                .Setup(x => x.GetConsentByEmailAsync(nullEmail))
                .ReturnsAsync((Consent)null);

            // Act
            var result = await _consentService.GetConsentByEmail(nullEmail);

            // Assert
            Assert.Null(result);
            _consentRepositoryMock.Verify(x => x.GetConsentByEmailAsync(nullEmail), Times.Once);
        }

        [Fact]
        public async Task GetConsentByEmail_WithSpecialCharacters_ShouldReturnConsent()
        {
            // Arrange
            var email = "a+b@example.com";
            var expectedConsent = new Consent
            {
                Email = email,
                ExternalSystemId = Guid.NewGuid(),
                ConsentDate = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _consentRepositoryMock
                .Setup(x => x.GetConsentByEmailAsync(email))
                .ReturnsAsync(expectedConsent);

            // Act
            var result = await _consentService.GetConsentByEmail(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedConsent, result);
            _consentRepositoryMock.Verify(x => x.GetConsentByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task ValidateConsentToken_WithValidToken_ShouldReturnTrue()
        {
            // Arrange
            var token = "valid-token";
            var consentToken = new ConsentToken
            {
                TokenString = token,
                IsValid = true,
                ExpireTime = DateTime.UtcNow.AddMinutes(5)
            };

            _consentTokenRepositoryMock
                .Setup(x => x.GetByIdAsync(token))
                .ReturnsAsync(consentToken);

            // Act
            var result = await _consentService.ValidateConsentToken(token);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateConsentToken_WithValidSystem_ShouldCreateToken()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            _consentTokenRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<ConsentToken>()))
                .ReturnsAsync((ConsentToken token) => token);

            // Act
            var result = await _consentService.CreateConsentToken(systemId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            _consentTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ConsentToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateConsentToken_WithMultipleCalls_ShouldCreateUniqueTokens()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            _consentTokenRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<ConsentToken>()))
                .ReturnsAsync((ConsentToken token) => token);

            // Act
            var token1 = await _consentService.CreateConsentToken(systemId);
            var token2 = await _consentService.CreateConsentToken(systemId);

            // Assert
            Assert.NotNull(token1);
            Assert.NotNull(token2);
            Assert.NotEqual(token1, token2);
            _consentTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ConsentToken>()), Times.Exactly(2));
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateConsentToken_WithEmptySystemId_ShouldCreateToken()
        {
            // Arrange
            var systemId = Guid.Empty;
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            _consentTokenRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<ConsentToken>()))
                .ReturnsAsync((ConsentToken token) => token);

            // Act
            var result = await _consentService.CreateConsentToken(systemId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            _consentTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ConsentToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateConsentToken_WhenSaveFails_ShouldThrowException()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            _consentTokenRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<ConsentToken>()))
                .ReturnsAsync((ConsentToken token) => token);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _consentService.CreateConsentToken(systemId));
        }

        [Fact]
        public async Task CreateConsentToken_ShouldSetCorrectExpiration()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };
            var expectedExpiration = DateTime.UtcNow.AddMinutes(5);

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            ConsentToken capturedToken = null;
            _consentTokenRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<ConsentToken>()))
                .Callback<ConsentToken>(token => capturedToken = token)
                .ReturnsAsync((ConsentToken token) => token);

            // Act
            await _consentService.CreateConsentToken(systemId);

            // Assert
            Assert.NotNull(capturedToken);
            Assert.True(capturedToken.ExpireTime >= expectedExpiration.AddMinutes(-1) &&
                       capturedToken.ExpireTime <= expectedExpiration.AddMinutes(1));
            _consentTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ConsentToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetConsentLogWithPurpose_WithValidQuery_ShouldReturnPagedList()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10
            };

            var consents = new List<Consent>
            {
                new Consent
                {
                    Email = "test1@example.com",
                    ExternalSystemId = Guid.NewGuid(),
                    ConsentDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                },
                new Consent
                {
                    Email = "test2@example.com",
                    ExternalSystemId = Guid.NewGuid(),
                    ConsentDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                }
            };

            var expectedResponse = new PagedResponse<Consent>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = consents.Count,
                Data = consents
            };

            _consentRepositoryMock
                .Setup(x => x.GetConsentsWithPurposeAsync(queryParams))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _consentService.GetConsentLogWithPurpose(queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse, result);
            Assert.Equal(consents.Count, result.Data.Count());
            _consentRepositoryMock.Verify(x => x.GetConsentsWithPurposeAsync(queryParams), Times.Once);
        }

        [Fact]
        public async Task GetConsentLogWithPurpose_WithEmptyDB_ShouldReturnEmptyPage()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10
            };

            var expectedResponse = new PagedResponse<Consent>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0,
                Data = new List<Consent>()
            };

            _consentRepositoryMock
                .Setup(x => x.GetConsentsWithPurposeAsync(queryParams))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _consentService.GetConsentLogWithPurpose(queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalRecords);
            _consentRepositoryMock.Verify(x => x.GetConsentsWithPurposeAsync(queryParams), Times.Once);
        }

        [Fact]
        public async Task GetConsentLogWithPurpose_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 2,
                PageSize = 10
            };

            var consents = Enumerable.Range(11, 10).Select(i => new Consent
            {
                Email = $"test{i}@example.com",
                ExternalSystemId = Guid.NewGuid(),
                ConsentDate = DateTime.Now,
                CreatedAt = DateTime.Now
            }).ToList();

            var expectedResponse = new PagedResponse<Consent>
            {
                PageNumber = 2,
                PageSize = 10,
                TotalPages = 5,
                TotalRecords = 50,
                Data = consents
            };

            _consentRepositoryMock
                .Setup(x => x.GetConsentsWithPurposeAsync(queryParams))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _consentService.GetConsentLogWithPurpose(queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Data.Count());
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(50, result.TotalRecords);
            Assert.Equal(5, result.TotalPages);
            _consentRepositoryMock.Verify(x => x.GetConsentsWithPurposeAsync(queryParams), Times.Once);
        }

        [Fact]
        public async Task GetConsentLogWithPurpose_WithSorting_ShouldReturnSortedResults()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "Date"
            };

            var consents = new List<Consent>
            {
                new Consent
                {
                    Email = "test1@example.com",
                    ExternalSystemId = Guid.NewGuid(),
                    ConsentDate = DateTime.Now.AddDays(-2),
                    CreatedAt = DateTime.Now.AddDays(-2)
                },
                new Consent
                {
                    Email = "test2@example.com",
                    ExternalSystemId = Guid.NewGuid(),
                    ConsentDate = DateTime.Now.AddDays(-1),
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                new Consent
                {
                    Email = "test3@example.com",
                    ExternalSystemId = Guid.NewGuid(),
                    ConsentDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                }
            }.OrderBy(c => c.ConsentDate).ToList();

            var expectedResponse = new PagedResponse<Consent>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = consents.Count,
                Data = consents
            };

            _consentRepositoryMock
                .Setup(x => x.GetConsentsWithPurposeAsync(queryParams))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _consentService.GetConsentLogWithPurpose(queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(consents.Count, result.Data.Count());
            Assert.Equal(consents.First().Email, result.Data.First().Email);
            Assert.Equal(consents.Last().Email, result.Data.Last().Email);
            _consentRepositoryMock.Verify(x => x.GetConsentsWithPurposeAsync(queryParams), Times.Once);
        }

        [Fact]
        public async Task GetConsentLogWithPurpose_WithInvalidSortKey_ShouldUseDefaultSorting()
        {
            // Arrange
            var queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "InvalidField"
            };

            var consents = new List<Consent>
            {
                new Consent
                {
                    Email = "test1@example.com",
                    ExternalSystemId = Guid.NewGuid(),
                    ConsentDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                }
            };

            var expectedResponse = new PagedResponse<Consent>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = consents.Count,
                Data = consents
            };

            _consentRepositoryMock
                .Setup(x => x.GetConsentsWithPurposeAsync(queryParams))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _consentService.GetConsentLogWithPurpose(queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(consents.Count, result.Data.Count());
            _consentRepositoryMock.Verify(x => x.GetConsentsWithPurposeAsync(queryParams), Times.Once);
        }

        [Fact]
        public async Task UpdateToken_WithValidToken_ShouldInvalidateToken()
        {
            // Arrange
            var token = "valid-token";
            var consentToken = new ConsentToken
            {
                TokenString = token,
                IsValid = true,
                ExpireTime = DateTime.UtcNow.AddMinutes(5)
            };

            _consentTokenRepositoryMock
                .Setup(x => x.GetByIdAsync(token))
                .ReturnsAsync(consentToken);

            // Act
            await _consentService.UpdateToken(token);

            // Assert
            Assert.False(consentToken.IsValid);
            _consentTokenRepositoryMock.Verify(x => x.Update(consentToken), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateToken_WithUnknownToken_ShouldThrowException()
        {
            // Arrange
            var unknownToken = "unknown-token";
            _consentTokenRepositoryMock
                .Setup(x => x.GetByIdAsync(unknownToken))
                .ReturnsAsync((ConsentToken)null);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _consentService.UpdateToken(unknownToken));
            _consentTokenRepositoryMock.Verify(x => x.Update(It.IsAny<ConsentToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateToken_WithAlreadyInvalidToken_ShouldStillUpdate()
        {
            // Arrange
            var token = "invalid-token";
            var consentToken = new ConsentToken
            {
                TokenString = token,
                IsValid = false,
                ExpireTime = DateTime.UtcNow.AddMinutes(-5)
            };

            _consentTokenRepositoryMock
                .Setup(x => x.GetByIdAsync(token))
                .ReturnsAsync(consentToken);

            // Act
            await _consentService.UpdateToken(token);

            // Assert
            Assert.False(consentToken.IsValid);
            _consentTokenRepositoryMock.Verify(x => x.Update(consentToken), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateToken_WhenSaveFails_ShouldThrowException()
        {
            // Arrange
            var token = "valid-token";
            var consentToken = new ConsentToken
            {
                TokenString = token,
                IsValid = true,
                ExpireTime = DateTime.UtcNow.AddMinutes(5)
            };

            _consentTokenRepositoryMock
                .Setup(x => x.GetByIdAsync(token))
                .ReturnsAsync(consentToken);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _consentService.UpdateToken(token));
            _consentTokenRepositoryMock.Verify(x => x.Update(consentToken), Times.Once);
        }

        [Fact]
        public async Task GetSystemFromToken_WithValidToken_ShouldReturnSystemId()
        {
            // Arrange
            var token = "valid-token";
            var expectedSystemId = Guid.NewGuid();
            var consentToken = new ConsentToken
            {
                TokenString = token,
                ExternalSystemId = expectedSystemId,
                IsValid = true,
                ExpireTime = DateTime.UtcNow.AddMinutes(5)
            };

            _consentTokenRepositoryMock
                .Setup(x => x.GetByIdAsync(token))
                .ReturnsAsync(consentToken);

            // Act
            var result = await _consentService.GetSystemFromToken(token);

            // Assert
            Assert.Equal(expectedSystemId, result);
            _consentTokenRepositoryMock.Verify(x => x.GetByIdAsync(token), Times.Once);
        }

        [Fact]
        public async Task GetSystemFromToken_WithUnknownToken_ShouldThrowException()
        {
            // Arrange
            var unknownToken = "unknown-token";
            _consentTokenRepositoryMock
                .Setup(x => x.GetByIdAsync(unknownToken))
                .ReturnsAsync((ConsentToken)null);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _consentService.GetSystemFromToken(unknownToken));
            _consentTokenRepositoryMock.Verify(x => x.GetByIdAsync(unknownToken), Times.Once);
        }

        [Fact]
        public async Task GetSystemFromToken_WithExpiredToken_ShouldReturnSystemId()
        {
            // Arrange
            var token = "expired-token";
            var expectedSystemId = Guid.NewGuid();
            var consentToken = new ConsentToken
            {
                TokenString = token,
                ExternalSystemId = expectedSystemId,
                IsValid = true,
                ExpireTime = DateTime.UtcNow.AddMinutes(-5) // Expired token
            };

            _consentTokenRepositoryMock
                .Setup(x => x.GetByIdAsync(token))
                .ReturnsAsync(consentToken);

            // Act
            var result = await _consentService.GetSystemFromToken(token);

            // Assert
            Assert.Equal(expectedSystemId, result);
            _consentTokenRepositoryMock.Verify(x => x.GetByIdAsync(token), Times.Once);
        }

        [Fact]
        public async Task ExportConsentLog_WithValidSystemId_ShouldReturnStream()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };
            var consent = new Consent
            {
                ExternalSystemId = systemId,
                Email = "test@example.com",
                ConsentDate = DateTime.Now,
                CreatedAt = DateTime.Now
            };
            var purpose = new Purpose { Id = Guid.NewGuid(), Name = "Test Purpose" };
            var consentPurpose = new ConsentPurpose
            {
                ConsentId = consent.Id,
                PurposeId = purpose.Id,
                Status = true
            };

            var consents = new List<Consent> { consent };
            var purposes = new List<Purpose> { purpose };
            var consentPurposes = new List<ConsentPurpose> { consentPurpose };

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            _consentRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Consent, bool>>>()))
                .ReturnsAsync(consents);

            _purposeServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(purposes);

            _consentPurposeServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(consentPurposes);

            var consentVM = new ConsentVM
            {
                Id = consent.Id,
                Email = consent.Email,
                ExternalSystemId = consent.ExternalSystemId,
                ExternalSystemName = system.Name,
                ConsentDate = consent.ConsentDate,
                CreatedAt = consent.CreatedAt
            };

            var purposeVM = new PurposeVM
            {
                Id = purpose.Id,
                Name = purpose.Name
            };

            var consentPVM = new ConsentPVM
            {
                ConsentId = consentPurpose.ConsentId,
                PurposeId = consentPurpose.PurposeId,
                Status = consentPurpose.Status
            };

            _mapperMock
                .Setup(x => x.Map<List<ConsentVM>>(consents))
                .Returns(new List<ConsentVM> { consentVM });

            _mapperMock
                .Setup(x => x.Map<List<PurposeVM>>(purposes))
                .Returns(new List<PurposeVM> { purposeVM });

            _mapperMock
                .Setup(x => x.Map<List<ConsentPVM>>(consentPurposes))
                .Returns(new List<ConsentPVM> { consentPVM });

            // Act
            var result = await _consentService.ExportConsentLog(systemId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CanRead);
        }

        [Fact]
        public async Task ExportConsentLog_WithNullSystemId_ShouldReturnAllConsents()
        {
            // Arrange
            var consents = new List<Consent>
            {
                new Consent { Email = "test1@example.com" },
                new Consent { Email = "test2@example.com" }
            };
            var purposes = new List<Purpose>
            {
                new Purpose { Id = Guid.NewGuid(), Name = "Test Purpose" }
            };
            var consentPurposes = new List<ConsentPurpose>();

            _consentRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(consents);

            _purposeServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(purposes);

            _consentPurposeServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(consentPurposes);

            _mapperMock
                .Setup(x => x.Map<List<ConsentVM>>(consents))
                .Returns(consents.Select(c => new ConsentVM { Email = c.Email }).ToList());

            _mapperMock
                .Setup(x => x.Map<List<PurposeVM>>(purposes))
                .Returns(purposes.Select(p => new PurposeVM { Id = p.Id, Name = p.Name }).ToList());

            _mapperMock
                .Setup(x => x.Map<List<ConsentPVM>>(consentPurposes))
                .Returns(new List<ConsentPVM>()); // empty list, still not null


            // Act
            var result = await _consentService.ExportConsentLog(null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CanRead);
        }

        [Fact]
        public async Task ExportConsentLog_WithInvalidSystemId_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidSystemId = Guid.NewGuid();
            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(invalidSystemId))
                .ReturnsAsync((ExternalSystem)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _consentService.ExportConsentLog(invalidSystemId));
        }

        [Fact]
        public async Task ExportConsentLog_WithNoData_ShouldReturnEmptyStream()
        {
            // Arrange
            var emptyConsents = new List<Consent>();
            var emptyPurposes = new List<Purpose>();
            var emptyConsentPurposes = new List<ConsentPurpose>();

            _consentRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(emptyConsents);

            _purposeServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(emptyPurposes);

            _consentPurposeServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(emptyConsentPurposes);

            _mapperMock
                .Setup(x => x.Map<List<ConsentVM>>(emptyConsents))
                .Returns(new List<ConsentVM>());

            _mapperMock
                .Setup(x => x.Map<List<PurposeVM>>(emptyPurposes))
                .Returns(new List<PurposeVM>());

            _mapperMock
                .Setup(x => x.Map<List<ConsentPVM>>(emptyConsentPurposes))
                .Returns(new List<ConsentPVM>());

            // Act
            var result = await _consentService.ExportConsentLog(null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CanRead);
        }

        [Fact]
        public async Task ExportConsentLog_WithValidData_ShouldMapCorrectly()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };
            var consent = new Consent
            {
                Id = Guid.NewGuid(),
                ExternalSystemId = systemId,
                Email = "test@example.com",
                ExternalSystem = system,
                ConsentDate = DateTime.Now,
                CreatedAt = DateTime.Now
            };
            var purpose = new Purpose { Id = Guid.NewGuid(), Name = "Test Purpose" };
            var consentPurpose = new ConsentPurpose
            {
                ConsentId = consent.Id,
                PurposeId = purpose.Id,
                Purpose = purpose,
                Status = true
            };

            var consents = new List<Consent> { consent };
            var purposes = new List<Purpose> { purpose };
            var consentPurposes = new List<ConsentPurpose> { consentPurpose };

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            _consentRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Consent, bool>>>()))
                .ReturnsAsync(consents);

            _purposeServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(purposes);

            _consentPurposeServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(consentPurposes);

            var consentVM = new ConsentVM
            {
                Id = consent.Id,
                Email = consent.Email,
                ExternalSystemId = consent.ExternalSystemId,
                ExternalSystemName = system.Name,
                ConsentDate = consent.ConsentDate,
                CreatedAt = consent.CreatedAt
            };

            var purposeVM = new PurposeVM
            {
                Id = purpose.Id,
                Name = purpose.Name
            };

            var consentPVM = new ConsentPVM
            {
                ConsentId = consentPurpose.ConsentId,
                PurposeId = consentPurpose.PurposeId,
                Status = consentPurpose.Status
            };

            _mapperMock
                .Setup(x => x.Map<List<ConsentVM>>(consents))
                .Returns(new List<ConsentVM> { consentVM });

            _mapperMock
                .Setup(x => x.Map<List<PurposeVM>>(purposes))
                .Returns(new List<PurposeVM> { purposeVM });

            _mapperMock
                .Setup(x => x.Map<List<ConsentPVM>>(consentPurposes))
                .Returns(new List<ConsentPVM> { consentPVM });

            // Act
            var result = await _consentService.ExportConsentLog(systemId);

            // Assert
            Assert.NotNull(result);
            _mapperMock.Verify(x => x.Map<List<ConsentVM>>(consents), Times.Once);
            _mapperMock.Verify(x => x.Map<List<PurposeVM>>(purposes), Times.Once);
            _mapperMock.Verify(x => x.Map<List<ConsentPVM>>(consentPurposes), Times.Once);
        }

        [Fact]
        public async Task ExportConsentLog_WithMissingTemplate_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            // Act & Assert
            await Assert.ThrowsAsync<FlexCelCoreException>(() => _consentService.ExportConsentLog(systemId));
        }

        [Fact]
        public async Task ExportConsentLog_WithCorruptTemplate_ShouldThrowException()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };
            var consents = new List<Consent>();
            var purposes = new List<Purpose>();
            var consentPurposes = new List<ConsentPurpose>();

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            _consentRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Consent, bool>>>()))
                .ReturnsAsync(consents);

            _purposeServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(purposes);

            _consentPurposeServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(consentPurposes);

            // Create a corrupt template file
            var corruptTemplatePath = "ExcelTemplates/ConsentTemplate.xlsx";
            File.WriteAllText(corruptTemplatePath, "This is not a valid Excel file");

            // Act & Assert
            await Assert.ThrowsAsync<FlexCelCoreException>(() => _consentService.ExportConsentLog(systemId));

            // Cleanup
            File.Delete(corruptTemplatePath);
        }

        [Fact]
        public async Task ExportConsentLog_WithMalformedPurposeData_ShouldThrowException()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };
            var consents = new List<Consent>
            {
                new Consent { ExternalSystemId = systemId }
            };
            var purposes = new List<Purpose>
            {
                new Purpose { Id = Guid.NewGuid() }
            };
            var consentPurposes = new List<ConsentPurpose>
            {
                new ConsentPurpose { ConsentId = Guid.NewGuid(), PurposeId = Guid.NewGuid() } // Mismatched IDs
            };

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            _consentRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Consent, bool>>>()))
                .ReturnsAsync(consents);

            _purposeServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(purposes);

            _consentPurposeServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(consentPurposes);

            // Act & Assert
            await Assert.ThrowsAsync<FlexCelCoreException>(() => _consentService.ExportConsentLog(systemId));
        }

        [Fact]
        public async Task SubmitConsent_WithValidData_ShouldSucceedAndWithdrawOldConsents()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var purposeId = Guid.NewGuid();
            var email = "test@example.com";

            var submitConsentVM = new SubmitConsentVM
            {
                Email = email,
                ExternalSystemId = systemId,
                ConsentMethod = ConsentMethod.WebForm,
                ConsentIp = "127.0.0.1",
                ConsentUserAgent = "Test Browser",
                PrivacyPolicyId = Guid.NewGuid(),
                ConsentPurposes = new List<ConsentPurposeVM>
                {
                    new ConsentPurposeVM { PurposeId = purposeId }
                }
            };

            var systemPurpose = new List<ExternalSystemPurpose>
            {
                new ExternalSystemPurpose { PurposeId = purposeId }
            };

            var oldConsents = new List<Consent>
            {
                new Consent { Email = email, IsWithdrawn = false }
            };

            var newConsent = new Consent { Email = email };

            _externalSystemPurposeRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<ExternalSystemPurpose, bool>>>()))
                .ReturnsAsync(systemPurpose);

            _consentRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Consent, bool>>>()))
                .ReturnsAsync(oldConsents);

            _mapperMock
                .Setup(x => x.Map<Consent>(submitConsentVM))
                .Returns(newConsent);

            // Act
            await _consentService.SubmitConsent(submitConsentVM);

            // Assert
            _consentRepositoryMock.Verify(x => x.Update(It.Is<Consent>(c => c.IsWithdrawn)), Times.Exactly(1));
            _consentRepositoryMock.Verify(x => x.AddAsync(It.Is<Consent>(c => c == newConsent)), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
        }


        [Fact]
        public async Task SubmitConsent_WithEmptyPurposes_ShouldThrowException()
        {
            // Arrange
            var submitConsentVM = new SubmitConsentVM
            {
                Email = "test@example.com",
                ExternalSystemId = Guid.NewGuid(),
                ConsentMethod = ConsentMethod.WebForm,
                ConsentIp = "127.0.0.1",
                ConsentUserAgent = "Test Browser",
                PrivacyPolicyId = Guid.NewGuid(),
                ConsentPurposes = new List<ConsentPurposeVM>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _consentService.SubmitConsent(submitConsentVM));
        }

        [Fact]
        public async Task SubmitConsent_WithInvalidPurposes_ShouldThrowException()
        {
            // Arrange
            var submitConsentVM = new SubmitConsentVM
            {
                Email = "test@example.com",
                ExternalSystemId = Guid.NewGuid(),
                ConsentMethod = ConsentMethod.WebForm,
                ConsentIp = "127.0.0.1",
                ConsentUserAgent = "Test Browser",
                PrivacyPolicyId = Guid.NewGuid(),
                ConsentPurposes = new List<ConsentPurposeVM>
                {
                    new ConsentPurposeVM { PurposeId = Guid.NewGuid() }
                }
            };

            _externalSystemPurposeRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<ExternalSystemPurpose, bool>>>()))
                .ReturnsAsync(new List<ExternalSystemPurpose>());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _consentService.SubmitConsent(submitConsentVM));
        }

        [Fact]
        public async Task SubmitConsent_WithExistingConsents_ShouldWithdrawOldConsents()
        {
            // Arrange
            var email = "test@example.com";
            var purposeId = Guid.NewGuid();

            var submitConsentVM = new SubmitConsentVM
            {
                Email = email,
                ExternalSystemId = Guid.NewGuid(),
                ConsentMethod = ConsentMethod.WebForm,
                ConsentIp = "127.0.0.1",
                ConsentUserAgent = "Test Browser",
                PrivacyPolicyId = Guid.NewGuid(),
                ConsentPurposes = new List<ConsentPurposeVM>
            {
                new ConsentPurposeVM { PurposeId = purposeId }
            }
            };

            var oldConsents = new List<Consent>
            {
                new Consent { Email = email, IsWithdrawn = false },
                new Consent { Email = email, IsWithdrawn = false }
            };

            var systemPurposes = new List<ExternalSystemPurpose>
            {
                new ExternalSystemPurpose { PurposeId = purposeId }
            };

            _externalSystemPurposeRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<ExternalSystemPurpose, bool>>>()))
                .ReturnsAsync(systemPurposes);

            _consentRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Consent, bool>>>()))
                .ReturnsAsync(oldConsents);

            var newConsent = new Consent { Email = email };
            _mapperMock.Setup(x => x.Map<Consent>(submitConsentVM)).Returns(newConsent);

            // Act
            await _consentService.SubmitConsent(submitConsentVM);

            // Assert
            _consentRepositoryMock.Verify(x => x.Update(It.Is<Consent>(c => c.IsWithdrawn == true)), Times.Exactly(oldConsents.Count));
            _consentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Consent>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
        }


        [Fact]
        public async Task SubmitConsent_WithNullEmail_ShouldSucceed()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var submitConsentVM = new SubmitConsentVM
            {
                Email = null,
                ExternalSystemId = Guid.NewGuid(),
                ConsentMethod = ConsentMethod.WebForm,
                ConsentIp = "127.0.0.1",
                ConsentUserAgent = "Test Browser",
                PrivacyPolicyId = Guid.NewGuid(),
                ConsentPurposes = new List<ConsentPurposeVM>
                {
                    new ConsentPurposeVM { PurposeId = purposeId }
                }
            };

            _externalSystemPurposeRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<ExternalSystemPurpose, bool>>>()))
                .ReturnsAsync(new List<ExternalSystemPurpose> { new ExternalSystemPurpose { PurposeId = purposeId } });

            var newConsent = new Consent();
            _mapperMock.Setup(x => x.Map<Consent>(submitConsentVM)).Returns(newConsent);

            // Act
            await _consentService.SubmitConsent(submitConsentVM);

            // Assert
            _consentRepositoryMock.Verify(x => x.AddAsync(newConsent), Times.Once);
        }

        [Fact]
        public async Task SubmitConsent_WithDuplicateSubmission_ShouldWithdrawAndAddNew()
        {
            // Arrange
            var email = "test@example.com";
            var purposeId = Guid.NewGuid();
            var submitConsentVM = new SubmitConsentVM
            {
                Email = email,
                ExternalSystemId = Guid.NewGuid(),
                ConsentMethod = ConsentMethod.WebForm,
                ConsentIp = "127.0.0.1",
                ConsentUserAgent = "Test Browser",
                PrivacyPolicyId = Guid.NewGuid(),
                ConsentPurposes = new List<ConsentPurposeVM>
                {
                    new ConsentPurposeVM { PurposeId = purposeId }
                }
            };

            var oldConsent = new Consent
            {
                Email = email,
                IsWithdrawn = false,
                ExternalSystemId = submitConsentVM.ExternalSystemId
            };
            var oldConsents = new List<Consent> { oldConsent };

            var systemPurposes = new List<ExternalSystemPurpose>
            {
                new ExternalSystemPurpose { PurposeId = purposeId }
            };

            _externalSystemPurposeRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<ExternalSystemPurpose, bool>>>()))
                .ReturnsAsync(systemPurposes);

            _consentRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Consent, bool>>>()))
                .ReturnsAsync(oldConsents);

            var newConsent = new Consent { Email = email };
            _mapperMock
                .Setup(x => x.Map<Consent>(submitConsentVM))
                .Returns(newConsent);

            // Act
            await _consentService.SubmitConsent(submitConsentVM);

            // Assert
            _consentRepositoryMock.Verify(x => x.Update(It.Is<Consent>(c => c.IsWithdrawn == true)), Times.Once);
            _consentRepositoryMock.Verify(x => x.AddAsync(It.Is<Consent>(c => c == newConsent)), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SubmitConsent_WithInvalidMapping_ShouldThrowException()
        {
            // Arrange
            var submitConsentVM = new SubmitConsentVM
            {
                Email = "test@example.com",
                ExternalSystemId = Guid.NewGuid(),
                ConsentMethod = ConsentMethod.WebForm,
                ConsentIp = "127.0.0.1",
                ConsentUserAgent = "Test Browser",
                PrivacyPolicyId = Guid.NewGuid(),
                ConsentPurposes = new List<ConsentPurposeVM>
                {
                    new ConsentPurposeVM { PurposeId = Guid.NewGuid() }
                }
            };

            _externalSystemPurposeRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<ExternalSystemPurpose, bool>>>()))
                .ReturnsAsync(new List<ExternalSystemPurpose> { new ExternalSystemPurpose() });

            _mapperMock.Setup(x => x.Map<Consent>(submitConsentVM)).Throws<Exception>();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _consentService.SubmitConsent(submitConsentVM));
        }

        [Fact]
        public async Task SubmitConsent_WithMissingSystemId_ShouldThrowException()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var submitConsentVM = new SubmitConsentVM
            {
                Email = "test@example.com",
                ConsentMethod = ConsentMethod.WebForm,
                ConsentIp = "127.0.0.1",
                ConsentUserAgent = "Test Browser",
                PrivacyPolicyId = Guid.NewGuid(),
                ConsentPurposes = new List<ConsentPurposeVM>
                {
                    new ConsentPurposeVM { PurposeId = purposeId }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _consentService.SubmitConsent(submitConsentVM));
        }

        [Fact]
        public async Task SubmitConsent_WithEmptyEmail_ShouldSucceed()
        {
            // Arrange
            var purposeId = Guid.NewGuid();
            var submitConsentVM = new SubmitConsentVM
            {
                Email = "",
                ExternalSystemId = Guid.NewGuid(),
                ConsentMethod = ConsentMethod.WebForm,
                ConsentIp = "127.0.0.1",
                ConsentUserAgent = "Test Browser",
                PrivacyPolicyId = Guid.NewGuid(),
                ConsentPurposes = new List<ConsentPurposeVM>
                {
                    new ConsentPurposeVM { PurposeId = purposeId }
                }
            };

            var systemPurposes = new List<ExternalSystemPurpose>
            {
                new ExternalSystemPurpose
                {
                    PurposeId = purposeId,
                    ExternalSystemId = submitConsentVM.ExternalSystemId
                }
            };

            _externalSystemPurposeRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<ExternalSystemPurpose, bool>>>()))
                .ReturnsAsync(systemPurposes);

            var newConsent = new Consent();
            _mapperMock.Setup(x => x.Map<Consent>(submitConsentVM)).Returns(newConsent);

            // Act
            await _consentService.SubmitConsent(submitConsentVM);

            // Assert
            _consentRepositoryMock.Verify(x => x.AddAsync(newConsent), Times.Once);
        }

        [Fact]
        public async Task SubmitConsent_WithPartiallyValidPurposes_ShouldThrowException()
        {
            // Arrange
            var validPurposeId = Guid.NewGuid();
            var invalidPurposeId = Guid.NewGuid();
            var submitConsentVM = new SubmitConsentVM
            {
                Email = "test@example.com",
                ExternalSystemId = Guid.NewGuid(),
                ConsentMethod = ConsentMethod.WebForm,
                ConsentIp = "127.0.0.1",
                ConsentUserAgent = "Test Browser",
                PrivacyPolicyId = Guid.NewGuid(),
                ConsentPurposes = new List<ConsentPurposeVM>
                {
                    new ConsentPurposeVM { PurposeId = validPurposeId },
                    new ConsentPurposeVM { PurposeId = invalidPurposeId }
                }
            };

            _externalSystemPurposeRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<ExternalSystemPurpose, bool>>>()))
                .ReturnsAsync(new List<ExternalSystemPurpose>
                {
                    new ExternalSystemPurpose { PurposeId = validPurposeId }
                });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _consentService.SubmitConsent(submitConsentVM));
        }

        [Fact]
        public async Task DownloadImportTemplateAsync_WithValidSystem_ShouldReturnTemplateStream()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };
            var policy = new PrivacyPolicy { Id = Guid.NewGuid(), Status = PolicyStatus.Active };
            var purposes = new List<Purpose>
            {
                new Purpose { Id = Guid.NewGuid(), Name = "Purpose 1" },
                new Purpose { Id = Guid.NewGuid(), Name = "Purpose 2" }
            };

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            _policyServiceMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>()))
                .ReturnsAsync(new List<PrivacyPolicy> { policy });

            _externalSystemServiceMock
                .Setup(x => x.GetSystemPurposesAsync(systemId))
                .ReturnsAsync(purposes);

            // Act
            var result = await _consentService.DownloadImportTemplateAsync(systemId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CanRead);
            _externalSystemServiceMock.Verify(x => x.GetByIdAsync(systemId), Times.Once);
            _policyServiceMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>()), Times.Once);
            _externalSystemServiceMock.Verify(x => x.GetSystemPurposesAsync(systemId), Times.Once);
        }

        [Fact]
        public async Task DownloadImportTemplateAsync_WithInvalidSystem_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var invalidSystemId = Guid.NewGuid();
            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(invalidSystemId))
                .ReturnsAsync((ExternalSystem)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _consentService.DownloadImportTemplateAsync(invalidSystemId));
            _externalSystemServiceMock.Verify(x => x.GetByIdAsync(invalidSystemId), Times.Once);
            _policyServiceMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>()), Times.Never);
            _purposeServiceMock.Verify(x => x.GetAllAsync(), Times.Never);
        }

        [Fact]
        public async Task DownloadImportTemplateAsync_WithNoActivePolicy_ShouldThrowException()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };


            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            _policyServiceMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>()))
                .ReturnsAsync(new List<PrivacyPolicy>()); // Empty list means no active policy

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _consentService.DownloadImportTemplateAsync(systemId));
            Assert.Equal("There is no active privacy policy", exception.Message);
            _externalSystemServiceMock.Verify(x => x.GetByIdAsync(systemId), Times.Once);
            _policyServiceMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>()), Times.Once);
            _externalSystemServiceMock.Verify(x => x.GetSystemPurposesAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DownloadImportTemplateAsync_WithNoPurposes_ShouldReturnEmptyTemplate()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };
            var policy = new PrivacyPolicy { Id = Guid.NewGuid(), Status = PolicyStatus.Active };
            var emptyPurposes = new List<Purpose>();

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            _policyServiceMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>()))
                .ReturnsAsync(new List<PrivacyPolicy> { policy });

            _externalSystemServiceMock
                .Setup(x => x.GetSystemPurposesAsync(systemId))
                .ReturnsAsync(emptyPurposes);

            // Act
            var result = await _consentService.DownloadImportTemplateAsync(systemId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CanRead);
            _externalSystemServiceMock.Verify(x => x.GetByIdAsync(systemId), Times.Once);
            _policyServiceMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>()), Times.Once);
            _externalSystemServiceMock.Verify(x => x.GetSystemPurposesAsync(systemId), Times.Once);
        }

        [Fact]
        public async Task DownloadImportTemplateAsync_WithMissingTemplate_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var system = new ExternalSystem { Id = systemId, Name = "Test System" };
            var policy = new PrivacyPolicy { Id = Guid.NewGuid(), Status = PolicyStatus.Active };
            var purposes = new List<Purpose>
            {
                new Purpose { Id = Guid.NewGuid(), Name = "Purpose 1" }
            };

            _externalSystemServiceMock
                .Setup(x => x.GetByIdAsync(systemId))
                .ReturnsAsync(system);

            _policyServiceMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<PrivacyPolicy, bool>>>()))
                .ReturnsAsync(new List<PrivacyPolicy> { policy });

            _externalSystemServiceMock
                .Setup(x => x.GetSystemPurposesAsync(systemId))
                .ReturnsAsync(purposes);

            var templatePath = Path.Combine(AppContext.BaseDirectory, "ExcelTemplates", "ConsentImportTemplate.xlsx");

            // Cut the template file
            if (File.Exists(templatePath))
            {
                File.Copy(templatePath, templatePath.Replace(".xlsx", "_test.xlsx"));
                File.Delete(templatePath);
            }

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => _consentService.DownloadImportTemplateAsync(systemId));
            _externalSystemServiceMock.Verify(x => x.GetByIdAsync(systemId), Times.Once);

            // Restore the template file
            if (File.Exists(templatePath.Replace(".xlsx", "_test.xlsx")))
            {
                File.Copy(templatePath.Replace(".xlsx", "_test.xlsx"), templatePath);
                File.Delete(templatePath.Replace(".xlsx", "_test.xlsx"));
            }
        }


        [Fact]
        public async Task DoImportConsentAsync_WithValidImport_ShouldReturnOk()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var policyId = Guid.NewGuid();
            var purposeId = Guid.NewGuid();

            var validData = new List<ConsentImportVM>
            {
                new ConsentImportVM
                {
                    Email = "test1@example.com",
                    ConsentDate = DateTime.Now,
                    ExternalSystemId = systemId,
                    PrivacyPolicyId = policyId,
                    ConsentMethod = ConsentMethod.Email,
                    Purposes = new List<ConsentPurposeImportVM>
                    {
                        new ConsentPurposeImportVM { PurposeId = purposeId, Status = true }
                    }
                }
            };

            // Create Excel file using FlexCel
            var xls = new XlsFile(1, TExcelFileFormat.v2019, true);
            xls.NewFile(1);

            // Sheet 1: data sheet
            xls.ActiveSheet = 1;
            xls.SetCellValue(5, 4, purposeId.ToString()); // purpose header row

            // Add data row starting from row 6
            xls.SetCellValue(6, 1, validData[0].Email);
            xls.SetCellValue(6, 2, validData[0].ConsentMethod.ToString());
            xls.SetCellValue(6, 3, validData[0].ConsentDate.ToString("yyyy-MM-dd"));
            xls.SetCellValue(6, 4, true); // status of purpose

            // Sheet 2: contains systemId and policyId
            xls.InsertAndCopySheets(1, 2, 1); // create new sheet 2
            xls.ActiveSheet = 2;
            xls.SetCellValue("A1", policyId.ToString()); // PolicyId
            xls.SetCellValue("A2", systemId.ToString()); // SystemId

            // Reset active sheet back to Sheet 1
            xls.ActiveSheet = 1;

            var memoryStream = new MemoryStream();
            xls.Save(memoryStream);
            memoryStream.Position = 0;

            // Mocks
            _externalSystemServiceMock
                .Setup(x => x.GetSystemPurposesAsync(systemId))
                .ReturnsAsync(new List<Purpose>
                {
            new Purpose { Id = purposeId, Name = "Purpose 1" }
                });

            _mapperMock
                .Setup(x => x.Map<List<Consent>>(It.IsAny<List<ConsentImportVM>>()))
                .Returns(new List<Consent> { new Consent { Email = validData[0].Email } });

            _consentRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Consent>()))
                .ReturnsAsync(new Consent { Email = validData[0].Email });

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _consentService.DoImportConsentAsync(memoryStream);

            // Assert
            Assert.True(result.IsSuccess);
            _mapperMock.Verify(x => x.Map<List<Consent>>(It.IsAny<List<ConsentImportVM>>()), Times.Once);
            _consentRepositoryMock.Verify(x => x.BulkAddAsync(It.IsAny<List<Consent>>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }


        [Fact]
        public async Task DoImportConsentAsync_WithMalformedStream_ShouldThrowException()
        {
            // Arrange
            var garbageData = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var memoryStream = new MemoryStream(garbageData);

            // Act & Assert
            await Assert.ThrowsAsync<FlexCelXlsAdapterException>(() => _consentService.DoImportConsentAsync(memoryStream));
            _mapperMock.Verify(x => x.Map<List<ConsentImportVM>>(It.IsAny<object>()), Times.Never);
            _consentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Consent>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DoImportConsentAsync_WithMissingSystemId_ShouldThrowException()
        {
            // Arrange
            var invalidData = new List<ConsentImportVM>
            {
                new ConsentImportVM
                {
                    Email = "test1@example.com",
                    ConsentDate = DateTime.Now,
                    ExternalSystemId = Guid.Empty, // Missing system ID
                    Purposes = new List<ConsentPurposeImportVM>()
                }
            };

            // Create Excel file using FlexCel
            var xls = new XlsFile(1, TExcelFileFormat.v2019, true);
            xls.NewFile(1);

            // Sheet 1: data sheet
            xls.ActiveSheet = 1;
            xls.SetCellValue(5, 4, ""); // empty purpose header row

            // Add data row starting from row 6
            xls.SetCellValue(6, 1, invalidData[0].Email);
            xls.SetCellValue(6, 2, ConsentMethod.Email.ToString());
            xls.SetCellValue(6, 3, invalidData[0].ConsentDate.ToString("yyyy-MM-dd"));
            xls.SetCellValue(6, 4, true); // status of purpose

            // Sheet 2: contains systemId and policyId
            xls.InsertAndCopySheets(1, 2, 1); // create new sheet 2
            xls.ActiveSheet = 2;
            xls.SetCellValue("A1", Guid.NewGuid().ToString()); // PolicyId
            xls.SetCellValue("A2", Guid.Empty.ToString()); // Empty SystemId

            // Reset active sheet back to Sheet 1
            xls.ActiveSheet = 1;

            var memoryStream = new MemoryStream();
            xls.Save(memoryStream);
            memoryStream.Position = 0;

            _mapperMock
                .Setup(x => x.Map<List<ConsentImportVM>>(It.IsAny<object>()))
                .Returns(invalidData);

            // Act & Assert
            await Assert.ThrowsAsync<IndexOutOfRangeException>(() => _consentService.DoImportConsentAsync(memoryStream));
            _consentRepositoryMock.Verify(x => x.BulkAddAsync(It.IsAny<List<Consent>>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DoImportConsentAsync_WithPurposesMismatch_ShouldThrowException()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var policyId = Guid.NewGuid();
            var purposeId = Guid.NewGuid();
            var invalidPurposeId = Guid.NewGuid();

            var invalidData = new List<ConsentImportVM>
            {
                new ConsentImportVM
                {
                    Email = "test1@example.com",
                    ConsentDate = DateTime.Now,
                    ExternalSystemId = systemId,
                    PrivacyPolicyId = policyId,
                    ConsentMethod = ConsentMethod.Email,
                    Purposes = new List<ConsentPurposeImportVM>
                    {
                        new ConsentPurposeImportVM { PurposeId = invalidPurposeId, Status = true } // Invalid purpose ID
                    }
                }
            };

            // Create Excel file using FlexCel
            var xls = new XlsFile(1, TExcelFileFormat.v2019, true);
            xls.NewFile(1);

            // Sheet 1: data sheet
            xls.ActiveSheet = 1;
            xls.SetCellValue(5, 4, invalidPurposeId.ToString()); // purpose header row

            // Add data row starting from row 6
            xls.SetCellValue(6, 1, invalidData[0].Email);
            xls.SetCellValue(6, 2, invalidData[0].ConsentMethod.ToString());
            xls.SetCellValue(6, 3, invalidData[0].ConsentDate.ToString("yyyy-MM-dd"));
            xls.SetCellValue(6, 4, true); // status of purpose

            // Sheet 2: contains systemId and policyId
            xls.InsertAndCopySheets(1, 2, 1); // create new sheet 2
            xls.ActiveSheet = 2;
            xls.SetCellValue("A1", policyId.ToString()); // PolicyId
            xls.SetCellValue("A2", systemId.ToString()); // SystemId

            // Reset active sheet back to Sheet 1
            xls.ActiveSheet = 1;

            var memoryStream = new MemoryStream();
            xls.Save(memoryStream);
            memoryStream.Position = 0;

            // _mapperMock
            //     .Setup(x => x.Map<List<ConsentImportVM>>(It.IsAny<object>()))
            //     .Returns(invalidData);

            _externalSystemServiceMock
                .Setup(x => x.GetSystemPurposesAsync(systemId))
                .ReturnsAsync(new List<Purpose>
                {
                    new Purpose { Id = purposeId, Name = "Purpose 1" }
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _consentService.DoImportConsentAsync(memoryStream));

            Assert.Contains("Purpose ID", exception.Message);

            _externalSystemServiceMock.Verify(x => x.GetSystemPurposesAsync(systemId));
            _consentRepositoryMock.Verify(x => x.BulkAddAsync(It.IsAny<List<Consent>>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }





    }
}

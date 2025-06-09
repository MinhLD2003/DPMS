using AutoMapper;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.ViewModels.DSAR;
using DPMS_WebAPI.ViewModels.ExternalSystem;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using System.Text;
using FlexCel.XlsAdapter;
using FlexCel.Core;

namespace DPMS_WebAPI.Tests.UnitTests.Services
{
    public class DSARServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IDsarRepository> _dsarRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<DsarService>> _loggerMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IExternalSystemRepository> _systemReposMock;
        private readonly DsarService _dsarService;

        public DSARServiceTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _dsarRepositoryMock = new Mock<IDsarRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<DsarService>>();
            _userServiceMock = new Mock<IUserService>();
            _systemReposMock = new Mock<IExternalSystemRepository>();

            _dsarService = new DsarService(
                _unitOfWorkMock.Object,
                _dsarRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _userServiceMock.Object,
                _systemReposMock.Object
            );
        }

        [Fact]
        public async Task BulkUpdatetStatus_WithValidData_ShouldUpdateStatus()
        {
            // Arrange
            var dsarId = Guid.NewGuid();
            var updateVMs = new List<UpdateStatusVM>
            {
                new UpdateStatusVM { Id = dsarId, Status = DSARStatus.Completed }
            };

            var existingDsar = new DSAR { Id = dsarId, Status = DSARStatus.Submitted };
            _dsarRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<DSAR, bool>>>()))
                .ReturnsAsync(new List<DSAR> { existingDsar });

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _dsarService.BulkUpdatetStatus(updateVMs);

            // Assert
            _dsarRepositoryMock.Verify(x => x.Update(It.IsAny<DSAR>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task BulkUpdatetStatus_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            var updateVMs = new List<UpdateStatusVM>
            {
                new UpdateStatusVM { Id = Guid.NewGuid(), Status = DSARStatus.Completed }
            };

            _dsarRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<DSAR, bool>>>()))
                .ReturnsAsync(new List<DSAR>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _dsarService.BulkUpdatetStatus(updateVMs));
        }

        [Fact]
        public async Task ChangeStatus_WithOverdueDsars_ShouldUpdateStatus()
        {
            // Arrange
            var overdueDsars = new List<DSAR>
            {
                new DSAR { Id = Guid.NewGuid(), Status = DSARStatus.Submitted, RequiredResponse = DateTime.UtcNow.AddDays(-1) }
            };

            _dsarRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<DSAR, bool>>>()))
                .ReturnsAsync(overdueDsars);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _dsarService.ChangeStatus();

            // Assert
            _dsarRepositoryMock.Verify(x => x.Update(It.Is<DSAR>(d => d.Status == DSARStatus.RequiredReponse)), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ChangeStatus_WithNoOverdueDsars_ShouldNotUpdate()
        {
            // Arrange
            _dsarRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<DSAR, bool>>>()))
                .ReturnsAsync(new List<DSAR>());

            // Act
            await _dsarService.ChangeStatus();

            // Assert
            _dsarRepositoryMock.Verify(x => x.Update(It.IsAny<DSAR>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ChangeStatus_WithSomeOverdue_ShouldUpdateStatus()
        {
            // Arrange
            var overdueDsarId = Guid.NewGuid();
            var overdueDsar = new DSAR
            {
                Id = overdueDsarId,
                Status = DSARStatus.Submitted,
                RequiredResponse = DateTime.UtcNow.AddMinutes(-1) // Past the due date
            };

            _dsarRepositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<DSAR, bool>>>()))
                .ReturnsAsync(new List<DSAR> { overdueDsar });

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _dsarService.ChangeStatus();

            // Assert
            // Verify the Update was called on DSAR
            _dsarRepositoryMock.Verify(
                x => x.Update(It.Is<DSAR>(d => d.Status == DSARStatus.RequiredReponse)),
                Times.Once);

            // Verify the logger called LogInformation with the expected message
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Updated DSAR ID: {overdueDsarId} to 'Required Response'")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify SaveChangesAsync is called
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }


        [Fact]
        public async Task ChangeStatus_WithNoOverdue_ShouldLogNoUpdates()
        {
            // Arrange
            _dsarRepositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<DSAR, bool>>>()))
                .ReturnsAsync(new List<DSAR>());

            // Act
            await _dsarService.ChangeStatus();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No overdue DSARs found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }


        [Fact]
        public async Task ChangeStatus_WithAlreadyUpdated_ShouldNotUpdateAgain()
        {
            // Arrange
            var alreadyUpdatedDsars = new List<DSAR>
            {
                new DSAR { Id = Guid.NewGuid(), Status = DSARStatus.RequiredReponse, RequiredResponse = DateTime.UtcNow.AddDays(-1) }
            };

            _dsarRepositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<DSAR, bool>>>()))
                .ReturnsAsync(new List<DSAR>()); // Should be filtered out

            // Act
            await _dsarService.ChangeStatus();

            // Assert
            _dsarRepositoryMock.Verify(r => r.Update(It.IsAny<DSAR>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ChangeStatus_WhenRepoThrows_ShouldCatchAndLogError()
        {
            // Arrange
            var exception = new Exception("DB error");

            _dsarRepositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<DSAR, bool>>>()))
                .ThrowsAsync(exception);

            // Act
            await _dsarService.ChangeStatus();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error while updating DSAR status.")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ChangeStatus_WithMixedValidAndInvalid_ShouldProcessValidOnly()
        {
            // Arrange
            var valid = new DSAR { Id = Guid.NewGuid(), Status = DSARStatus.Submitted, RequiredResponse = DateTime.UtcNow.AddDays(-1) };
            var invalid = new DSAR { Id = Guid.NewGuid(), Status = DSARStatus.Submitted, RequiredResponse = DateTime.MaxValue }; // Not overdue

            var dsars = new List<DSAR> { valid, invalid };

            _dsarRepositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<DSAR, bool>>>()))
                .ReturnsAsync(dsars.Where(d => d.RequiredResponse < DateTime.UtcNow).ToList());

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _dsarService.ChangeStatus();

            // Assert
            _dsarRepositoryMock.Verify(r => r.Update(valid), Times.Once);
            _dsarRepositoryMock.Verify(r => r.Update(invalid), Times.Never);
        }


        [Fact]
        public async Task DownloadImportTemplate_WithValidUser_ShouldReturnStream()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                UserName = "testuser",
                FullName = "Test User"
            };
            var systems = new List<ExternalSystemVM>
            {
                new ExternalSystemVM
                {
                    Id = Guid.NewGuid(),
                    Name = "Test System",
                    Description = "Test System Description",
                    Domain = "test.com"
                }
            };

            _userServiceMock
                .Setup(x => x.GetManageSystems(user.Email))
                .ReturnsAsync(systems);

            // Create a proper Excel file
            var excelFile = new XlsFile(true);
            excelFile.NewFile(1);
            excelFile.SetCellValue(1, 1, "Email");
            excelFile.SetCellValue(1, 2, "ConsentDate");
            excelFile.SetCellValue(1, 3, "ExternalSystemId");
            excelFile.SetCellValue(1, 4, "Purposes");

            var templatePath = "ExcelTemplates/template_import_dsar.xlsx";
            Directory.CreateDirectory("ExcelTemplates");
            using (var fileStream = File.Create(templatePath))
            {
                excelFile.Save(fileStream);
            }

            // Act
            var result = await _dsarService.DownloadImportTemplate(user);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CanRead);

            // Cleanup
            // File.Delete(templatePath);
            // Thread.Sleep(100); // Give the system time to release the file handle
            // Directory.Delete("ExcelTemplates", true);
        }

        [Fact]
        public async Task DownloadImportTemplate_WithNullUser_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _dsarService.DownloadImportTemplate(null));
        }

        [Fact]
        public async Task DownloadImportTemplate_WithMissingTemplateFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                UserName = "testuser",
                FullName = "Test User"
            };

            var templateFilePath = "ExcelTemplates/template_import_dsar.xlsx";

            // Ensure the directory exists, but remove the file to simulate it missing
            Directory.CreateDirectory("ExcelTemplates");
            if (File.Exists(templateFilePath))
            {
                File.Delete(templateFilePath);
            }

            _userServiceMock
                .Setup(x => x.GetManageSystems(user.Email))
                .ReturnsAsync(new List<ExternalSystemVM>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => _dsarService.DownloadImportTemplate(user));
            Assert.Equal("Import template not found", exception.Message);
            // Cleanup
            // Directory.Delete("ExcelTemplates", true);
        }

        [Fact]
        public async Task DownloadImportTemplate_WithNoManagedSystems_ShouldReturnEmptySheet()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                UserName = "testuser",
                FullName = "Test User"
            };

            var systems = new List<ExternalSystemVM>(); // No systems to manage

            _userServiceMock
                .Setup(x => x.GetManageSystems(user.Email))
                .ReturnsAsync(systems);

            // Create a proper Excel file
            var excelFile = new XlsFile(true);
            excelFile.NewFile(1);
            excelFile.SetCellValue(1, 1, "Email");
            excelFile.SetCellValue(1, 2, "ConsentDate");
            excelFile.SetCellValue(1, 3, "ExternalSystemId");
            excelFile.SetCellValue(1, 4, "Purposes");

            var templatePath = "ExcelTemplates/template_import_dsar.xlsx";
            Directory.CreateDirectory("ExcelTemplates");
            using (var fileStream = File.Create(templatePath))
            {
                excelFile.Save(fileStream);
            }

            // Act
            var result = await _dsarService.DownloadImportTemplate(user);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CanRead);

            // Verify that the sheet is empty (apart from the header)
            var excel = new XlsFile();
            excel.Open(result);
            var rowCount = excel.RowCount;
            Assert.Equal(1, rowCount);  // Only header row should be present

            // Cleanup
            // File.Delete(templatePath);
            // Thread.Sleep(100); // Give the system time to release the file handle
            // Directory.Delete("ExcelTemplates", true);
        }

        [Fact]
        public async Task DownloadImportTemplate_ShouldDisposeStreamCorrectly()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                UserName = "testuser",
                FullName = "Test User"
            };

            var systems = new List<ExternalSystemVM>
            {
                new ExternalSystemVM
                {
                    Id = Guid.NewGuid(),
                    Name = "Test System",
                    Description = "Test System Description",
                    Domain = "test.com"
                }
            };

            _userServiceMock
                .Setup(x => x.GetManageSystems(user.Email))
                .ReturnsAsync(systems);

            // Create a proper Excel file
            var excelFile = new XlsFile(true);
            excelFile.NewFile(1);
            excelFile.SetCellValue(1, 1, "Email");
            excelFile.SetCellValue(1, 2, "ConsentDate");
            excelFile.SetCellValue(1, 3, "ExternalSystemId");
            excelFile.SetCellValue(1, 4, "Purposes");

            var templatePath = "ExcelTemplates/template_import_dsar.xlsx";
            Directory.CreateDirectory("ExcelTemplates");
            using (var fileStream = File.Create(templatePath))
            {
                excelFile.Save(fileStream);
            }

            // Act
            var result = await _dsarService.DownloadImportTemplate(user);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CanRead);

            // Check that the stream is disposed
            result.Dispose();
            // Ensure that result is a Stream type
            Assert.IsType<MemoryStream>(result); // Check if result is a Stream

            // After disposing, CanRead should return false
            Assert.False(result.CanRead);

        }


        [Fact]
        public async Task DoImportDsarAsync_WithValidData_ShouldImportSuccessfully()
        {
            // Arrange
            var systemId = Guid.NewGuid();
            var importData = new List<DsarImportVM>
            {
                new DsarImportVM
                {
                    RequesterName = "Test User",
                    RequesterEmail = "test@example.com",
                    TypeStr = "Access",
                    StatusStr = "Submitted",
                    ExternalSystemName = "Test System",
                    RequiredResponseStr = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd")
                }
            };

            _systemReposMock
                .Setup(x => x.GetIdByName(It.IsAny<string>()))
                .ReturnsAsync(systemId);

            _mapperMock
                .Setup(x => x.Map<List<DSAR>>(It.IsAny<List<DsarImportVM>>()))
                .Returns(new List<DSAR> { new DSAR() });

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Create Excel file
            var excelFile = new XlsFile(true);
            excelFile.NewFile(1);
            excelFile.SetCellValue(8, 1, importData[0].RequesterName);
            excelFile.SetCellValue(8, 2, importData[0].RequesterEmail);
            excelFile.SetCellValue(8, 3, importData[0].TypeStr);
            excelFile.SetCellValue(8, 4, importData[0].StatusStr);
            excelFile.SetCellValue(8, 5, importData[0].RequiredResponseStr);
            excelFile.SetCellValue(8, 6, importData[0].ExternalSystemName);

            var memoryStream = new MemoryStream();
            excelFile.Save(memoryStream);
            memoryStream.Position = 0;

            // Act
            var result = await _dsarService.DoImportDsarAsync(memoryStream);

            // Assert
            Assert.True(result.IsSuccess);
            _mapperMock.Verify(x => x.Map<List<DSAR>>(It.IsAny<List<DsarImportVM>>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DoImportDsarAsync_WithInvalidData_ShouldReturnFail()
        {
            // Arrange
            var importData = new List<DsarImportVM>
            {
                new DsarImportVM
                {
                    RequesterName = "Test User",
                    RequesterEmail = "test@example.com",
                    TypeStr = "InvalidType",
                    StatusStr = "InvalidStatus",
                    ExternalSystemName = "NonExistentSystem"
                }
            };

            _systemReposMock
                .Setup(x => x.GetIdByName(It.IsAny<string>()))
                .ReturnsAsync((Guid?)null);

            // Create Excel file with invalid data
            var excelFile = new XlsFile(true);
            excelFile.NewFile(1);
            excelFile.SetCellValue(8, 1, importData[0].RequesterName);
            excelFile.SetCellValue(8, 2, importData[0].RequesterEmail);
            excelFile.SetCellValue(8, 3, importData[0].TypeStr);
            excelFile.SetCellValue(8, 4, importData[0].StatusStr);
            excelFile.SetCellValue(8, 5, importData[0].ExternalSystemName);

            var memoryStream = new MemoryStream();
            excelFile.Save(memoryStream);
            memoryStream.Position = 0;

            // Act
            var result = await _dsarService.DoImportDsarAsync(memoryStream);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Import failed", result.Errors[0].Message);
        }

        [Fact]
        public async Task DoImportDsarAsync_WithAllInvalidRows_ShouldReturnFail()
        {
            // Arrange
            var importData = new List<DsarImportVM>
            {
                new DsarImportVM
                {
                    RequesterName = "Invalid User",
                    RequesterEmail = "invalid@example.com",
                    TypeStr = "InvalidType", // Invalid type
                    StatusStr = "InvalidStatus", // Invalid status
                    ExternalSystemName = "NonExistentSystem"
                }
            };

            _systemReposMock
                .Setup(x => x.GetIdByName(It.IsAny<string>()))
                .ReturnsAsync((Guid?)null); // Simulate non-existent system for all rows

            // Create Excel file with invalid data
            var excelFile = new XlsFile(true);
            excelFile.NewFile(1);
            excelFile.SetCellValue(8, 1, importData[0].RequesterName);
            excelFile.SetCellValue(8, 2, importData[0].RequesterEmail);
            excelFile.SetCellValue(8, 3, importData[0].TypeStr);
            excelFile.SetCellValue(8, 4, importData[0].StatusStr);
            excelFile.SetCellValue(8, 5, importData[0].RequiredResponseStr);
            excelFile.SetCellValue(8, 6, importData[0].ExternalSystemName);

            var memoryStream = new MemoryStream();
            excelFile.Save(memoryStream);
            memoryStream.Position = 0;

            // Act
            var result = await _dsarService.DoImportDsarAsync(memoryStream);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Import failed", result.Errors[0].Message);
        }

        [Fact]
        public async Task DoImportDsarAsync_WithNullStream_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _dsarService.DoImportDsarAsync(null));
        }

        [Fact]
        public async Task DoImportDsarAsync_WithCorruptedExcel_ShouldThrowError()
        {
            // Arrange
            var memoryStream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02 }); // Corrupted data

            // Act & Assert
            await Assert.ThrowsAsync<EndOfStreamException>(() => _dsarService.DoImportDsarAsync(memoryStream));
        }


        [Fact]
        public async Task BulkUpdatetStatus_WithValidUpdate_ShouldUpdateAllDSARs()
        {
            // Arrange
            var dsarIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var updateVMs = new List<UpdateStatusVM>
            {
                new UpdateStatusVM { Id = dsarIds[0], Status = DSARStatus.Completed },
                new UpdateStatusVM { Id = dsarIds[1], Status = DSARStatus.Rejected }
            };

            var existingDsars = new List<DSAR>
            {
                new DSAR { Id = dsarIds[0], Status = DSARStatus.Submitted },
                new DSAR { Id = dsarIds[1], Status = DSARStatus.Submitted }
            };

            _dsarRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<DSAR, bool>>>()))
                .ReturnsAsync(existingDsars);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(2);

            _mapperMock.Setup(m => m.Map(updateVMs[0], existingDsars[0]))
                       .Callback<UpdateStatusVM, DSAR>((src, dest) => dest.Status = src.Status);

            _mapperMock.Setup(m => m.Map(updateVMs[1], existingDsars[1]))
                       .Callback<UpdateStatusVM, DSAR>((src, dest) => dest.Status = src.Status);

            // Act
            await _dsarService.BulkUpdatetStatus(updateVMs);

            // Assert
            _dsarRepositoryMock.Verify(x => x.Update(It.Is<DSAR>(d => d.Status == DSARStatus.Completed)), Times.Once);
            _dsarRepositoryMock.Verify(x => x.Update(It.Is<DSAR>(d => d.Status == DSARStatus.Rejected)), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }


        [Fact]
        public async Task BulkUpdatetStatus_WithEmptyInput_ShouldNotUpdate()
        {
            // Arrange
            var emptyList = new List<UpdateStatusVM>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _dsarService.BulkUpdatetStatus(emptyList));

            _dsarRepositoryMock.Verify(x => x.Update(It.IsAny<DSAR>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task BulkUpdatetStatus_WithSomeInvalidIds_ShouldUpdateValidOnes()
        {
            // Arrange
            var validId = Guid.NewGuid();
            var invalidId = Guid.NewGuid();
            var updateVMs = new List<UpdateStatusVM>
            {
                new UpdateStatusVM { Id = validId, Status = DSARStatus.Completed },
                new UpdateStatusVM { Id = invalidId, Status = DSARStatus.Rejected }
            };

            var existingDsar = new List<DSAR>
            {
                new DSAR { Id = validId, Status = DSARStatus.Submitted }
            };

            _dsarRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<DSAR, bool>>>()))
                .ReturnsAsync(existingDsar);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _dsarService.BulkUpdatetStatus(updateVMs);

            // Assert
            _dsarRepositoryMock.Verify(x => x.Update(It.Is<DSAR>(d => d.Id == validId)), Times.Once);
            _dsarRepositoryMock.Verify(x => x.Update(It.Is<DSAR>(d => d.Id == invalidId)), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task BulkUpdatetStatus_WithAllInvalidIds_ShouldThrowException()
        {
            // Arrange
            var invalidIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var updateVMs = new List<UpdateStatusVM>
            {
                new UpdateStatusVM { Id = invalidIds[0], Status = DSARStatus.Completed },
                new UpdateStatusVM { Id = invalidIds[1], Status = DSARStatus.Rejected }
            };

            _dsarRepositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<DSAR, bool>>>()))
                .ReturnsAsync(new List<DSAR>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _dsarService.BulkUpdatetStatus(updateVMs));
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }


        [Fact]
        public async Task BulkUpdatetStatus_WithNullInput_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _dsarService.BulkUpdatetStatus(null));
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }
    }
}

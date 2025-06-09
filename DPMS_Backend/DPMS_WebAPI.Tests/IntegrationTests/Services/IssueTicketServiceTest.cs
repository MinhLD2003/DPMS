using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using Microsoft.AspNetCore.Http;

namespace DPMS_WebAPI.Tests.IntegrationTests.Services
{
    public class IssueTicketServiceTest : ServiceTestEnvironment
    {
        private readonly IIssueTicketService _issueTicketService;
        private Guid _testTicketId;
        private List<IssueTicketDocument> _initialDocuments;
        public IssueTicketServiceTest() : base()
        {
            _issueTicketService = new IssueTicketService(_unitOfWork, _mapper, _fileRepository);
            SetupTestData().Wait();
        }
        #region Setup
        private IFormFile CreateTestFile(string fileName, string contentType)
        {
            var content = $"Test content for {fileName}";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            return new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        private IFormFile CreateLargeTestFile(string fileName, string contentType, long sizeInBytes)
        {
            var stream = new MemoryStream();
            // Create a file of specified size
            stream.SetLength(sizeInBytes);
            stream.Position = 0;

            return new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }
        #endregion

        #region Create
        [Fact]
        public async Task CreateIssueTicket_WithMultipleFiles_ShouldSaveAllFilesCorrectly()
        {
            // Arrange
            var ticket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "Multiple Files Ticket",
                Description = "Issue ticket with multiple files of different types",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending,
                ExternalSystemId = Guid.NewGuid()
            };

            var files = new List<IFormFile>
            {
                CreateTestFile("document.pdf", "application/pdf"),
                CreateTestFile("spreadsheet.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
                CreateTestFile("image.png", "image/png")
            };

            // Act
            var result = await _issueTicketService.CreateIssueTicket(ticket, files);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ticket.Title, result.Title);
            Assert.Equal(ticket.TicketType, result.TicketType);
            Assert.Equal(3, result.Documents.Count);

            // Check each file type was handled correctly
            Assert.Contains(result.Documents, d => d.Title == "document.pdf" && d.FileFormat == "pdf");
            Assert.Contains(result.Documents, d => d.Title == "spreadsheet.xlsx" && d.FileFormat == "xlsx");
            Assert.Contains(result.Documents, d => d.Title == "image.png" && d.FileFormat == "png");

            // Verify correct paths were generated
            foreach (var doc in result.Documents)
            {
                Assert.StartsWith($"{DocumentType.IssueTicket}/{ticket.Id}/", doc.FileUrl);
                Assert.Equal(ticket.Id, doc.IssueTicketId);
            }
        }

        [Fact]
        public async Task CreateIssueTicket_WithSingleFile_ShouldSaveFileCorrectly()
        {
            // Arrange
            var ticket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "Single File Ticket",
                Description = "Issue ticket with a single file",
                TicketType = TicketType.Risk,
                IssueTicketStatus = IssueTicketStatus.Accept,
                ExternalSystemId = Guid.NewGuid()
            };

            var files = new List<IFormFile>
            {
                CreateTestFile("report.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            };

            // Act
            var result = await _issueTicketService.CreateIssueTicket(ticket, files);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ticket.Title, result.Title);
            Assert.Equal(ticket.TicketType, result.TicketType);
            Assert.Single(result.Documents);

            var singleDoc = result.Documents.First();
            Assert.Equal("report.docx", singleDoc.Title);
            Assert.Equal("docx", singleDoc.FileFormat);
            Assert.StartsWith($"{DocumentType.IssueTicket}/{ticket.Id}/", singleDoc.FileUrl);
            Assert.Equal(ticket.Id, singleDoc.IssueTicketId);
        }

        [Fact]
        public async Task CreateIssueTicket_WithNoFiles_ShouldCreateTicketWithoutDocuments()
        {
            // Arrange
            var ticket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "No Files Ticket",
                Description = "Issue ticket without any files",
                TicketType = TicketType.Violation,
                IssueTicketStatus = IssueTicketStatus.Reject,
                ExternalSystemId = Guid.NewGuid()
            };

            var files = new List<IFormFile>();

            // Act
            var result = await _issueTicketService.CreateIssueTicket(ticket, files);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ticket.Title, result.Title);
            Assert.Equal(ticket.Description, result.Description);
            Assert.Equal(ticket.TicketType, result.TicketType);
            Assert.Empty(result.Documents);
        }

        [Fact]
        public async Task CreateIssueTicket_WithLargeFile_ShouldHandleCorrectly()
        {
            // Arrange
            var ticket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "Large File Ticket",
                Description = "Issue ticket with a large file",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending,
                ExternalSystemId = Guid.NewGuid()
            };

            // Create a large file (10MB)
            var largeFile = CreateLargeTestFile("large.pdf", "application/pdf", 10 * 1024 * 1024);
            var files = new List<IFormFile> { largeFile };

            // Act
            var result = await _issueTicketService.CreateIssueTicket(ticket, files);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Documents);
            Assert.Equal("large.pdf", result.Documents.First().Title);
        }



        [Fact]
        public async Task CreateIssueTicket_WithInvalidFileType_ShouldThrowException()
        {
            // Arrange
            var ticket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "Invalid File Type Ticket",
                Description = "Testing handling of unsupported file types",
                TicketType = TicketType.Risk,
                IssueTicketStatus = IssueTicketStatus.Pending,
                ExternalSystemId = Guid.NewGuid()
            };
            // Create a file with an unusual extension
            var unusualFile = CreateTestFile("unusual.xyz", "application/octet-stream");
            var files = new List<IFormFile> { unusualFile };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _issueTicketService.CreateIssueTicket(ticket, files));

            Assert.Contains("invalid extension", exception.Message);
        }
        #endregion

        #region Update
        private async Task SetupTestData()
        {
            // Create a test ticket with initial documents
            var ticket = new IssueTicket
            {
                Id = Guid.NewGuid(),
                Title = "Test Ticket for Update",
                Description = "Testing file update functionality",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending,
                ExternalSystemId = Guid.NewGuid()
            };

            var initialFiles = new List<IFormFile>
            {
                CreateTestFile("document1.pdf", "application/pdf"),
                CreateTestFile("image1.png", "image/png"),
                CreateTestFile("spreadsheet1.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            };

            // Create the test ticket with initial files
            var createdTicket = await _issueTicketService.CreateIssueTicket(ticket, initialFiles);
            _testTicketId = createdTicket.Id;
            _initialDocuments = createdTicket.Documents.ToList();
        }
        [Fact]
        public async Task UpdateIssueTicketFilesOnS3_AddNewFilesOnly_ShouldAddFilesCorrectly()
        {
            // Arrange
            var newFiles = new List<IFormFile>
            {
                CreateTestFile("newdocument.pdf", "application/pdf"),
                CreateTestFile("newimage.jpeg", "image/jpeg")
            };

            var removedFiles = new List<string>(); // No files to remove

            // Act
            var result = await _issueTicketService.UpdateIssueTicketFilesOnS3(_testTicketId, newFiles, removedFiles);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_initialDocuments.Count + newFiles.Count, result.Count);

            // Verify original files still exist
            foreach (var originalDoc in _initialDocuments)
            {
                Assert.Contains(result, d => d.Title == originalDoc.Title);
            }

            // Verify new files were added
            Assert.Contains(result, d => d.Title == "newdocument.pdf" && d.FileFormat == "pdf");
            Assert.Contains(result, d => d.Title == "newimage.jpeg" && d.FileFormat == "jpeg");
        }


        [Fact]
        public async Task UpdateIssueTicketFilesOnS3_RemoveFilesOnly_ShouldRemoveFilesCorrectly()
        {
            // Arrange
            var newFiles = new List<IFormFile>(); // No new files

            // Get URLs of files to remove (first file in our initial set)
            var removedFiles = new List<string> { _initialDocuments[0].FileUrl };

            // Act
            var result = await _issueTicketService.UpdateIssueTicketFilesOnS3(_testTicketId, newFiles, removedFiles);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_initialDocuments.Count - 1, result.Count);

            // Verify removed file is no longer in the result
            Assert.DoesNotContain(result, d => d.FileUrl == _initialDocuments[0].FileUrl);

            // Verify remaining original files still exist
            for (int i = 1; i < _initialDocuments.Count; i++)
            {
                Assert.Contains(result, d => d.Title == _initialDocuments[i].Title);
            }
        }

        [Fact]
        public async Task UpdateIssueTicketFilesOnS3_AddAndRemoveFiles_ShouldUpdateCorrectly()
        {
            // Arrange
            var newFiles = new List<IFormFile>
            {
                CreateTestFile("replacement.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            };

            // Get URLs of files to remove (first file in our initial set)
            var removedFiles = new List<string> { _initialDocuments[0].FileUrl };

            // Act
            var result = await _issueTicketService.UpdateIssueTicketFilesOnS3(_testTicketId, newFiles, removedFiles);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_initialDocuments.Count, result.Count); // Removed 1, added 1

            // Verify removed file is no longer in the result
            Assert.DoesNotContain(result, d => d.FileUrl == _initialDocuments[0].FileUrl);

            // Verify new file was added
            Assert.Contains(result, d => d.Title == "replacement.docx" && d.FileFormat == "docx");

            // Verify remaining original files still exist
            for (int i = 1; i < _initialDocuments.Count; i++)
            {
                Assert.Contains(result, d => d.Title == _initialDocuments[i].Title);
            }
        }

        [Fact]
        public async Task UpdateIssueTicketFilesOnS3_NoChanges_ShouldReturnOriginalFiles()
        {
            // Arrange
            var newFiles = new List<IFormFile>(); 
            var removedFiles = new List<string>(); 

            // Act
            var result = await _issueTicketService.UpdateIssueTicketFilesOnS3(_testTicketId, newFiles, removedFiles);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_initialDocuments.Count, result.Count);

            // Verify all original files still exist
            foreach (var originalDoc in _initialDocuments)
            {
                Assert.Contains(result, d => d.Title == originalDoc.Title);
            }
        }

        [Fact]
        public async Task UpdateIssueTicketFilesOnS3_WithLargeFile_ShouldHandleCorrectly()
        {
            // Arrange
            var largeFile = CreateLargeTestFile("largefile.pdf", "application/pdf", 10 * 1024 * 1024);
            var newFiles = new List<IFormFile> { largeFile };
            var removedFiles = new List<string>(); // No files to remove

            // Act
            var result = await _issueTicketService.UpdateIssueTicketFilesOnS3(_testTicketId, newFiles, removedFiles);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_initialDocuments.Count + 1, result.Count);
            Assert.Contains(result, d => d.Title == "largefile.pdf");
        }

        [Fact]
        public async Task UpdateIssueTicketFilesOnS3_RemoveNonExistentFile_ShouldHandleGracefully()
        {
            // Arrange
            var newFiles = new List<IFormFile>(); // No new files
            var removedFiles = new List<string> { "NonExistentFile.pdf" }; // Non-existent file

            // Act
            var result = await _issueTicketService.UpdateIssueTicketFilesOnS3(_testTicketId, newFiles, removedFiles);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_initialDocuments.Count, result.Count); // Count should remain the same

            // Verify all original files still exist
            foreach (var originalDoc in _initialDocuments)
            {
                Assert.Contains(result, d => d.Title == originalDoc.Title);
            }
        }

        #endregion
    }
}
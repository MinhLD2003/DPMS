using DPMS_WebAPI.Controllers;
using DPMS_WebAPI.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace DPMS_WebAPI.Tests.UnitTests.Controllers
{
    public class FileControllerTests
    {
        private readonly Mock<IFileRepository> _mockFileRepository;
        private readonly FileController _controller;

        public FileControllerTests()
        {
            _mockFileRepository = new Mock<IFileRepository>();
            _controller = new FileController(_mockFileRepository.Object);
        }

        #region UploadFile Tests

        [Fact]
        public async Task UploadFile_WithValidFile_ReturnsOkResultWithFileUrl()
        {
            // Arrange
            var fileName = "testfile.txt";
            var content = "This is a test file";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var contentType = "text/plain";

            var file = new FormFile(stream, 0, stream.Length, "Data", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            var expectedFileUrl = "https://storage.example.com/files/some-guid.txt";

            _mockFileRepository.Setup(repo => repo.UploadFileAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(expectedFileUrl);

            // Act
            var result = await _controller.UploadFile(file);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            // Deserialize the result to access the fileUrl
            var resultDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                JsonSerializer.Serialize(okResult.Value));

            Assert.True(resultDict.ContainsKey("fileUrl"));
            Assert.Equal(expectedFileUrl, resultDict["fileUrl"].GetString());

            _mockFileRepository.Verify(repo => repo.UploadFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                contentType), Times.Once);
        }

        [Fact]
        public async Task UploadFile_WithNullFile_ReturnsBadRequest()
        {
            // Arrange
            IFormFile file = null;

            // Act
            var result = await _controller.UploadFile(file);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("File is empty");

            _mockFileRepository.Verify(repo => repo.UploadFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UploadFile_WithEmptyFile_ReturnsBadRequest()
        {
            // Arrange
            var emptyStream = new MemoryStream();
            var file = new FormFile(emptyStream, 0, 0, "Data", "empty.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            // Act
            var result = await _controller.UploadFile(file);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("File is empty");

            _mockFileRepository.Verify(repo => repo.UploadFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region DeleteFile Tests

        [Fact]
        public async Task DeleteFile_WithValidFileUrl_ReturnsOkResult()
        {
            // Arrange
            var fileUrl = "https://storage.example.com/files/some-guid.txt";

            _mockFileRepository.Setup(repo => repo.DeleteFileAsync(fileUrl))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteFile(fileUrl);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            // Fix for accessing anonymous object properties
            var resultDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                JsonSerializer.Serialize(okResult.Value));

            Assert.True(resultDict.ContainsKey("result"));
            Assert.True(resultDict["result"].GetBoolean());

            _mockFileRepository.Verify(repo => repo.DeleteFileAsync(fileUrl), Times.Once);
        }

        [Fact]
        public async Task DeleteFile_WithEmptyFileUrl_ReturnsBadRequest()
        {
            // Arrange
            string fileUrl = string.Empty;

            // Act
            var result = await _controller.DeleteFile(fileUrl);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("File URL is empty");

            _mockFileRepository.Verify(repo => repo.DeleteFileAsync(
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteFile_WithNullFileUrl_ReturnsBadRequest()
        {
            // Arrange
            string fileUrl = null;

            // Act
            var result = await _controller.DeleteFile(fileUrl);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("File URL is empty");

            _mockFileRepository.Verify(repo => repo.DeleteFileAsync(
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteFile_WhenRepositoryReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            var fileUrl = "https://storage.example.com/files/nonexistent.txt";

            _mockFileRepository.Setup(repo => repo.DeleteFileAsync(fileUrl))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteFile(fileUrl);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            // Fix for accessing anonymous object properties
            var resultDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                JsonSerializer.Serialize(okResult.Value));

            Assert.True(resultDict.ContainsKey("result"));
            Assert.False(resultDict["result"].GetBoolean());

            _mockFileRepository.Verify(repo => repo.DeleteFileAsync(fileUrl), Times.Once);
        }

        #endregion

        #region GetFile Tests

        [Fact]
        public async Task GetFile_WithValidFileName_ReturnsFileResult()
        {
            // Arrange
            var fileName = "testfile.txt";
            var fileContent = "This is a test file content";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            _mockFileRepository.Setup(repo => repo.GetFileAsync(fileName))
                .ReturnsAsync(stream);

            // Act
            var result = await _controller.GetFile(fileName);

            // Assert
            var fileResult = result.Should().BeOfType<FileStreamResult>().Subject;
            fileResult.FileDownloadName.Should().Be(fileName);
            fileResult.ContentType.Should().Be("application/octet-stream");

            _mockFileRepository.Verify(repo => repo.GetFileAsync(fileName), Times.Once);
        }

        [Fact]
        public async Task GetFile_WithNonExistentFile_ReturnsNotFound()
        {
            // Arrange
            var fileName = "nonexistent.txt";

            _mockFileRepository.Setup(repo => repo.GetFileAsync(fileName))
                .ReturnsAsync((Stream)null);

            // Act
            var result = await _controller.GetFile(fileName);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;

            // Fix for accessing anonymous object properties
            var resultDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                JsonSerializer.Serialize(notFoundResult.Value));

            Assert.True(resultDict.ContainsKey("message"));
            Assert.Equal("File not found", resultDict["message"].GetString());

            _mockFileRepository.Verify(repo => repo.GetFileAsync(fileName), Times.Once);
        }

        [Fact]
        public async Task GetFile_WithEmptyFileName_ReturnsNotFound()
        {
            // Arrange
            var fileName = string.Empty;

            _mockFileRepository.Setup(repo => repo.GetFileAsync(fileName))
                .ReturnsAsync((Stream)null);

            // Act
            var result = await _controller.GetFile(fileName);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;

            // Fix for accessing anonymous object properties
            var resultDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                JsonSerializer.Serialize(notFoundResult.Value));

            Assert.True(resultDict.ContainsKey("message"));
            Assert.Equal("File not found", resultDict["message"].GetString());

            _mockFileRepository.Verify(repo => repo.GetFileAsync(fileName), Times.Once);
        }

        #endregion
    }
}
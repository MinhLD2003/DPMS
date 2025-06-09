using AutoMapper;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.Tests.IntegrationTests;
using DPMS_WebAPI.ViewModels.Risk;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DPMS_WebAPI.Tests.UnitTests.Services
{
    public class UT_RiskServiceTest : ServiceTestEnvironment
    {
        private readonly IRiskService _riskService;
        private readonly Mock<IRiskRepository> _mockRiskRepository;
        private readonly Mock<IMapper> _mapper;
        private List<Risk> _testRisks;
        private string EXPORT_TEMPLATE_PATH = "ExcelTemplates/RiskExportTemplate.xlsx";


        public UT_RiskServiceTest() : base()
        {
            _mockRiskRepository = new Mock<IRiskRepository>();
            _mapper = new Mock<IMapper>();
            _riskService = new RiskService(_unitOfWork, _mockRiskRepository.Object, _mapper.Object);
            SetupTestData().Wait();
          
        }

        private async Task SetupTestData()
        {
            // Create test risks with various categories and strategies
            var user = new User
            {
                FullName = "Test User",
                UserName = "TestUser",
                Email = "Test@gmail.com"
            };

            _testRisks = new List<Risk>
            {
                new Risk {
                    Id = Guid.NewGuid(),
                    RiskName = "Technical System Failure",
                    Mitigation = "Implement redundant systems and failover mechanisms",
                    Category = RiskCategory.Technical,
                    RiskContingency = "Manual processes as backup",
                    Strategy = ResponseStrategy.Mitigate,
                    RiskImpact = 5,
                    RiskProbability = 3,
                    Priority = 15,
                    RiskOwner = "IT Manager",
                    CreatedBy = user,
                },
                new Risk {
                    Id =  Guid.NewGuid(),
                    RiskName = "Budget Overrun",
                    Mitigation = "Regular financial reviews and contingency budget",
                    Category = RiskCategory.Organizational,
                    RiskContingency = "Scope reduction plan",
                    Strategy = ResponseStrategy.Transfer,
                    RiskImpact = 4,
                    RiskProbability = 4,
                    Priority = 16,
                    RiskOwner = "Finance Director",
                        CreatedBy = user,
                },
                new Risk {
                    Id = Guid.NewGuid(),
                    RiskName = "Regulatory Compliance Issue",
                    Mitigation = "Regular compliance audits",
                    Category = RiskCategory.Quality,
                    RiskContingency = "Remediation plan",
                    Strategy = ResponseStrategy.Prevent,
                    RiskImpact = 5,
                    RiskProbability = 2,
                    Priority = 10,
                    RiskOwner = "Compliance Officer",

                }
            };

            // Setup repository with test data
            _mockRiskRepository.Setup(r => r.GetExportData())
                .ReturnsAsync(() => _testRisks);

            // Setup mapper
            _mapper.Setup(m => m.Map<List<ExportRiskVM>>(It.IsAny<List<Risk>>()))
                .Returns<List<Risk>>(risks => risks.Select(r => new ExportRiskVM
                {
                    Id = r.Id,
                    RiskName = r.RiskName,
                    Mitigation = r.Mitigation,
                    Category = r.Category.ToString(),
                    RiskContingency = r.RiskContingency,
                    Strategy = r.Strategy.ToString(),
                    RiskImpact = r.RiskImpact,
                    RiskProbability = r.RiskProbability,
                    Priority = r.Priority,
                    RiskImpactAfterMitigation = r.RiskImpactAfterMitigation,
                    RiskProbabilityAfterMitigation = r.RiskProbabilityAfterMitigation,
                    PriorityAfterMitigation = r.PriorityAfterMitigation,
                    RiskOwner = r.RiskOwner,
                    RaisedAt = r.RaisedAt
                }).ToList());

            // Ensure template directory exists
            string directory = Path.GetDirectoryName(EXPORT_TEMPLATE_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Verify the template exists or copy it from a known location
            if (!File.Exists(EXPORT_TEMPLATE_PATH))
            {
                throw new FileNotFoundException(
                    $"Risk export template not found at: {EXPORT_TEMPLATE_PATH}. " +
                    $"Please ensure the template exists in this location before running tests.");
            }
        }



        #region Export Tests
        [Fact]
        public async Task ExportAsync_WithDefaultRisks_ShouldReturnValidExcelStream()
        {
            // Act - uses the risks set up in SetupTestData
            var result = await _riskService.ExportAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0, "The stream should contain data");
            Assert.Equal(0, result.Position);

            // Verify the repository and mapper were called correctly
            _mockRiskRepository.Verify(r => r.GetExportData(), Times.Once);
            _mapper.Verify(m => m.Map<List<ExportRiskVM>>(It.IsAny<List<Risk>>()), Times.Once);
        }

        [Fact]
        public async Task ExportAsync_WithNoRisks_ShouldReturnValidEmptyExcelStream()
        {
            // Arrange - modify the test data to be empty
            var originalRisks = _testRisks.ToList(); // Save original state
            _testRisks.Clear(); // Modify the shared state to have no risks

            try
            {
                // Act
                var result = await _riskService.ExportAsync();

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Length > 0, "Even with no data, a valid Excel stream should be returned");
                Assert.Equal(0, result.Position);
            }
            finally
            {
                // Restore the original test data
                _testRisks.Clear();
                _testRisks.AddRange(originalRisks);
            }
        }

        [Fact]
        public async Task ExportAsync_TemplateNotFound_ShouldThrowFileNotFoundException()
        {
            // Arrange - temporarily rename the template file
            string backupPath = EXPORT_TEMPLATE_PATH + ".bak";
            if (File.Exists(EXPORT_TEMPLATE_PATH))
            {
                File.Move(EXPORT_TEMPLATE_PATH, backupPath);
            }

            try
            {
                // Act & Assert
                await Assert.ThrowsAsync<FileNotFoundException>(async () =>
                    await _riskService.ExportAsync());
            }
            finally
            {
                // Cleanup - restore the template file
                if (File.Exists(backupPath))
                {
                    File.Move(backupPath, EXPORT_TEMPLATE_PATH);
                }
            }
        }

        [Fact]
        public async Task ExportAsync_WithLargeNumberOfRisks_ShouldHandleCorrectly()
        {
            // Arrange - modify test data to have many risks
            var originalRisks = _testRisks.ToList(); // Save original state

            try
            {
                // Add many more risks to the collection
                var largeRiskSet = Enumerable.Range(1, 50)
                    .Select(i => new Risk
                    {
                        Id = Guid.NewGuid(), // Avoid ID conflicts with existing test data
                        RiskName = $"Risk {i}",
                        Category = (RiskCategory)(i % 7), // Cycle through risk categories
                        Strategy = (ResponseStrategy)(i % 5), // Cycle through strategies
                        RiskImpact = i % 5 + 1,
                        RiskProbability = i % 5 + 1,
                        Priority = (i % 5 + 1) * (i % 5 + 1),
                        RaisedAt = DateTime.Now.AddDays(-i)
                    });

                _testRisks.AddRange(largeRiskSet);

                // Act
                var result = await _riskService.ExportAsync();

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Length > 0);
                Assert.Equal(0, result.Position);
            }
            finally
            {
                // Restore original test data
                _testRisks.Clear();
                _testRisks.AddRange(originalRisks);
            }
        }
        #endregion

        #region Risk Category and Strategy Tests
        [Theory]
        [InlineData(RiskCategory.Technical, ResponseStrategy.Mitigate)]
        [InlineData(RiskCategory.Organizational, ResponseStrategy.Transfer)]
        [InlineData(RiskCategory.Scope, ResponseStrategy.Acceptance)]
        [InlineData(RiskCategory.Schedule, ResponseStrategy.Prevent)]
        [InlineData(RiskCategory.Usability, ResponseStrategy.Exploitation)]
        [InlineData(RiskCategory.Communication, ResponseStrategy.Mitigate)]
        [InlineData(RiskCategory.Quality, ResponseStrategy.Transfer)]
        public async Task ExportAsync_WithVariousRiskCategoriesAndStrategies_ShouldExportCorrectly(
            RiskCategory category, ResponseStrategy strategy)
        {
            // Arrange - modify test data to have a specific risk
            var originalRisks = _testRisks.ToList(); // Save original state

            try
            {
                // Replace with a single specific risk
                var specificRisk = new Risk
                {
                    Id = Guid.NewGuid(),
                    RiskName = $"Test {category} Risk with {strategy} Strategy",
                    Mitigation = "Test mitigation",
                    Category = category,
                    RiskContingency = "Test contingency",
                    Strategy = strategy,
                    RiskImpact = 3,
                    RiskProbability = 3,
                    Priority = 9,
                    RaisedAt = DateTime.Now
                };

                _testRisks.Clear();
                _testRisks.Add(specificRisk);

                // Act
                var result = await _riskService.ExportAsync();

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Length > 0);

                // Verify repository was called
                _mockRiskRepository.Verify(r => r.GetExportData(), Times.AtLeastOnce);
            }
            finally
            {
                // Restore original test data
                _testRisks.Clear();
                _testRisks.AddRange(originalRisks);
            }
        }
        #endregion

        #region Edge Cases
        [Fact]
        public async Task ExportAsync_WithRepositoryException_ShouldPropagateException()
        {
            // Arrange - temporarily change the repository setup to throw an exception
            var originalSetup = _mockRiskRepository.Setup(r => r.GetExportData());
            _mockRiskRepository.Setup(r => r.GetExportData())
                .ThrowsAsync(new InvalidOperationException("Database connection error"));

            try
            {
                // Act & Assert
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await _riskService.ExportAsync());

                Assert.Equal("Database connection error", exception.Message);
            }
            finally
            {
                // Restore original repository setup
                _mockRiskRepository.Setup(r => r.GetExportData())
                    .ReturnsAsync(() => _testRisks);
            }
        }

        [Fact]
        public async Task ExportAsync_WithMapperException_ShouldPropagateException()
        {
            // Arrange - temporarily change the mapper setup to throw an exception
            var originalSetup = _mapper.Setup(m => m.Map<List<ExportRiskVM>>(It.IsAny<List<Risk>>()));
            _mapper.Setup(m => m.Map<List<ExportRiskVM>>(It.IsAny<List<Risk>>()))
                .Throws(new AutoMapperMappingException("Mapping error"));

            try
            {
                // Act & Assert
                await Assert.ThrowsAsync<AutoMapperMappingException>(async () =>
                    await _riskService.ExportAsync());
            }
            finally
            {
                // Restore original mapper setup
                _mapper.Setup(m => m.Map<List<ExportRiskVM>>(It.IsAny<List<Risk>>()))
                    .Returns<List<Risk>>(risks => risks.Select(r => new ExportRiskVM
                    {
                        Id = r.Id,
                        RiskName = r.RiskName,
                        Mitigation = r.Mitigation,
                        Category = r.Category.ToString(),
                        RiskContingency = r.RiskContingency,
                        Strategy = r.Strategy.ToString(),
                        RiskImpact = r.RiskImpact,
                        RiskProbability = r.RiskProbability,
                        Priority = r.Priority,
                        RiskImpactAfterMitigation = r.RiskImpactAfterMitigation,
                        RiskProbabilityAfterMitigation = r.RiskProbabilityAfterMitigation,
                        PriorityAfterMitigation = r.PriorityAfterMitigation,
                        RiskOwner = r.RiskOwner,
                        RaisedAt = r.RaisedAt
                    }).ToList());
            }
        }
        #endregion
    }
}

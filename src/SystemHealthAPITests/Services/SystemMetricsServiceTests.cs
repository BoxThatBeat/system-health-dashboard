using Moq;
using SystemHealthAPI.Models;
using SystemHealthAPI.Services;

namespace SystemHealthAPITests.Services;

[TestClass]
public class SystemMetricsServiceTests
{
    private Mock<IDataAccessService> _mockDataAccessService;
    private SystemMetricsService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockDataAccessService = new Mock<IDataAccessService>();
        _service = new SystemMetricsService(_mockDataAccessService.Object);
    }

    [TestMethod]
    public async Task AddMetricsAsync_ReturnsTrue_WhenDataAccessServiceSucceeds()
    {
        // Arrange
        var metrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1234567890 }
        };
        
        _mockDataAccessService
            .Setup(x => x.AddMetricsAsync(It.IsAny<IEnumerable<OSMetric>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.AddMetricsAsync(metrics);

        // Assert
        Assert.IsTrue(result);
        _mockDataAccessService.Verify(x => x.AddMetricsAsync(It.IsAny<IEnumerable<OSMetric>>()), Times.Once);
    }

    [TestMethod]
    public async Task AddMetricsAsync_ReturnsFalse_WhenDataAccessServiceFails()
    {
        // Arrange
        var metrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1234567890 }
        };
        
        _mockDataAccessService
            .Setup(x => x.AddMetricsAsync(It.IsAny<IEnumerable<OSMetric>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.AddMetricsAsync(metrics);

        // Assert
        Assert.IsFalse(result);
        _mockDataAccessService.Verify(x => x.AddMetricsAsync(It.IsAny<IEnumerable<OSMetric>>()), Times.Once);
    }

    [TestMethod]
    public async Task AddMetricsAsync_PassesCorrectMetricsToDataAccessService()
    {
        // Arrange
        var metrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1234567890 },
            new() { Key = "memory_usage", Name = "Memory Usage", Value = 8192, Unit = MeasurementUnit.Bytes, Timestamp = 1234567891 }
        };
        
        _mockDataAccessService
            .Setup(x => x.AddMetricsAsync(It.IsAny<IEnumerable<OSMetric>>()))
            .ReturnsAsync(true);

        // Act
        await _service.AddMetricsAsync(metrics);

        // Assert
        _mockDataAccessService.Verify(x => x.AddMetricsAsync(
            It.Is<IEnumerable<OSMetric>>(m => 
                m.Count() == 2 && 
                m.First().Key == "cpu_usage" && 
                m.Last().Key == "memory_usage")), 
            Times.Once);
    }

    [TestMethod]
    public async Task RetrieveAllMetricsAsync_ReturnsMetrics_WhenDataAccessServiceReturnsMetrics()
    {
        // Arrange
        var expectedMetrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1234567890 },
            new() { Key = "memory_usage", Name = "Memory Usage", Value = 8192, Unit = MeasurementUnit.Bytes, Timestamp = 1234567891 }
        };
        
        _mockDataAccessService
            .Setup(x => x.RetrieveAllMetricsAsync())
            .ReturnsAsync(expectedMetrics);

        // Act
        var result = await _service.RetrieveAllMetricsAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("cpu_usage", resultList[0].Key);
        Assert.AreEqual("memory_usage", resultList[1].Key);
        
        _mockDataAccessService.Verify(x => x.RetrieveAllMetricsAsync(), Times.Once);
    }

    [TestMethod]
    public async Task RetrieveAllMetricsAsync_ReturnsEmptyList_WhenDataAccessServiceReturnsEmptyList()
    {
        // Arrange
        var expectedMetrics = new List<OSMetric>();
        
        _mockDataAccessService
            .Setup(x => x.RetrieveAllMetricsAsync())
            .ReturnsAsync(expectedMetrics);

        // Act
        var result = await _service.RetrieveAllMetricsAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        
        _mockDataAccessService.Verify(x => x.RetrieveAllMetricsAsync(), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task RetrieveAllMetricsAsync_ThrowsException_WhenDataAccessServiceThrowsException()
    {
        // Arrange
        _mockDataAccessService
            .Setup(x => x.RetrieveAllMetricsAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await _service.RetrieveAllMetricsAsync();
    }
}
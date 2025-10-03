using Microsoft.AspNetCore.Mvc;
using Moq;
using SystemHealthAPI.Controllers;
using SystemHealthAPI.Exceptions;
using SystemHealthAPI.Models;
using SystemHealthAPI.Services;

namespace SystemHealthAPITests.Controllers;

[TestClass]
public class SystemMetricsControllerTests
{
    private Mock<ISystemMetricsService> _mockSystemMetricsService;
    private SystemMetricsController _controller;

    [TestInitialize]
    public void Setup()
    {
        _mockSystemMetricsService = new Mock<ISystemMetricsService>();
        _controller = new SystemMetricsController(_mockSystemMetricsService.Object);
    }

    [TestMethod]
    public async Task GetAll_ReturnsAllMetrics_WhenMetricsExist()
    {
        // Arrange
        var expectedMetrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1234567890 },
            new() { Key = "memory_usage", Name = "Memory Usage", Value = 8192, Unit = MeasurementUnit.Bytes, Timestamp = 1234567891 }
        };
        
        _mockSystemMetricsService
            .Setup(x => x.RetrieveAllMetricsAsync())
            .ReturnsAsync(expectedMetrics);

        // Act
        var result = await _controller.GetAll();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("cpu_usage", resultList[0].Key);
        Assert.AreEqual("memory_usage", resultList[1].Key);
    }

    [TestMethod]
    public async Task GetAll_ReturnsEmptyList_WhenNoMetricsExist()
    {
        // Arrange
        var expectedMetrics = new List<OSMetric>();
        
        _mockSystemMetricsService
            .Setup(x => x.RetrieveAllMetricsAsync())
            .ReturnsAsync(expectedMetrics);

        // Act
        var result = await _controller.GetAll();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    [ExpectedException(typeof(MetricsNotAvailableException))]
    public async Task GetAll_ThrowsMetricsNotAvailableException_WhenExceptionOccurs()
    {
        // Arrange
        _mockSystemMetricsService
            .Setup(x => x.RetrieveAllMetricsAsync())
            .ThrowsAsync(new MetricsNotAvailableException("No metrics available"));

        // Act & Assert
        await _controller.GetAll();
    }

    [TestMethod]
    public async Task Post_ReturnsOk_WhenMetricsAddedSuccessfully()
    {
        // Arrange
        var metrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1234567890 }
        };
        
        _mockSystemMetricsService
            .Setup(x => x.AddMetricsAsync(It.IsAny<IEnumerable<OSMetric>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Post(metrics) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.AreEqual("Metrics added successfully.", result.Value);
    }

    [TestMethod]
    public async Task Post_ReturnsInternalServerError_WhenAddMetricsFails()
    {
        // Arrange
        var metrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1234567890 }
        };
        
        _mockSystemMetricsService
            .Setup(x => x.AddMetricsAsync(It.IsAny<IEnumerable<OSMetric>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Post(metrics) as ObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(500, result.StatusCode);
        Assert.AreEqual("An error occurred while adding metrics.", result.Value);
    }

    [TestMethod]
    public async Task Post_ReturnsBadRequest_WhenMetricsListIsNull()
    {
        // Act
        var result = await _controller.Post(null) as BadRequestObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Metrics list cannot be null or empty.", result.Value);
        
        _mockSystemMetricsService.Verify(x => x.AddMetricsAsync(It.IsAny<IEnumerable<OSMetric>>()), Times.Never);
    }

    [TestMethod]
    public async Task Post_ReturnsBadRequest_WhenMetricsListIsEmpty()
    {
        // Arrange
        var emptyMetrics = new List<OSMetric>();

        // Act
        var result = await _controller.Post(emptyMetrics) as BadRequestObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Metrics list cannot be null or empty.", result.Value);
        
        _mockSystemMetricsService.Verify(x => x.AddMetricsAsync(It.IsAny<IEnumerable<OSMetric>>()), Times.Never);
    }
}
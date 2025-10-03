using SystemHealthAPI.Exceptions;
using SystemHealthAPI.Models;
using SystemHealthAPI.Services;

namespace SystemHealthAPITests.Services;

[TestClass]
public class InMemoryDataAccessServiceTests
{
    private InMemoryDataAccessService _service;

    [TestInitialize]
    public void Setup()
    {
        _service = new InMemoryDataAccessService();
    }

    [TestMethod]
    public async Task AddMetricAsync_ReturnsTrue_WhenMetricIsAdded()
    {
        // Arrange
        var metric = new OSMetric
        {
            Key = "cpu_usage",
            Name = "CPU Usage",
            Value = 75.5,
            Total = 100,
            Unit = MeasurementUnit.Percentage,
            Timestamp = 1234567890
        };

        // Act
        var result = await _service.AddMetricAsync(metric);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task AddMetricsAsync_ReturnsTrue_WhenMetricsAreAdded()
    {
        // Arrange
        var metrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Total = 100, Unit = MeasurementUnit.Percentage, Timestamp = 1234567890 },
            new() { Key = "memory_usage", Name = "Memory Usage", Value = 8192, Total = 10000, Unit = MeasurementUnit.Bytes, Timestamp = 1234567891 }
        };

        // Act
        var result = await _service.AddMetricsAsync(metrics);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task RetrieveAllMetricsAsync_ReturnsAllMetrics_WhenMetricsExist()
    {
        // Arrange
        var metrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1234567890 },
            new() { Key = "memory_usage", Name = "Memory Usage", Value = 8192, Unit = MeasurementUnit.Bytes, Timestamp = 1234567891 }
        };
        
        await _service.AddMetricsAsync(metrics);

        // Act
        var result = await _service.RetrieveAllMetricsAsync();

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        
        var cpuMetric = resultList.FirstOrDefault(m => m.Key == "cpu_usage");
        var memoryMetric = resultList.FirstOrDefault(m => m.Key == "memory_usage");
        
        Assert.IsNotNull(cpuMetric);
        Assert.IsNotNull(memoryMetric);
        Assert.AreEqual(75.5, cpuMetric.Value);
        Assert.AreEqual(8192, memoryMetric.Value);
    }

    [TestMethod]
    [ExpectedException(typeof(MetricsNotAvailableException))]
    public async Task RetrieveAllMetricsAsync_ThrowsMetricsNotAvailableException_WhenNoMetricsExist()
    {
        // Act & Assert
        await _service.RetrieveAllMetricsAsync();
    }

    [TestMethod]
    public async Task RetrieveMetricsAsync_ReturnsMetricsInRange_WhenMetricsExistInRange()
    {
        // Arrange
        var metrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1000 },
            new() { Key = "memory_usage", Name = "Memory Usage", Value = 8192, Unit = MeasurementUnit.Bytes, Timestamp = 2000 },
            new() { Key = "disk_usage", Name = "Disk Usage", Value = 50.0, Unit = MeasurementUnit.Percentage, Timestamp = 3000 }
        };
        
        await _service.AddMetricsAsync(metrics);

        // Act
        var result = await _service.RetrieveMetricsAsync(1500, 2500);

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("memory_usage", resultList[0].Key);
        Assert.AreEqual(2000, resultList[0].Timestamp);
    }

    [TestMethod]
    public async Task RetrieveMetricsAsync_ReturnsEmptyList_WhenNoMetricsInRange()
    {
        // Arrange
        var metrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1000 }
        };
        
        await _service.AddMetricsAsync(metrics);

        // Act
        var result = await _service.RetrieveMetricsAsync(2000, 3000);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    [ExpectedException(typeof(MetricsNotAvailableException))]
    public async Task RetrieveMetricsAsync_ThrowsMetricsNotAvailableException_WhenNoMetricsExist()
    {
        // Act & Assert
        await _service.RetrieveMetricsAsync(1000, 2000);
    }

    [TestMethod]
    public async Task RetrieveMetricsAsync_ReturnsMetricsAtBoundaries_WhenTimestampsMatchBoundaries()
    {
        // Arrange
        var metrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1000 },
            new() { Key = "memory_usage", Name = "Memory Usage", Value = 8192, Unit = MeasurementUnit.Bytes, Timestamp = 2000 }
        };
        
        await _service.AddMetricsAsync(metrics);

        // Act
        var result = await _service.RetrieveMetricsAsync(1000, 2000);

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
    }

    [TestMethod]
    public async Task ClearAllMetricsAsync_ReturnsTrue_AndClearsAllMetrics()
    {
        // Arrange
        var metrics = new List<OSMetric>
        {
            new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1234567890 }
        };
        
        await _service.AddMetricsAsync(metrics);

        // Act
        var result = await _service.ClearAllMetricsAsync();

        // Assert
        Assert.IsTrue(result);
        
        // Verify metrics are cleared
        await Assert.ThrowsExceptionAsync<MetricsNotAvailableException>(
            async () => await _service.RetrieveAllMetricsAsync());
    }

    [TestMethod]
    public async Task ClearAllMetricsAsync_ReturnsTrue_WhenNoMetricsExist()
    {
        // Act
        var result = await _service.ClearAllMetricsAsync();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task AddMetric_AndRetrieve_WorksConcurrently()
    {
        // Arrange
        var tasks = new List<Task>();
        var metricsToAdd = 100;

        // Act - Add metrics concurrently
        for (int i = 0; i < metricsToAdd; i++)
        {
            var index = i;
            tasks.Add(_service.AddMetricAsync(new OSMetric
            {
                Key = $"metric_{index}",
                Name = $"Metric {index}",
                Value = index,
                Unit = MeasurementUnit.Percentage,
                Timestamp = index
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        var allMetrics = await _service.RetrieveAllMetricsAsync();
        Assert.AreEqual(metricsToAdd, allMetrics.Count());
    }
}
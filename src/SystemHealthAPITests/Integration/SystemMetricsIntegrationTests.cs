using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SystemHealthAPI.Models;

namespace SystemHealthAPITests.Integration;

[TestClass]
public class SystemMetricsIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public SystemMetricsIntegrationTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [TestInitialize]
    public void Initialize()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task PostAndGetMetrics_WorksEndToEnd()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var metrics = new List<OSMetric>
        {
            new()
            {
                Key = "cpu_usage",
                Name = "CPU Usage",
                Value = 75.5,
                Total = 100,
                Unit = MeasurementUnit.Percentage,
                Timestamp = timestamp
            },
            new()
            {
                Key = "memory_usage",
                Name = "Memory Usage",
                Value = 8192,
                Total = 16384,
                Unit = MeasurementUnit.Bytes,
                Timestamp = timestamp + 1000
            }
        };

        // Act - Post metrics
        var postResponse = await _client.PostAsJsonAsync("/SystemMetrics", metrics);
        Assert.AreEqual(HttpStatusCode.OK, postResponse.StatusCode);

        // Act - Get metrics
        var getResponse = await _client.GetAsync("/SystemMetrics");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

        var responseContent = await getResponse.Content.ReadAsStringAsync();
        var returnedMetrics = JsonSerializer.Deserialize<List<OSMetric>>(responseContent, _jsonOptions);

        Assert.IsNotNull(returnedMetrics);
        Assert.AreEqual(metrics.Count, returnedMetrics.Count);
    }


    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}
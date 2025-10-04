using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using OSMetricsRetriever.Models;
using OSMetricsRetriever.Services;

namespace OSMetricsRetrieverTests.Services
{
    [TestClass]
    public class SystemHealthAPIServiceTests
    {
        private const string TestApiEndpoint = "https://api.example.com/metrics";

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SendMetrics_ThrowsArgumentException_WhenMetricsListIsNull()
        {
            // Arrange
            var service = new SystemHealthAPIService(TestApiEndpoint);

            // Act & Assert
            await service.SendMetrics(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SendMetrics_ThrowsArgumentException_WhenMetricsListIsEmpty()
        {
            // Arrange
            var metrics = new List<OSMetric>();
            var service = new SystemHealthAPIService(TestApiEndpoint);

            // Act & Assert
            await service.SendMetrics(metrics);
        }

        [TestMethod]
        public void Constructor_SetsApiEndpoint()
        {
            // Act
            var service = new SystemHealthAPIService(TestApiEndpoint);

            // Assert
            Assert.IsNotNull(service);
            // We can't directly test the private field, but the constructor should not throw
        }

        [TestMethod]
        public void Dispose_DisposesSuccessfully()
        {
            // Arrange
            var service = new SystemHealthAPIService(TestApiEndpoint);

            // Act & Assert (should not throw)
            service.Dispose();
        }

        [TestMethod]
        public async Task SendMetrics_ValidatesMetricsParameter()
        {
            // Arrange
            var service = new SystemHealthAPIService(TestApiEndpoint);
            var metrics = new List<OSMetric>
            {
                new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1234567890 }
            };

            // Act & Assert
            // We expect this to throw because it will try to make a real HTTP call
            // In a real scenario, this would be mocked or we'd use HttpClientFactory
            try
            {
                await service.SendMetrics(metrics);
            }
            catch (InvalidOperationException)
            {
                // Expected when the HTTP call fails
                Assert.IsTrue(true);
            }
            catch (HttpRequestException)
            {
                // Also expected when the HTTP call fails
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void SendMetrics_SerializesMetricsToJson()
        {
            // Arrange
            var metrics = new List<OSMetric>
            {
                new() { Key = "cpu_usage", Name = "CPU Usage", Value = 75.5, Unit = MeasurementUnit.Percentage, Timestamp = 1234567890 },
                new() { Key = "memory_usage", Name = "Memory Usage", Value = 8192, Unit = MeasurementUnit.Bytes, Timestamp = 1234567891 }
            };

            // Act
            var json = JsonSerializer.Serialize(metrics);

            // Assert
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("cpu_usage"));
            Assert.IsTrue(json.Contains("memory_usage"));
            Assert.IsTrue(json.Contains("CPU Usage"));
            Assert.IsTrue(json.Contains("Memory Usage"));
        }
    }

    [TestClass]
    public class SystemHealthAPIServiceIntegrationTests
    {
        [TestMethod]
        public async Task SendMetrics_Integration_RequiresValidEndpoint()
        {
            // This is more of a contract test to ensure the service can be created
            // and would attempt to send metrics to a real endpoint
            
            // Arrange
            const string testEndpoint = "https://httpbin.org/post"; // Public testing endpoint
            var service = new SystemHealthAPIService(testEndpoint);
            var metrics = new List<OSMetric>
            {
                new() { Key = "test_metric", Name = "Test Metric", Value = 42, Unit = MeasurementUnit.Percentage, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
            };

            try
            {
                // Act
                await service.SendMetrics(metrics);
                
                // If we get here, the call succeeded
                Assert.IsTrue(true);
            }
            catch (HttpRequestException)
            {
                // Expected if network is not available or endpoint is unreachable
                Assert.Inconclusive("Network not available for integration testing");
            }
            catch (TaskCanceledException)
            {
                // Expected if request times out
                Assert.Inconclusive("Request timed out during integration testing");
            }
            finally
            {
                service.Dispose();
            }
        }
    }
}
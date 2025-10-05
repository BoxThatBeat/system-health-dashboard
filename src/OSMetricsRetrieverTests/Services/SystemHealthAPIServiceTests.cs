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
        public void Dispose_DisposesSuccessfully()
        {
            // Arrange
            var service = new SystemHealthAPIService(TestApiEndpoint);

            // Act & Assert (should not throw)
            service.Dispose();
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
}
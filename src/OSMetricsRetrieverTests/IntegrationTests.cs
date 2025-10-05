using OSMetricsRetriever.Models;
using OSMetricsRetriever.Services;
using OSMetricsRetriever.MetricsPlugins;

namespace OSMetricsRetrieverTests
{
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        [TestCategory("Integration")]
        public void CompleteWorkflow_CollectAndPrepareMetricsForTransmission()
        {
            try
            {
                // Arrange
                var metricsService = new MetricsRetrieverService.MetricsReceiverServiceBuilder()
                    .AddPlugin<CPUUtilizationPlugin>()
                    .AddPlugin<MemoryUsagePlugin>()
                    .AddPlugin<StorageUsagePlugin>()
                    .Build();

                // Act
                var metrics = metricsService.CollectMetrics();

                // Assert
                Assert.IsNotNull(metrics);
                
                // Each metric should have the required properties
                foreach (var metric in metrics)
                {
                    Assert.AreNotEqual("", metric.Name);
                    Assert.AreNotEqual("", metric.Key);
                    Assert.IsTrue(metric.Timestamp > 0);
                    Assert.IsTrue(Enum.IsDefined(typeof(MeasurementUnit), metric.Unit));
                }

                // Demonstrate that metrics can be serialized for API transmission
                var json = System.Text.Json.JsonSerializer.Serialize(metrics);
                Assert.IsNotNull(json);
                Assert.IsTrue(json.Length > 0);

                // Demonstrate that they can be deserialized back
                var deserializedMetrics = System.Text.Json.JsonSerializer.Deserialize<List<OSMetric>>(json);
                Assert.IsNotNull(deserializedMetrics);
                Assert.AreEqual(metrics.Count, deserializedMetrics.Count);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is System.Management.ManagementException)
            {
                Assert.Inconclusive("WMI access not available for end-to-end testing: " + ex.Message);
            }
        }
    }
}
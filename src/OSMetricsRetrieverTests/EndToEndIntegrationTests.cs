using OSMetricsRetriever.Models;
using OSMetricsRetriever.Services;
using OSMetricsRetriever.MetricsPlugins;

namespace OSMetricsRetrieverTests
{
    /// <summary>
    /// Integration tests that verify the complete workflow of the OSMetricsRetriever solution.
    /// These tests use real implementations and validate end-to-end functionality.
    /// </summary>
    [TestClass]
    public class EndToEndIntegrationTests
    {
        [TestMethod]
        public void CompleteWorkflow_CollectAndPrepareMetricsForTransmission()
        {
            try
            {
                // Arrange - Build a service with all plugins
                var metricsService = new MetricsRetrieverService.MetricsReceiverServiceBuilder()
                    .AddPlugin<CPUUtilizationPlugin>()
                    .AddPlugin<MemoryUsagePlugin>()
                    .AddPlugin<StorageUsagePlugin>()
                    .Build();

                // Act - Collect metrics
                var metrics = metricsService.CollectMetrics();

                // Assert - Verify we got some metrics
                Assert.IsNotNull(metrics);
                
                // Each metric should have the required properties
                foreach (var metric in metrics)
                {
                    Assert.IsNotNull(metric.Key);
                    Assert.IsNotNull(metric.Name);
                    Assert.IsTrue(metric.Timestamp > 0);
                    Assert.IsTrue(Enum.IsDefined(typeof(MeasurementUnit), metric.Unit));
                    
                    // Verify metric values are reasonable
                    Assert.IsTrue(metric.Value >= 0);
                    if (metric.Total.HasValue)
                    {
                        Assert.IsTrue(metric.Total.Value > 0);
                        Assert.IsTrue(metric.Value <= metric.Total.Value);
                    }
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

        [TestMethod]
        public void ServiceBuilder_CanCreateServiceWithVariousPluginCombinations()
        {
            // Test various combinations of plugins
            var testCombinations = new[]
            {
                new[] { typeof(CPUUtilizationPlugin) },
                new[] { typeof(MemoryUsagePlugin) },
                new[] { typeof(StorageUsagePlugin) },
                new[] { typeof(CPUUtilizationPlugin), typeof(MemoryUsagePlugin) },
                new[] { typeof(CPUUtilizationPlugin), typeof(StorageUsagePlugin) },
                new[] { typeof(MemoryUsagePlugin), typeof(StorageUsagePlugin) },
                new[] { typeof(CPUUtilizationPlugin), typeof(MemoryUsagePlugin), typeof(StorageUsagePlugin) }
            };

            foreach (var combination in testCombinations)
            {
                var builder = new MetricsRetrieverService.MetricsReceiverServiceBuilder();
                
                foreach (var pluginType in combination)
                {
                    if (pluginType == typeof(CPUUtilizationPlugin))
                        builder.AddPlugin<CPUUtilizationPlugin>();
                    else if (pluginType == typeof(MemoryUsagePlugin))
                        builder.AddPlugin<MemoryUsagePlugin>();
                    else if (pluginType == typeof(StorageUsagePlugin))
                        builder.AddPlugin<StorageUsagePlugin>();
                }

                var service = builder.Build();
                Assert.IsNotNull(service);
                Assert.AreEqual(combination.Length, builder.Plugins.Count);
            }
        }

        [TestMethod]
        public void SystemHealthAPIService_CanBeInstantiatedWithValidEndpoint()
        {
            // Arrange & Act
            var service = new SystemHealthAPIService("https://api.example.com/metrics");

            // Assert
            Assert.IsNotNull(service);
            
            // Cleanup
            service.Dispose();
        }

        [TestMethod]
        public void AllPlugins_HaveExpectedStaticProperties()
        {
            // Verify all plugins have their Name property correctly set
            Assert.AreEqual("CPU Utilization", CPUUtilizationPlugin.Name);
            Assert.AreEqual("Memory Usage", MemoryUsagePlugin.Name);
            Assert.AreEqual("Storage Usage", StorageUsagePlugin.Name);
        }
    }
}
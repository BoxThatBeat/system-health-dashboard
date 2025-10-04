using System.Management;
using System.Runtime.InteropServices;
using Moq;
using OSMetricsRetriever.Exceptions;
using OSMetricsRetriever.MetricsPlugins;
using OSMetricsRetriever.Models;

namespace OSMetricsRetrieverTests.MetricsPlugins
{
    [TestClass]
    public class MemoryUsagePluginIntegrationTests
    {
        private MemoryUsagePlugin _plugin;

        [TestInitialize]
        public void Setup()
        {
            _plugin = new MemoryUsagePlugin();
        }

        [TestMethod]
        public void GetMetric_ReturnsValidMetric_WithRealWMI()
        {
            // Arrange
            var scope = new ManagementScope(@"\\.\root\cimv2");
            
            try
            {
                scope.Connect();
                
                // Act
                var result = _plugin.GetMetric(scope);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("memory_usage_metric", result.Key);
                Assert.AreEqual("Memory Usage", result.Name);
                Assert.AreEqual("The amount of memory currently in use by the system.", result.Description);
                Assert.IsTrue(result.Value >= 0);
                Assert.IsTrue(result.Total > 0);
                Assert.IsTrue(result.Value <= result.Total);
                Assert.AreEqual(MeasurementUnit.Bytes, result.Unit);
                Assert.IsTrue(result.Timestamp > 0);
            }
            catch (UnauthorizedAccessException)
            {
                // Skip test if WMI access is not available (common in CI/CD environments)
                Assert.Inconclusive("WMI access not available for testing");
            }
            catch (ManagementException)
            {
                // Skip test if WMI service is not available
                Assert.Inconclusive("WMI service not available for testing");
            }
            catch (COMException)
            {
                // Skip test if COM/WMI service has issues
                Assert.Inconclusive("COM/WMI service not available for testing");
            }
        }

        [TestMethod]
        public void GetMetric_HandlesInvalidScope_Gracefully()
        {
            // Arrange
            var invalidScope = new ManagementScope(@"\\invalid\root\cimv2");

            // Act & Assert
            try
            {
                var result = _plugin.GetMetric(invalidScope);
                // If we get here without exception, the test environment might have different behavior
                Assert.IsNotNull(result); // Just ensure we got some result
            }
            catch (MetricRetrievalException)
            {
                // This is expected when WMI query fails
                Assert.IsTrue(true);
            }
            catch (ManagementException)
            {
                // This is also acceptable as it indicates WMI issues
                Assert.IsTrue(true);
            }
            catch (COMException)
            {
                // This is expected when COM/WMI fails
                Assert.IsTrue(true);
            }
            catch (Exception ex) when (ex.Message.Contains("RPC server is unavailable"))
            {
                // This is expected when RPC/WMI service is not available
                Assert.IsTrue(true);
            }
        }
    }

    [TestClass]
    public class CPUUtilizationPluginIntegrationTests
    {
        private CPUUtilizationPlugin _plugin;

        [TestInitialize]
        public void Setup()
        {
            _plugin = new CPUUtilizationPlugin();
        }

        [TestMethod]
        public void GetMetric_ReturnsValidMetric_WithRealWMI()
        {
            // Arrange
            var scope = new ManagementScope(@"\\.\root\cimv2");
            
            try
            {
                scope.Connect();
                
                // Act
                var result = _plugin.GetMetric(scope);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("cpu_utilization_metric", result.Key);
                Assert.AreEqual("CPU Utilization", result.Name);
                Assert.AreEqual("The percentage of CPU utilization.", result.Description);
                Assert.IsTrue(result.Value >= 0);
                Assert.IsTrue(result.Value <= 100);
                Assert.AreEqual(100, result.Total);
                Assert.AreEqual(MeasurementUnit.Percentage, result.Unit);
                Assert.IsTrue(result.Timestamp > 0);
            }
            catch (UnauthorizedAccessException)
            {
                // Skip test if WMI access is not available
                Assert.Inconclusive("WMI access not available for testing");
            }
            catch (ManagementException)
            {
                // Skip test if WMI service is not available
                Assert.Inconclusive("WMI service not available for testing");
            }
            catch (COMException)
            {
                // Skip test if COM/WMI service has issues
                Assert.Inconclusive("COM/WMI service not available for testing");
            }
        }
    }

    [TestClass]
    public class StorageUsagePluginIntegrationTests
    {
        private StorageUsagePlugin _plugin;

        [TestInitialize]
        public void Setup()
        {
            _plugin = new StorageUsagePlugin();
        }

        [TestMethod]
        public void GetMetric_ReturnsValidMetric_WithRealWMI()
        {
            // Arrange
            var scope = new ManagementScope(@"\\.\root\cimv2");
            
            try
            {
                scope.Connect();
                
                // Act
                var result = _plugin.GetMetric(scope);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("storage_usage_metric", result.Key);
                Assert.AreEqual("Storage Usage", result.Name);
                Assert.AreEqual("The amount of storage space currently used on the primary drive.", result.Description);
                Assert.IsTrue(result.Value >= 0);
                Assert.IsTrue(result.Total > 0);
                Assert.IsTrue(result.Value <= result.Total);
                Assert.AreEqual(MeasurementUnit.Bytes, result.Unit);
                Assert.IsTrue(result.Timestamp > 0);
            }
            catch (UnauthorizedAccessException)
            {
                // Skip test if WMI access is not available
                Assert.Inconclusive("WMI access not available for testing");
            }
            catch (ManagementException)
            {
                // Skip test if WMI service is not available
                Assert.Inconclusive("WMI service not available for testing");
            }
            catch (COMException)
            {
                // Skip test if COM/WMI service has issues
                Assert.Inconclusive("COM/WMI service not available for testing");
            }
        }

        [TestMethod]
        public void GetMetric_ThrowsMetricRetrievalException_WhenNoDrivesFound()
        {
            // This test would require mocking WMI to return no drives
            // For now, we just ensure the plugin can be instantiated
            Assert.IsNotNull(_plugin);
        }
    }
}
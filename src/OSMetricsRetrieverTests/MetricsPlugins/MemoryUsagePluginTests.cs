using OSMetricsRetriever.MetricsPlugins;
using Moq;
using OSMetricsRetriever.Providers;
using System.Management;
using OSMetricsRetriever.Exceptions;
using OSMetricsRetriever.Models;

namespace OSMetricsRetrieverTests.MetricsPlugins
{
    [TestClass]
    public class MemoryUsagePluginTests
    {
        private MemoryUsagePlugin _plugin = null!;
        private Mock<IWMIProvider> _wmiProviderMock = null!;

        [TestInitialize]
        public void Setup()
        {
            _plugin = new MemoryUsagePlugin();
            _wmiProviderMock = new Mock<IWMIProvider>();
        }

        [TestMethod]
        public void Name_ReturnsCorrectValue()
        {
            // Assert
            Assert.AreEqual("Memory Usage", MemoryUsagePlugin.Name);
        }

        [TestMethod]
        public void GetMetric_UsesCorrectWMIQuery()
        {
            // Arrange
            ObjectQuery? capturedQuery = null;
            
            _wmiProviderMock
                .Setup(p => p.QueryWMI(It.IsAny<ObjectQuery>()))
                .Callback<ObjectQuery>(query => capturedQuery = query)
                .Returns(new List<ManagementObject>());

            // Act
            try
            {
                _plugin.GetMetric(_wmiProviderMock.Object);
            }
            catch (MetricRetrievalException)
            {
                // Expected when empty list is returned - ignore for this test
            }

            // Assert
            Assert.IsNotNull(capturedQuery);
            Assert.IsTrue(capturedQuery.QueryString.Contains("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"));
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void GetMetric_ThrowsNullReferenceException_WhenProviderIsNull()
        {
            // Act & Assert - The plugin doesn't validate null input, so it throws NullReferenceException
            _plugin.GetMetric(null!);
        }

        [TestMethod]
        [ExpectedException(typeof(MetricRetrievalException))]
        public void GetMetric_ThrowsMetricRetrievalException_WhenNoMemoryInfoFound()
        {
            // Arrange
            _wmiProviderMock
                .Setup(p => p.QueryWMI(It.IsAny<ObjectQuery>()))
                .Returns(new List<ManagementObject>());

            // Act & Assert - The plugin throws MetricRetrievalException when no memory info is found
            _plugin.GetMetric(_wmiProviderMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ManagementException))]
        public void GetMetric_ThrowsManagementException_WhenWMIProviderThrowsException()
        {
            // Arrange
            _wmiProviderMock
                .Setup(p => p.QueryWMI(It.IsAny<ObjectQuery>()))
                .Throws(new ManagementException("WMI query failed"));

            // Act & Assert - The plugin doesn't wrap exceptions, so ManagementException is thrown directly
            _plugin.GetMetric(_wmiProviderMock.Object);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetMetric_ReturnsValidMetric_WhenRunAgainstRealSystem()
        {
            try
            {
                // Arrange - Use real WMI provider for integration test
                var scope = new ManagementScope(@"\\.\root\cimv2");
                var realProvider = new OSMetricsRetriever.Providers.WMIProvider(scope);

                // Act
                var result = _plugin.GetMetric(realProvider);

                // Assert - Validate the structure and reasonable values
                Assert.IsNotNull(result);
                Assert.AreEqual("memory_usage_metric", result.Key);
                Assert.AreEqual("Memory Usage", result.Name);
                Assert.AreEqual("The amount of memory currently in use by the system.", result.Description);
                Assert.IsTrue(result.Value >= 0, "Memory usage should be non-negative");
                Assert.IsTrue(result.Total > 0, "Total memory should be positive");
                Assert.IsTrue(result.Value <= result.Total, "Used memory should not exceed total memory");
                Assert.AreEqual(MeasurementUnit.Bytes, result.Unit);
                Assert.IsTrue(result.Timestamp > 0, "Timestamp should be positive");
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is ManagementException)
            {
                Assert.Inconclusive("WMI access not available for testing: " + ex.Message);
            }
        }
    }
}